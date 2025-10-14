using System.Reflection;
using API_ovni.Data;
using MongoDB.Driver;

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

builder.Services.AddSingleton<MongodbService>();

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
