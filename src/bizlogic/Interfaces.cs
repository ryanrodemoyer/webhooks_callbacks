using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bizlogic
{
    public interface ISecureKeyGenerator
    {
        string GetKey();
    }

    public interface ISecretRetrieverAsync
    {
        Task<string> GetSecretAsync();
    }

    public interface IHMACHasher
    {
        string GenerateHash(string message, ISecretRetrieverAsync secretRetrieverAsync, IBinaryFormatter binaryFormatter);
    }

    public interface IBinaryFormatter
    {
        string Format(byte[] bytes);
    }

    public interface ISignatureRetriever
    {
        bool GetSignature(IDictionary<string, StringValues> headers, out string signature);
    }

    public interface IWeatherReadingsProvider
    {
        WeatherReading InsertWeatherReading(WeatherReadingInput weatherReadingInput);

        IEnumerable<WeatherReading> GetWeatherReadings();
    }
}
