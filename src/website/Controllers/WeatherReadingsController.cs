using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using bizlogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace website.Controllers
{
    [Route("api/[controller]")]
    public class WeatherReadingsController : Controller
    {
        private readonly IWeatherReadingsProvider _weatherReadingsProvider;

        public WeatherReadingsController(
            IWeatherReadingsProvider weatherReadingsProvider
            )
        {
            _weatherReadingsProvider = weatherReadingsProvider;
        }

        //[HttpGet("[action]")]
        public IActionResult Get()
        {
            return Ok(_weatherReadingsProvider.GetWeatherReadings());
        }
    }

    public class SessionWeatherReadingsProvider : IWeatherReadingsProvider
    {
        private static readonly ConcurrentDictionary<int, WeatherReading> seed = new ConcurrentDictionary<int, WeatherReading>(
            new List<KeyValuePair<int, WeatherReading>>
            {
                new KeyValuePair<int, WeatherReading>(0, new WeatherReading
                {
                    Barometer = 15, DeviceId = 999, Humidity = 98, Id=0, Temperature = 87, Timestamp = DateTime.Now, Windspeed = 3
                })
            }
        );


        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionWeatherReadingsProvider(
            IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public WeatherReading InsertWeatherReading(WeatherReadingInput weatherReadingInput)
        {
            string webhooks = _httpContextAccessor.HttpContext.Session.GetString("webhooks");

            var obj = JsonConvert.DeserializeObject<ConcurrentDictionary<int, WeatherReading>>(webhooks);
            bool validate = WeatherReadingInputValidator.Validate(weatherReadingInput);
            if (validate)
            {
                int id = ThreadSafeCounter.GetNext();
                var wr = WeatherReading.FromInput(id, weatherReadingInput);
                obj.TryAdd(id, wr);


                string serialized = JsonConvert.SerializeObject(obj);
                _httpContextAccessor.HttpContext.Session.SetString("webhooks", serialized);


                return wr;
            }

            return null;
        }

        public IEnumerable<WeatherReading> GetWeatherReadings()
        {
            string webhooks = _httpContextAccessor.HttpContext.Session.GetString("webhooks");
            if (string.IsNullOrWhiteSpace(webhooks))
            {
                string serialized = JsonConvert.SerializeObject(seed);
                _httpContextAccessor.HttpContext.Session.SetString("webhooks", serialized);
            }

            string webhooks2 = _httpContextAccessor.HttpContext.Session.GetString("webhooks");

            var obj = JsonConvert.DeserializeObject<ConcurrentDictionary<int, WeatherReading>>(webhooks2);
            return obj.Select(x => x.Value);
        }
    }
}
