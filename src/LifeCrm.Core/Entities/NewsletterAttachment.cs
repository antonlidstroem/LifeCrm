namespace LifeCrm.Core.Entities
{
    /// <summary>
    /// A PDF or image file attached to a Newsletter.
    /// Stored in the database; served via the API download endpoint.
    /// </summary>
    public class NewsletterAttachment : TenantEntity
    {
        public Guid   NewsletterId { get; set; }
        public string FileName     { get; set; } = string.Empty;
        public string ContentType  { get; set; } = string.Empty;  // MIME type
        public byte[] FileBytes    { get; set; } = Array.Empty<byte>();
        public long   FileSizeBytes{ get; set; }

        public Newsletter? Newsletter { get; set; }
    }
}
