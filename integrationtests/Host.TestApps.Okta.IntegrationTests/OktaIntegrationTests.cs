using Host.TestApps.Okta.IntegrationTests.Pom;
using PuppeteerSharp.Input;

namespace Host.TestApps.Okta.IntegrationTests
{
    public class OktaIntegrationTests : IClassFixture<Host.TestApps.Okta.IntegrationTests.HostApplicaiton>
    {
        #region "Do Not Look"
        //fix this
        private string testUserId = "scott.yeargain@gmail.com";
        private string testPassword = "@ut0G1mp!";
        #endregion

        //hitting bff login should redirect to okta site defined in config
        //clicking login with correct creds should trigger callback with authcode
        //callback exchanges auth code for token  providing pkce
        //callback saves tokem in session
        //callback parses token
        //calls on authenticated , passing access token claims.

        [Fact]
        public async Task ItShouldIntegrateWithOkta()
        {
            var app = new App();

            try
            {
                var res = await app.NavigateToProxy();
                var rchain = res.Request.RedirectChain;
                await Task.Delay(1000);

                //var url = app.IdSvrLoginPage.url
                var idInput = app.IdSvrLoginPage.UserIdInput();
                await idInput.TypeAsync(testUserId, new TypeOptions { Delay = 100 });

                var pwdInput = app.IdSvrLoginPage.PasswordInput();
                await pwdInput.TypeAsync(testPassword, new TypeOptions { Delay = 100 });
                await Task.Delay(1000);

                var loginButton = app.IdSvrLoginPage.LoginButton();
                await loginButton.ClickAsync();
                var respNav = await app.WaitForNavigationAsync();

                var chain2 = respNav.Request.RedirectChain;
                await Task.Delay(1000);


                //await Task.Delay(1000);

                //var contents = app.CurrentPage.Text;
                //await app.NavigateToMe();




                // Test ASP.NET Core authorization pipeline
                //await app.GoTo("/custom/me");
                // await Task.Delay(1000);

                //   app.CurrentPage.Text.Should().Contain(subYoda);

                // Assert the user was logged in
                //await app.GoTo("/api/echo");
                //await Task.Delay(1000);

                //app.CurrentPage.Text.Should().Contain("Bearer ey");

                //// Log out
                //await app.GoTo("/.auth/end-session");
                //await Task.Delay(1000);

                //await app.IdSvrSignOutPage.BtnYes().ClickAsync();
                //await app.WaitForNavigationAsync();

                //// Assert user details flushed
                //await app.GoTo("/.auth/me");
                //await Task.Delay(1000);

                //app.CurrentPage.Text.Should().NotContain(subYoda);

                //// Assert token removed
                //await app.GoTo("/api/echo");
                //await Task.Delay(1000);

                //app.CurrentPage.Text.Should().NotContain("Bearer ey");
            }
            finally
            {
                await app.CloseBrowser();
            }
        }

    }
}
