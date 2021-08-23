using commercetools.Sdk.Client;
using commercetools.Sdk.HttpApi.CommandBuilders;
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
            await AddTestNote();

            Console.ReadKey();
            Console.Clear();
        }
    }

    private async Task AddTestNote()
    {
        var customer = await _client.Builder().Customers().GetById("12332-CUST0M3R-GU1D-ID-ETC").ExecuteAsync();
        await _client.Builder().Customers().UpdateById(customer).AddAction(() =>
            new commercetools.Sdk.Domain.Customers.UpdateActions.SetCustomFieldUpdateAction
            {
                Name = "Notes",
                Value = new[] { "Test note" }
            }
            ).ExecuteAsync();
    }
}