using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace HotelGuestEditor
{
    internal sealed class AppSettings
    {
        public string BaseUrl { get; private set; } = "https://example.com";
        public string KioskId { get; private set; } = "K1";
        public string KioskName { get; private set; } = "K1";
        public string TemplateId { get; private set; } = "T_Checkin";
        public int SessionMonitorIntervalSeconds { get; private set; } = 10;

        public static AppSettings Load()
        {
            var settings = new AppSettings();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            if (!File.Exists(path))
                return settings;

            JObject root = JObject.Parse(File.ReadAllText(path));
            JObject guestGate = root["GuestGate"] as JObject ?? new JObject();

            settings.BaseUrl = ReadString(guestGate, "BaseUrl", settings.BaseUrl).TrimEnd('/');
            settings.KioskId = ReadString(guestGate, "KioskId", settings.KioskId);
            settings.KioskName = ReadString(guestGate, "KioskName", settings.KioskId);
            settings.TemplateId = ReadString(guestGate, "TemplateId", settings.TemplateId);
            settings.SessionMonitorIntervalSeconds = ReadInt(guestGate, "SessionMonitorIntervalSeconds", settings.SessionMonitorIntervalSeconds, 1, 3600);

            return settings;
        }

        private static string ReadString(JObject source, string key, string fallback)
        {
            string value = source[key]?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static int ReadInt(JObject source, string key, int fallback, int min, int max)
        {
            if (!int.TryParse(source[key]?.ToString(), out int value))
                return fallback;

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
