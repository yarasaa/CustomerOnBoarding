using System.Collections.Generic;
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
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using Azure.Core;

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
       [FromServices] TemplateDbContext context,

            HttpContext httpContext
        ) =>
        {
            // daprClient.PublishEventAsync("customeronboarding-pubsub", "EFT.IncomingFailure.MASTER");

            using (var reader = new StreamReader(httpContext.Request.Body))
            {
                var body = reader.ReadToEnd();
                var nfcMobDto = JsonConvert.DeserializeObject<NFCMobDto>(body);
                if (nfcMobDto != null && nfcMobDto.message.data != null)
                {
                    long citizenshipNumber = await context
                                                .Set<DepositMobApproval>()
                                                .Where(
                                                    x =>
                                                        x.Iban.Equals(nfcMobDto.message.data.TEXT_04)
                                                        && x.FullName.Equals(nfcMobDto.message.data.TEXT_03)
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
            }

        }).WithTopic("kafka-binding", "EFT.IncomingFailure.MASTER", true);

        // routeGroupBuilder.MapGet("/search", SearchMethod);

    }

    // protected async ValueTask Test(
    //     [FromServices] DaprClient daprClient
    // )
    // {
    //     daprClient.PublishEventAsync("customeronboarding-pubsub", "EFT.IncomingFailure.MASTER");
    // }

    //     private async Task SearchMethod(
    //        [FromServices] TemplateDbContext context,
    //        [FromServices] IMapper mapper,
    //        [AsParameters] DepositMobApprovalSearch userSearch,
    //        HttpClient httpClient
    //    )
    //     {
    //         long citizenshipNumber = await context
    //             .Set<DepositMobApproval>()
    //             .Where(
    //                 x =>
    //                     x.Iban.Equals(userSearch.Iban)
    //                     && x.FullName.Equals(userSearch.FullName)
    //                     && x.IsMobApproved == false
    //             ).Select(x => x.CitizenshipNumber).FirstOrDefaultAsync();

    //         if (citizenshipNumber > 0)
    //         {
    //             using (var client = new HttpClient())
    //             {
    //                 client.BaseAddress = new Uri(dodgeGatewayApiServiceUrl);

    //                 var jsonObject = new
    //                 {
    //                     CitizenshipNumber = citizenshipNumber
    //                 };
    //                 string jsonData = JsonConvert.SerializeObject(jsonObject);

    //                 var content = new StringContent(jsonData, Encoding.UTF8, @"application/json");
    //                 await client.PostAsync(dodgeGatewayApiServiceResourceUrl, content);
    //             }
    //         }
    //     }

}
