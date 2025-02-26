using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
// Configurar serviços personalizados
// builder.Services.Add<SeuServico>();

var app = builder.Build();

// Configurar middleware personalizado
app.UseHttpsRedirection();

// Mapear endpoints personalizados
app.MapGet("/", () => "Hello World!");

app.MapGet("/consulta", () =>
{
    Consulta consulta = new Consulta(1000, 500, "c0173a");
    consulta.ConsultarRetornar();

});

app.Run();

//API de consulta ao mongoDB Atlas