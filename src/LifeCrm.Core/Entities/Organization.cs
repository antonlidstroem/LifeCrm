namespace LifeCrm.Core.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}
