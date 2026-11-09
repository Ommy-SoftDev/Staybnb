using System.Text.Json;
using Staybnb.Models;
using Staybnb.ViewModels;

namespace Staybnb.Services
{
    public static class CheckInHelper
    {
        // Reads the host's check-in setup. If the host never set one up,
        // guests only need to upload an ID document.
        public static CheckInConfig Read(CheckInProcess? process)
        {
            CheckInConfig? config = null;

            if (process != null && !string.IsNullOrWhiteSpace(process.StepsJson))
            {
                try
                {
                    config = JsonSerializer.Deserialize<CheckInConfig>(process.StepsJson);
                }
                catch (JsonException)
                {
                    config = null;
                }
            }

            config ??= new CheckInConfig();

            if (config.Documents.Count == 0)
                config.Documents.Add("ID Document");

            return config;
        }
    }
}
