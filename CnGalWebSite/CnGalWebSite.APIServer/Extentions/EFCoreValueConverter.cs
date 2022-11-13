using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using System;

namespace CnGalWebSite.APIServer.Extentions
{

    class EFCoreUtcValueConverter : ValueConverter<DateTime, DateTime>
    {
        public EFCoreUtcValueConverter()
            : base(v => v.AddHours(8), v => DateTime.SpecifyKind(v, DateTimeKind.Utc).AddHours(v==DateTime.MinValue?0:-8))
        {
        }
    }

    class EFCoreUtcNullableValueConverter : ValueConverter<DateTime?, DateTime?>
    {
        public EFCoreUtcNullableValueConverter()
            : base(v => v==null?null:v.Value.AddHours(8), v =>v==null?null: DateTime.SpecifyKind(v.Value, DateTimeKind.Utc).AddHours(v.Value == DateTime.MinValue ? 0 :- 8))
        {
        }
    }
}
