using Microsoft.AspNetCore.Mvc;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/change-state", async void (
            [FromBody] dynamic body,
            HttpRequest request,
            HttpContext httpContext)
            =>
{

        Console.WriteLine(request.Headers["X-Zeebe-Element-Instance-Key"].ToString());
})
.WithOpenApi();


app.UseCloudEvents();
app.UseRouting();
app.MapSubscribeHandler();
app.UseSwagger();
app.UseSwaggerUI();



app.MapMetrics();

app.Run();

