using Task_2_TranscriptAnalysis.Services;

// Program.cs — the application entry point. It builds the web app,
// registers all services for dependency injection (DI), and starts the server.

var builder = WebApplication.CreateBuilder(args);

// Настройка порта для корректной работы в Docker/Azure App Service Linux
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// ---------------------------------------------------------------------------
// 1. Register framework services
// ---------------------------------------------------------------------------
builder.Services.AddControllers();          // enables [ApiController] classes in /Controllers
builder.Services.AddEndpointsApiExplorer(); // needed by Swagger to discover endpoints
builder.Services.AddSwaggerGen();           // Swagger UI for manual testing in the browser

// ---------------------------------------------------------------------------
// 2. Register OUR services (dependency injection)
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<IAzureLanguageService, AzureLanguageService>(); // Member 1: Azure connection
builder.Services.AddScoped<ITranscriptAnalysisService, TranscriptAnalysisService>(); // Member 2: PII extraction
builder.Services.AddScoped<ISpeakerRoleService, SpeakerRoleService>();               // Member 3: speaker roles
builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// 3. Configure the HTTP request pipeline (middleware)
// ---------------------------------------------------------------------------

// Включаем Swagger для ВСЕХ окружений (включая Production в Azure)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transcript Analysis API V1");
    
    // Делает Swagger стартовой страницей. 
    // Теперь корень сайта "/" будет сразу открывать документацию вместо ошибки 404!
    c.RoutePrefix = string.Empty; 
});

// На Linux в Azure App Service автоматическое перенаправление на HTTPS может иногда 
// зацикливаться, если перед ним стоит прокси-сервер. 
// Если возникнут проблемы с бесконечным редиректом, эту строчку можно будет закомментировать.
app.UseHttpsRedirection(); 

app.MapControllers(); // route incoming requests to the controllers in /Controllers

app.Run(); // start listening for requests (blocks until the app shuts down)

/// <summary>
/// This empty partial class makes the auto-generated Program class public so
/// the integration tests (/Tests) can boot the app with
/// WebApplicationFactory&lt;Program&gt;. Do not remove it.
/// </summary>
public partial class Program { }
