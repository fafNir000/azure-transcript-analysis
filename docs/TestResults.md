# Test Results — Transcript Analysis Service

**Date:** 2026-07-09 (live tests run 2026-07-08 and 2026-07-09)
**Azure resource:** `transcript-language-service` (Language, Free F0 tier, East US)
**Model version reported by Azure:** `2025-11-01`

This document records what was actually tested and what actually happened.
Two kinds of testing were performed:

1. **Automated integration tests** (`Tests/TranscriptAnalysisTests.cs`, run
   with `dotnet test`) — Azure is replaced with a fake, so these verify OUR
   code deterministically: validation, role splitting, attribute mapping,
   chunking.
2. **Live manual tests** against real Azure — these verify the real end-to-end
   behavior including Azure's actual PII detection.

---

## 1. Automated test suite — 11/11 passing

| # | Test | Verifies | Result |
|---|---|---|---|
| 1 | `Analyze_EnglishTranscript_ReturnsExtractedAttributes` | All 5 attributes mapped from entities (TEST 1, 3) | ✅ PASS |
| 2 | `Analyze_ArmenianTranscript_ReturnsExtractedAttributes` | Armenian text + Armenian labels → Agent/Caller (TEST 2) | ✅ PASS |
| 3 | `Analyze_TranscriptWithoutPii_ReturnsNullAttributes` | Missing attributes stay `null` (TEST 4) | ✅ PASS |
| 4 | `Analyze_EmptyTranscript_ReturnsBadRequest` | Empty input → 400 (TEST 5) | ✅ PASS |
| 5 | `Analyze_UnsupportedLanguage_ReturnsBadRequest` | `"fr"` → 400 (TEST 6) | ✅ PASS |
| 6 | `Analyze_TextOver50000Chars_ReturnsBadRequest` | 50,001 chars → 400 (boundary) | ✅ PASS |
| 7 | `Analyze_LabeledConversation_UsesLabels` | Explicit labels win, labels stripped from text (TEST 7) | ✅ PASS |
| 8 | `Analyze_UnlabeledConversation_AlternatesRoles` | No labels → Agent first, then alternate (TEST 8) | ✅ PASS |
| 9 | `Analyze_SsnClassifiedAsPhoneByAzure_IsReclassified` | XXX-XX-XXXX "phone" rerouted to SSN field | ✅ PASS |
| 10 | `SplitIntoChunks_LongText_RespectsLimitAndPreservesContent` | Chunking ≤5,000 chars/chunk, no content loss | ✅ PASS |
| 11 | `Analyze_MixedLabelConversation_UnlabeledLineContinuesPreviousSpeaker` | Unlabeled line continues previous speaker, not `Speaker 1` | ✅ PASS |

Command: `dotnet test` → `Passed! - Failed: 0, Passed: 11, Skipped: 0, Total: 11`

---

## 2. Live tests against real Azure

### Test A — English transcript, all five attributes (PASS)

Input: 9-line Agent/Caller dialogue containing a name, address, phone, email,
and an SSN spoken alone in a reply line ("It is 123-45-6789.").

Result: `200 OK`, conversation split into 9 correctly-labeled turns, and:

```json
{
  "name": "John Smith",
  "address": "123 Main Street, Springfield",
  "socialSecurityNumber": "123-45-6789",
  "phoneNumber": "555-123-4567",
  "email": "john.smith@example.com"
}
```

Note: on the FIRST run (2026-07-08, before the SSN fix) `socialSecurityNumber`
was `null` — see Finding 1 below. After the fix, all five extract correctly.

### Test B — Armenian transcript (PASS)

Input: 5-line dialogue with Armenian labels (`Օպերատոր:` / `Հաճախորդ:`)
containing a name, phone, and email.

Result: `200 OK`. Labels correctly mapped to Agent/Caller. Extracted:
name `Արամ Պետրոսյան`, phone `093-123456`, email `aram.petrosyan@example.com`.
Address/SSN correctly `null` (not present in the text).

### Test C — validation (PASS)

- Empty `transcriptText` → `400`, message "Transcript text must not be null, empty, or whitespace."
- `language: "fr"` → `400`, message "Unsupported language. Language must be either 'en' (English) or 'hy' (Armenian)."

### Test D — long transcript / chunking (PASS)

Input: 15,023-character transcript (244 lines), with the name at the START and
the phone + email at the END — chosen so the result proves every chunk was
analyzed.

Result: `200 OK` in **1.7 seconds**. 244 conversation turns. Extracted the
name (from chunk 1) and phone + email (from chunk 4). Attributes not present
in the text were `null`.

---

## 3. Findings, limitations, and open questions

### Finding 1 — Azure classifies a "bare" SSN as PhoneNumber (FIXED)

Verified by querying Azure's REST API directly:

- `"My social security number is 123-45-6789"` (context words in the same
  sentence) → category `USSocialSecurityNumber`, confidence 0.85 ✅
- Agent asks for the SSN, caller replies `"It is 123-45-6789"` (number alone
  in its line) → category `PhoneNumber`, confidence 1.0 ❌

Since real transcripts almost always have the question and answer on separate
lines, this affected us directly. **Fix applied** in
`TranscriptAnalysisService`: any `PhoneNumber` entity matching `XXX-XX-XXXX`
(a grouping US phone numbers never use) is routed to the SSN field. Covered by
automated test #9 and re-verified live (Test A).

### Finding 2 — transient Azure timeout (FIXED)

During live testing, one batch request (4 documents) hung for over 2 minutes
with no response; the identical request succeeded in ~1.4 s minutes later.
Interpretation: a transient service/network issue, possibly free-tier related.
**Fix applied:** `AzureLanguageService` now configures `TextAnalyticsClientOptions.Retry`
with a 20s per-attempt `NetworkTimeout`, 2 retries, and exponential backoff.
A hung call now fails within ~60s worst case instead of hanging indefinitely,
surfacing as `RequestFailedException(Status 0)` which the controller already
maps to `503 Service Unavailable`.

### Finding 3 — Azure's per-document limit vs our API limit (FIXED)

Azure's synchronous PII API accepts max 5,120 characters per document and
5 documents per request
([official limits](https://learn.microsoft.com/azure/ai-services/language-service/concepts/data-limits)),
while our API accepts 50,000 characters. **Fix applied:** `AzureLanguageService`
splits long text into ≤5,000-character chunks at line boundaries and batches
up to 5 chunks per request. Verified by automated test #10 and live Test D.

### Open questions

1. **Armenian address / SSN detection quality** — not yet tested with real
   Armenian addresses or ID numbers; needs native-speaker sample transcripts.
2. **"Other important information"** (task spec) — the system currently
   extracts exactly 5 attribute types. Azure also returns Organization,
   DateTime, and other categories that could be added if the company wants
   them.
3. **Mixed-label transcripts (FIXED)** — if only SOME lines have labels,
   unlabeled lines now continue the previous speaker's role instead of being
   marked `Speaker 1`. Covered by automated test
   `Analyze_MixedLabelConversation_UnlabeledLineContinuesPreviousSpeaker`.
4. **Armenian punctuation** — the Armenian full stop `։` (U+0589) looks like
   the Latin colon `:` but is a different character; a transcript using it as
   the label separator (`Օպերատոր։`) would not be recognized as labeled.
