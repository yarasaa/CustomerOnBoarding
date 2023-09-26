using System.Text.Json.Serialization;
using amorphie.core.Extension;
using amorphie.core.Identity;
using amorphie.core.Swagger;
using amorphie.template.data;
using amorphie.template.HealthCheck;
using amorphie.template.Validator;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Prometheus;



var builder = WebApplication.CreateBuilder(args);
await builder.Configuration.AddVaultSecrets("amorphie-secretstore", new string[] { "amorphie-secretstore" });
var dodgeBusiness = builder.Configuration["DodgeBusiness"];

// If you use AutoInclude in context you should add  ReferenceHandler.IgnoreCycles to avoid circular load
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddDaprClient();
builder.Services.AddHealthChecks().AddBBTHealthCheck();

builder.Services.AddScoped<IBBTIdentity, FakeIdentity>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<AddSwaggerParameterFilter>();
});

builder.Services.AddValidatorsFromAssemblyContaining<DepositMobApprovalValidator>(includeInternalTypes: true);
builder.Services.AddAutoMapper(typeof(Program).Assembly);



builder.Services.AddDbContext<TemplateDbContext>
    (options => options.UseSqlServer(dodgeBusiness, b => b.MigrationsAssembly("amorphie.template.data")));


var app = builder.Build();


using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();

// db.Database.Migrate();
// DbInitializer.Initialize(db);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.AddRoutes();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapMetrics();

app.Run();

