using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            await serviceProvider.GetService<App>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(Config.Default)))
                .Build();
            services.AddSingleton(config);
            services.UseCommercetools(config, "Client");
            services.AddTransient<App>();
            return services;
        }
    }
}
