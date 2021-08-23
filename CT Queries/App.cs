using commercetools.Sdk.Client;
using System;
using System.Threading.Tasks;

public class App
{
    private readonly IClient _client;
    public App(IClient client) { _client = client; }

    public async Task Run()
    {
        while (true)
        {

            // Use _client to query CT here.

            Console.ReadKey();
            Console.Clear();
        }
    }
}