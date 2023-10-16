using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.GameSite.API.Extentions
{


    public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
    {
        /// <summary>
        /// API 端
        /// 内部处理：UTC
        ///
        /// 读取：UTC
        /// 保存：UTC
        /// </summary>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));
            var str = reader.GetString() ?? string.Empty;
            var dt = DateTime.Parse(str);
            var re = dt.ToUniversalTime();
            return re;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            if (value.Kind == DateTimeKind.Unspecified)
            {
                value = value.ToUniversalTime();
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
            return reader.GetString() == null ? null : DateTime.Parse(reader.GetString() ?? string.Empty).ToUniversalTime();
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
                    value = value?.ToUniversalTime();
                }
                writer.WriteStringValue(value?.ToString("O"));
            }

        }
    }
}
