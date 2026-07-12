using Task_2_TranscriptAnalysis.Services;

// Program.cs — the application entry point. It builds the web app,
// registers all services for dependency injection (DI), and starts the server.

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Register framework services
// ---------------------------------------------------------------------------
builder.Services.AddControllers();          // enables [ApiController] classes in /Controllers
builder.Services.AddEndpointsApiExplorer(); // needed by Swagger to discover endpoints
builder.Services.AddSwaggerGen();           // Swagger UI for manual testing in the browser

// CORS: browsers block a frontend served from another address (e.g. React on
// localhost:3000) from calling this API unless we explicitly allow it.
//
// Locally (no AllowedOrigins configured) we stay open so any dev port works.
// In production, set the AllowedOrigins environment variable to the real
// frontend's URL(s) (comma-separated for more than one) — e.g. the Netlify
// site's https://your-app.netlify.app.
const string FrontendCorsPolicy = "Frontend";
string[] allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        // Production with no AllowedOrigins configured: no cross-origin
        // requests are allowed — a safe default until it's set.
    }));

// ---------------------------------------------------------------------------
// 2. Register OUR services (dependency injection)
//
// "Register" means: when a class asks for an interface in its constructor,
// ASP.NET Core knows which implementation to create and hand over.
//
// Lifetimes used here:
//   - Singleton: ONE instance for the whole app. Used for AzureLanguageService
//     because the Azure TextAnalyticsClient is thread-safe and expensive to create.
//   - Scoped: one instance per HTTP request. A safe default for the other services.
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<IAzureLanguageService, AzureLanguageService>(); // Member 1: Azure connection
builder.Services.AddScoped<ITranscriptAnalysisService, TranscriptAnalysisService>(); // Member 2: PII extraction
builder.Services.AddScoped<ISpeakerRoleService, SpeakerRoleService>();               // Member 3: speaker roles

var app = builder.Build();

// ---------------------------------------------------------------------------
// 3. Configure the HTTP request pipeline (middleware)
// ---------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    // Swagger UI is available at /swagger while running in Development.
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only redirect locally. Render (and most PaaS hosts) terminate HTTPS at
// their own edge proxy and forward plain HTTP internally — redirecting here
// too would create a redirect loop.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy); // must run before MapControllers

app.MapControllers(); // route incoming requests to the controllers in /Controllers

app.Run(); // start listening for requests (blocks until the app shuts down)

/// <summary>
/// This empty partial class makes the auto-generated Program class public so
/// the integration tests (/Tests) can boot the app with
/// WebApplicationFactory&lt;Program&gt;. Do not remove it.
/// </summary>
public partial class Program { }
