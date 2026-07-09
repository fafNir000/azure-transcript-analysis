using System.Text.RegularExpressions;
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
/// Extracts business attributes from Azure AI Language Service PII results.
///
/// SSN NOTE (found during live testing): when a caller says an SSN alone in
/// a reply line ("It is 123-45-6789"), Azure classifies it as PhoneNumber,
/// because the surrounding words "social security number" are in the AGENT's
/// previous line. We therefore re-check every PhoneNumber entity against the
/// SSN pattern XXX-XX-XXXX — a US phone number never has that 3-2-4 grouping,
/// so the reclassification is safe.
/// </summary>
public class TranscriptAnalysisService : ITranscriptAnalysisService
{
    /// <summary>US Social Security Number pattern: 3 digits - 2 digits - 4 digits.</summary>
    private static readonly Regex SsnPattern = new(@"^\d{3}-\d{2}-\d{4}$", RegexOptions.Compiled);

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
        // Call Azure AI Language Service to detect PII entities.
        // (Long transcripts are chunked inside AzureLanguageService — the
        // returned list already contains the entities of ALL chunks.)
        List<PiiEntity> entities =
            await _azureLanguageService.AnalyzeText(transcriptText, language);

        var result = new ExtractedAttributes();

        // Keep the highest-confidence entity for each category.
        double personConfidence = 0;
        double addressConfidence = 0;
        double ssnConfidence = 0;
        double phoneConfidence = 0;
        double emailConfidence = 0;

        foreach (var entity in entities)
        {
            // Ignore low-confidence detections.
            if (entity.ConfidenceScore < 0.5)
                continue;

            string category = entity.Category.ToString();

            if (category == "Person")
            {
                if (entity.ConfidenceScore > personConfidence)
                {
                    personConfidence = entity.ConfidenceScore;
                    result.Name = entity.Text;
                }
            }
            else if (category == "Address")
            {
                if (entity.ConfidenceScore > addressConfidence)
                {
                    addressConfidence = entity.ConfidenceScore;
                    result.Address = entity.Text;
                }
            }
            else if (category == "USSocialSecurityNumber")
            {
                if (entity.ConfidenceScore > ssnConfidence)
                {
                    ssnConfidence = entity.ConfidenceScore;
                    result.SocialSecurityNumber = entity.Text;
                }
            }
            else if (category == "PhoneNumber")
            {
                // SSN said alone in a line comes back as PhoneNumber (see
                // class comment). Route XXX-XX-XXXX values to the SSN field.
                if (SsnPattern.IsMatch(entity.Text.Trim()))
                {
                    if (entity.ConfidenceScore > ssnConfidence)
                    {
                        ssnConfidence = entity.ConfidenceScore;
                        result.SocialSecurityNumber = entity.Text.Trim();
                    }
                }
                else if (entity.ConfidenceScore > phoneConfidence)
                {
                    phoneConfidence = entity.ConfidenceScore;
                    result.PhoneNumber = entity.Text;
                }
            }
            else if (category == "Email")
            {
                if (entity.ConfidenceScore > emailConfidence)
                {
                    emailConfidence = entity.ConfidenceScore;
                    result.Email = entity.Text;
                }
            }
        }

        return result;
    }
}
