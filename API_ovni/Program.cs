using System.Reflection;
using API_ovni.Data;
using MongoDB.Driver;
using API_ovni.Services;

var builder = WebApplication.CreateBuilder(args);

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

// 3. Registra as Coleções qde serviços 
builder.Services.AddSingleton<IMongoCollection<OvniData>>(s =>
{
    var database = s.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<OvniData>("voos"); // Nome da coleção de dados
});

builder.Services.AddSingleton<IMongoCollection<ConfiguracaoLimpeza>>(s =>
{
    var database = s.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<ConfiguracaoLimpeza>("Configuracoes_Servidor");
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
