using commercetools.Sdk.Client;
using commercetools.Sdk.Domain.Predicates;
using commercetools.Sdk.Domain.Query;
using commercetools.Sdk.Domain.ShoppingLists;
using commercetools.Sdk.HttpApi.CommandBuilders;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using commercetools.Sdk.Domain;
using commercetools.Sdk.Domain.Customers;
using commercetools.Sdk.Domain.Customers.UpdateActions;
using System.Globalization;
using commercetools.Sdk.Domain.Carts.UpdateActions;
using commercetools.Sdk.Domain.Products.UpdateActions;
using commercetools.Sdk.Domain.Types.UpdateActions;
using commercetools.Sdk.Domain.Subscriptions;

public class App
{
    private readonly IClient _client;
    public App(IClient client) { _client = client; }

    public async Task Run()
    {
        while (true)
        {

            Console.WriteLine("Press any key to run query again...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private async Task AddTestNote()
    {
        var customer = await _client.Builder().Customers().GetById("207ce853-4816-4c9f-a2f9-d4aaee2d39bd").ExecuteAsync();
        await _client.Builder().Customers().UpdateById(customer).AddAction(() =>
            new commercetools.Sdk.Domain.Customers.UpdateActions.SetCustomFieldUpdateAction
            {
                Name = "Notes",
                Value = new[] { "Test note" }
            }
            ).ExecuteAsync();
    }
}

public static class Extensions
{
    public static TValue GetValue<TValue>(this CustomFields customFields, string fieldName, TValue defaultValue = default)
    {
        var field = customFields?.Fields.SingleOrDefault(f => f.Key == fieldName);
        if (field is null || field.Equals(default(KeyValuePair<string, object>)))
            return defaultValue;

        if (field.Value is TValue value)
            return value;

        if (field.Value.Value is TValue innerValue)
            return innerValue;

        // When you have a FieldSet without any values the commercetools SDK won't know which the generic type is so we need
        // some fiddling to return the correct type
        if (field.Value.Value is FieldSet<object> genericFieldSet && !genericFieldSet.Any() && typeof(TValue).IsGenericType && typeof(FieldSet<>) == typeof(TValue).GetGenericTypeDefinition())
            return Activator.CreateInstance<TValue>();

        try
        {
            return (TValue)Convert.ChangeType(field.Value.Value, typeof(TValue), CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            throw new ArgumentException($"The Commercetools custom field {fieldName} is of type {field.Value.GetType().FullName} but tried to access it as {typeof(TValue).FullName}", e);
        }
    }

    public static LocalizedString ToLocalizedString(this Dictionary<string, string> dictionary)
    {
        if (dictionary is null)
            return null;

        var localizedString = new LocalizedString();
        foreach (var kvp in dictionary)
        {
            localizedString[kvp.Key] = kvp.Value;
        }
        return localizedString;
    }
}