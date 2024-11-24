namespace EFAuditable
{
    public record AuditableHistory
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public required string Entity { get; set; }
        public required string Values { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public required string User { get; set; }
        public required string State { get; set; }
    }
}