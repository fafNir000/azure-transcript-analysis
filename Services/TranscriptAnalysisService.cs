using Azure.AI.TextAnalytics;
using Task_2_TranscriptAnalysis.Models;

namespace Task_2_TranscriptAnalysis.Services;

/// <summary>
/// Interface for the PII extraction service (implemented by Member 2).
/// The controller (Member 4) will call this through the interface.
/// </summary>
public interface ITranscriptAnalysisService
{
    /// <summary>
    /// Analyzes a transcript and returns the extracted PII attributes.
    /// Attributes that are not found in the text must stay null.
    /// </summary>
    /// <param name="transcriptText">The full transcript text.</param>
    /// <param name="language">"en" or "hy".</param>
    Task<ExtractedAttributes> ExtractAttributes(string transcriptText, string language);
}

/// <summary>
/// OWNER: Member 2
///
/// TODO (Member 2): Create extraction logic for:
///   - Person (name)
///   - Address
///   - USSocialSecurityNumber
///   - PhoneNumber
///   - Email
/// Return the ExtractedAttributes model.
///
/// Suggested steps:
///   1. Call _azureLanguageService.AnalyzeText(transcriptText, language).
///      It returns a PiiEntityCollection — a list of entities Azure found.
///   2. Loop over the entities. Each entity has:
///        entity.Text            -> the value found (e.g. "John Smith")
///        entity.Category        -> the type (e.g. PiiEntityCategory.Person)
///        entity.ConfidenceScore -> how sure Azure is (0.0 to 1.0)
///   3. Map each category to the matching ExtractedAttributes property:
///        PiiEntityCategory.Person                 -> result.Name
///        PiiEntityCategory.Address                -> result.Address
///        PiiEntityCategory.USSocialSecurityNumber -> result.SocialSecurityNumber
///        PiiEntityCategory.PhoneNumber            -> result.PhoneNumber
///        PiiEntityCategory.Email                  -> result.Email
///   4. If Azure finds several entities of the same category, keep the one
///      with the highest ConfidenceScore (or the first one — team decision).
///   5. If a category is not found at all, leave the property null.
///
/// Tip: consider ignoring entities with a very low confidence score
/// (e.g. below 0.5) to reduce false positives.
/// </summary>
public class TranscriptAnalysisService : ITranscriptAnalysisService
{
    private readonly IAzureLanguageService _azureLanguageService;

    /// <summary>
    /// The Azure connection service (Member 1's work) is injected here.
    /// Use it — do NOT create your own TextAnalyticsClient in this class.
    /// </summary>
    public TranscriptAnalysisService(IAzureLanguageService azureLanguageService)
    {
        _azureLanguageService = azureLanguageService;
    }

    /// <inheritdoc />
    public async Task<ExtractedAttributes> ExtractAttributes(string transcriptText, string language)
    {
        // TODO (Member 2): replace this placeholder with the real logic
        // described in the class comment above.

        // Placeholder so the project compiles and runs:
        // currently returns an empty result (all attributes null).
        await Task.CompletedTask; // remove this line once you call Azure for real
        return new ExtractedAttributes();
    }
}
