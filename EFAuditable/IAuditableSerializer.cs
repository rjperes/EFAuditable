using System.Text.Json.Serialization;
using System.Text.Json;

namespace EFAuditable
{
    public interface IAuditableSerializer
    {
        string Serialize(object entity);
    }

    class JsonAuditableSerializer : IAuditableSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            MaxDepth = 2,
            IncludeFields = false,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        public string Serialize(object entity)
        {
            return JsonSerializer.Serialize(entity, _serializerOptions);
        }
    }
}
