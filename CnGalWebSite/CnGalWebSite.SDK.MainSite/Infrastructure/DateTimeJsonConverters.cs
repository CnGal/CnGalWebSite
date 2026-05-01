using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public class DateTimeConverterUsingDateTimeParse : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString() ?? string.Empty).ToLocalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            value = value.ToLocalTime();
        }
        writer.WriteStringValue(value.ToString("O"));
    }
}

public class DateTimeConverterUsingDateTimeNullableParse : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
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
