using System.Reflection;
using API_ovni.Data;
using API_ovni.Models;
using API_ovni.Security;
using API_ovni.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            //em produção, especifique as origens permitidas
            policy.AllowAnyOrigin() 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add services to the container.
builder.Services.AddControllers();

// Add MongoDB service
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

// Registra o Banco de Dados
builder.Services.AddSingleton<IMongoDatabase>(s =>
{
    var client = s.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["DataBaseName"];
    return client.GetDatabase(databaseName); // "aviao"
});

// Registra as Coleções qde serviços 
builder.Services.AddSingleton<IMongoCollection<OvniData>>(s =>
{
    var database = s.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<OvniData>("voos"); // Nome da coleção de dados
});
// Coleção de Configurações de Limpeza
builder.Services.AddSingleton<IMongoCollection<ConfiguracaoLimpeza>>(s =>
{
    var database = s.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<ConfiguracaoLimpeza>("Configuracoes_Servidor");//nome da coleção de configurações 
});

// Coleção de Usuários (Chaves de API)
builder.Services.AddSingleton<IMongoCollection<ApiKeyUser>>(s =>
{
    var database = s.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<ApiKeyUser>("apiKeys"); // Nome da coleção no banco
});

//Autenticação de Chave de API
builder.Services.AddAuthentication("ApiKey") 
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

builder.Services.AddAuthorization(options =>
{
    // Política para o Admin (Pode LER e ESCREVER)
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Política para Usuários (Pode APENAS LER)
    options.AddPolicy("UserCanRead", policy =>
        policy.RequireRole("Admin", "User"));
});


builder.Services.AddScoped<LimpezaArmazenamentoService>(); //SERVIÇO DE LIMPEZA
builder.Services.AddSingleton<MongodbService>(); // Registra o serviço original
builder.Services.AddSingleton<ApiKeyService>();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Ovni - Rastreamento ADS-B",
        Version = "v1",
        Description = "API para coleta e análise de dados de tráfego aéreo capturados via Raspberry Pi (ADS-B). Projeto Acadêmico.",
        Contact = new OpenApiContact
        {
            Name = "Seu Nome",
            Email = "seu.email@exemplo.com"
        }
    });

    // Habilita os comentários XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Cabeçalho de autenticação usando API Key. Exemplo: 'X-Api-Key: 12345abcdef'",
        Name = "X-Api-Key", 
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                Scheme = "oauth2",
                Name = "ApiKey",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
defaultFilesOptions.DefaultFileNames.Clear();
defaultFilesOptions.DefaultFileNames.Add("html/index.html");
app.UseDefaultFiles(defaultFilesOptions);
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

//esses app .Use são middlewares que processam as requisições HTTP em uma aplicação ASP.NET Core.