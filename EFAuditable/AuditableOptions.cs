namespace EFAuditable
{
    public class AuditableOptions
    {
        public bool History { get; set; } = false;
        public string? HistoryTableName { get; set; } = nameof(AuditableHistory);
    }
}