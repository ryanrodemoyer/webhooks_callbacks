using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace bizlogic
{
    public class InMemoryWeatherReadingsProvider : IWeatherReadingsProvider
    {
        private static readonly ConcurrentDictionary<int, WeatherReading> WeatherReadings = new ConcurrentDictionary<int, WeatherReading>(
            new List<KeyValuePair<int, WeatherReading>>
            {
                new KeyValuePair<int, WeatherReading>(0, new WeatherReading
                {
                    Barometer = 15, DeviceId = 999, Humidity = 98, Id=0, Temperature = 87, Timestamp = DateTime.Now, Windspeed = 3
                })
            }
        );

        public WeatherReading InsertWeatherReading(WeatherReadingInput weatherReadingInput)
        {
            bool validate = WeatherReadingInputValidator.Validate(weatherReadingInput);
            if (validate)
            {
                int id = ThreadSafeCounter.GetNext();
                var wr = WeatherReading.FromInput(id, weatherReadingInput);
                WeatherReadings.TryAdd(id, wr);

                return wr;
            }

            return null;
        }

        public IEnumerable<WeatherReading> GetWeatherReadings()
        {
            return WeatherReadings.Values;
        }
    }

    public class HttpHeaderSignatureRetriever : ISignatureRetriever
    {
        public bool GetSignature(IDictionary<string, StringValues> headers, out string signature)
        {
            signature = null;

            bool result = headers.TryGetValue("x-payload-sig", out StringValues signatureValues);
            if (result)
            {
                signature = signatureValues.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(signature))
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public class DotNetSecureKeyGenerator : ISecureKeyGenerator
    {
        public string GetKey()
        {
            var bytes = new byte[32]; // to get a 256-bit key, recommended since we are using SHA256 for the hash

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }
    }

    //public class EnvVarSecretRetriever : ISecretRetrieverAsync
    //{
    //    public string GetSecretAsync()
    //    {
    //        string secret = Environment.GetEnvironmentVariable("WEBHOOKS_SHARED_SECRET", EnvironmentVariableTarget.Machine);
    //        return secret;
    //    }
    //}

    public class Base64BinaryFormatter : IBinaryFormatter
    {
        public string Format(byte[] bytes)
        {
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }
    }

    public class HMACSha256Hasher : IHMACHasher
    {
        public string GenerateHash(string message, ISecretRetrieverAsync secretRetrieverAsync, IBinaryFormatter binaryFormatter)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (secretRetrieverAsync == null)
            {
                throw new ArgumentNullException(nameof(secretRetrieverAsync));
            }

            string secret = secretRetrieverAsync.GetSecretAsync().Result;

            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            HMACSHA256 crypto = new HMACSHA256(secretBytes);
            byte[] hashed = crypto.ComputeHash(messageBytes);

            string format = binaryFormatter.Format(hashed);
            return format;
        }
    }
}
