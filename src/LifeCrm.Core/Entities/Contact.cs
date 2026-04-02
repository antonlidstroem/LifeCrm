using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class Contact : TenantEntity
    {
        public string Name { get; set; } = string.Empty;
        public ContactType Type { get; set; } = ContactType.Individual;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Tags { get; set; }
        public string? Notes { get; set; }
        public string? PrimaryContactName { get; set; }
        public bool EmailOptOut { get; set; } = false;
        public string CreatedBy { get; set; } = "system";
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
