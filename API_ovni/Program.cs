using System.Reflection;
using API_ovni.Data;
using API_ovni.Models;
using API_ovni.Security;
using API_ovni.Services;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            // Permite qualquer origem, cabeçalho e método
            policy.AllowAnyOrigin() 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Configuração explícita do Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // HTTPS (com certificado de desenvolvimento)
    serverOptions.ListenAnyIP(7199, listenOptions =>
    {
        listenOptions.UseHttps(); // Usa o certificado padrão de desenvolvimento
    });

    // HTTP
    serverOptions.ListenAnyIP(5206);
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


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    

    // Usa Reflection para pegar o nome do arquivo XML gerado (ex: API_ovni.xml)
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);

    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Api-Key", // O nome do Header
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "Chave de API para autorização (Admin ou Usuário)"
    });

    
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] {}
        }
    });

    // Diz ao Swashbuckle para incluir os comentários deste arquivo
    options.IncludeXmlComments(xmlPath);
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