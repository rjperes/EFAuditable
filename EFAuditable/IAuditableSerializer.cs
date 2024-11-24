using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace EFAuditable
{
    public interface IAuditableSerializer
    {
        string Serialize(object entity, params string[] ignoreProperties);
    }

    class JsonAuditableSerializer : IAuditableSerializer
    {
        private JsonSerializerOptions GetSerializerOptions() => new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            MaxDepth = 2,
            IncludeFields = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        public string Serialize(object entity, params string[] ignoreProperties)
        {
            var resolver = new DefaultJsonTypeInfoResolver();
            resolver.Modifiers.Add(info => Array.ForEach(info.Properties.ToArray(), prop =>
                prop.ShouldSerialize = (c, p) => !ignoreProperties.Contains(prop.Name)));

            var options = GetSerializerOptions();
            options.TypeInfoResolver = resolver;

            return JsonSerializer.Serialize(entity, options);
        }
    }
}
