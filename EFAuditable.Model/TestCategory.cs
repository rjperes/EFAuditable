namespace EFAuditable.Model
{
    public class TestCategory
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<Test> Tests { get; } = new List<Test>();
    }
}
