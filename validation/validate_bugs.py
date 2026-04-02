#!/usr/bin/env python3
"""validate_bugs.py — validates all 13 bug fixes. Run from repo root."""
import os, re, sys

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC  = os.path.join(BASE, "src")
PASS = "\033[92m✓\033[0m"
FAIL = "\033[91m✗\033[0m"
errors = []

def read(rel):
    p = os.path.join(SRC, rel)
    return open(p, encoding="utf-8").read() if os.path.exists(p) else ""

def check(ok, label, detail=""):
    if ok: print(f"  {PASS} {label}")
    else:
        print(f"  {FAIL} {label}" + (f"\n       ↳ {detail}" if detail else ""))
        errors.append(label)

FILES = {
    "users":          "LifeCrm.Web/Pages/Admin/UsersPage.razor",
    "contacts":       "LifeCrm.Web/Pages/Contacts/ContactsPage.razor",
    "donation_form":  "LifeCrm.Web/Components/Dialogs/DonationFormDialog.razor",
    "account":        "LifeCrm.Web/Pages/Account/AccountPage.razor",
    "layout":         "LifeCrm.Web/Components/Layout/MainLayout.razor",
    "login":          "LifeCrm.Web/Pages/Auth/LoginPage.razor",
    "campaign_detail":"LifeCrm.Web/Pages/Campaigns/CampaignDetailPage.razor",
    "dashboard":      "LifeCrm.Web/Pages/Dashboard/DashboardPage.razor",
    "mud_color":      "LifeCrm.Web/Extensions/MudColorExtensions.cs",
    "signalr":        "LifeCrm.Web/Services/SignalRService.cs",
    "app_state":      "LifeCrm.Web/Services/AppState.cs",
    "api_client":     "LifeCrm.Web/Services/ApiClient.cs",
    "imports":        "LifeCrm.Web/_Imports.razor",
    "ic":             "LifeCrm.Application/Interactions/Commands/InteractionCommands.cs",
    "cc":             "LifeCrm.Application/Contacts/Commands/ContactCommands.cs",
    "dq":             "LifeCrm.Application/Donations/Queries/DonationQueries.cs",
    "hub":            "LifeCrm.Api/Hubs/ActivityHub.cs",
    "prog":           "LifeCrm.Api/Program.cs",
    "roles":          "LifeCrm.Core/Constants/Roles.cs",
}
C = {k: read(v) for k, v in FILES.items()}

print("\n=== Bug 1: UsersPage — no bare return; in markup ===")
code_start = C["users"].find("@code")
markup     = C["users"][:code_start] if code_start != -1 else C["users"]
check(not re.search(r'^\s*return;\s*$', markup, re.MULTILINE), "No bare return; in markup")
check("AppState.IsAdmin" in markup, "IsAdmin guard in markup")
check("else" in markup, "Guard uses if/else block")

print("\n=== Bug 2: UsersPage — no unused Nav inject ===")
check("NavigationManager Nav" not in C["users"], "Nav not injected")

print("\n=== Bug 3: ContactsPage — ValueChanged method ref ===")
check(not re.search(r'ValueChanged.*=>.*OnSearchChanged', C["contacts"]), "No void lambda wrapping async handler")
check('ValueChanged="OnSearchChanged"' in C["contacts"], "ValueChanged uses direct method reference")

print("\n=== Bug 4: DonationFormDialog — no invalid ContactId ===")
check("Guid.Empty" not in C["donation_form"], "No Guid.Empty (was failing NotEmpty() validation)")
check("_originalContactId" in C["donation_form"], "_originalContactId field present")
check("_originalContactId = d.ContactId" in C["donation_form"], "_originalContactId populated from loaded donation")
check("ContactId  = _originalContactId" in C["donation_form"] or "ContactId = _originalContactId" in C["donation_form"], "_originalContactId used in UpdateDonationRequest")

print("\n=== Bug 5: DonationFormDialog — try/finally ===")
check("finally" in C["donation_form"], "_saving reset in finally block")

print("\n=== Bug 6: AccountPage — try/finally ===")
check("finally" in C["account"], "ChangePasswordAsync uses try/finally")

print("\n=== Bug 7: MainLayout — no unused Snackbar ===")
has_snackbar = "@inject ISnackbar Snackbar" in C["layout"]
if has_snackbar: check("Snackbar." in C["layout"], "Snackbar injected and used")
else: check(True, "Snackbar not injected (correct)")

print("\n=== Bug 8: LoginPage — no unused Snackbar ===")
check("@inject ISnackbar Snackbar" not in C["login"], "Snackbar not injected in LoginPage")

print("\n=== Bug 9: CampaignDetailPage — no unused Snackbar ===")
check("@inject ISnackbar Snackbar" not in C["campaign_detail"], "Snackbar not injected in CampaignDetailPage")

print("\n=== Bug 10: DashboardPage — no unused AppState ===")
check("@inject AppState AppState" not in C["dashboard"], "AppState not injected in DashboardPage")

print("\n=== Bug 11: MudColorExtensions — Planning case ===")
for case in ["Planning", "Active", "Completed", "OnHold", "Archived"]:
    check(f"ProjectStatus.{case}" in C["mud_color"], f"ProjectStatus.{case} handled")

print("\n=== Bug 12: SignalRService — null-safe Reconnected closure ===")
check("var hub = _hub;" in C["signalr"], "Reconnected captures hub in local var")
check("if (hub is null) return;" in C["signalr"], "Null guard on captured hub")

print("\n=== Bug 13: No duplicate ApiResponse definition ===")
web_resp_path = os.path.join(SRC, "LifeCrm.Web/Services/ApiResponse.cs")
check(not os.path.exists(web_resp_path), "Services/ApiResponse.cs does not exist (was duplicate)")

print("\n=== Structural integrity ===")
for ns in ["LifeCrm.Web.Extensions", "LifeCrm.Web.Components.Dialogs", "LifeCrm.Application.Users.DTOs", "LifeCrm.Core.Enums"]:
    check(ns in C["imports"], f"_Imports has {ns}")
check("UpdateInteractionAsync" in C["api_client"], "ApiClient.UpdateInteractionAsync")
check("DownloadLatestReceiptAsync" in C["api_client"], "ApiClient.DownloadLatestReceiptAsync")
check("ActivateUserAsync" in C["api_client"], "ApiClient.ActivateUserAsync")
check("fromDate" in C["api_client"] and "toDate" in C["api_client"], "GetDonationsAsync date filters")
check("ReceiptDocumentId" in C["dq"], "DonationQueries populates ReceiptDocumentId")
check("UpdateInteractionCommand" in C["ic"], "UpdateInteractionCommand defined")
check("JoinOrganization" in C["hub"], "ActivityHub.JoinOrganization present")
check("WithOrigins(allowedOrigins)" in C["prog"], "CORS uses config-driven origins")
check("IActivityNotifier, ActivityNotifier" in C["prog"] or ("IActivityNotifier" in C["prog"] and "ActivityNotifier" in C["prog"]), "IActivityNotifier registered")
check("RequireRateLimiting" in C["prog"], "Rate limiter applied globally")
check('const string Admin' in C["roles"], "Roles.Admin constant")
check("ContactCreatedNotification" in C["cc"], "CreateContactHandler publishes notification")
check("ErrorCount = importErrors.Count" in C["cc"].replace("  ", " "), "ImportContactsResult.ErrorCount fixed")
check("UserId" in C["app_state"], "AppState.UserId property")
check("Roles.Admin" in C["app_state"], "AppState uses Roles constants")
check("ConnectAsync(string baseUrl, string jwtToken, string orgId)" in C["signalr"], "SignalRService.ConnectAsync takes orgId")
check("PresetContactId" in read("LifeCrm.Web/Pages/Contacts/ContactDetailPage.razor"), "ContactDetailPage uses PresetContactId")

print("\n" + "="*55)
print(f"  Errors: {len(errors)}")
if errors:
    print(f"\n\033[91m✗ FAILED — {len(errors)} issue(s):\033[0m")
    for e in errors: print(f"    • {e}")
    sys.exit(1)
else:
    print(f"\n\033[92m✓ ALL CHECKS PASSED\033[0m")
    sys.exit(0)
