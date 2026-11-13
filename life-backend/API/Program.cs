using API.Authorization;
using API.Extensions;
using Application;
using Infrastructure;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Services
    .AddApiStructure(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddAccessTokenAuthentication()
    .AddApiAuthorization()
    .AddSubscriptionReconciliationWorker();

var app = builder.Build();

app.UseApiPipeline();

app.Run();

public partial class Program { }
