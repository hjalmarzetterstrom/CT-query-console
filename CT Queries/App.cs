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

    private async Task RemoveCustomField()
    {
        var type = await _client.Builder().Types().GetByKey("cart-custom-type-v1").ExecuteAsync();
        List<UpdateAction<commercetools.Sdk.Domain.Types.Type>> updateActions = new List<UpdateAction<commercetools.Sdk.Domain.Types.Type>>
            {
                new RemoveFieldDefinitionUpdateAction {FieldName = "ingridSelectedMethod"},
            };

        await _client.ExecuteAsync(new UpdateByIdCommand<commercetools.Sdk.Domain.Types.Type>(type, updateActions));
    }

    private async Task AddAssets()
    {
        var product = await _client.Builder().Products().GetById("86224042-c6b7-4b6e-8641-62841941a422").ExecuteAsync();
        var masterVariant = product.MasterData.Current.MasterVariant;

        var actions = new List<UpdateAction<Product>>
            {
                //new RemoveAssetUpdateAction(masterVariant.Sku, assetId: "ba28c05e-78d6-4367-abea-fb95b870aa7e"),
                //new RemoveAssetUpdateAction(masterVariant.Sku, assetId: "e2400856-070b-40e9-8103-166e628263f7")

                new AddAssetUpdateAction(masterVariant.Sku, new AssetDraft
                {
                    Key = product.Key + "_a_1",
                    Name = new Dictionary<string, string> { { "sv-SE", product.Key + " asset 1" } }.ToLocalizedString(),
                    Sources = new List<AssetSource>
                    {
                        new AssetSource
                        {
                            ContentType = "image/jpeg",
                            Dimensions = new AssetDimensions { H = masterVariant.Images[0].Dimensions.H, W = masterVariant.Images[0].Dimensions.W },
                            Key = "Excite_" + product.Key + "_as_1",
                            Uri = masterVariant.Images[0].Url,
                        }
                    },
                }, 0),
                new AddAssetUpdateAction(masterVariant.Sku, new AssetDraft
                {
                    Key = product.Key + "_a_2",
                    Name = new Dictionary<string, string> { { "sv-SE", product.Key + " asset 2" } }.ToLocalizedString(),
                    Sources = new List<AssetSource>
                    {
                        new AssetSource
                        {
                            ContentType = "image/jpeg",
                            Dimensions = new AssetDimensions { H = masterVariant.Images[1].Dimensions.H, W = masterVariant.Images[1].Dimensions.W },
                            Key = "Excite_" + product.Key + "_as_2",
                            Uri = masterVariant.Images[1].Url,
                        }
                    },
                }, 1),
            };


        var update = _client.Builder().Products().UpdateById(product);
        actions.ForEach(x => update.AddAction(x));

        var result = await update.ExecuteAsync();
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