namespace EFAuditable
{
    public class AuditableOptions
    {
        public bool StoreOldValues { get; set; } = false;
        public string? OldValuesTableName { get; set; } = nameof(AuditableHistory);
    }
}