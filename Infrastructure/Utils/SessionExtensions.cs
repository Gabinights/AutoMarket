using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Infrastructure.Utils
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value, _jsonOptions));
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null) return default;

            try
            {
                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (JsonException)
            {
                // In case of corruption, return default/null to prevent crash
                return default;
            }
        }
    }
}
