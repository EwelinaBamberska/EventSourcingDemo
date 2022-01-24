using EventSourcingDemo;
using System;
using System.Linq;

public record ShoppingCardInitialized (
    Guid ShoppingCardId,
    Guid ClientId,
    ShoppingCardStatus ShoppingCardStatus
);

public record ProductAddedToShoppingCard (
    Guid ShoppingCardId,
    PricedProductItem Product
);

public record ProductItemRemovedFromShoppingCard (
    Guid ShoppingCardId,
    PricedProductItem Product
);

public record ShoppingCardConfirmed (
    Guid ShoppingCardId,
    DateTime confirmedAt
);

public enum ShoppingCardStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}

public record ShoppingCard (
    Guid Id,
    Guid ClientId,
    ShoppingCardStatus Status,
    ProductItemsList ProductItems,
    DateTime? ConfirmedAt = null
)
{
    public static ShoppingCard When( ShoppingCard? entity, object @event )
    {
        return @event switch
        {
            ShoppingCardInitialized( var cardId, var clientId, var cardStatus ) =>
            new ShoppingCard(cardId, clientId, cardStatus, ProductItemsList.Empty()),

            ProductAddedToShoppingCard(_, var product) => 
            entity! with
            {
                ProductItems = entity.ProductItems.Add(product)
            },

            ProductItemRemovedFromShoppingCard(_, var product) =>
            entity! with
            {
                ProductItems = entity.ProductItems.Remove(product)
            },

            ShoppingCardConfirmed(_, var confirmedAt) =>
            entity! with
            {
                Status = ShoppingCardStatus.Confirmed,
                ConfirmedAt = confirmedAt
            },
            _ => entity!
        };
    }
}



namespace EventSourcingDemo
{
    class EventSourcing
    {
        static void Main(string[] args)
        {
            var clientId = Guid.NewGuid();
            var shoppingCardId = Guid.NewGuid();

            var shoes = new PricedProductItem(new ProductItem(Guid.NewGuid(), 1), 100);
            var shirt = new PricedProductItem(new ProductItem(Guid.NewGuid(), 2), 150);

            var events = new object[]
            {
                new ShoppingCardInitialized( shoppingCardId, clientId, ShoppingCardStatus.Pending ),
                new ProductAddedToShoppingCard( shoppingCardId, shoes),      
                new ProductAddedToShoppingCard( shoppingCardId, shirt),
                new ProductItemRemovedFromShoppingCard(shoppingCardId, shoes),
                new ShoppingCardConfirmed(shoppingCardId, DateTime.Now)
            };


            var shoppingCard = events.Aggregate(default(ShoppingCard), ShoppingCard.When)!;

            Console.Write(shoppingCard);
        }
    }
}
