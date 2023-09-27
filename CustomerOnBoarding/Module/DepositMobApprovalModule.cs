using amorphie.template.core.Model;
using amorphie.core.Module.minimal_api;
using amorphie.template.data;
using Microsoft.AspNetCore.Mvc;
using amorphie.template.core.DTO;
using Dapr.Client;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace amorphie.template.Module;


public sealed class DepositMobApprovalModule : BaseBBTRoute<DepositMobApprovalDto, DepositMobApproval, TemplateDbContext>
{
    private readonly string dodgeGatewayApiServiceUrl;
    private readonly string dodgeGatewayApiServiceResourceUrl;
    private readonly string dodgeGatewayApiTokenUrl;
    private readonly string dodgeGatewayApiToken_ClientId;
    private readonly string dodgeGatewayApiToken_ClientSecret;
    private readonly string dodgeGatewayApiToken_GrantType;
    public DepositMobApprovalModule(WebApplication app)
        : base(app)
    {

        dodgeGatewayApiServiceUrl = app.Configuration["DodgeGatewayApiServiceUrl"];
        dodgeGatewayApiServiceResourceUrl = app.Configuration["DodgeGatewayApiServiceResourceUrl"];
        dodgeGatewayApiTokenUrl = app.Configuration["DodgeGatewayApiTokenUrl"];
        dodgeGatewayApiToken_ClientId = app.Configuration["DodgeGatewayApiToken_ClientId"];
        dodgeGatewayApiToken_ClientSecret = app.Configuration["DodgeGatewayApiToken_ClientSecret"];
        dodgeGatewayApiToken_GrantType = app.Configuration["DodgeGatewayApiToken_GrantType"];
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
                            string tokenContent = string.Format("grant_type={0}&client_secret={1}&client_id={2}", dodgeGatewayApiToken_GrantType, dodgeGatewayApiToken_ClientSecret, dodgeGatewayApiToken_ClientId);

                            var httpRequest = new HttpRequestMessage();
                            httpRequest.Method = HttpMethod.Post;
                            httpRequest.RequestUri = new Uri(dodgeGatewayApiTokenUrl);
                            httpRequest.Content = new StringContent(tokenContent);
                            httpRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

                            var responseToken = await client.SendAsync(httpRequest);

                            if (responseToken.StatusCode == HttpStatusCode.OK)
                            {

                                var json = await responseToken.Content.ReadAsStringAsync();
                                var token = JsonConvert.DeserializeObject<TokenResponse>(json);

                                using (var clientGateWay = new HttpClient())
                                {
                                    clientGateWay.BaseAddress = new Uri(dodgeGatewayApiTokenUrl);

                                    clientGateWay.DefaultRequestHeaders.Add("Authorization", token.TokenType + " " + token.AccessToken);

                                    // var jsonObject = new
                                    // {
                                    //     CitizenshipNumber = citizenshipNumber
                                    // };
                                    var parameters = new Dictionary<string, string> { { "citizenshipNumber", citizenshipNumber.ToString() } };
                                    var encodedContent = new FormUrlEncodedContent(parameters);
                                    // string jsonData = JsonConvert.SerializeObject(jsonObject);

                                    // var content = new StringContent(jsonData, Encoding.UTF8, @"application/json");
                                    await clientGateWay.PostAsync(dodgeGatewayApiServiceResourceUrl, encodedContent);
                                }

                            }
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
