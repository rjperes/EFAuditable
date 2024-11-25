namespace EFAuditable
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AuditableAttribute : Attribute
    {
        public bool History { get; init; }
    }
}
