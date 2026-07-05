using Microsoft.AspNetCore.Mvc;
using Task_2_TranscriptAnalysis.Models;
using Task_2_TranscriptAnalysis.Services;

namespace Task_2_TranscriptAnalysis.Controllers;

/// <summary>
/// OWNER: Member 4
///
/// TODO (Member 4): Create POST endpoint /api/transcript/analyze
///   - Accept the TranscriptRequest model (transcriptText, language).
///   - Add validation:
///       * transcriptText must not be null/empty/whitespace  -> return 400 Bad Request
///       * language must be "en" or "hy"                     -> return 400 Bad Request
///       * transcriptText max length is 50,000 characters    -> return 400 Bad Request
///   - Add error handling:
///       * Azure down / unreachable          -> return an error message (503 Service Unavailable)
///       * Wrong Azure key                   -> return 401 Unauthorized
///         (do NOT leak the key or full Azure error details to the client)
///       * Any other unexpected error        -> return 500 Internal Server Error
///     Tip: Azure SDK errors are thrown as Azure.RequestFailedException —
///     check its .Status property to tell the cases apart (401 = bad key).
///   - Return the TranscriptResponse model:
///       * Conversation        -> from ISpeakerRoleService.SplitConversation(...)
///       * ExtractedAttributes -> from ITranscriptAnalysisService.ExtractAttributes(...)
/// </summary>
[ApiController]
[Route("api/transcript")]
public class TranscriptController : ControllerBase
{
    private readonly ITranscriptAnalysisService _transcriptAnalysisService;
    private readonly ISpeakerRoleService _speakerRoleService;
    private readonly ILogger<TranscriptController> _logger;

    /// <summary>
    /// All dependencies arrive via dependency injection (configured in Program.cs).
    /// Member 2's and Member 3's services are already injected and ready to use.
    /// </summary>
    public TranscriptController(
        ITranscriptAnalysisService transcriptAnalysisService,
        ISpeakerRoleService speakerRoleService,
        ILogger<TranscriptController> logger)
    {
        _transcriptAnalysisService = transcriptAnalysisService;
        _speakerRoleService = speakerRoleService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/transcript/analyze
    /// Analyzes a call transcript: splits it into speaker turns and extracts PII.
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<TranscriptResponse>> Analyze([FromBody] TranscriptRequest request)
    {
        // TODO (Member 4): implement validation, service calls, and error
        // handling as described in the class comment above. Rough outline:
        //
        //   1. Validate request.TranscriptText and request.Language
        //      (return BadRequest("...") with a clear message when invalid).
        //   2. var conversation = _speakerRoleService.SplitConversation(
        //          request.TranscriptText, request.Language);
        //   3. var attributes = await _transcriptAnalysisService.ExtractAttributes(
        //          request.TranscriptText, request.Language);
        //   4. Wrap Azure calls in try/catch (Azure.RequestFailedException).
        //   5. return Ok(new TranscriptResponse { ... });

        await Task.CompletedTask; // remove once the real implementation is added

        // Placeholder so the project compiles and the route is visible in Swagger:
        return StatusCode(StatusCodes.Status501NotImplemented,
            "Not implemented yet — Member 4's task.");
    }
}
