
namespace EFAuditable.Model
{
    public class Test : IAuditable
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public TestCategory? Category { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset? CreatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}