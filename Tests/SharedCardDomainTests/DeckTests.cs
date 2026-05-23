using System.Reflection;
using System.Linq;
using NUnit.Framework;
using SharedCardDomain;
namespace CardDomainTests;

[TestFixture]
public class DeckTests
{
    private Deck _deck;
    
    [SetUp]
    public void Setup()
    {
        _deck = Deck.StandardDeck();
    }

    [Test]
    public void ShuffleTest()
    {
        // Capture the order before shuffling
        var cardsBeforeShuffle = _deck.CardsInDeck.ToList();

        // Act
        _deck.Shuffle(_deck);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_deck.CardsInDeck, Has.Count.EqualTo(52), "A standard deck should have 52 cards.");
            Assert.That(_deck.CardsInDeck, Is.Unique, "The deck should not contain duplicate cards.");
            Assert.That(_deck.CardsInDeck, Is.Not.EqualTo(cardsBeforeShuffle), "The order of cards should change after shuffling.");
        });
    }

    [Test]
    public void DrawCardTest()
    {
        // Act
        Card drawnCard = _deck.DrawCard();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(drawnCard, Is.Not.Null, "Drawing a card should return a card.");
            Assert.That(_deck.CardsInDeck, Has.Count.EqualTo(51), "After drawing one card, the deck should have 51 cards left.");
            Assert.That(_deck.CardsInDeck, Does.Not.Contain(drawnCard), "The drawn card should no longer be in the deck.");
        });
    }

    [Test]
    public void GetValueTest()
    {
        // Arrange
        Card aceOfSpades = new Card(Suit.Spade, new Rank("Ace"));
        Card tenOfHearts = new Card(Suit.Heart, new Rank("10"));
        Card jackOfDiamonds = new Card(Suit.Diamond, new Rank("Jack"));

        // Act
        int aceValue = _deck.GetValue(aceOfSpades);
        int tenValue = _deck.GetValue(tenOfHearts);
        int jackValue = _deck.GetValue(jackOfDiamonds);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(aceValue, Is.EqualTo(14), "Ace should have a value of 14.");
            Assert.That(tenValue, Is.EqualTo(10), "10 should have a value of 10.");
            Assert.That(jackValue, Is.EqualTo(11), "Jack should have a value of 11.");
        });
    }   
    

}
