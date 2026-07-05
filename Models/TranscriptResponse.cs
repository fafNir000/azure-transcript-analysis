namespace Task_2_TranscriptAnalysis.Models;

/// <summary>
/// The JSON body returned by POST /api/transcript/analyze.
///
/// Example response body:
/// {
///   "conversation": [
///     { "role": "Agent",  "text": "Hello, how can I help you?" },
///     { "role": "Caller", "text": "My name is John Smith." }
///   ],
///   "extractedAttributes": {
///     "name": "John Smith",
///     "address": null,
///     "socialSecurityNumber": null,
///     "phoneNumber": null,
///     "email": null
///   }
/// }
/// </summary>
public class TranscriptResponse
{
    /// <summary>
    /// The transcript split into turns, each labeled with a speaker role
    /// (filled in by SpeakerRoleService — Member 3's work).
    /// </summary>
    public List<ConversationTurn> Conversation { get; set; } = new();

    /// <summary>
    /// PII attributes found in the transcript
    /// (filled in by TranscriptAnalysisService — Member 2's work).
    /// </summary>
    public ExtractedAttributes ExtractedAttributes { get; set; } = new();
}
