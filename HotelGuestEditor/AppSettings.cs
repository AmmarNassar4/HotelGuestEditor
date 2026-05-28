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

        // ─── Database ─────────────────────────────────────────────────────
        public string DbServer { get; private set; } = string.Empty;
        public string DbDatabase { get; private set; } = string.Empty;
        public string DbUser { get; private set; } = string.Empty;
        public string DbPassword { get; private set; } = string.Empty;
        public bool DbEncrypt { get; private set; }

        public string ConnectionString =>
            $"Server={DbServer};Database={DbDatabase};User Id={DbUser};Password={DbPassword};Encrypt={(DbEncrypt ? "true" : "false")};";

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

            JObject database = root["Database"] as JObject ?? new JObject();
            settings.DbServer = ReadString(database, "Server", settings.DbServer);
            settings.DbDatabase = ReadString(database, "Database", settings.DbDatabase);
            settings.DbUser = ReadString(database, "User", settings.DbUser);
            settings.DbPassword = ReadString(database, "Password", settings.DbPassword);
            settings.DbEncrypt = bool.TryParse(database["Encrypt"]?.ToString(), out bool encrypt) ? encrypt : false;

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
