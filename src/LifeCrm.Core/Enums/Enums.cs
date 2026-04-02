namespace LifeCrm.Core.Enums
{
    public enum UserRole       { Admin, Finance, Manager, Viewer }
    public enum ContactType    { Individual, Organization, Church, Foundation }
    public enum DonationStatus { Confirmed, Pending, Refunded, Voided, Cancelled }
    public enum CampaignStatus { Draft, Active, Paused, Completed, Cancelled }
    public enum ProjectStatus  { Planning, Active, OnHold, Completed, Archived }
    public enum InteractionType{ Call, Email, Meeting, Note, Other }
    public enum DocumentType   { DonationReceipt, DonationSummary, Other }
}
