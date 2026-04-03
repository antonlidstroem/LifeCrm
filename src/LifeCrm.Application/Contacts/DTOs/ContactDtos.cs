using System.ComponentModel.DataAnnotations;
using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Contacts.DTOs
{
    public record ContactListDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ContactType Type { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? Tags { get; init; }
        public decimal TotalDonations { get; init; }
        public DateOnly? LastDonationDate { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }

    public record ContactDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public ContactType Type { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; }
        public string? City { get; init; }
        public string? StateProvince { get; init; }
        public string? PostalCode { get; init; }
        public string? Country { get; init; }
        public string? Tags { get; init; }
        public string? Notes { get; init; }
        public string? PrimaryContactName { get; init; }
        public bool EmailOptOut { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? LastModifiedAt { get; init; }
        public int DonationCount { get; init; }
        public decimal TotalDonated { get; init; }
        public int InteractionCount { get; init; }
    }

    public record CreateContactRequest
    {
        [Required][MaxLength(200)] public string Name { get; init; } = string.Empty;
        public ContactType Type { get; init; } = ContactType.Individual;
        [EmailAddress][MaxLength(320)] public string? Email { get; init; }
        [MaxLength(50)] public string? Phone { get; init; }
        [MaxLength(200)] public string? AddressLine1 { get; init; }
        [MaxLength(200)] public string? AddressLine2 { get; init; }
        [MaxLength(100)] public string? City { get; init; }
        [MaxLength(100)] public string? StateProvince { get; init; }
        [MaxLength(20)]  public string? PostalCode { get; init; }
        [MaxLength(100)] public string? Country { get; init; }
        [MaxLength(500)] public string? Tags { get; init; }
        [MaxLength(4000)] public string? Notes { get; init; }
        [MaxLength(200)] public string? PrimaryContactName { get; init; }
        public bool EmailOptOut { get; init; } = false;
    }

    public record UpdateContactRequest : CreateContactRequest
    {
        [Required] public Guid Id { get; init; }
    }

    // FIXED: Removed bogus ErrorCount property — it was being mapped from CSV columns
    // and polluting import results. CSV row DTOs should only contain column-mapped fields.
    public record ContactCsvRow
    {
        public string  Name         { get; init; } = string.Empty;
        public string? Type         { get; init; }
        public string? Email        { get; init; }
        public string? Phone        { get; init; }
        public string? Tags         { get; init; }
        public string? AddressLine1 { get; init; }
        public string? City         { get; init; }
        public string? StateProvince{ get; init; }
        public string? PostalCode   { get; init; }
        public string? Country      { get; init; }
        public string? Notes        { get; init; }
    }
}
