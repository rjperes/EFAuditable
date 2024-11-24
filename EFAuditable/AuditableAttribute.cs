namespace EFAuditable
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AuditableAttribute : Attribute
    {
    }
}