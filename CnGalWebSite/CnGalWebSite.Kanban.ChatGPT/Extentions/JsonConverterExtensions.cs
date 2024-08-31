using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services
{
    public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            var str = reader.GetString() ?? string.Empty;
            var dt = DateTime.Parse(str);
            var re = dt.ToLocalTime();
            return re;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                value = value.ToLocalTime();
            }
            var str = value.ToString("O");
            writer.WriteStringValue(str);
        }
    }
    public class DateTimeConverterUsingDateTimeNullableParse : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime?));
            if (reader.GetString() == null)
            {
                return null;
            }
            return DateTime.Parse(reader.GetString() ?? string.Empty).ToLocalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStringValue(value?.ToString("O"));
            }
            else
            {
                if (value?.Kind == DateTimeKind.Unspecified)
                {
                    value = value?.ToLocalTime();
                }
                writer.WriteStringValue(value?.ToString("O"));
            }

        }
    }
}
