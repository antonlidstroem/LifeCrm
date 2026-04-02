using System.ComponentModel.DataAnnotations;

namespace LifeCrm.Application.Common.DTOs
{
    public record PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public record PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 25;
        public int Page { get; init; } = 1;
        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
        }
        public string? Search { get; init; }
        public string? SortBy { get; init; }
        public bool SortAscending { get; init; } = true;
    }

    public record ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static ApiResponse<T> Ok(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string error)
            => new() { Success = false, Errors = new[] { error } };

        public static ApiResponse<T> Fail(IEnumerable<string> errors)
            => new() { Success = false, Errors = errors.ToList().AsReadOnly() };
    }

    public record ApiResponse
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static ApiResponse Ok(string? message = null)
            => new() { Success = true, Message = message };

        public static ApiResponse Fail(string error)
            => new() { Success = false, Errors = new[] { error } };

        public static ApiResponse Fail(IEnumerable<string> errors)
            => new() { Success = false, Errors = errors.ToList().AsReadOnly() };
    }

    public record DashboardDto
    {
        public decimal DonationsThisMonth { get; init; }
        public decimal DonationsLastMonth { get; init; }
        public decimal DonationsMoMChangePercent { get; init; }
        public int TotalContacts { get; init; }
        public int NewContactsThisMonth { get; init; }
        public int ActiveCampaigns { get; init; }
        public IReadOnlyList<CampaignSummaryDto> TopCampaigns { get; init; } = Array.Empty<CampaignSummaryDto>();
        public IReadOnlyList<ActivityFeedItemDto> RecentActivity { get; init; } = Array.Empty<ActivityFeedItemDto>();
    }

    public record CampaignSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal? BudgetGoal { get; init; }
        public decimal TotalRaised { get; init; }
        public decimal? ProgressPercent { get; init; }
    }

    public record ActivityFeedItemDto
    {
        public string ActivityType { get; init; } = string.Empty;
        public Guid EntityId { get; init; }
        public string ContactName { get; init; } = string.Empty;
        public Guid ContactId { get; init; }
        public string Summary { get; init; } = string.Empty;
        public DateTimeOffset OccurredAt { get; init; }
    }

    public record LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        [MaxLength(320)]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(1, ErrorMessage = "Password cannot be empty.")]
        [MaxLength(128)]
        public string Password { get; init; } = string.Empty;
    }

    public record LoginResponse
    {
        public string Token { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; init; }
        public string UserId { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string OrganizationId { get; init; } = string.Empty;
        public string OrganizationName { get; init; } = string.Empty;
    }

    public record ChangePasswordRequest
    {
        [Required]
        [MaxLength(128)]
        public string CurrentPassword { get; init; } = string.Empty;

        [Required]
        [MinLength(10, ErrorMessage = "New password must be at least 10 characters.")]
        [MaxLength(128)]
        public string NewPassword { get; init; } = string.Empty;
    }
}
