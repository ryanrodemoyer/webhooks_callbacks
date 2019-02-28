using System;
using System.IO;
using System.Threading.Tasks;
using bizlogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using website.Controllers;

namespace website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc(options =>
                {
                    // https://stackoverflow.com/a/47807117/1444511
                    options.InputFormatters.Insert(0, new JsonAsStringInputFormatter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            
            services.AddSingleton<IWeatherReadingsProvider, InMemoryWeatherReadingsProvider>();
            services.AddSingleton<IHMACHasher, HMACSha256Hasher>();
            services.AddSingleton<ISecretRetriever, EnvVarSecretRetriever>();
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
