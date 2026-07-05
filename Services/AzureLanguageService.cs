using Azure;
using Azure.AI.TextAnalytics;

namespace Task_2_TranscriptAnalysis.Services;

/// <summary>
/// Interface for the Azure connection service.
/// Other services depend on this INTERFACE (not the concrete class) so that
/// Member 5 can replace it with a fake/mock implementation in tests.
/// </summary>
public interface IAzureLanguageService
{
    /// <summary>
    /// Sends text to Azure AI Language Service and returns the PII
    /// (Personally Identifiable Information) entities it detected.
    /// </summary>
    /// <param name="text">The text to analyze (e.g. the whole transcript).</param>
    /// <param name="language">Language code: "en" for English, "hy" for Armenian.</param>
    /// <returns>The collection of PII entities Azure found (may be empty).</returns>
    Task<PiiEntityCollection> AnalyzeText(string text, string language);
}

/// <summary>
/// OWNER: Member 1 (this file is complete — do not edit unless the Azure setup changes).
///
/// This is the ONLY class that talks to Azure directly. It:
///   1. Reads the endpoint + key from configuration (appsettings.json).
///   2. Creates a TextAnalyticsClient (the official Azure SDK client).
///   3. Exposes one method, AnalyzeText, that runs PII detection.
///
/// Everyone else (Members 2-4) should call this service instead of using the
/// Azure SDK directly. That keeps all Azure details in one place.
/// </summary>
public class AzureLanguageService : IAzureLanguageService
{
    private readonly TextAnalyticsClient _client;

    /// <summary>
    /// The constructor receives IConfiguration automatically through
    /// dependency injection (registered in Program.cs) and builds the Azure client.
    /// </summary>
    public AzureLanguageService(IConfiguration configuration)
    {
        // Read the Azure settings from appsettings.json.
        // In production these should come from environment variables or a
        // secret store — never commit real keys to source control!
        string? endpoint = configuration["AzureLanguageEndpoint"];
        string? key = configuration["AzureLanguageKey"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Azure Language Service is not configured. " +
                "Set 'AzureLanguageEndpoint' and 'AzureLanguageKey' in appsettings.json.");
        }

        // The client is thread-safe, so one instance is shared by the whole app
        // (that is why this service is registered as a singleton in Program.cs).
        _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    /// <inheritdoc />
    public async Task<PiiEntityCollection> AnalyzeText(string text, string language)
    {
        // Call the "Recognize PII Entities" feature of Azure AI Language Service.
        // Azure scans the text and returns entities such as Person, Address,
        // PhoneNumber, Email, USSocialSecurityNumber, each with a confidence score.
        //
        // NOTE: this call can throw RequestFailedException if:
        //   - the endpoint/key is wrong (HTTP 401),
        //   - Azure is unreachable or down,
        //   - the language is not supported for PII detection.
        // Member 4 handles those errors in the controller.
        Response<PiiEntityCollection> response =
            await _client.RecognizePiiEntitiesAsync(text, language);

        return response.Value;
    }
}
