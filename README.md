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
| `Services/AzureLanguageService.cs` | **Member 1** | ✅ Done | Connects to Azure AI Language Service; `AnalyzeText(text, language)` runs PII detection. All Azure access goes through this class. |
| `Services/TranscriptAnalysisService.cs` | **Member 2** | 🔲 TODO | Extraction logic: map Azure PII entities (Person, Address, USSocialSecurityNumber, PhoneNumber, Email) into the `ExtractedAttributes` model. |
| `Services/SpeakerRoleService.cs` | **Member 3** | 🔲 TODO | Speaker role detection: split the conversation into Agent/Caller turns; fall back to Speaker 1/Speaker 2; support English and Armenian. |
| `Controllers/TranscriptController.cs` | **Member 4** | 🔲 TODO | `POST /api/transcript/analyze` endpoint: input validation (empty text, language must be `en`/`hy`, max 50,000 chars) and error handling (Azure down, wrong key, invalid input). |
| `Tests/TranscriptAnalysisTests.cs` | **Member 5** | 🔲 TODO | Integration tests: English transcript, Armenian transcript, missing attributes (nulls), empty-input validation, wrong-language validation. |
| `Models/` | shared | ✅ Done | `TranscriptRequest`, `TranscriptResponse`, `ExtractedAttributes`, `ConversationTurn`. Coordinate with the team before changing these. |
| `Program.cs` | shared | ✅ Done | App startup; registers all services for dependency injection. |
| `appsettings.json` | shared | ⚠️ | Azure endpoint + key. **Placeholder values — put your real values in locally, never commit real keys.** |
| `docs/` | shared | — | Team documentation. |

Each TODO file contains detailed step-by-step hints in its class comment — start there.

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

   All tests are currently skipped placeholders — Member 5 enables them as the
   endpoint gets implemented.

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
