﻿using PuppeteerSharp;

namespace Host.TestApps.Okta.IntegrationTests.Pom
{
    public class IdSvrSignOutPage
    {
        private readonly IPage _page;

        public IdSvrSignOutPage(IPage page)
        {
            _page = page;
        }

        public IElementHandle BtnYes()
        {
            IElementHandle rv = _page.QuerySelectorAsync("#yes").GetAwaiter().GetResult();
            return rv;

        }
    }
}
