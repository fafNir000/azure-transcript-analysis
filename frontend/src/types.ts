/**
 * Shared TypeScript types.
 * The request/response shapes MUST match the backend models
 * (see /Models in the backend and docs/ApiDocumentation.md).
 */

export type Language = 'en' | 'hy';

/** Body of POST /api/transcript/analyze */
export interface AnalyzeRequest {
  transcriptText: string;
  language: Language;
}

/** One utterance of the conversation, labeled with who said it. */
export interface ConversationTurn {
  role: string; // "Agent" | "Caller" | "Speaker 1" | "Speaker 2"
  text: string;
}

/** PII attributes; null = not mentioned in the transcript. */
export interface ExtractedAttributes {
  name: string | null;
  address: string | null;
  socialSecurityNumber: string | null;
  phoneNumber: string | null;
  email: string | null;
}

/** Successful response of POST /api/transcript/analyze */
export interface AnalyzeResponse {
  conversation: ConversationTurn[];
  extractedAttributes: ExtractedAttributes;
}

/** One saved analysis in the browser's localStorage history. */
export interface HistoryItem {
  id: string;
  createdAt: string; // ISO date string
  request: AnalyzeRequest;
  response: AnalyzeResponse;
}
