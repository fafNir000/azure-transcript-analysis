using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Task_2_TranscriptAnalysis.Models;
using Xunit;

namespace Task_2_TranscriptAnalysis.Tests;

/// <summary>
/// OWNER: Member 5
///
/// TODO (Member 5): Create integration tests:
///   - Test English transcript
///   - Test Armenian transcript
///   - Test missing attributes (null values)
///   - Test empty input validation
///   - Test wrong language validation
///
/// How these tests work:
///   WebApplicationFactory<Program> boots the whole API in memory (no real
///   HTTP server needed) and gives you an HttpClient to call it with.
///   That makes these INTEGRATION tests: they go through the real controller,
///   validation, and services.
///
/// Run the tests from the project folder with:  dotnet test
///
/// NOTE: every test below is currently marked Skip so the placeholder suite
/// passes while the endpoint is unfinished. When Members 2-4 finish their
/// parts, remove the Skip argument (change [Fact(Skip = "...")] to [Fact])
/// and fill in the TODOs.
///
/// TIP: tests that talk to real Azure need valid keys in appsettings.json.
/// To test WITHOUT Azure, register a fake IAzureLanguageService using
/// factory.WithWebHostBuilder(...) — ask the team before deciding the approach.
/// </summary>
public class TranscriptAnalysisTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TranscriptAnalysisTests(WebApplicationFactory<Program> factory)
    {
        // One in-memory instance of the API shared by all tests in this class.
        _client = factory.CreateClient();
    }

    [Fact(Skip = "TODO (Member 5): implement once the analyze endpoint is ready.")]
    public async Task Analyze_EnglishTranscript_ReturnsExtractedAttributes()
    {
        // TODO (Member 5):
        //   1. Build a TranscriptRequest with an English transcript that
        //      contains a name, phone number, email, address, and SSN.
        //   2. POST it: await _client.PostAsJsonAsync("/api/transcript/analyze", request);
        //   3. Assert the status code is 200 OK.
        //   4. Read the body: await response.Content.ReadFromJsonAsync<TranscriptResponse>();
        //   5. Assert the expected attributes were extracted (e.g. Name is not null).
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO (Member 5): implement once the analyze endpoint is ready.")]
    public async Task Analyze_ArmenianTranscript_ReturnsExtractedAttributes()
    {
        // TODO (Member 5): same as the English test but with an Armenian ("hy")
        // transcript. Verify Armenian text is handled without errors.
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO (Member 5): implement once the analyze endpoint is ready.")]
    public async Task Analyze_TranscriptWithoutPii_ReturnsNullAttributes()
    {
        // TODO (Member 5):
        //   Send a transcript that contains NO personal information and assert
        //   that every property of ExtractedAttributes is null (not "" or "N/A").
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO (Member 5): implement once validation is ready.")]
    public async Task Analyze_EmptyTranscript_ReturnsBadRequest()
    {
        // TODO (Member 5):
        //   Send a request with transcriptText = "" and assert the response
        //   status code is HttpStatusCode.BadRequest (400).
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO (Member 5): implement once validation is ready.")]
    public async Task Analyze_UnsupportedLanguage_ReturnsBadRequest()
    {
        // TODO (Member 5):
        //   Send a request with language = "fr" (or any value that is not
        //   "en"/"hy") and assert the response status code is 400 Bad Request.
        await Task.CompletedTask;
    }
}
