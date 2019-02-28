using bizlogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace website.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class CallbacksController : Controller
    {
        private readonly IWeatherReadingsProvider _weatherReadingsProvider;
        private readonly ISecretRetriever _secretRetriever;
        private readonly IBinaryFormatter _binaryFormatter;
        private readonly IHMACHasher _hmacHasher;
        private readonly ISignatureRetriever _signatureRetriever;

        public CallbacksController(
            IWeatherReadingsProvider weatherReadingsProvider,
            ISecretRetriever secretRetriever
            , IBinaryFormatter binaryFormatter
            , IHMACHasher hmacHasher
            , ISignatureRetriever signatureRetriever)
        {
            _weatherReadingsProvider = weatherReadingsProvider;
            _secretRetriever = secretRetriever;
            _binaryFormatter = binaryFormatter;
            _hmacHasher = hmacHasher;
            _signatureRetriever = signatureRetriever;
        }

        [HttpPost("[action]")]
        public IActionResult Data([FromBody] string input)
        {
            return Ok(input);
        }

        [HttpPost("[action]")]
        public IActionResult Weather()
        {
            // asp.net does not understand Content-Type: text/plain *when using a method parameter to bind the post data to the parameter*
            // workaround is to use no parameter and deserialize Body from within the method
            string input;
            using (var reader = new StreamReader(Request.Body))
            {
                input = reader.ReadToEnd();
            }

            bool foundSignature = _signatureRetriever.GetSignature(Request.Headers, out var expectedSignature);
            if (foundSignature)        
            {
                // setx /M WEBHOOKS_SHARED_SECRET MaryHadALittleLambLittleLamb
                string actualSignature = _hmacHasher.GenerateHash(input, _secretRetriever, _binaryFormatter);            
                if (string.Equals(expectedSignature, actualSignature))
                {
                    var weatherReadingInput = JsonConvert.DeserializeObject<WeatherReadingInput>(input);
                    WeatherReading result = _weatherReadingsProvider.InsertWeatherReading(weatherReadingInput);
                    if (result == null)
                    {
                        return BadRequest("Input data failed validation.");                    
                    }

                    return Ok(result);

                    //var weatherReadingInput = JsonConvert.DeserializeObject<WeatherReadingInput>(input);
                    //bool validate = WeatherReadingInputValidator.Validate(weatherReadingInput);
                    //if (validate)
                    //{
                    //    int id = ThreadSafeCounter.GetNext();
                    //    var wr = WeatherReading.FromInput(id, weatherReadingInput);
                    //    WeatherReadings.TryAdd(id, wr);

                    //    //string data = JsonConvert.SerializeObject(new { payload = input, expectedSignature, actualSignature, input });
                    //}

                }
            }            

            return BadRequest("Signature header is invalid.");
        }

        [HttpGet("weather")]
        public IActionResult WeatherGet(
            string deviceId
            , string timestamp
            , string temperature
            , string windspeed
            , string humidity
            , string barometer)
        {
            bool foundSignature = _signatureRetriever.GetSignature(Request.Headers, out var expectedSignature);
            if (foundSignature)
            {
                string input = string.Join(",", deviceId, timestamp, temperature, windspeed, humidity, barometer);

                // setx /M WEBHOOKS_SHARED_SECRET MaryHadALittleLambLittleLamb
                string actualSignature = _hmacHasher.GenerateHash(input, _secretRetriever, _binaryFormatter);
                if (string.Equals(expectedSignature, actualSignature))
                {
                    string data = JsonConvert.SerializeObject(new { payload = input, expectedSignature, actualSignature });
                    return Ok(data);
                }
            }

            return BadRequest("Signature header is invalid.");

            //return BadRequest($"Signature mismtached: Expected: {expectedSignature} | Actual: {actualSignature}");
        }

        [HttpGet("[action]")]
        public IActionResult Secret()
        {
            string secret = Environment.GetEnvironmentVariable("WEBHOOKS_SHARED_SECRET", EnvironmentVariableTarget.Machine);
            return Ok(secret);
        }
    }
}
