using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using bizlogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using website.Controllers;

namespace website
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public Startup(
            IConfiguration configuration
            , IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(
            IServiceCollection services)
        {
            if (_hostingEnvironment.IsDevelopment())
            {
                services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = "localhost";
                    options.InstanceName = "WebhookInstance";
                });
            }
            else
            {
                services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = _configuration["AZURE_REDIS_CACHE"];
                    options.InstanceName = "WebhookInstance";
                });
            }

            // Add framework services.
            services.AddMvc(options =>
                {
                    // https://stackoverflow.com/a/47807117/1444511
                    options.InputFormatters.Insert(0, new JsonAsStringInputFormatter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSession(options =>
            {
                options.Cookie.Name = ".Webhooks.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(20d);
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IWeatherReadingsProvider, SessionWeatherReadingsProvider>();
            services.AddSingleton<IHMACHasher, HMACSha256Hasher>();

            //if (_hostingEnvironment.IsDevelopment())
            //{
            //    //services.AddSingleton<ISecretRetrieverAsync, EnvVarSecretRetriever>();
            //}
            //else
            {
                services.AddSingleton<ISecretRetrieverAsync, CachedSecretRetriever>();
            }

            services.AddSingleton<ISecureKeyGenerator, DotNetSecureKeyGenerator>();
            services.AddSingleton<IBinaryFormatter, Base64BinaryFormatter>();
            services.AddSingleton<ISignatureRetriever, HttpHeaderSignatureRetriever>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Webpack initialization with hot-reload.
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }

    public class ConfigurationSecretRetrieverAsync : ISecretRetrieverAsync
    {
        private readonly IConfiguration _configuration;

        public ConfigurationSecretRetrieverAsync(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<string> GetSecretAsync()
        {
            string secret = _configuration["WEBHOOKS_SHARED_SECRET"];
            return Task.FromResult(secret);
        }
    }

    public class CachedSecretRetriever : ISecretRetrieverAsync
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ISecureKeyGenerator _secureKeyGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CachedSecretRetriever(
            IDistributedCache distributedCache
            , ISecureKeyGenerator secureKeyGenerator
            , IHttpContextAccessor httpContextAccessor
        )
        {
            _distributedCache = distributedCache;
            _secureKeyGenerator = secureKeyGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetSecretAsync()
        {
            string id = _httpContextAccessor.HttpContext.Session.Id;

            string secret = await _distributedCache.GetStringAsync($"{id}.secret");
            if (string.IsNullOrWhiteSpace(secret))
            {
                secret = _secureKeyGenerator.GetKey();
                await _distributedCache.SetStringAsync($"{id}.secret", secret, new DistributedCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)});
            }
            return secret;
        }
    }

    /// <summary>
    /// For Content-Type: application/json requests, give ASP.NET Core the ability to move the raw JSON text to a string via method parameter and model binding.
    /// </summary>
    /// <remarks>https://stackoverflow.com/a/47807117/1444511</remarks>
    public class JsonAsStringInputFormatter : InputFormatter
    {
        public JsonAsStringInputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            using (var reader = new StreamReader(context.HttpContext.Request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }
}
