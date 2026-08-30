using TechNotes.API.Extensions;
using TechNotes.Application;
using TechNotes.Infrastructure;
using TechNotes.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddPersistence(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
await app.SeedDatabaseAsync();

app.Run();