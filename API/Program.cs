using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    //Dependendo do usuário será necessário mudar os parametros
    serverOptions.ListenLocalhost(5000); // Porta HTTP
    serverOptions.ListenLocalhost(6000, listenOptions =>
    {
        listenOptions.UseHttps(); // Porta HTTPS
    });
});

// Configurar serviços personalizados
// builder.Services.Add<SeuServico>();

var app = builder.Build();

// Configurar middleware personalizado
app.UseHttpsRedirection();

// Mapear endpoints personalizados
app.MapGet("/", () => "Hello World!");

app.MapGet("/teste1SSs", () => "teste 1");

Consulta consulta = new Consulta(1000, 500, "a1b2c3");
consulta.ConectarMongoDB();

app.Run();