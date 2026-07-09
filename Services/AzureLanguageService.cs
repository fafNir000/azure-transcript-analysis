using System.Text;
using Azure;
using Azure.AI.TextAnalytics;

namespace Task_2_TranscriptAnalysis.Services;

/// <summary>
/// Interface for the Azure connection service.
/// Other services depend on this INTERFACE (not the concrete class) so that
/// tests can replace it with a fake implementation (see /Tests).
/// </summary>
public interface IAzureLanguageService
{
    /// <summary>
    /// Sends text to Azure AI Language Service and returns the PII
    /// (Personally Identifiable Information) entities it detected.
    /// Long texts are split into chunks internally (Azure's synchronous API
    /// accepts at most 5,120 characters per document), so callers can pass
    /// transcripts up to the API's own 50,000-character limit safely.
    /// </summary>
    /// <param name="text">The text to analyze (e.g. the whole transcript).</param>
    /// <param name="language">Language code: "en" for English, "hy" for Armenian.</param>
    /// <returns>All PII entities Azure found across all chunks (may be empty).</returns>
    Task<List<PiiEntity>> AnalyzeText(string text, string language);
}

/// <summary>
/// OWNER: Member 1 (complete).
///
/// This is the ONLY class that talks to Azure directly. It:
///   1. Reads the endpoint + key from configuration (user-secrets/appsettings.json).
///   2. Creates a TextAnalyticsClient (the official Azure SDK client).
///   3. Exposes one method, AnalyzeText, that runs PII detection.
///
/// CHUNKING (why it exists): Azure's synchronous PII API has two hard limits —
/// max 5,120 characters per document and max 5 documents per request
/// (https://learn.microsoft.com/azure/ai-services/language-service/concepts/data-limits).
/// Our API accepts transcripts up to 50,000 characters, so this class splits
/// long text into chunks of at most 5,000 characters (cutting at line breaks,
/// never mid-line), sends up to 5 chunks per batch request, and merges the
/// entities from all chunks into one list. Callers never notice the chunking.
/// </summary>
public class AzureLanguageService : IAzureLanguageService
{
    /// <summary>Safety margin under Azure's 5,120-characters-per-document limit.</summary>
    public const int MaxChunkSize = 5000;

    /// <summary>Azure allows at most 5 documents per synchronous PII request.</summary>
    public const int MaxDocumentsPerRequest = 5;

    private readonly TextAnalyticsClient _client;

    /// <summary>
    /// The constructor receives IConfiguration automatically through
    /// dependency injection (registered in Program.cs) and builds the Azure client.
    /// </summary>
    public AzureLanguageService(IConfiguration configuration)
    {
        // Real values come from user-secrets on each developer's machine;
        // appsettings.json holds only placeholders. Never commit real keys!
        string? endpoint = configuration["AzureLanguageEndpoint"];
        string? key = configuration["AzureLanguageKey"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Azure Language Service is not configured. " +
                "Set 'AzureLanguageEndpoint' and 'AzureLanguageKey' via user-secrets.");
        }

        // The client is thread-safe, so one instance is shared by the whole app
        // (that is why this service is registered as a singleton in Program.cs).
        _client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    /// <inheritdoc />
    public async Task<List<PiiEntity>> AnalyzeText(string text, string language)
    {
        List<string> chunks = SplitIntoChunks(text, MaxChunkSize);
        var allEntities = new List<PiiEntity>();

        // Send the chunks in batches of up to 5 documents per request.
        // A maximum-size transcript (50,000 chars) becomes ~10 chunks = 2 requests.
        //
        // NOTE: these calls throw Azure.RequestFailedException if the key is
        // wrong (Status 401) or Azure is unreachable (Status 0). Deliberately
        // NOT caught here — the controller translates errors into HTTP answers.
        for (int i = 0; i < chunks.Count; i += MaxDocumentsPerRequest)
        {
            List<string> batch = chunks.Skip(i).Take(MaxDocumentsPerRequest).ToList();

            Response<RecognizePiiEntitiesResultCollection> response =
                await _client.RecognizePiiEntitiesBatchAsync(batch, language);

            foreach (RecognizePiiEntitiesResult result in response.Value)
            {
                // A document-level error (e.g. unsupported language) does not
                // throw by itself in batch mode — surface it as an exception
                // so failures are never silently swallowed.
                if (result.HasError)
                {
                    throw new InvalidOperationException(
                        $"Azure could not analyze part of the transcript: {result.Error.Message}");
                }

                allEntities.AddRange(result.Entities);
            }
        }

        return allEntities;
    }

    /// <summary>
    /// Splits text into chunks of at most <paramref name="maxChunkSize"/>
    /// characters, cutting at line breaks so that no line (and therefore no
    /// entity, which never spans lines in a transcript) is ever cut in half.
    /// A single line longer than the limit is hard-split as a last resort.
    /// Public and static so the unit tests can verify it directly.
    /// </summary>
    public static List<string> SplitIntoChunks(string text, int maxChunkSize)
    {
        var chunks = new List<string>();

        if (text.Length <= maxChunkSize)
        {
            chunks.Add(text);
            return chunks;
        }

        var current = new StringBuilder();

        foreach (string rawLine in text.Split('\n'))
        {
            string line = rawLine;

            // Last resort: one single line longer than the limit gets hard-split.
            while (line.Length > maxChunkSize)
            {
                if (current.Length > 0)
                {
                    chunks.Add(current.ToString());
                    current.Clear();
                }
                chunks.Add(line.Substring(0, maxChunkSize));
                line = line.Substring(maxChunkSize);
            }

            // Would adding this line overflow the current chunk? Close it first.
            if (current.Length > 0 && current.Length + 1 + line.Length > maxChunkSize)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0)
                current.Append('\n');
            current.Append(line);
        }

        if (current.Length > 0)
            chunks.Add(current.ToString());

        return chunks;
    }
}
