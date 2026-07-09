# API Documentation — Transcript Analysis Service

This document describes how to use the Transcript Analysis API. It is intended
for anyone integrating with the service (e.g. the frontend team) — no knowledge
of the internal implementation is required.

## Overview

The API accepts an already-transcribed phone conversation (English or
Armenian), splits it into speaker turns (Agent / Caller), and extracts
personally identifiable information (PII) using Azure AI Language Service.

Base technology: ASP.NET Core Web API (.NET 8). Interactive documentation is
available at `/swagger` when the service runs in Development mode.

---

## Endpoint

```
POST /api/transcript/analyze
Content-Type: application/json
```

### Request body

| Field | Type | Required | Rules |
|---|---|---|---|
| `transcriptText` | string | yes | Not empty/whitespace. Maximum **50,000 characters**. |
| `language` | string | yes | `"en"` (English) or `"hy"` (Armenian). Case-insensitive. |

```json
{
  "transcriptText": "Agent: Hello, how can I help you?\nCaller: My name is John Smith, my phone is 555-123-4567.",
  "language": "en"
}
```

Transcript format notes:

- One utterance per line (separated by `\n`).
- Speaker labels (`Agent:`, `Caller:`, `Operator:`, `Customer:`, `Client:`,
  `Speaker 1:`, `Speaker 2:`, Armenian `Օպերատոր:`, `Հաճախորդ:`) are used when
  present.
- If no labels are present, the first line is treated as the Agent and roles
  alternate from there.

### Successful response — `200 OK`

```json
{
  "conversation": [
    { "role": "Agent",  "text": "Hello, how can I help you?" },
    { "role": "Caller", "text": "My name is John Smith, my phone is 555-123-4567." }
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

| Field | Meaning |
|---|---|
| `conversation[].role` | `"Agent"` / `"Caller"`, or `"Speaker 1"` / `"Speaker 2"` when roles cannot be determined. |
| `conversation[].text` | The utterance with the speaker label removed. |
| `extractedAttributes.name` | Person's name, or `null` if not mentioned. |
| `extractedAttributes.address` | Postal address, or `null`. |
| `extractedAttributes.socialSecurityNumber` | US SSN (XXX-XX-XXXX), or `null`. |
| `extractedAttributes.phoneNumber` | Phone number, or `null`. |
| `extractedAttributes.email` | Email address, or `null`. |

**`null` always means "not mentioned in the transcript"** — the API never
returns empty strings or placeholder values for missing attributes.

### Error responses

The body of an error response is a plain-text explanation.

| Status | When | Example message |
|---|---|---|
| `400 Bad Request` | `transcriptText` empty or whitespace | `Transcript text must not be null, empty, or whitespace.` |
| `400 Bad Request` | `transcriptText` longer than 50,000 chars | `Transcript text length exceeds the limit of 50,000 characters.` |
| `400 Bad Request` | `language` is not `en`/`hy` | `Unsupported language. Language must be either 'en' (English) or 'hy' (Armenian).` |
| `401 Unauthorized` | The configured Azure key is invalid | `Unauthorized: Invalid configuration key.` |
| `503 Service Unavailable` | Azure is down or unreachable | `Service Unavailable: Azure service is currently unreachable.` |
| `500 Internal Server Error` | Any unexpected server error | `Internal Server Error: An unexpected error occurred.` |

Error responses never contain Azure keys, endpoints, or internal stack traces.

---

## Example error request/response

Request:

```json
{ "transcriptText": "Bonjour", "language": "fr" }
```

Response: `400 Bad Request`

```
Unsupported language. Language must be either 'en' (English) or 'hy' (Armenian).
```

---

## Full Armenian example

Request:

```json
{
  "transcriptText": "Օպերատոր: Բարև ձեզ, ինչպե՞ս կարող եմ օգնել:\nՀաճախորդ: Բարև, իմ անունը Արամ Պետրոսյան է, հեռախոսս՝ 093-123456:",
  "language": "hy"
}
```

Response: `200 OK`

```json
{
  "conversation": [
    { "role": "Agent",  "text": "Բարև ձեզ, ինչպե՞ս կարող եմ օգնել:" },
    { "role": "Caller", "text": "Բարև, իմ անունը Արամ Պետրոսյան է, հեռախոսս՝ 093-123456:" }
  ],
  "extractedAttributes": {
    "name": "Արամ Պետրոսյան",
    "address": null,
    "socialSecurityNumber": null,
    "phoneNumber": "093-123456",
    "email": null
  }
}
```

(Verified against live Azure — see `TestResults.md`.)

---

## Behavior details & limitations

- **Long transcripts:** Azure's synchronous PII API accepts at most 5,120
  characters per document. The service transparently splits longer transcripts
  into ≤5,000-character chunks (cut at line breaks) and merges the results, so
  the full 50,000-character request limit is usable. Verified live with a
  15,000-character transcript.
- **SSN classification:** Azure labels an SSN spoken alone in a reply line
  (e.g. "It is 123-45-6789") as a `PhoneNumber`. The service re-routes values
  matching the SSN pattern `XXX-XX-XXXX` to `socialSecurityNumber`.
- **Multiple mentions:** if the same attribute type appears multiple times,
  the highest-confidence detection wins. Detections with confidence below 0.5
  are ignored.
- **CORS:** any origin is currently allowed (development policy) so a browser
  frontend can call the API directly. Restrict before production.
- **Armenian support:** verified working for names, phone numbers, and emails.
  Armenian address/SSN detection quality is an open question — see
  `TestResults.md`.
- **Cost:** each 1,000 characters sent to Azure = 1 billed "text record"
  (free tier: 5,000 records/month).
