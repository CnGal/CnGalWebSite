using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Typesense;

namespace CnGalWebSite.APIServer.CustomMiddlewares
{
    public record MyField : Field
    {
        [JsonPropertyName("locale")]
        public string Locale { get; init; }

        [JsonConstructor]
        public MyField(string name, FieldType type, bool facet, bool optional = false, bool index = true, string locale = "zh") : base(name, type, facet, optional, index)
        {
            Locale = locale;

        }

        [Obsolete("A better choice going forward is using the constructor with 'FieldType' enum instead.")]
        public MyField(string name, string type, bool facet, bool optional = false, bool index = true, string locale = "zh") : base(name, type, facet, optional, index)
        {
            Locale = locale;
        }
    }
    public record MySchema : Schema
    {
        [JsonPropertyName("fields")]
        public new IEnumerable<MyField> Fields { get; init; }

        public MySchema(string name, IEnumerable<MyField> fields)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException();
            Name = name;
            Fields = fields;
        }

        public MySchema(string name, IEnumerable<MyField> fields, string defaultSortingField)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException();
            Name = name;
            Fields = fields;
            DefaultSortingField = defaultSortingField;
        }
    }

}
