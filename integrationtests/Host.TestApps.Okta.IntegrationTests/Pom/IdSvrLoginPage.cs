using PuppeteerSharp;

namespace Host.TestApps.Okta.IntegrationTests.Pom
{
    public class IdSvrLoginPage
    {
        private readonly IPage _page;

        public IdSvrLoginPage(IPage page)
        {
            _page = page;
            //System.Diagnostics.Deb ""should be ablke 


        }


        public IElementHandle UserIdInput()
        {
            return _page.QuerySelectorAsync("input[type=text][autocomplete=username]").GetAwaiter().GetResult();
        }

        public IElementHandle PasswordInput()
        {
            return _page.QuerySelectorAsync(".password-with-toggle").GetAwaiter().GetResult();
        }
        public IElementHandle LoginButton()
        {
            IElementHandle rv = _page.QuerySelectorAsync(".button-primary").GetAwaiter().GetResult();
            return rv;
        }




    }
}
