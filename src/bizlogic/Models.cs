using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace bizlogic
{
    public static class ThreadSafeCounter
    {
        private static int _count = 0;

        public static int GetNext()
        {
            Interlocked.Increment(ref _count);
            return _count;
        }
    }

    public static class WeatherReadingInputValidator
    {
        public static bool Validate(WeatherReadingInput input)
        {
            if (input.DeviceId == 0)
            {
                return false;
            }

            if (input.Barometer == 0)
            {
                return false;
            }

            if (input.Timestamp == DateTime.MinValue)
            {
                return false;
            }

            if (input.Humidity == 0)
            {
                return false;
            }

            return true;
        }
    }

    public class WeatherReading : WeatherReadingInput
    {
        public int Id { get; set; }

        public static WeatherReading FromInput(int id, WeatherReadingInput weatherReadingInput)
        {
            var wr = new WeatherReading
            {
                Id = id,
                DeviceId = weatherReadingInput.DeviceId,
                Temperature = weatherReadingInput.Temperature,
                Timestamp = weatherReadingInput.Timestamp,
                Barometer = weatherReadingInput.Barometer,
                Humidity = weatherReadingInput.Humidity,
                Windspeed = weatherReadingInput.Windspeed
            };

            return wr;
        }
    }

    public class WeatherReadingInput
    {
        public int DeviceId { get; set; }

        public DateTime Timestamp { get; set; }

        public decimal Temperature { get; set; }

        public decimal Windspeed { get; set; }

        public decimal Humidity { get; set; }

        public decimal Barometer { get; set; }
    }
}
