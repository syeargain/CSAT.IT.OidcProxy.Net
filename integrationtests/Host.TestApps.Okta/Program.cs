using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.Okta;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OktaProxyConfig>();


builder.Services.AddOidcProxy(config);

var app = builder.Build();


app.MapGet("/login/callback", async context =>
{

    Console.WriteLine(new string('-', 100));
    Console.WriteLine($"{context.Request.Path}");
    Console.WriteLine($"{context.Response.StatusCode}");
    Console.WriteLine($"{context.Request.Path}");
    Console.WriteLine(new string('-', 100));
    await Task.CompletedTask;

});

//app.MapGet("/custom/me", async context =>
//{
//    var identity = (ClaimsIdentity)context.User.Identity;
//    //await context.Response.WriteAsJsonAsync(new
//    //{
//    //    Sub = identity.Name,
//    //    Claims = identity.Claims.Select(x => new
//    //    {
//    //        Type = x.Type,
//    //        Value = x.Value
//    //    })
//    });
////})
//    .RequireAuthorization();

//app.UseOidcProxy();

app.Run();