# Task_2_TranscriptAnalysis

An ASP.NET Core Web API (**.NET 8**, C#) for a **Transcript Analysis** system.
It takes a call-center transcript (English `en` or Armenian `hy`), splits it into
speaker turns (Agent / Caller), and uses **Azure AI Language Service** to detect
PII (personally identifiable information): name, address, US Social Security
Number, phone number, and email.

## How it works (request flow)

```
Client
  │  POST /api/transcript/analyze  { transcriptText, language }
  ▼
TranscriptController (validation + error handling)
  ├──► SpeakerRoleService        → splits text into Agent/Caller turns
  └──► TranscriptAnalysisService → extracts PII attributes
            │
            └──► AzureLanguageService → calls Azure AI Language (PII detection)
  ▼
TranscriptResponse  { conversation, extractedAttributes }
```

## Project structure & team responsibilities

| Path | Owner | Status | Description |
|---|---|---|---|
| `Services/AzureLanguageService.cs` | **Member 1** | ✅ Done | Connects to Azure AI Language Service; `AnalyzeText(text, language)` runs PII detection. Splits long texts into ≤5,000-char chunks (Azure's sync limit is 5,120/document). All Azure access goes through this class. |
| `Services/TranscriptAnalysisService.cs` | **Member 2** | ✅ Done | Extraction logic: maps Azure PII entities (Person, Address, USSocialSecurityNumber, PhoneNumber, Email) into the `ExtractedAttributes` model; reroutes SSN-pattern "phone numbers" to the SSN field. |
| `Services/SpeakerRoleService.cs` | **Member 3** | ✅ Done | Speaker role detection: splits the conversation into Agent/Caller turns (labels or alternating), falls back to Speaker 1/Speaker 2; English and Armenian labels. |
| `Controllers/TranscriptController.cs` | **Member 4** | ✅ Done | `POST /api/transcript/analyze` endpoint: input validation (empty text, language must be `en`/`hy`, max 50,000 chars) and error handling (400/401/503/500). |
| `Tests/TranscriptAnalysisTests.cs` | **Member 5** | ✅ Done | 10 passing integration tests (Azure faked for determinism): attributes, Armenian, nulls, validation, roles, SSN reclassification, chunking. |
| `Models/` | shared | ✅ Done | `TranscriptRequest`, `TranscriptResponse`, `ExtractedAttributes`, `ConversationTurn`. Coordinate with the team before changing these. |
| `Program.cs` | shared | ✅ Done | App startup; DI registrations + CORS policy for the future frontend. |
| `appsettings.json` | shared | ⚠️ | Azure endpoint + key. **Placeholder values — real values go in user-secrets, never in git.** |
| `docs/ApiDocumentation.md` | shared | ✅ Done | Full API reference for integrators (frontend team). |
| `docs/TestResults.md` | shared | ✅ Done | Real automated + live-Azure test results, findings, and open questions. |

## Getting started

Prerequisites: [.NET 8 SDK (or newer)](https://dotnet.microsoft.com/download) and an
Azure **Language** resource (Azure Portal → Create resource → "Language service").

1. **Configure Azure credentials.** Open `appsettings.json` and replace the
   placeholders with your resource's endpoint and key
   (Azure Portal → your Language resource → *Keys and Endpoint*).
   Safer alternative that keeps keys out of git:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "AzureLanguageEndpoint" "https://<your-resource>.cognitiveservices.azure.com/"
   dotnet user-secrets set "AzureLanguageKey" "<your-key>"
   ```

2. **Run the API:**

   ```bash
   dotnet run
   ```

   Then open the Swagger UI at `https://localhost:<port>/swagger`
   (the port is printed in the console) to try the endpoint in the browser.

3. **Run the tests:**

   ```bash
   dotnet test
   ```

   All 10 integration tests run without an Azure key (Azure is replaced with a
   fake inside the tests), so `dotnet test` works on any machine.

## API contract

`POST /api/transcript/analyze`

Request body:

```json
{
  "transcriptText": "Agent: Hello, how can I help you?\nCaller: Hi, my name is John Smith, my number is 555-123-4567.",
  "language": "en"
}
```

Successful response (`200 OK`):

```json
{
  "conversation": [
    { "role": "Agent",  "text": "Hello, how can I help you?" },
    { "role": "Caller", "text": "Hi, my name is John Smith, my number is 555-123-4567." }
  ],
  "extractedAttributes": {
    "name": "John Smith",
    "address": null,
    "socialSecurityNumber": null,
    "phoneNumber": "555-123-4567",
    "email": null
  }
}
```

Error responses: `400` invalid input (empty text, bad language, text too long),
`401` wrong Azure key, `503` Azure down/unreachable, `500` unexpected errors.
Attributes not found in the transcript are returned as `null`.

## Team conventions

- Depend on **interfaces** (`IAzureLanguageService`, `ITranscriptAnalysisService`,
  `ISpeakerRoleService`), not concrete classes — this is what makes the code testable.
- Only `AzureLanguageService` may talk to Azure directly.
- Keep the placeholder behavior compiling: the project must always build (`dotnet build`).
- Never commit real Azure keys.
