using Task_2_TranscriptAnalysis.Models;

namespace Task_2_TranscriptAnalysis.Services;

/// <summary>
/// Interface for the speaker role detection service (implemented by Member 3).
/// The controller (Member 4) will call this through the interface.
/// </summary>
public interface ISpeakerRoleService
{
    /// <summary>
    /// Splits a raw transcript into conversation turns and assigns a role
    /// ("Agent" / "Caller", or "Speaker 1" / "Speaker 2" as a fallback) to each turn.
    /// </summary>
    /// <param name="transcriptText">The full transcript text.</param>
    /// <param name="language">"en" or "hy".</param>
    List<ConversationTurn> SplitConversation(string transcriptText, string language);
}

/// <summary>
/// OWNER: Member 3
///
/// TODO (Member 3): Create speaker role detection logic:
///   - Split the conversation into Agent / Caller roles.
///   - Fall back to "Speaker 1" / "Speaker 2" if roles cannot be detected.
///   - Support Armenian ("hy") and English ("en").
///
/// Suggested steps:
///   1. Split transcriptText into lines/turns. Transcripts often look like:
///        "Agent: Hello!"                          (explicit label)
///        "Speaker 1: Hello!"                      (generic label)
///        or plain alternating lines with no labels at all.
///   2. If a line starts with an explicit label (e.g. "Agent:", "Caller:",
///      "Operator:"), use it to assign the role.
///   3. If there are NO labels, use simple logic (team decision):
///      - First speaker  = "Agent"
///      - Second speaker = "Caller"
///      - Alternate between them for the following turns.
///      (Optional improvement: keyword heuristics — the speaker who says
///      "how can I help you" / "ինչպե՞ս կարող եմ օգնել" is likely the Agent.)
///   4. If roles cannot be detected at all, fall back to
///      "Speaker 1" and "Speaker 2" (alternating between consecutive turns).
///   5. Return one ConversationTurn per utterance:
///        new ConversationTurn { Role = "Agent", Text = "Hello!" }
///
/// Note: this method is pure text processing — it does NOT need to call Azure.
/// </summary>
public class SpeakerRoleService : ISpeakerRoleService
{
    /// <inheritdoc />
    public List<ConversationTurn> SplitConversation(string transcriptText, string language)
    {
        // TODO (Member 3): replace this placeholder with the real logic
        // described in the class comment above.

        // Placeholder so the project compiles and runs:
        // returns the whole transcript as a single unlabeled turn.
        return new List<ConversationTurn>
        {
            new ConversationTurn { Role = "Speaker 1", Text = transcriptText }
        };
    }
}
