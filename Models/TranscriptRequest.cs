namespace Task_2_TranscriptAnalysis.Models;

/// <summary>
/// The JSON body that clients send to POST /api/transcript/analyze.
///
/// Example request body:
/// {
///   "transcriptText": "Agent: Hello, how can I help you? Caller: My name is John Smith...",
///   "language": "en"
/// }
/// </summary>
public class TranscriptRequest
{
    /// <summary>
    /// The full call transcript as plain text.
    /// Validation rules (implemented by Member 4 in TranscriptController):
    /// - Must not be null or empty.
    /// - Maximum length: 50,000 characters.
    /// </summary>
    public string TranscriptText { get; set; } = string.Empty;

    /// <summary>
    /// Language of the transcript.
    /// Supported values: "en" (English) and "hy" (Armenian).
    /// </summary>
    public string Language { get; set; } = string.Empty;
}
