namespace OidcProxy.Net.Okta.Tests.TestServer
{

    public static class StringExtensions
    {
        public static IResult ToOkResult(this string json)
        {
            return Results.Text(json);
        }
    }
}

