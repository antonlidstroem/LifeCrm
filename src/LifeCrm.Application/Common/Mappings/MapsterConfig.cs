using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Core.Entities;
using Mapster;

namespace LifeCrm.Application.Common.Mappings
{
    public static class MapsterConfig
    {
        public static void RegisterMappings()
        {
            TypeAdapterConfig<Contact, ContactDto>.NewConfig()
                .Map(dest => dest.DonationCount, src => src.Donations.Count(d => !d.IsDeleted))
                .Map(dest => dest.TotalDonated, src => src.Donations.Where(d => !d.IsDeleted).Sum(d => (decimal?)d.Amount) ?? 0)
                .Map(dest => dest.InteractionCount, src => src.Interactions.Count(i => !i.IsDeleted));

            TypeAdapterConfig<Donation, DonationDto>.NewConfig()
                .Map(dest => dest.ContactName, src => src.Contact != null ? src.Contact.Name : string.Empty)
                .Map(dest => dest.CampaignName, src => src.Campaign != null ? src.Campaign.Name : null)
                .Map(dest => dest.ProjectName, src => src.Project != null ? src.Project.Name : null);

            TypeAdapterConfig<Interaction, InteractionDto>.NewConfig()
                .Map(dest => dest.ContactName, src => src.Contact != null ? src.Contact.Name : null)
                .Map(dest => dest.ProjectName, src => src.Project != null ? src.Project.Name : null)
                .Map(dest => dest.CreatedByName, src => src.CreatedBy);
        }
    }
}
