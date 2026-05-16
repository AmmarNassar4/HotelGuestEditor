using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HotelGuestEditor
{
    internal sealed class CountryInfo
    {
        public CountryInfo(string code, string name)
        {
            Code = (code ?? string.Empty).Trim().ToUpperInvariant();
            Name = (name ?? string.Empty).Trim();
        }

        public string Code { get; }
        public string Name { get; }
        public string DisplayName => $"{Name} - {Code}";

        public override string ToString()
        {
            return DisplayName;
        }
    }

    internal static class CountryCatalog
    {
        private const string FileName = "Countries.csv";

        public static List<CountryInfo> Load()
        {
            string path = FindCountriesCsvPath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return new List<CountryInfo>();

            var countries = new List<CountryInfo>();

            foreach (string line in File.ReadLines(path, Encoding.UTF8).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] columns = ParseCsvLine(line).ToArray();
                if (columns.Length < 2)
                    continue;

                string code = columns[0].Trim();
                string name = columns[1].Trim();

                if (code.Length == 3 && !string.IsNullOrWhiteSpace(name))
                    countries.Add(new CountryInfo(code, name));
            }

            return countries
                .GroupBy(c => c.Code, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();
        }

        public static bool TryFindCode(IEnumerable<CountryInfo> countries, string text, out string code)
        {
            code = string.Empty;
            string value = (text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            string normalized = value.ToUpperInvariant();
            string[] parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (CountryInfo country in countries)
            {
                if (string.Equals(country.Code, normalized, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(country.Name, value, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(country.DisplayName, value, StringComparison.OrdinalIgnoreCase) ||
                    parts.Any(p => string.Equals(p, country.Code, StringComparison.OrdinalIgnoreCase)) ||
                    parts.Any(p => string.Equals(p, country.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    code = country.Code;
                    return true;
                }
            }

            CountryInfo containsMatch = countries.FirstOrDefault(c =>
                c.Name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 ||
                c.Code.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 ||
                c.DisplayName.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

            if (containsMatch != null)
            {
                code = containsMatch.Code;
                return true;
            }

            return false;
        }

        private static string FindCountriesCsvPath()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;

            for (int i = 0; i < 8 && !string.IsNullOrWhiteSpace(directory); i++)
            {
                string candidate = Path.Combine(directory, FileName);
                if (File.Exists(candidate))
                    return candidate;

                directory = Directory.GetParent(directory)?.FullName;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
        }

        private static IEnumerable<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            result.Add(current.ToString());
            return result;
        }
    }
}
