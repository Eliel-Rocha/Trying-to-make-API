using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços personalizados
builder.Services.AddSingleton<MongoDBService>();

// Configurar o Kestrel diretamente no código
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5000); // Porta HTTP
    serverOptions.ListenLocalhost(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Porta HTTPS
    });
});

//Testando branch!

var app = builder.Build();

// Configurar middleware personalizado
app.UseHttpsRedirection();

// Mapear endpoints personalizados
app.MapGet("/", () => "Hello World!");

app.MapGet("/consulta", async (MongoDBService mongoDBService) =>
{
    var consulta = new Consulta(1000, 500, "c0173a"); // Defina os valores conforme necessário
    var dados = await mongoDBService.ConsultarRetornarAsync(consulta.Hexid);

    if (dados != null)
    {
        return Results.Json(dados);
    }
    else
    {
        return Results.NotFound(new { message = "Dados não encontrados" });
    }

});

app.Run();
