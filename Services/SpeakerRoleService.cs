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
    List<ConversationTurn> SplitConversation(string transcriptText, string language);
}

/// <summary>
/// OWNER: Member 3
///
/// Splits a transcript into conversation turns and determines speaker roles.
/// This service performs only text processing and does not call Azure.
/// </summary>
public class SpeakerRoleService : ISpeakerRoleService
{
    public List<ConversationTurn> SplitConversation(string transcriptText, string language)
    {
        var conversation = new List<ConversationTurn>();

        if (string.IsNullOrWhiteSpace(transcriptText))
            return conversation;

        var lines = transcriptText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count == 0)
            return conversation;

        bool hasExplicitLabels = lines.Any(line =>
            line.StartsWith("Agent:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Caller:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Operator:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Customer:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Client:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Speaker 1:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Speaker 2:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Օպերատոր:", StringComparison.OrdinalIgnoreCase) ||
            line.StartsWith("Հաճախորդ:", StringComparison.OrdinalIgnoreCase));

        if (hasExplicitLabels)
        {
            foreach (var line in lines)
            {
                string role;
                string text;

                if (line.StartsWith("Agent:", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Operator:", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("Օպերատոր:", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Agent";
                    text = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else if (line.StartsWith("Caller:", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("Customer:", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("Client:", StringComparison.OrdinalIgnoreCase) ||
                         line.StartsWith("Հաճախորդ:", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Caller";
                    text = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else if (line.StartsWith("Speaker 1:", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Speaker 1";
                    text = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else if (line.StartsWith("Speaker 2:", StringComparison.OrdinalIgnoreCase))
                {
                    role = "Speaker 2";
                    text = line.Substring(line.IndexOf(':') + 1).Trim();
                }
                else
                {
                    role = "Speaker 1";
                    text = line;
                }

                conversation.Add(new ConversationTurn
                {
                    Role = role,
                    Text = text
                });
            }

            return conversation;
        }

        // No labels found -> alternate Agent / Caller
        bool agentTurn = true;

        foreach (var line in lines)
        {
            conversation.Add(new ConversationTurn
            {
                Role = agentTurn ? "Agent" : "Caller",
                Text = line
            });

            agentTurn = !agentTurn;
        }

        return conversation;
    }
}
