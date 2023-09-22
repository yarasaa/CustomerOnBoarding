using amorphie.template.core.Model;
using amorphie.core.Module.minimal_api;
using amorphie.template.data;
using Microsoft.AspNetCore.Mvc;
using amorphie.core.Swagger;
using Microsoft.OpenApi.Models;
using amorphie.template.core.DTO;
using Dapr.Client;
using Newtonsoft.Json;
using System.Text;
using AutoMapper;
using amorphie.template.core.Search;
using Microsoft.EntityFrameworkCore;

namespace amorphie.template.Module;


public sealed class DepositMobApprovalModule : BaseBBTRoute<DepositMobApprovalDto, DepositMobApproval, TemplateDbContext>
{
    private readonly string dodgeGatewayApiServiceUrl;
    private readonly string dodgeGatewayApiServiceResourceUrl;
    public DepositMobApprovalModule(WebApplication app)
        : base(app)
    {

        dodgeGatewayApiServiceUrl = app.Configuration["DodgeGatewayApiServiceUrl"];
        dodgeGatewayApiServiceResourceUrl = app.Configuration["DodgeGatewayApiServiceResourceUrl"];
    }

    public override string[]? PropertyCheckList => new string[] { "Iban", "FullName" };

    public override string? UrlFragment => "route";

    public override void AddRoutes(RouteGroupBuilder routeGroupBuilder)
    {
        base.AddRoutes(routeGroupBuilder);

        // routeGroupBuilder.MapGet("/Test", Test);
        // routeGroupBuilder.MapGet("/custom-method", CustomMethod);
        routeGroupBuilder.MapPost("/consumer", async (
            [FromServices] DaprClient daprClient,
            HttpContext httpContext
        // [FromBody] string message
        ) =>
        {
            // daprClient.PublishEventAsync("customeronboarding-pubsub", "EFT.IncomingFailure.MASTER");

            // var nfcMobDto = JsonConvert.DeserializeObject<NFCMobDto>(message);
            // Console.WriteLine($"Received message - Type: {nfcMobDto.type}, FullName: {nfcMobDto.data.TEXT_03}, IBAN: {nfcMobDto.data.TEXT_04}");
            Console.WriteLine("test");
        }).WithTopic("kafka-binding","EFT.IncomingFailure.MASTER");

        // routeGroupBuilder.MapGet("/search", SearchMethod);

    }

    // protected async ValueTask Test(
    //     [FromServices] DaprClient daprClient
    // )
    // {
    //     daprClient.PublishEventAsync("customeronboarding-pubsub", "EFT.IncomingFailure.MASTER");
    // }

    protected async ValueTask SearchMethod(
       [FromServices] TemplateDbContext context,
       [FromServices] IMapper mapper,
       [AsParameters] DepositMobApprovalSearch userSearch,
       HttpClient httpClient
   )
    {
        long citizenshipNumber = await context
            .Set<DepositMobApproval>()
            .Where(
                x =>
                    x.Iban.Equals(userSearch.Iban)
                    && x.FullName.Equals(userSearch.FullName)
                    && x.IsMobApproved == false
            ).Select(x => x.CitizenshipNumber).FirstOrDefaultAsync();

        if (citizenshipNumber > 0)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(dodgeGatewayApiServiceUrl);

                var jsonObject = new
                {
                    CitizenshipNumber = citizenshipNumber
                };
                string jsonData = JsonConvert.SerializeObject(jsonObject);

                var content = new StringContent(jsonData, Encoding.UTF8, @"application/json");
                await client.PostAsync(dodgeGatewayApiServiceResourceUrl, content);
            }
        }
    }

    // protected override ValueTask<IResult> UpsertMethod([FromServices] IMapper mapper, [FromServices] FluentValidation.IValidator<Student> validator, [FromServices] TemplateDbContext context, [FromServices] IBBTIdentity bbtIdentity, [FromBody] StudentDTO data, HttpContext httpContext, CancellationToken token)
    // {
    //     return base.UpsertMethod(mapper, validator, context, bbtIdentity, data, httpContext, token);
    // }

    [AddSwaggerParameter("Test Required", ParameterLocation.Header, true)]
    protected async ValueTask<IResult> CustomMethod()
    {
        return Results.Ok();
    }


}
