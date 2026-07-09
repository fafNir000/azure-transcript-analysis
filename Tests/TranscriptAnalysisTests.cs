using System.Net;
using System.Net.Http.Json;
using Azure.AI.TextAnalytics;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Task_2_TranscriptAnalysis.Models;
using Task_2_TranscriptAnalysis.Services;
using Xunit;

namespace Task_2_TranscriptAnalysis.Tests;

/// <summary>
/// OWNER: Member 5
///
/// Integration tests: WebApplicationFactory&lt;Program&gt; boots the WHOLE API in
/// memory (real Program.cs, real DI, real controller and services) and gives
/// us an HttpClient whose requests pass through all the real code.
///
/// AZURE IS FAKED here: we replace IAzureLanguageService with
/// FakeAzureLanguageService so tests are fast, free, deterministic, and run
/// without internet or a key. Each test declares exactly which entities
/// "Azure" should return. The REAL Azure behavior was verified manually —
/// see docs/TestResults.md for those live results.
///
/// Run with:  dotnet test
/// </summary>
public class TranscriptAnalysisTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Our tests live INSIDE the web project (no separate test project), so
    /// WebApplicationFactory cannot discover the app's content root by itself.
    /// This environment variable tells it: use the test output folder —
    /// appsettings.json is copied there at build time, which is all the app
    /// needs to boot. Runs once, before any test in this class.
    /// </summary>
    static TranscriptAnalysisTests()
    {
        Environment.SetEnvironmentVariable(
            "ASPNETCORE_TEST_CONTENTROOT_TASK_2_TRANSCRIPTANALYSIS",
            AppContext.BaseDirectory);
    }

    public TranscriptAnalysisTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // ------------------------------------------------------------------
    // Test infrastructure
    // ------------------------------------------------------------------

    /// <summary>
    /// A stand-in for the real Azure connection. Returns exactly the entities
    /// a test hands it — no network, no key, always the same result.
    /// </summary>
    private sealed class FakeAzureLanguageService : IAzureLanguageService
    {
        private readonly (string Text, string Category)[] _entities;

        public FakeAzureLanguageService(params (string Text, string Category)[] entities)
        {
            _entities = entities;
        }

        public Task<List<PiiEntity>> AnalyzeText(string text, string language)
        {
            // TextAnalyticsModelFactory exists in the SDK precisely for tests:
            // it can construct response objects that normally only Azure creates.
            List<PiiEntity> result = _entities
                .Select(e => TextAnalyticsModelFactory.PiiEntity(e.Text, e.Category, null, 0.9, 0, e.Text.Length))
                .ToList();
            return Task.FromResult(result);
        }
    }

    /// <summary>
    /// Builds a client whose IAzureLanguageService is replaced by a fake that
    /// returns the given entities. ConfigureTestServices runs AFTER Program.cs
    /// registrations, so the fake wins over the real AzureLanguageService.
    /// </summary>
    private HttpClient CreateClient(params (string Text, string Category)[] fakeEntities)
    {
        return _factory
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IAzureLanguageService>(
                        new FakeAzureLanguageService(fakeEntities))))
            .CreateClient();
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string text, string language)
    {
        var request = new TranscriptRequest { TranscriptText = text, Language = language };
        return client.PostAsJsonAsync("/api/transcript/analyze", request);
    }

    // ------------------------------------------------------------------
    // TEST 1 + 3 — English transcript with all five attributes
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_EnglishTranscript_ReturnsExtractedAttributes()
    {
        var client = CreateClient(
            ("John Smith", "Person"),
            ("123 Main Street", "Address"),
            ("555-123-4567", "PhoneNumber"),
            ("john.smith@example.com", "Email"),
            ("123-45-6789", "USSocialSecurityNumber"));

        var response = await PostAsync(client,
            "Agent: Hello, how can I help you?\nCaller: My name is John Smith.", "en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Equal("John Smith", body!.ExtractedAttributes.Name);
        Assert.Equal("123 Main Street", body.ExtractedAttributes.Address);
        Assert.Equal("555-123-4567", body.ExtractedAttributes.PhoneNumber);
        Assert.Equal("john.smith@example.com", body.ExtractedAttributes.Email);
        Assert.Equal("123-45-6789", body.ExtractedAttributes.SocialSecurityNumber);
    }

    // ------------------------------------------------------------------
    // TEST 2 — Armenian transcript
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_ArmenianTranscript_ReturnsExtractedAttributes()
    {
        var client = CreateClient(
            ("Արամ Պետրոսյան", "Person"),
            ("093-123456", "PhoneNumber"));

        var response = await PostAsync(client,
            "Օպերատոր: Բարև ձեզ, ինչպե՞ս կարող եմ օգնել:\nՀաճախորդ: Իմ անունը Արամ Պետրոսյան է:", "hy");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Equal("Արամ Պետրոսյան", body!.ExtractedAttributes.Name);
        Assert.Equal("093-123456", body.ExtractedAttributes.PhoneNumber);
        // Armenian labels must be mapped to Agent/Caller:
        Assert.Equal("Agent", body.Conversation[0].Role);
        Assert.Equal("Caller", body.Conversation[1].Role);
    }

    // ------------------------------------------------------------------
    // TEST 4 — attributes not present in the text must be null
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_TranscriptWithoutPii_ReturnsNullAttributes()
    {
        var client = CreateClient(/* Azure finds nothing */);

        var response = await PostAsync(client,
            "Agent: Hello!\nCaller: I just have a general question about opening hours.", "en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Null(body!.ExtractedAttributes.Name);
        Assert.Null(body.ExtractedAttributes.Address);
        Assert.Null(body.ExtractedAttributes.SocialSecurityNumber);
        Assert.Null(body.ExtractedAttributes.PhoneNumber);
        Assert.Null(body.ExtractedAttributes.Email);
    }

    // ------------------------------------------------------------------
    // TEST 5 — empty input must be rejected with 400
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_EmptyTranscript_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await PostAsync(client, "", "en");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // TEST 6 — unsupported language must be rejected with 400
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_UnsupportedLanguage_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await PostAsync(client, "Hello world", "fr");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // Text over 50,000 characters must be rejected with 400 (boundary test)
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_TextOver50000Chars_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await PostAsync(client, new string('a', 50001), "en");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ------------------------------------------------------------------
    // TEST 7 — conversation WITH Agent:/Caller: labels uses those labels
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_LabeledConversation_UsesLabels()
    {
        var client = CreateClient();

        var response = await PostAsync(client,
            "Caller: Hi, I need help.\nAgent: Of course, what can I do for you?\nCaller: Thanks!", "en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body!.Conversation.Count);
        // Labels must be respected even though the CALLER speaks first here:
        Assert.Equal("Caller", body.Conversation[0].Role);
        Assert.Equal("Agent", body.Conversation[1].Role);
        Assert.Equal("Caller", body.Conversation[2].Role);
        // The label itself must be stripped from the text:
        Assert.Equal("Hi, I need help.", body.Conversation[0].Text);
    }

    // ------------------------------------------------------------------
    // TEST 8 — conversation WITHOUT labels alternates Agent/Caller
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_UnlabeledConversation_AlternatesRoles()
    {
        var client = CreateClient();

        var response = await PostAsync(client,
            "Hello, how can I help you?\nMy name is John Smith.\nHow can I assist further?", "en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body!.Conversation.Count);
        // Team decision: first speaker = Agent, then alternate.
        Assert.Equal("Agent", body.Conversation[0].Role);
        Assert.Equal("Caller", body.Conversation[1].Role);
        Assert.Equal("Agent", body.Conversation[2].Role);
    }

    // ------------------------------------------------------------------
    // SSN reclassification — documents REAL Azure behavior found in live
    // testing: an SSN said alone in a line comes back as PhoneNumber.
    // Our extraction must route XXX-XX-XXXX values to the SSN field.
    // ------------------------------------------------------------------
    [Fact]
    public async Task Analyze_SsnClassifiedAsPhoneByAzure_IsReclassified()
    {
        var client = CreateClient(
            ("555-123-4567", "PhoneNumber"),   // a real phone number
            ("123-45-6789", "PhoneNumber"));   // an SSN that Azure mislabeled

        var response = await PostAsync(client,
            "Agent: Your phone and SSN please?\nCaller: 555-123-4567 and 123-45-6789.", "en");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        Assert.NotNull(body);
        Assert.Equal("555-123-4567", body!.ExtractedAttributes.PhoneNumber);
        Assert.Equal("123-45-6789", body.ExtractedAttributes.SocialSecurityNumber);
    }

    // ------------------------------------------------------------------
    // Chunking — long texts must be split at line breaks into pieces that
    // respect Azure's 5,120-characters-per-document limit.
    // (Unit test: calls the splitter directly, no HTTP involved.)
    // ------------------------------------------------------------------
    [Fact]
    public void SplitIntoChunks_LongText_RespectsLimitAndPreservesContent()
    {
        // 300 lines of ~55 chars = ~16,800 characters -> at least 4 chunks of max 5,000.
        var lines = Enumerable.Range(1, 300)
            .Select(i => $"Caller: This is line number {i} of a very long transcript.");
        string text = string.Join("\n", lines);

        List<string> chunks = AzureLanguageService.SplitIntoChunks(text, AzureLanguageService.MaxChunkSize);

        Assert.True(chunks.Count >= 2, "long text must produce multiple chunks");
        Assert.All(chunks, c => Assert.True(c.Length <= AzureLanguageService.MaxChunkSize));
        // Joining the chunks back together must reproduce the original text
        // exactly — nothing lost, nothing duplicated, no line cut in half.
        Assert.Equal(text, string.Join("\n", chunks));
    }
}
