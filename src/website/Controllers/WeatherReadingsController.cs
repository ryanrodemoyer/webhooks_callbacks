using bizlogic;
using Microsoft.AspNetCore.Mvc;

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
}
