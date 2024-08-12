using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RagChat.Logic;
using RagChat.Models;
using RagChatLogic.BlobService;
using RagChatLogic.ServiceWrappers;
using RagChatLogic.OpenAIService;
using RagChatLogic.SearchService;
using RagChatLogic.StorageService;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();

//builder.Configuration.AddJsonFile("appsettings.json");
var keyVaultUri = builder.Configuration["AzureKeyVault:VaultUri"];

if (!string.IsNullOrEmpty(keyVaultUri))
{
    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = builder.Configuration["AzureKeyVault:ClientId"] // Optional: specify if using user-assigned managed identity
    });
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

// Database.
builder.Services
    .AddDbContext<RagChatbotDbContext>(options =>
        options.UseSqlServer(builder.Configuration["ConnectionStrings-RagChatbotDb"]
        ?? throw new InvalidOperationException("Connection string not found!")));

// Identity in entity framework core.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<RagChatbotDbContext>()
    .AddSignInManager()
    .AddApiEndpoints();

// Register UserManager and RoleManager.
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Authentication with Jwt bearer token.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt-Issuer"],
            ValidAudience = builder.Configuration["Jwt-Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt-Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });


builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

// Injecting IUserAccount interface in AccountLogic.
builder.Services.AddScoped<ITokenLogic, TokenLogic>();
builder.Services.AddScoped<IAccountLogic, AccountLogic>();
builder.Services.AddScoped<IConfigurationWrapper, ConfigurationWrapper>();
builder.Services.AddScoped<IBlobServiceLogic, BlobServiceLogic>();
builder.Services.AddScoped<ISearchServiceLogic, SearchServiceLogic>();
builder.Services.AddScoped<IOpenAIService, OpenAIServiceLogic>();

builder.Services.AddScoped<IBlobServiceClientWrapper>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var connectionString = configuration["ConnectionStrings-BlobStorage"];

    return new BlobServiceClientWrapper(connectionString);
});

builder.Services.AddScoped<ISearchClientsWrapper>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    string apiUrl = configuration["Secrets-SearchApiUri"];
    string apiKey = configuration["Secrets-SearchApiKey"];

    return new SearchClientsWrapper(apiUrl, apiKey);
});

builder.Services.AddScoped<IOpenAIClientWrapper>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    return new OpenAIClientWrapper(configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
