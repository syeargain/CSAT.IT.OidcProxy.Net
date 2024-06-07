using PuppeteerSharp;

namespace Host.TestApps.Okta.IntegrationTests.Pom
{
    public class App
    {
        private IPage? _page;
        private IBrowser? _browser;

        private const string BaseAddress = "https://localhost:7114";

        public async Task<IResponse> NavigateToProxy()
        {
            using var browserFetcher = new BrowserFetcher(SupportedBrowser.Chrome);
            await browserFetcher.DownloadAsync();

            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                Devtools = true,
                IgnoreHTTPSErrors = true,
                Args = new[] { "--no-sandbox" }
            });
            _page = await _browser.NewPageAsync();

            //await _page.SetRequestInterceptionAsync(true);

            //_page.RequestFinished += (sender, e) =>
            //{
            //    Console.WriteLine(new string('-', 100));
            //    Console.WriteLine($"Request: {e.Request.Method} {e.Request.Url}");
            //    foreach (var header in e.Request.Headers)
            //    {
            //        Console.WriteLine($"{header.Key}: {header.Value}");
            //    }
            //    Console.WriteLine(new string('-', 100));
            //};
            //_page.DefaultNavigationTimeout = 0;
            //_page.DefaultTimeout = 0;
            await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3419.0 Safari/537.36");

            return await _page.GoToAsync($"{BaseAddress}/.auth/login");
        }
        public async Task<string> CurrentUrl()
        {
            return await Task.FromResult(_page.Url);

        }
        public async Task NavigateToMe()
        {
            await _page.GoToAsync($"{BaseAddress}/.auth/me");
        }


        public async Task GoTo(string uri)
        {
            await _page!.GoToAsync($"{BaseAddress}{uri}");
        }

        public async Task<IResponse> WaitForNavigationAsync()
        {
            var iresp = await _page!.WaitForNavigationAsync();
            await Task.Delay(500);
            return iresp;
        }

        public async Task CloseBrowser()
        {
            await _page!.CloseAsync();
            await _browser!.CloseAsync();
        }

        public IdSvrLoginPage IdSvrLoginPage => new(_page!);
        public IdSvrSignOutPage IdSvrSignOutPage => new(_page!);

        public Endpoint CurrentPage => new(_page!);
    }
}
