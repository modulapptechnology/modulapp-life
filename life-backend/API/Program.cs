using API.Extensions;
using Application;
using Infrastructure;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true, reloadOnChange: true);
}

builder.Services
    .AddApiStructure(builder.Configuration)
    .AddApplication()
    .AddInfrastructure()
    .AddPersistence();

var app = builder.Build();

app.UseApiPipeline();

app.Run();
