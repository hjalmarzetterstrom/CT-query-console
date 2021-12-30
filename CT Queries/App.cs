using Avensia.Excite.Commercetools.Subscriptions;
using BabyWorld.Core.Voyado;
using BabyWorld.Core.Customers;
using commercetools.Sdk.Client;
using commercetools.Sdk.Domain;
using commercetools.Sdk.Domain.APIExtensions;
using commercetools.Sdk.Domain.Carts.UpdateActions;
using commercetools.Sdk.Domain.Messages.Customers;
using commercetools.Sdk.Domain.Predicates;
using commercetools.Sdk.Domain.Products.UpdateActions;
using commercetools.Sdk.Domain.ProductTypes.UpdateActions;
using commercetools.Sdk.Domain.Query;
using commercetools.Sdk.Domain.ShoppingLists;
using commercetools.Sdk.Domain.Subscriptions;
using commercetools.Sdk.Domain.Types.UpdateActions;
using commercetools.Sdk.HttpApi.CommandBuilders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using commercetools.Sdk.Domain.Orders;
using commercetools.Sdk.Domain.Messages.Orders;
using commercetools.Sdk.Domain.Messages.Products;
using commercetools.Sdk.Domain.Subscriptions.UpdateActions;

public class App
{
    private readonly IClient _client;
    private readonly IConfigurationRoot _configuration;

    public App(IClient client, IConfigurationRoot configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task Run(IServiceProvider service)
    {
        while (true)
        {
            //var crm = service.GetService<VoyadoContactService>();
            //var customer = await _client.Builder().Customers().GetById("b47c82f8-8f20-4a72-a0a3-ac03a13e1114").ExecuteAsync();

            //var contact = await crm.FindOrCreateMemberWithConsent(customer);

            var orderService = service.GetService<VoyadoOrderService>();
            var expands = new List<Expansion<Order>>
            {
                new ReferenceExpansion<Order>(x => x.LineItems.ExpandAll().Variant.Prices.ExpandAll().Discounted.Discount),
                new ReferenceExpansion<Order>(x => x.LineItems.ExpandAll().DiscountedPricePerQuantity.ExpandAll().DiscountedPrice.IncludedDiscounts.ExpandAll().Discount),
                new ReferenceExpansion<Order>(x => x.PaymentInfo.Payments.ExpandAll()),
            };

            var order = await _client.Builder().Orders().GetById("bbb55c39-388f-429d-af17-d4c121dc8c1b", expands).ExecuteAsync();
            var returnInfo = order.ReturnInfo.First();

            await orderService.PostReturnAsync(order, returnInfo);

            //await CreateSubscription();

            await UpdateSubscription();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

            Console.WriteLine("\r\nPress any key to run query again...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private async Task RemoveProductAttribute()
    {
        var type = await _client.Builder().ProductTypes().GetByKey("excite-package").ExecuteAsync();
        List<UpdateAction<ProductType>> updateActions = new List<UpdateAction<ProductType>>
            {
                new RemoveAttributeDefinitionUpdateAction
                {
                    Name = "productRatings"
                }
            };

        await _client.ExecuteAsync(new UpdateByIdCommand<ProductType>(type, updateActions));
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

    private async Task RemoveLineItem()
    {
        var list = await _client.Builder().Carts().GetById("2e4e5020-a0c8-4a41-8573-ec915bc99e29").ExecuteAsync();

        await _client.Builder().Carts().UpdateById(list).AddAction(new RemoveLineItemUpdateAction
        {
            LineItemId = "cacd2937-d8ba-4f37-bf1d-b6436659408b",
            Quantity = 2,
        }).ExecuteAsync();
    }

    private async Task UpdatePasswordHashAndSalt()
    {
        string customerPreviouslyHashedPassword = "customer-previous-hashed-password";
        string customerPasswordSalt = "customer-password-salt";

        var customer = await _client.Builder().Customers().GetById("80a1a150-d4bc-4149-b200-187a52b51c8b").ExecuteAsync();
        var previouslyHashedPassword = customer.Custom.GetValue<string>(customerPreviouslyHashedPassword);
        var passwordSalt = customer.Custom.GetValue<string>(customerPasswordSalt);

        if (!string.IsNullOrWhiteSpace(previouslyHashedPassword) || !string.IsNullOrWhiteSpace(passwordSalt))
            customer = await _client.Builder().Customers().UpdateById(customer)
                .AddAction(new commercetools.Sdk.Domain.Customers.UpdateActions.SetCustomFieldUpdateAction
                {
                    Name = customerPreviouslyHashedPassword,
                    Value = "",
                })
                .AddAction(new commercetools.Sdk.Domain.Customers.UpdateActions.SetCustomFieldUpdateAction
                {
                    Name = customerPasswordSalt,
                    Value = "",
                }).ExecuteAsync();
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

    private async Task ShowShoppingList()
    {
        var list = await _client.Builder().ShoppingLists().GetByKey("wishlist_cce1728b-05ca-4884-9c71-cca036019e28", new List<Expansion<ShoppingList>>
                {
                    new LineItemExpansion<ShoppingList>(x => x.LineItems.ExpandVariants())
                }).ExecuteAsync();

        list.LineItems.ForEach(x => Console.WriteLine(
            x.Name.TryGetValue("sv-SE", out var name)
                ? name : x.Name.FirstOrDefault().Value
            )
        );
    }

    private async Task ListSubscriptions()
    {
        var existingSubscriptions = await _client.Builder().Subscriptions().Query().ExecuteAsync();

        existingSubscriptions.Results.ForEach(x => Console.WriteLine($"Subscription {x.Id}\r\nChanges:\r\n\t{string.Join("\r\n\t", x.Changes.Select(m => m.ResourceTypeId))}\r\nMessages:\r\n\t{string.Join("\r\n\t", x.Messages.Select(m => m.ResourceTypeId))}\r\n-------------"));
    }

    private async Task DeleteSubscription(string id)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        var subscription = await _client.Builder().Subscriptions().GetById(id).ExecuteAsync();
        _ = await _client.ExecuteAsync(new DeleteByIdCommand<Subscription>(subscription));

        Console.WriteLine("Deleted subscription: " + subscription.Id);
    }

    private async Task CreateSubscription()
    {
        var destination = new AzureServiceBusDestination(_configuration.GetConnectionString("AzureServiceBus"));
        var draft = new SubscriptionDraft
        {
            Destination = destination,
            Messages = new List<MessageSubscription>
            {
                new MessageSubscription
                {
                    ResourceTypeId = ReferenceTypeId.Order.GetDescription(),
                    Types = new List<string>
                    {
                        new ReturnInfoAddedMessage().Type,
                        new DeliveryAddedMessage().Type
                    }
                },
                new MessageSubscription
                {
                    ResourceTypeId = ReferenceTypeId.Product.GetDescription(),
                    Types = new List<string>
                    {
                        new ProductPublishedMessage().Type
                    }
                }
            }
        };
        var response = await _client.Builder().Subscriptions().Create(draft).ExecuteAsync();

        Console.ForegroundColor = ConsoleColor.DarkGreen;

        Console.WriteLine($"Subscription added [{response.Id}] with resouce types: ");
        draft.Messages?.ForEach(x => Console.WriteLine("Message: " + x.ResourceTypeId));
        draft.Changes?.ForEach(x => Console.WriteLine("Change: " + x.ResourceTypeId));

        Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine("\r\nPress enter to delete subscription or close application to keep it.");
        Console.ReadLine();

        await DeleteSubscription(response.Id);
    }

    private async Task UpdateSubscription()
    {
        var action = new SetMessagesUpdateAction
        {
            Messages  = new List<MessageSubscription>
            {
                new MessageSubscription
                {
                    ResourceTypeId = ReferenceTypeId.Order.GetDescription(),
                    Types = new List<string>
                    {
                        new OrderCreatedMessage().Type,
                        new ReturnInfoAddedMessage().Type
                    }
                },
                new MessageSubscription
                {
                    ResourceTypeId = ReferenceTypeId.Product.GetDescription(),
                    Types = new List<string>
                    {
                        new ProductPublishedMessage().Type,
                        new ProductCreatedMessage().Type
                    }
                }
            }
        };

        var subscription = await _client.Builder().Subscriptions().GetById("19312fb4-2d62-483c-9555-05c67338a4fc").ExecuteAsync();
        var response = await _client.Builder().Subscriptions().UpdateById(subscription).AddAction(action).ExecuteAsync();

        Console.WriteLine($"Subscription updated [{response.Id}]: ");
        response.Messages?.ForEach(x => Console.WriteLine("Message: " + x.ResourceTypeId));
        response.Changes?.ForEach(x => Console.WriteLine("Change: " + x.ResourceTypeId));
    }

    private async Task CreateExtension()
    {
        var ngrokUrl = "http://675b-217-115-58-195.ngrok.io";
        var extensionUrl = new Uri(new Uri(ngrokUrl), "/api/commercetools/extensions/excite/customers").AbsoluteUri;

        var extensionDraft = new ExtensionDraft
        {
            Destination = new HttpDestination
            {
                Authentication = new AuthorizationHeader
                {
                    HeaderValue = "localhostApiKey"
                },
                Url = extensionUrl
            },
            Key = "hjz-test-extension",
            TimeoutInMs = 2000,
            Triggers = new List<Trigger>
            {
                new Trigger
                {
                    Actions = new List<TriggerType>
                    {
                        TriggerType.Create
                    },
                    ResourceTypeId = ExtensionResourceType.Customer
                }
            }
        };

        var extension = await _client.ExecuteAsync(new CreateCommand<Extension>(extensionDraft));

        Console.WriteLine($"Extension created with ID {extension.Id}");
        Console.WriteLine($"Press enter to delete extension...");
        Console.ReadLine();

        await _client.ExecuteAsync(extension.DeleteById());
        Console.WriteLine($"Deleted extension with ID {extension.Id}");
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