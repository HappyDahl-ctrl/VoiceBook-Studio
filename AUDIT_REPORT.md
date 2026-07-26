# VoiceBook Studio — Full Codebase Audit

**Scope:** Full repository at `/home/user/VoiceBook-Studio`, C# 12 / .NET 8 WPF app (~11,400 lines of C#, ~2,250 lines of XAML).
**Method:** Manual review of manuals + git history, plus three independent deep-read passes over the entire ViewModels/Services/Views tree, cross-checked against each other. Every finding below cites `file:line`. No code was changed at the time this report was first written — it was a read-only audit.

## Update — Tier 1 JAWS/repetition fixes applied

Following this audit, the Tier 1 (accessibility-breaking) findings involving repetitive or competing audio were fixed:

- **§3.0 `LiveAnnounce` infinite recursion** — fixed. The non-JAWS fallback now calls `_audio.Speak(msg)` (`ViewModels/MainViewModel.cs`) instead of calling itself, so the app no longer crashes with a `StackOverflowException` on the first system event in any Dragon-only/JSay-only/no-AT session.
- **§3.6 `SpeakGoodbye` audio leak** — fixed. `SystemAnnouncementService.SpeakSync` now checks `_jawsDetected` like every other method on the class, so the app no longer speaks "Goodbye" over JAWS on close (`Services/SystemAnnouncementService.cs`). This now matches what the User Manual already documented ("plays only when JAWS is not running").
- **§3.6 Azure "Test Voice" audio leak** — fixed. `AzureTtsDialog.xaml.cs`'s `TestButton_Click` now skips audible Azure test playback when JAWS is detected, confirming success via the (Polite live-region) status label instead of competing spoken audio.
- **§3.7 No throttling in `SystemAnnouncementService`** — fixed. `Speak()` now calls `StopSpeaking()` before starting new speech, mirroring `AudioFeedbackService`'s cancel-and-replace policy, so rapid events can no longer queue into a stale audio backlog that plays back after the app has moved on.
- **§3.5 Inconsistent JAWS coverage for system events** — fixed. ~20 call sites that previously announced state changes only through `SystemAnnouncementService.Speak` (silent under JAWS beyond a Polite status-bar update) — chapter added/renamed/deleted, project opened/created/saved, import results, export success, application status, and others — were switched to the same `LiveAnnounce` path already used correctly by sibling events (chapter moved, AI feedback, export failure), so JAWS now gets a `RaiseNotificationEvent` announcement consistently across the whole event set, and non-JAWS users continue to hear the same messages via `AudioFeedbackService` (which itself already cancels-and-replaces, so no new backlog risk).

Everything else in this report (architecture, dead code, manual-vs-code accuracy, remaining accessibility items like the chapter list's missing live region and the three voice-command gaps) is unchanged and still open for review.

## 0. Which documents this audit treats as authoritative

The repo contains **six** documents that all claim to be setup/user instructions, and they actively disagree with each other:

| File | Status |
|---|---|
| `Docs/Installation-and-Configuration-Guide.md` | **Canonical "Configuration/Setup Manual"** — added in commit `d1edfde`, kept current through the latest commits (`3b2e1fb`, `5620aa4`). Used as ground truth below. |
| `Docs/User-Manual.md` | **Canonical "User Manual"** — same provenance as above. Used as ground truth below. |
| `VoiceBook-Studio-User-Guide.md` (root) | **Stale duplicate.** Contradicts the canonical User Manual on core behavior (see §1.7) — no commit ever updated it after the canonical docs were introduced. |
| `Dragon-Commands-Setup-Guide.md` (root) | **Stale duplicate** of Config Guide §7, different phrasing conventions, not touched by the branch/tutorial fixes in later commits. |
| `Installer/VoiceBook_Studio_Setup_Guide.txt` | **Legacy v1.0.0 doc.** References F1/F2/F3 panel keys (replaced by Ctrl+1/2/3 in commit `37d537e`), registry-based settings claims, `My Documents\VoiceBook Projects\` save location, 30-second autosave, and a 75-prompt library — none of which match the current app. |
| `DragonCommands/*.txt` (3 files) | **Legacy v16 "Advanced Scripting" setup docs**, describing an entirely different Dragon integration approach (dot-DAT scripts) than the Type A/B keystroke system actually shipped and documented in the Config Guide. Command vocabulary here (`"ask assistant"`, `"voice off"`, `"prompt A1"`) diverges from both canonical manuals. |

**Finding A-0 (doc sprawl):** Five stale/conflicting documents remain in the repo alongside the two current manuals, with no deprecation notice anywhere. A support person or Kelly's caregiver could easily open `Installer/VoiceBook_Studio_Setup_Guide.txt` or `VoiceBook-Studio-User-Guide.md` and follow instructions that no longer match the app (wrong panel shortcuts, wrong save locations, wrong API-key storage claims). **Recommendation:** delete or clearly mark these five files as superseded.

---

## 1. Manual-vs-Code Accuracy

### 1.1 API key storage — manual claim is factually wrong

Both canonical manuals state the key is stored in `%APPDATA%\VoiceBookStudio\settings.json` (Config Guide lines 88–91; User Manual line 345 says "stored locally"). **Actual:** `Services/ApiKeyService.cs:11-24` stores it in the Windows Registry at `HKCU\SOFTWARE\VoiceBookStudio\AnthropicApiKey`, as a plain string. `Utils/AppSettings.cs`'s `settings.json` only ever persists `defaultProjectFolder`/`firstLaunchComplete` (`AppSettings.cs:155-166`) — never the key. Azure TTS credentials are likewise registry-based (`AppSettings.cs:86-98`), not JSON. A user told to "back up/edit your key in settings.json" per the manual will not find it there. **This is also a security problem** — see §2.4.

### 1.2 Import chapter-detection priority is reversed from what's documented

User-Manual.md:206 reads as "uses Claude (if API key set), or built-in patterns" — implying Claude-first. Actual code (`MainViewModel.cs:1106-1123`) always runs the regex/heading heuristic **first**, and only calls Claude when the heuristic finds fewer than 2 breaks AND a key is set. Both mechanisms are real, but the precedence is inverted from the manual's plain-English reading.

### 1.3 Prompt Library — data matches Config Guide, not User Manual; category K is a dead end for voice

`Data/PromptLibrary/prompts.json`: A–E have 5 prompts each, F–J have 10 each, K has exactly 1 (76 total) — this matches Config Guide §7 Section M exactly, but contradicts User-Manual.md:288/511 ("letter A–K, number one–ten," implying every letter goes to 10). Separately, `Services/SpeechListenerService.cs:272,292` (the app's own built-in speech grammar) only lists letters **a–j** — letter **k** is missing, so prompt K1 is unreachable via the app's built-in microphone (Dragon MyCommands and the command bar still work). Also, `TutorialViewModel.cs:530` tells users the library has "75" prompts; the real count is 76.

### 1.4 Response Card capacity — both manuals disagree with each other, and code enforces neither

Config Guide §7 Section N lists 10 insert commands / 5 delete commands. User-Manual.md:308-309/520-521 says insert goes to "twenty," delete to "five." **Actual code enforces no fixed cap at all** — `ResponseCardViewModel.cs:108-118,154-164` bounds-check only against the live card count; the 1–20 spoken-number grammar (`SpeechListenerService.cs:249-261`) is applied identically to both insert and delete. So "delete card one through five" is simply false — delete supports 1–20 exactly like insert.

### 1.5 ScrollLock LED claim overstates what the app does

Both manuals state the ScrollLock LED "lights when the app mic is on, giving you a physical indicator" as if this is a deliberate feature. In reality (`MainWindow.xaml.cs:132-153,212-216,380-381`, `DragonMicService.cs:41-53`) the app only listens for the ScrollLock keydown and mutes/unmutes Dragon via COM — it never reads or sets the keyboard LED state (confirmed: no `GetKeyboardState`/`SetKeyboardState`/`keybd_event` calls anywhere). The LED lights purely as a Windows OS side-effect of the physical key toggle, coincidentally correlated, not app-driven.

### 1.6 "Book analysis" precondition is stricter in the manual than in code (harmless)

User-Manual.md implies you must select "Whole Book" before running book analysis. `MainViewModel.cs:1633-1686` shows `CanRunBookFeedback()` has no such requirement — it works from any chapter selection and always rebuilds full manuscript content. Not a bug, just an over-strict manual.

### 1.7 Root-level `VoiceBook-Studio-User-Guide.md` directly contradicts the canonical User Manual

Canonical User-Manual.md:36 says JAWS running → **no** app TTS at all, ever. The stale root guide says startup/closing announcements "always play, regardless of whether JAWS is running." These cannot both be true; code (§3, Claim 6) shows the canonical manual is closer to correct but not fully accurate either (see the `SpeakGoodbye` leak in §3).

### 1.8 Two entirely separate tutorial systems exist; only one is real

Both manuals correctly describe a "17-step" guided tour, and `TutorialViewModel.cs` does define exactly 17 steps — confirmed accurate. But `Services/TutorialService.cs` implements a **second, complete, 5-step tutorial** ("the five-step guided tour," per its own header comment) via `MainViewModel.StartGuidedTour()` (`MainViewModel.cs:831-842`), which is **never called from anywhere** in the app. It's fully dead, unreachable code — harmless to users, but a maintenance trap, and stale comments in `VoiceCommandRouter.cs:14,46-47` still reference "the five-step guided tour."

### 1.9 Internal document self-contradiction (not code-related)

Config Guide's own table of contents (line 15) says "201 commands"; its own totals table (lines 560–582) sums to 205 (verified by re-adding all 18 section rows). The document contradicts itself before code is even involved.

### 1.10 `.vbk` vs `.vbsproj` — likely functional bug, not just a doc issue

`ProjectService.cs:22-23` defines the project file extension/filter as `.vbk` (matches the manual, User-Manual.md:194), but `ProjectService.GetRecentProjects` (`ProjectService.cs:99-126`) scans for `*.vbsproj` files instead — a different extension. If real `.vbk` files are what gets saved, the recent-projects list would never find any of them. Worth verifying at runtime.

### 1.11 Working features that exist in code but appear in **no** manual

- `"ask assistant [question]"` — sends a question straight to Claude (`VoiceCommandRouter.cs:428-434` → `MainViewModel.cs:2471-2480`).
- `"open chapter"` / `"show chapter"` / `"select chapter"` (`VoiceCommandRouter.cs:349-353`).
- `"show [category] cards"`, e.g. "show fiction cards" (`VoiceCommandRouter.cs:262-268`).
- Assorted extra synonyms (`"save chapter"`, `"save all"`, `"import from word"`, etc., `VoiceCommandRouter.cs:215-221,283`).

### 1.12 Minor stale source comment (not manual-facing)

`Models/SectionType.cs:4` doc-comment says "14 recognised section types"; the enum actually defines 17, and its own `AllTypes` comment at line 107 correctly says "All 17 types."

---

## 2. Architecture Review

### 2.1 The task's assumed "LeftPanel / MiddleEditor / RightClaudePanel" ViewModels do not exist

Confirmed by both the codebase and `Docs/UIA_Audit_Results.txt:328-330`, a prior internal audit, which explicitly notes `Views/Panels/LeftPanel.xaml`, `MiddleEditor.xaml`, and `RightClaudePanel.xaml` were never built as separate files — "content embedded in MainWindow." There are no corresponding ViewModel classes either. `ViewModels/` contains only: `MainViewModel`, `ChapterViewModel`, `ChapterConfirmationViewModel`, `FeedbackLibraryViewModel`, `ProjectSelectionViewModel`, `PromptLibraryViewModel`, `ResponseCardViewModel`, `TutorialViewModel`, `WelcomeDialogViewModel`, `WholeBookViewModel`.

### 2.2 `MainViewModel.cs` (2,588 lines) is a god object doing the work of at least 3 panel ViewModels

It owns chapter-list state, editor state, AI-assistant state, project lifecycle, import/export orchestration, dialog-launching, and the entire "Try*" facade layer `VoiceCommandRouter` calls into. Rough breakdown: fields/ctor (43-127), observable properties (167-319), library-reading state machine (369-434), panel focus + tutorial delegates (435-494), project load/save (639-1041), import (1046-1264), chapter CRUD + export (1269-1513), "editor sync" — effectively the missing Editor-panel VM's logic (1518-1559), AI feedback (1565-1718), chat/insert/save-card — effectively the missing AI-panel VM's logic (1724-1856), settings/dialogs (1862-1994), internal helpers (2000-2151), and the ~50-method voice-router "Try*" facade (2172-2587).

### 2.3 CommunityToolkit.Mvvm is used, but inconsistently — three different idioms coexist

`CommunityToolkit.Mvvm` 8.4.2 (`VoiceBookStudio.csproj:16`) is genuinely used, not a phantom dependency, but split three ways:
- **Toolkit-based** (`ObservableObject` + `[ObservableProperty]`/`[RelayCommand]` or the toolkit's manual `RelayCommand`): `MainViewModel`, `ChapterViewModel`, `WholeBookViewModel`, `TutorialViewModel`, `WelcomeDialogViewModel`.
- **Hand-rolled** `INotifyPropertyChanged` + `Utils/RelayCommand.cs`: `PromptLibraryViewModel`, `ResponseCardViewModel`, `FeedbackLibraryViewModel` (confirmed by `RaiseCanExecuteChanged()` calls, e.g. `PromptLibraryViewModel.cs:58`, a method that only exists on the hand-rolled class).
- **Neither** — plain classes with no change notification at all: `ChapterConfirmationViewModel.cs:10`, `ProjectSelectionViewModel.cs:12` (both also dead code, see §2.6).

`Utils/RelayCommand.cs` is not removable without touching the 3 hand-rolled ViewModels — it's a genuine, actively-used duplicate of functionality the toolkit already provides.

### 2.4 `ApiKeyService.cs` stores the Anthropic API key in plaintext — contradicts its own "secure" claim

```csharp
// Services/ApiKeyService.cs:11-24
private const string RegPath  = @"SOFTWARE\VoiceBookStudio";
private const string RegValue = "AnthropicApiKey";
...
key.SetValue(RegValue, apiKey ?? string.Empty);   // no encryption
```
`System.Security.Cryptography.ProtectedData` (DPAPI) is never used anywhere in the codebase (repo-wide search confirms zero matches). Any other process running as the same Windows user can read this key directly from the registry. `Views/HelpDialog.xaml.cs:159` explicitly tells the user "Your key is stored securely in the Windows registry" — **this claim is false as written**; registry storage alone is not secure storage. Wrapping the value with `ProtectedData.Protect`/`Unprotect` (`DataProtectionScope.CurrentUser`) would close this gap with minimal code change.

### 2.5 Service layer has no DI container and almost no interfaces

Of 21 services, only `ITutorialPresenter` (`Services/ITutorialPresenter.cs:10`) exists as an interface. Everything else is instantiated with `new` directly in `App.xaml.cs:36-40`, `MainViewModel.cs`, or dialog constructors — none of it is mockable for unit tests. `VoiceCommandRouter` (828 lines) takes a concrete `MainViewModel` in its constructor (`VoiceCommandRouter.cs:12,17-20`) rather than an interface, and its `TryRoute` method (lines 38-746) is one giant if/else chain — a table/dictionary-driven dispatcher would be both more testable and more maintainable.

### 2.6 Duplicate logic across services

`AudioFeedbackService.SanitizeForSpeech`/`SelectBestSapiVoice` (`AudioFeedbackService.cs:350-374,383-404`) is near-identical, separately-implemented logic to `SystemAnnouncementService.cs:213-237,239-260` — the same behavior maintained in two places.

### 2.7 AiService error handling

`AiService.cs:284-321` (`CallClaudeAsync`) throws on any non-2xx response with no retry/backoff for transient 429/5xx failures. Callers in `MainViewModel.cs` generally catch and surface exceptions to the user (lines 1614-1620, 1672-1679, 1757-1763), **except** the chapter-detection call during import (`MainViewModel.cs:1121-1123`), which does `catch { detected = null; }` — silently swallowing all errors (bad key, network failure, malformed response) and falling back to single-chapter import with zero diagnostic to the user.

### 2.8 Dead ViewModels / dialogs

- **`ProjectSelectionViewModel` + `ProjectSelectionDialog`** — never instantiated anywhere (`new ProjectSelectionViewModel` returns zero hits); `MainViewModel.SetProjectSelection` (`MainViewModel.cs:162-165`) is never called; its own command handlers are empty stubs. Appears to be an abandoned project-picker screen superseded by the current `OpenFileDialog` flow.
- **`ChapterConfirmationViewModel`** — never instantiated; `ChapterConfirmationDialog`'s `DataContext` is never set to it (`MainViewModel.cs:1128` creates the dialog with no `DataContext` assignment), so the VM class is pure dead weight — the dialog's real logic lives entirely in its own code-behind.
- **`AiService.SuggestImprovementAsync` / `AiSuggestion`** (`AiService.cs:114-142,446-452`) — a full prompt+API round trip, never called from anywhere.
- **`AiFeedback.IsStub` / `AiSuggestion.IsStub`** (`AiService.cs:443,451`) — declared, never set `true`, vestigial.
- **`AppSound.Success`** (`AppSoundService.cs:49,229-233`) — defined and synthesized, never played.
- **`Services/TutorialService.cs`** — the dead 5-step tutorial from §1.8.

---

## 3. Accessibility Compliance

### 3.0 CRITICAL — `LiveAnnounce()` is unconditionally, infinitely recursive whenever JAWS is not detected

```csharp
// ViewModels/MainViewModel.cs:145-151
private void LiveAnnounce(string msg, bool urgent = false)
{
    if (string.IsNullOrWhiteSpace(msg)) return;
    AnnouncementRequested?.Invoke(msg, urgent);
    if (!Utils.AppSettings.IsJawsDetected)
        LiveAnnounce(msg);   // calls itself — no base case, no depth limit
}
```
The surrounding comment (lines 132-136) says this should "fall through to `AudioFeedbackService` so non-JAWS users still hear the feedback" — but the code calls **itself** instead of the audio service. This is nearly certainly a copy/refactor bug where the intended call target (`_audioFeedback.Speak(msg)` or similar) got replaced with a recursive self-call.

**Impact:** `LiveAnnounce` backs almost every system-event announcement in the app — panel switches, save, chapter add/rename/delete/move, export, AI feedback complete, errors (~35 call sites in `MainViewModel.cs`). The very first one fired in any session where JAWS is not detected — e.g. the first `Ctrl+2` panel switch (`MainViewModel.cs:455`) — triggers unbounded recursion and a `StackOverflowException`, which .NET cannot catch; **the process terminates immediately.**

This means **all three of the app's documented non-JAWS modes are currently unusable**: "Dragon alone," "JSay alone," and "no assistive technology at all" (User-Manual.md:34-37 explicitly documents these as fully supported). It also means a JAWS user whose JAWS instance is slow to start (see §3.6, the 3-retry/6-second detection window) will hit this same crash if detection completes just after the first system event fires. This is the single highest-priority bug in the codebase — it breaks core functionality for every user who isn't running JAWS, including Kelly's Dragon-only workflow if JAWS isn't also running.

### 3.1 AutomationProperties.Name — present everywhere, but not always matching the visible label as the manual claims

Every interactive control checked has *some* `AutomationProperties.Name`, but several don't match the visible `Content`/label text ("match their visible labels exactly" — User-Manual.md:363), e.g.: `MainWindow.xaml:205-211` visible `"Import .docx"` vs Name `"Import Document"`; `:224-244` `"+ Chapter"`/`"↑ Up"`/`"↓ Down"` vs `"Add Chapter"`/`"Move Up"`/`"Move Down"`; `SettingsDialog.xaml:63-70` and `ProjectSelectionDialog.xaml:54-61` `"Browse..."` vs `"Browse"`. Directionally fine for JAWS users (the Name is arguably more descriptive), but the manual's specific wording overstates it.

### 3.2 Chapter list is not actually a live region, despite the manual's explicit claim

User-Manual.md:364 lists "the chapter list" as a UIA live region. `MainWindow.xaml:352-380` (`ChapterListBox`) has `AutomationProperties.Name`/`HelpText` but **no `AutomationProperties.LiveSetting`** anywhere — neither on the ListBox nor its `ItemContainerStyle`. Status bar (`:902-915`) and AI response box (`:593-605`) are correctly `LiveSetting="Polite"`, confirming this is a real gap specific to the chapter list, not a documentation overstatement across the board.

### 3.3 Dialog assertive live regions — confirmed working, and more thorough than documented

All 6 dialogs named in the manual (Welcome, Settings, API Key, Azure TTS, Add Prompt, Project Selection) do fire an assertive live-region announcement on open, verified in both XAML and code-behind. Bonus: 4 more undocumented dialogs (`ChapterConfirmationDialog`, `InputDialog`, `SaveCardDialog`, `SectionTypeDialog`) implement the identical pattern even though the manual doesn't credit them.

### 3.4 Tutorial `RaiseNotificationEvent` on Next/Previous/Repeat — confirmed accurate

`TutorialDialog.xaml.cs:72-84` fires `UiaAnnouncer.Announce(..., isUrgent: true)` (→ `RaiseNotificationEvent`, `Helpers/UiaAnnouncer.cs:29-49`) exactly as the manual describes, gated correctly on `IsJawsDetected`.

### 3.5 Some "system events" bypass the UIA notification system despite being named in the same sentence that claims they don't

User-Manual.md:367 lists "chapter added, chapter moved, save confirmed, AI complete, errors" as a single group all routed through the UIA notification system. Chapter moved / AI complete / most errors do go through `LiveAnnounce` → UIA. But **chapter added** (`MainViewModel.cs:1303-1306`), **chapter renamed** (`:1433-1449`), **chapter deleted** (`:1452-1474`), **save confirmed** (`:1011-1025`), and the "could not open project" error (`:698-703`) call **only** `_systemAnnouncements.Speak(...)` — which is a hard no-op under JAWS (`SystemAnnouncementService.cs:96-101`) — with no `LiveAnnounce`/UIA path at all. Under JAWS, these five events produce **no spoken announcement whatsoever** beyond a Polite status-bar text change, contradicting the manual's blanket claim.

### 3.6 Two confirmed audio leaks that violate "no SAPI voice at all when JAWS is running"

1. **`SpeakGoodbye()`** (`MainViewModel.cs:2575-2576` → `SystemAnnouncementService.SpeakSync`, `SystemAnnouncementService.cs:126-146`) has **no JAWS guard**, unlike every other method on that class. The app audibly says "VoiceBook Studio closing. Goodbye." over SAPI/Azure on every close, even while JAWS is actively speaking — directly contradicting "no overlap...under any circumstances" (User-Manual.md:371).
2. **Azure TTS "Test Voice" button** (`AzureTtsDialog.xaml.cs:51-93`, specifically the Azure branch at line 80) calls `SpeakAndWaitAsync` with no JAWS check anywhere in `AzureTtsService.cs`. The SAPI-fallback branch of the same handler (line 62) correctly routes through the JAWS-gated `AudioFeedbackService`, making this an inconsistency/oversight rather than a deliberate design choice.

By contrast, `AudioFeedbackService` itself is implemented correctly and consistently — every speaking method checks `IsJawsDetected` and no-ops.

### 3.7 No throttling/debounce anywhere; the two speech services handle bursts inconsistently

No `Debounce`/`Throttle`/cooldown/rate-limit code exists anywhere in the repo. `AudioFeedbackService.Speak()` cancels in-flight speech before starting new speech (an interrupt/cutoff policy). `SystemAnnouncementService.Speak()` does **not** cancel prior speech — it queues via SAPI's own `SpeakAsync`, with no coalescing. A burst of system events (e.g., confirming a multi-chapter import, each chapter separately calling `Speak($"Added {label}: {title}")`) will queue up and lag behind real app state, the opposite of the two services behaving consistently.

### 3.8 Voice-command gaps — three features are keyboard/mouse-reachable only, with no Dragon/voice phrase

- **Configure Voice / Azure TTS dialog** (`MainViewModel.cs:1922`) — no phrase in `VoiceCommandRouter.cs`, no keyboard shortcut either. Menu/Tab/mouse only.
- **Reopen Welcome/Tutorial dialog** (`MainViewModel.cs:866`) — no phrase, no shortcut (note: "start tutorial" resumes the *already-running* tutorial VM, it does not open this dialog).
- **Delete Entry in Feedback Library** (`FeedbackLibraryViewModel.cs:97,112`) — no phrase.

Also, User-Manual.md:353 tells users to "click the voice button in the toolbar" for Azure TTS setup — there is no such toolbar button; it's a Settings-menu item only (`MainWindow.xaml:144-147`).

### 3.9 Sounds reference — mostly accurate, two discrepancies

`AppSound.Success` (dead, never played — §2.8). `AppSound.Error` is played for "could not open project" (`MainViewModel.cs:700`) but that event/sound pairing isn't in the manual's Sounds Reference table at all. Separately, `ExportSuccess` and `TutorialComplete` (`AppSoundService.cs:188-194,216-222`) are near-identical 4-note ascending fanfares in the synth code, despite the manual describing them as distinct ("Bell" vs "Fanfare") — in practice they'd sound very similar to Kelly.

---

## 4. Code Quality / Optimization

1. **`MainViewModel.cs` god object** (2,588 lines) — see §2.2.
2. **Inconsistent MVVM idiom** across ViewModels — see §2.3.
3. **No DI container, one interface across 21 services** — see §2.5.
4. **`VoiceCommandRouter.TryRoute`** — 700+ line if/else chain, tightly coupled to the concrete `MainViewModel` class — see §2.5.
5. **Duplicated `SanitizeForSpeech`/`SelectBestSapiVoice`** across `AudioFeedbackService` and `SystemAnnouncementService` — see §2.6.
6. **`Utils/RelayCommand.cs`** duplicates functionality `CommunityToolkit.Mvvm` already ships, actively used by 3 ViewModels — see §2.3.
7. **Dead code:** `ProjectSelectionViewModel`+dialog, `ChapterConfirmationViewModel`, `AiService.SuggestImprovementAsync`/`AiSuggestion`, `AppSound.Success`, entire `TutorialService.cs` 5-step tutorial — see §2.8.
8. **No retry/backoff for transient Anthropic API failures**; import's chapter-detection call silently swallows all exceptions with a bare `catch { }` — see §2.7.
9. **`ProjectService.GetRecentProjects` scans `*.vbsproj`** while the app saves `.vbk` — see §1.10 (verify at runtime; likely means "recent projects" never populates).
10. **`LiveAnnounce` recursion bug** — also a code-quality failure (no base case in a self-recursive method), not just an accessibility bug — see §3.0.

---

## 5. Prioritized Fix List

### Tier 1 — Breaks accessibility for Kelly (fix first)

1. **`LiveAnnounce` infinite recursion crash** — `ViewModels/MainViewModel.cs:145-151`. Crashes the app on the first system event in any session without JAWS detected (Dragon-only, JSay-only, no AT). This is the single most severe bug in the codebase.
2. **`SpeakGoodbye` bypasses JAWS silencing** — `MainViewModel.cs:2575-2576`, `SystemAnnouncementService.cs:126-146`. Audible SAPI/Azure speech collides with JAWS on every app close.
3. **Azure "Test Voice" bypasses JAWS silencing** — `AzureTtsDialog.xaml.cs:51-93`.
4. **Chapter list is not a live region** — `MainWindow.xaml:352-380`. JAWS will not auto-announce chapter list changes as the manual promises.
5. **`SystemAnnouncementService` has no cancel/throttle** vs. `AudioFeedbackService`'s cancel-and-replace — inconsistent behavior risks queued audio backlog spamming JAWS/Dragon during bursts (e.g., multi-chapter import confirmations).
6. **Chapter added/renamed/deleted and save-confirmed produce no JAWS announcement at all** (only a Polite status-bar text change) — `MainViewModel.cs:1303-1306,1433-1449,1452-1474,1011-1025`.

### Tier 2 — Breaks core functionality

1. **API key stored in plaintext registry, contradicting the app's own "stored securely" claim** — `Services/ApiKeyService.cs:11-24`, `Views/HelpDialog.xaml.cs:159`. Real security gap, not just a doc issue.
2. **`.vbk` vs `.vbsproj` mismatch** in `ProjectService.cs:22-23,99-126` — recent-projects list likely never populates. Needs runtime verification.
3. **Prompt category K unreachable via the app's built-in microphone** — `SpeechListenerService.cs:272,292` grammar omits letter "k".
4. **Three keyboard/mouse-only features with no voice/Dragon path**: Configure Voice dialog, reopening Welcome/Tutorial, Delete Feedback Entry — §3.8.
5. **Two dead dialog/ViewModel pairs** shipped in a broken, unreachable state (`ProjectSelectionViewModel`+dialog, `ChapterConfirmationViewModel`) — low runtime risk since unreachable, but confusing if anyone tries to wire them up later.

### Tier 3 — Diverges from the manuals (documentation accuracy)

1. API key storage path claim is simply wrong (registry, not `settings.json`) — §1.1.
2. Import chapter-detection precedence is reversed from the documented behavior — §1.2.
3. Response Card capacity: the two canonical manuals disagree with each other, and neither matches the actual no-cap behavior — §1.4.
4. ScrollLock LED description overstates what the app does (OS side-effect, not app-driven) — §1.5.
5. Five stale/conflicting legacy documents remain in the repo with no deprecation notice (§0) — real risk of someone following outdated instructions (wrong panel shortcuts, wrong save paths, wrong command vocabulary).
6. Config Guide self-contradicts on total command count (201 vs. 205) — §1.9.
7. Working, undocumented voice commands: `"ask assistant"`, `"open chapter"`, `"show [category] cards"`, and several synonyms — §1.11.
8. Minor: 75 vs. 76 prompts (tutorial narration vs. actual data), `SectionType.cs` stale "14" comment, "Book analysis" precondition documented stricter than code enforces.

### Tier 4 — Code quality / nice-to-have

1. Split `MainViewModel` into panel-scoped ViewModels (Chapters, Editor, AI Assistant) — it currently does the job the task assumed three separate VMs already did.
2. Standardize on one MVVM idiom (recommend: fully adopt CommunityToolkit.Mvvm's source generators everywhere, retire `Utils/RelayCommand.cs`).
3. Introduce interfaces + a lightweight DI setup for services beyond `ITutorialPresenter`, especially `VoiceCommandRouter`'s dependency on the concrete `MainViewModel`.
4. Convert `VoiceCommandRouter.TryRoute`'s if/else chain to a table-driven dispatcher.
5. Deduplicate `SanitizeForSpeech`/`SelectBestSapiVoice` between the two announcement services.
6. Remove dead code: `AiService.SuggestImprovementAsync`/`AiSuggestion`/`IsStub` fields, `AppSound.Success`, the entire unused 5-step `TutorialService.cs`, and its stale references in `VoiceCommandRouter.cs:14,46-47`.
7. Add retry/backoff for transient Anthropic API errors; stop silently swallowing the chapter-detection exception during import.
8. Reconcile `AutomationProperties.Name` values with visible button labels where they diverge (§3.1) — cosmetic but worth tidying given the manual's explicit "match exactly" claim.

---

*This report changes no code. All line numbers reflect the repository state at the time of this audit; re-verify before acting if the branch has moved on.*
