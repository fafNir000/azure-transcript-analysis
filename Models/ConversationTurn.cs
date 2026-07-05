namespace Task_2_TranscriptAnalysis.Models;

/// <summary>
/// One "turn" (one utterance) of the conversation, labeled with who said it.
/// </summary>
public class ConversationTurn
{
    /// <summary>
    /// Who is speaking in this turn.
    /// Expected values: "Agent" or "Caller".
    /// If the roles cannot be detected, fall back to "Speaker 1" / "Speaker 2"
    /// (this logic is implemented by Member 3 in SpeakerRoleService).
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>What was said in this turn.</summary>
    public string Text { get; set; } = string.Empty;
}
