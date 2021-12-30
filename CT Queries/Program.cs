using Avensia.Excite.Core.Content;
using Avensia.Excite.Core.Hosting;
using BabyWorld.Core.Customers;
using BabyWorld.Core.Voyado;
using BabyWorld.Core.Voyado.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CT_Queries
{
    class Program
    {
        static async Task Main()
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetService<App>().Run(serviceProvider);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(Config.Default)))
                .Build();
            services.AddSingleton(config);
            services.UseCommercetools(config, "Client");
            services.AddExciteVoyado(config);
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<VoyadoContextProviderBase, VoyadoContextProvider>();
            services.AddSingleton<VoyadoOrderService>();
            services.AddSingleton<ISiteLoader, Fake>();
            services.AddSingleton<IContentUrlGenerator, Fake>();
            services.AddTransient<App>();
            return services;
        }
    }

    class Fake : ISiteLoader, IContentUrlGenerator, ISite
    {
        public SiteId SiteId => throw new NotImplementedException();

        public string Url => throw new NotImplementedException();

        public Dictionary<string, SiteDomain> Domains => new Dictionary<string, SiteDomain> { { "test", new SiteDomain { IsPublic = true, Languages = new Dictionary<string, string> { { "sv", "" } } } } };

        public string HeaderScriptContainer => throw new NotImplementedException();

        public string BodyContainer => throw new NotImplementedException();

        public string GoogleMeasureApiEndpoint => throw new NotImplementedException();

        public string GoogleTagManagerSourceUrl => throw new NotImplementedException();

        public string GoogleAnalyticsKey => throw new NotImplementedException();

        public Task<IDictionary<SiteId, IDictionary<CultureInfo, IList<ContentUrl>>>> BuildUncachedUrlsForAllSitesAsync(IContent content)
        {
            throw new NotImplementedException();
        }

        public Task CacheUrlsAsync(IContent content)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ISite>> GetAllAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<ISite> GetAsync(SiteId siteId)
        {
            return new Fake();
        }

        public Task<IList<ISite>> GetAsync(IList<SiteId> siteIds)
        {
            throw new System.NotImplementedException();
        }

        public Task<ISite> GetByDomainAsync(string domain)
        {
            throw new System.NotImplementedException();
        }

        public ISite GetByHttpContext(HttpContext httpContext)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Uri> GetCanonicalUriAsync(IContent content, ISite site, SiteDomain domain, CultureInfo language)
        {
            return new ValueTask<Uri>(new Uri("http://dev.babyworld.se:4000/sv/"));
        }

        public ValueTask<Uri> GetCanonicalUriAsync(IContent content, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetCanonicalUrlAsync(IContent content, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IDictionary<SiteId, IDictionary<CultureInfo, ContentUrl>>> GetCanonicalUrlsForAllSitesAsync(IContent content)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ContentUrl> GetCurrentContentUrlAsync(HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public string GetLanguagePrefix(ISite site, CultureInfo language, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public string GetLanguagePrefix(ISite site, SiteDomain domain, CultureInfo language)
        {
            throw new NotImplementedException();
        }

        public Task<ISite> GetPrimarySiteAsync()
        {
            throw new System.NotImplementedException();
        }

        public ValueTask<Uri> GetSiteUriAsync(ISite site, SiteDomain domain, CultureInfo language)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Uri> GetUriAsync(ContentUrl contentUrl, ISite site, SiteDomain domain, CultureInfo language, bool ignoreLanguageInPath = false)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetUrlAsync(IContent content, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public ValueTask<string> GetUrlAsync(ContentUrl contentUrl, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IList<ContentUrl>> GetUrlsAsync(IContent content, ISite site, CultureInfo language)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IDictionary<CultureInfo, IList<ContentUrl>>> GetUrlsAsync(IContent content, ISite site)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IDictionary<SiteId, IDictionary<CultureInfo, IList<ContentUrl>>>> GetUrlsForAllSitesAsync(IContent content)
        {
            throw new NotImplementedException();
        }

        public bool TryGetByHttpContext(HttpContext httpContext, out ISite site)
        {
            throw new System.NotImplementedException();
        }
    }
}
