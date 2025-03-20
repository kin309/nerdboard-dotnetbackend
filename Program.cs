using System.Text;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .WithOrigins("http://localhost:3001", "http://localhost:5173") // Allow frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Adiciona a autenticação JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;  // Habilite apenas em produção
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "https://securetoken.google.com/nerdboard-956ae",
            ValidAudience = "nerdboard-956ae",
        };
    });

FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-adminsdk.json")
});

// Adiciona o Firestore
builder.Services.AddSingleton(FirestoreDb.Create("nerdboard-956ae"));

builder.Services.AddSingleton<IRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IUserIdProvider, FirebaseUserIdProvider>();

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAllOrigins");

app.UseMiddleware<FirebaseAuthMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RoomHub>("/roomHub");

app.Run();
