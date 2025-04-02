using System.Text;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Console;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Configure logging first
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => 
{
    options.IncludeScopes = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.UseUtcTimestamp = true;
    options.SingleLine = false;
    options.ColorBehavior = LoggerColorBehavior.Enabled;
});

// Set default log level
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Add filters
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Warning);

// Register all custom services and dependencies
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IUserIdProvider, FirebaseUserIdProvider>();
builder.Services.AddSingleton<FirebaseHubAuthFilter>();  // Must be registered before SignalR config

// Initialize Firebase services
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-adminsdk.json"),
    ProjectId = "nerdboard-956ae" // Explicitly set project ID
});

var firebaseApp = FirebaseApp.DefaultInstance;
Console.WriteLine($"Firebase initialized for project: {firebaseApp.Options.ProjectId}");

builder.Services.AddSingleton(FirestoreDb.Create("nerdboard-956ae"));

// Configure Authentication (must come before SignalR)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/nerdboard-956ae";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/nerdboard-956ae",
            ValidateAudience = true,
            ValidAudience = "nerdboard-956ae",
            ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Handle both query string and header tokens
                var accessToken = context.Request.Query["access_token"];
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Remove "Bearer " prefix if present in query string
                    context.Token = accessToken.ToString().Replace("Bearer ", "");
                    return Task.CompletedTask;
                }

                // Fall back to header
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

// Configure SignalR (after all dependencies are registered)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
})
.AddHubOptions<RoomHub>(options =>
{
    options.AddFilter<FirebaseHubAuthFilter>();
    options.EnableDetailedErrors = true;
}).AddJsonProtocol(options => {
    options.PayloadSerializerOptions.WriteIndented = true;
});

// 6. Configure CORS (after SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .WithOrigins("http://localhost:3001", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Middleware pipeline configuration
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RoomHub>("/roomHub");

app.Run();
