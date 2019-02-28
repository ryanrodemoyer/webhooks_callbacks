using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace bizlogic
{
    public interface ISecretRetriever
    {
        string GetSecret();
    }

    public interface IHMACHasher
    {
        string GenerateHash(string message, ISecretRetriever secretRetriever, IBinaryFormatter binaryFormatter);
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
