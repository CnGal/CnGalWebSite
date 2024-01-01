using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using System;

namespace CnGalWebSite.TimedTask.Extentions
{

    class EFCoreUtcValueConverter : ValueConverter<DateTime, DateTime>
    {
        public EFCoreUtcValueConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    class EFCoreUtcNullableValueConverter : ValueConverter<DateTime?, DateTime?>
    {
        public EFCoreUtcNullableValueConverter()
            : base(v => v==null?null:v.Value, v =>v==null?null: DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
        {
        }
    }
}
