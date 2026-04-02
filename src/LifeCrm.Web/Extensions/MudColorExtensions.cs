using LifeCrm.Core.Enums;
using MudBlazor;

namespace LifeCrm.Web.Extensions
{
    public static class MudColorExtensions
    {
        public static Color ToMudColor(this DonationStatus status) => status switch
        {
            DonationStatus.Confirmed => Color.Success,
            DonationStatus.Pending   => Color.Warning,
            DonationStatus.Refunded  => Color.Info,
            DonationStatus.Voided    => Color.Error,
            _                        => Color.Default
        };

        public static Color ToMudColor(this CampaignStatus status) => status switch
        {
            CampaignStatus.Active    => Color.Success,
            CampaignStatus.Completed => Color.Info,
            CampaignStatus.Draft     => Color.Default,
            CampaignStatus.Paused    => Color.Warning,
            CampaignStatus.Cancelled => Color.Error,
            _                        => Color.Default
        };

        // FIX: Planning was missing — caused SwitchExpressionException at runtime
        public static Color ToMudColor(this ProjectStatus status) => status switch
        {
            ProjectStatus.Planning  => Color.Default,
            ProjectStatus.Active    => Color.Success,
            ProjectStatus.Completed => Color.Info,
            ProjectStatus.OnHold    => Color.Warning,
            ProjectStatus.Archived  => Color.Default,
            _                       => Color.Default
        };

        public static Color ToMudColor(this ContactType type) => type switch
        {
            ContactType.Individual   => Color.Primary,
            ContactType.Organization => Color.Secondary,
            ContactType.Church       => Color.Tertiary,
            ContactType.Foundation   => Color.Info,
            _                        => Color.Default
        };
    }
}
