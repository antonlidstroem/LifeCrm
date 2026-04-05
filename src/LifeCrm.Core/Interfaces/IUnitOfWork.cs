using LifeCrm.Core.Entities;

namespace LifeCrm.Core.Interfaces
{
    public interface ICampaignRepository : IRepository<Campaign>
    {
        Task<IReadOnlyList<Campaign>> GetActiveAsync(CancellationToken ct = default);
    }

    public interface IUnitOfWork : IDisposable
    {
        IRepository<Contact>               Contacts              { get; }
        IRepository<Donation>              Donations             { get; }
        ICampaignRepository                Campaigns             { get; }
        IRepository<Project>               Projects              { get; }
        IRepository<Interaction>           Interactions          { get; }
        IInteractionRepository             InteractionRepo       { get; }
        IRepository<Newsletter>            Newsletters           { get; }
        IRepository<NewsletterAttachment>  NewsletterAttachments { get; }  // Phase 2
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }

    public interface IInteractionRepository : IRepository<Interaction>
    {
        Task<IReadOnlyList<Interaction>> GetByContactAsync(Guid contactId, CancellationToken ct = default);
        Task<IReadOnlyList<Interaction>> GetByProjectAsync(Guid projectId, CancellationToken ct = default);
    }
}
