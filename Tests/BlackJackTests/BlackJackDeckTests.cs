namespace BlackJackTests;

using System.Reflection;
using BlackJackDomain;
using NUnit.Framework;
using BlackJackDomain;
using SharedCardDomain;

[TestFixture]
public class BlackJackDeckTests
{

    BlackJackDeck deck;

    [SetUp]
    public void Setup()
    {
        deck = new BlackJackDeck();
    }   

    [Test]
    public void GetValueTest()
    {
        // Arrange
        var aceOfSpades = new Card(Suit.Spade, new Rank("Ace"));
        var tenOfHearts = new Card(Suit.Heart, new Rank("10"));
        var jackOfDiamonds = new Card(Suit.Diamond, new Rank("Jack"));

        // Act
        int aceValue = deck.GetValue(aceOfSpades);
        int tenValue = deck.GetValue(tenOfHearts);
        int jackValue = deck.GetValue(jackOfDiamonds);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(aceValue, Is.EqualTo(11), "Ace should be worth 11 in Blackjack.");
            Assert.That(tenValue, Is.EqualTo(10), "10 should be worth 10 in Blackjack.");
            Assert.That(jackValue, Is.EqualTo(10), "Jack should be worth 10 in Blackjack.");
        });
    }

    [Test]
    public void ResetDeckTest()
    {
        //Act
        var card = deck.DrawCard(); // Draw a card to change the state of the deck
        int cardsBeforeReset = deck.CardsInDeck.Count;

        deck = deck.ResetDeck(1); // Reset the deck to 1 standard deck
        int cardsAfterReset = deck.CardsInDeck.Count;

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(cardsBeforeReset, Is.EqualTo(51), "After drawing one card, there should be 51 cards left in the deck.");
            Assert.That(cardsAfterReset, Is.EqualTo(52), "After resetting the deck, there should be 52 cards in the deck.");
            Assert.That(deck.CardsInDeck, Does.Contain(card), "The drawn card should be in after resetting the deck.");
        });
    }
}
