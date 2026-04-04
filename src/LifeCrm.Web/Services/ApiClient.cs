using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using LifeCrm.Application.Campaigns.DTOs;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Application.Contacts.Commands;
using LifeCrm.Application.Contacts.DTOs;
using LifeCrm.Application.Dashboard;
using LifeCrm.Application.Documents.DTOs;
using LifeCrm.Application.Donations.DTOs;
using LifeCrm.Application.Interactions.DTOs;
using LifeCrm.Application.Projects.DTOs;
using LifeCrm.Application.Users.DTOs;
using LifeCrm.Core.Enums;
using Microsoft.JSInterop;

namespace LifeCrm.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient           _http;
        private readonly ILocalStorageService _storage;
        private readonly IJSRuntime           _js;
        private const string TokenKey = "lifecrm_token";

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient http, ILocalStorageService storage, IJSRuntime js)
        { _http = http; _storage = storage; _js = js; }

        private async Task AttachTokenAsync()
        {
            var token = await _storage.GetItemAsStringAsync(TokenKey);
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task SaveTokenAsync(string token) => await _storage.SetItemAsStringAsync(TokenKey, token);
        public async Task ClearTokenAsync()            => await _storage.RemoveItemAsync(TokenKey);

        private async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            await AttachTokenAsync();
            try { return await _http.GetFromJsonAsync<ApiResponse<T>>(url, _jsonOpts) ?? ApiResponse<T>.Fail("Tom respons."); }
            catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null)
        {
            await AttachTokenAsync();
            try
            {
                var resp = body is null
                    ? await _http.PostAsync(url, null)
                    : await _http.PostAsJsonAsync(url, body, _jsonOpts);
                return await resp.Content.ReadFromJsonAsync<ApiResponse<T>>(_jsonOpts)
                    ?? ApiResponse<T>.Fail("Tom respons.");
            }
            catch (Exception ex) { return ApiResponse<T>.Fail(ex.Message); }
        }

        private async Task<ApiResponse> PostVoidAsync(string url, object? body = null)
        {
            await AttachTokenAsync();
            try
            {
                var resp = body is null
                    ? await _http.PostAsync(url, null)
                    : await _http.PostAsJsonAsync(url, body, _jsonOpts);
                if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
                return await resp.Content.ReadFromJsonAsync<ApiResponse>(_jsonOpts)
                    ?? ApiResponse.Fail("Okänt fel.");
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        private async Task<ApiResponse> PutAsync(string url, object body)
        {
            await AttachTokenAsync();
            try
            {
                var resp = await _http.PutAsJsonAsync(url, body, _jsonOpts);
                if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
                return await resp.Content.ReadFromJsonAsync<ApiResponse>(_jsonOpts)
                    ?? ApiResponse.Fail("Okänt fel.");
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        private async Task<ApiResponse> PatchAsync(string url, object body)
        {
            await AttachTokenAsync();
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(body, _jsonOpts), Encoding.UTF8, "application/json");
                var resp = await _http.PatchAsync(url, content);
                if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
                return await resp.Content.ReadFromJsonAsync<ApiResponse>(_jsonOpts)
                    ?? ApiResponse.Fail("Okänt fel.");
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        private async Task<ApiResponse> DeleteAsync(string url)
        {
            await AttachTokenAsync();
            try
            {
                var resp = await _http.DeleteAsync(url);
                if (resp.IsSuccessStatusCode) return ApiResponse.Ok();
                return await resp.Content.ReadFromJsonAsync<ApiResponse>(_jsonOpts)
                    ?? ApiResponse.Fail("Okänt fel.");
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        private async Task<ApiResponse> DownloadFileAsync(string url, string filename, string mime = "application/octet-stream")
        {
            await AttachTokenAsync();
            try
            {
                var bytes = await _http.GetByteArrayAsync(url);
                await _js.InvokeVoidAsync("lifecrm.downloadFile", filename, Convert.ToBase64String(bytes), mime);
                return ApiResponse.Ok();
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        // ── AUTH ────────────────────────────────────────────────────────────────
        // FIXED: Wrapped in try/catch so network errors return a failure response
        // instead of throwing an unhandled HttpRequestException to the LoginPage.
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/v1/auth/login", request, _jsonOpts);
                return await resp.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(_jsonOpts)
                    ?? ApiResponse<LoginResponse>.Fail("Tom respons från servern.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponse>.Fail($"Anslutningsfel: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequest request)
            => await PostVoidAsync("api/v1/auth/change-password", request);

        // ── CONTACTS ────────────────────────────────────────────────────────────
        public async Task<ApiResponse<PagedResult<ContactListDto>>> GetContactsAsync(PaginationParams p)
            => await GetAsync<PagedResult<ContactListDto>>(
                $"api/v1/contacts?page={p.Page}&pageSize={p.PageSize}" +
                $"&search={Uri.EscapeDataString(p.Search ?? "")}" +
                $"&sortBy={Uri.EscapeDataString(p.SortBy ?? "")}&sortAscending={p.SortAscending}");

        public async Task<ApiResponse<ContactDto>> GetContactAsync(Guid id)
            => await GetAsync<ContactDto>($"api/v1/contacts/{id}");

        public async Task<ApiResponse<Guid>> CreateContactAsync(CreateContactRequest req)
            => await PostAsync<Guid>("api/v1/contacts", req);

        public async Task<ApiResponse> UpdateContactAsync(Guid id, UpdateContactRequest req)
            => await PutAsync($"api/v1/contacts/{id}", req);

        public async Task<ApiResponse> DeleteContactAsync(Guid id)
            => await DeleteAsync($"api/v1/contacts/{id}");

        public async Task<ApiResponse> ExportContactsCsvAsync()
        {
            await AttachTokenAsync();
            try
            {
                var bytes = await _http.GetByteArrayAsync("api/v1/contacts/export");
                await _js.InvokeVoidAsync("lifecrm.downloadFile",
                    $"contacts-{DateTime.Today:yyyy-MM-dd}.csv",
                    Convert.ToBase64String(bytes), "text/csv");
                return ApiResponse.Ok();
            }
            catch (Exception ex) { return ApiResponse.Fail(ex.Message); }
        }

        public async Task<ApiResponse<ImportContactsResult>> ImportContactsCsvAsync(Stream fileStream, string fileName)
        {
            await AttachTokenAsync();
            try
            {
                using var ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(ms.ToArray()), "file", fileName);
                var resp = await _http.PostAsync("api/v1/contacts/import", content);
                return await resp.Content.ReadFromJsonAsync<ApiResponse<ImportContactsResult>>(_jsonOpts)
                    ?? ApiResponse<ImportContactsResult>.Fail("Import misslyckades.");
            }
            catch (Exception ex) { return ApiResponse<ImportContactsResult>.Fail(ex.Message); }
        }

        // ── DONATIONS ───────────────────────────────────────────────────────────
        public async Task<ApiResponse<PagedResult<DonationListDto>>> GetDonationsAsync(
            PaginationParams p,
            Guid?     contactId  = null,
            DateOnly? fromDate   = null,
            DateOnly? toDate     = null,
            Guid?     campaignId = null,
            Guid?     projectId  = null)
        {
            var url = $"api/v1/donations?page={p.Page}&pageSize={p.PageSize}";
            if (contactId.HasValue)  url += $"&contactId={contactId.Value}";
            if (fromDate.HasValue)   url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue)     url += $"&toDate={toDate.Value:yyyy-MM-dd}";
            if (campaignId.HasValue) url += $"&campaignId={campaignId.Value}";
            if (projectId.HasValue)  url += $"&projectId={projectId.Value}";
            return await GetAsync<PagedResult<DonationListDto>>(url);
        }

        public async Task<ApiResponse<DonationDto>> GetDonationAsync(Guid id)
            => await GetAsync<DonationDto>($"api/v1/donations/{id}");

        public async Task<ApiResponse<Guid>> CreateDonationAsync(CreateDonationRequest req)
            => await PostAsync<Guid>("api/v1/donations", req);

        public async Task<ApiResponse> UpdateDonationAsync(Guid id, UpdateDonationRequest req)
            => await PutAsync($"api/v1/donations/{id}", req);

        public async Task<ApiResponse> DeleteDonationAsync(Guid id)
            => await DeleteAsync($"api/v1/donations/{id}");

        public async Task<ApiResponse<DocumentDto>> GenerateReceiptAsync(Guid id, bool sendByEmail)
            => await PostAsync<DocumentDto>($"api/v1/donations/{id}/receipt?sendByEmail={sendByEmail}");

        public async Task<ApiResponse> DownloadReceiptAsync(Guid donationId, Guid documentId)
            => await DownloadFileAsync(
                $"api/v1/donations/{donationId}/receipt/{documentId}/download",
                $"receipt-{donationId:N}.pdf", "application/pdf");

        public async Task<ApiResponse> DownloadLatestReceiptAsync(Guid donationId)
            => await DownloadFileAsync(
                $"api/v1/documents/donation/{donationId}/receipt/latest",
                $"receipt-{donationId:N}.pdf", "application/pdf");

        // ── CAMPAIGNS ───────────────────────────────────────────────────────────
        public async Task<ApiResponse<PagedResult<CampaignListDto>>> GetCampaignsAsync(PaginationParams p)
            => await GetAsync<PagedResult<CampaignListDto>>(
                $"api/v1/campaigns?page={p.Page}&pageSize={p.PageSize}" +
                $"&search={Uri.EscapeDataString(p.Search ?? "")}");

        public async Task<ApiResponse<CampaignDto>> GetCampaignAsync(Guid id)
            => await GetAsync<CampaignDto>($"api/v1/campaigns/{id}");

        public async Task<ApiResponse<Guid>> CreateCampaignAsync(CreateCampaignRequest req)
            => await PostAsync<Guid>("api/v1/campaigns", req);

        public async Task<ApiResponse> UpdateCampaignAsync(Guid id, UpdateCampaignRequest req)
            => await PutAsync($"api/v1/campaigns/{id}", req);

        public async Task<ApiResponse> DeleteCampaignAsync(Guid id)
            => await DeleteAsync($"api/v1/campaigns/{id}");

        // ── PROJECTS ────────────────────────────────────────────────────────────
        public async Task<ApiResponse<PagedResult<ProjectListDto>>> GetProjectsAsync(PaginationParams p)
            => await GetAsync<PagedResult<ProjectListDto>>(
                $"api/v1/projects?page={p.Page}&pageSize={p.PageSize}" +
                $"&search={Uri.EscapeDataString(p.Search ?? "")}");

        public async Task<ApiResponse<ProjectDto>> GetProjectAsync(Guid id)
            => await GetAsync<ProjectDto>($"api/v1/projects/{id}");

        public async Task<ApiResponse<Guid>> CreateProjectAsync(CreateProjectRequest req)
            => await PostAsync<Guid>("api/v1/projects", req);

        public async Task<ApiResponse> UpdateProjectAsync(Guid id, UpdateProjectRequest req)
            => await PutAsync($"api/v1/projects/{id}", req);

        public async Task<ApiResponse> DeleteProjectAsync(Guid id)
            => await DeleteAsync($"api/v1/projects/{id}");

        // ── INTERACTIONS ────────────────────────────────────────────────────────
        // FIXED: Added GetInteractionAsync for the interaction edit dialog
        public async Task<ApiResponse<InteractionDto>> GetInteractionAsync(Guid id)
            => await GetAsync<InteractionDto>($"api/v1/interactions/{id}");

        public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetInteractionsAsync(Guid contactId)
            => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/contacts/{contactId}/interactions");

        public async Task<ApiResponse<IReadOnlyList<InteractionDto>>> GetProjectInteractionsAsync(Guid projectId)
            => await GetAsync<IReadOnlyList<InteractionDto>>($"api/v1/projects/{projectId}/interactions");

        public async Task<ApiResponse<Guid>> CreateInteractionAsync(CreateInteractionRequest req)
            => await PostAsync<Guid>("api/v1/interactions", req);

        public async Task<ApiResponse> UpdateInteractionAsync(Guid id, UpdateInteractionRequest req)
            => await PutAsync($"api/v1/interactions/{id}", req);

        public async Task<ApiResponse> DeleteInteractionAsync(Guid id)
            => await DeleteAsync($"api/v1/interactions/{id}");

        // ── USERS ───────────────────────────────────────────────────────────────
        public async Task<ApiResponse<IReadOnlyList<UserSummaryDto>>> GetUsersAsync()
            => await GetAsync<IReadOnlyList<UserSummaryDto>>("api/v1/users");

        public async Task<ApiResponse> ChangeUserRoleAsync(Guid id, UserRole newRole)
            => await PatchAsync($"api/v1/users/{id}/role", newRole);

        public async Task<ApiResponse> DeactivateUserAsync(Guid id)
            => await PatchAsync($"api/v1/users/{id}/deactivate", new { });

        public async Task<ApiResponse> ActivateUserAsync(Guid id)
            => await PatchAsync($"api/v1/users/{id}/activate", new { });

        // ── DASHBOARD ───────────────────────────────────────────────────────────
        public async Task<ApiResponse<DashboardDto>> GetDashboardAsync()
            => await GetAsync<DashboardDto>("api/v1/dashboard");

        // ── DOCUMENTS ───────────────────────────────────────────────────────────
        public async Task<ApiResponse<DocumentDto>> GenerateDonationSummaryAsync(GenerateDonationSummaryRequest req)
            => await PostAsync<DocumentDto>("api/v1/documents/summary", req);

        public async Task<ApiResponse> DownloadDocumentAsync(Guid id)
            => await DownloadFileAsync($"api/v1/documents/{id}/download",
                $"document-{id:N}.pdf", "application/pdf");
    }
}
