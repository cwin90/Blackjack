namespace BlackJackTests;

using System.Reflection;
using BlackJackDomain;
using NUnit.Framework;
using SharedCardDomain;

[TestFixture]
public class BlackJackGameTests
{
    BlackJackGame game;
    [SetUp]
    public void Setup()
    {
        game = new BlackJackGame();
    }

    [Test]
    public void Soft17Test()
    {
        // Arrange
        var playerHand = new List<Card>();
        playerHand.Add(new Card(Suit.Heart, new Rank("Ace")));
        playerHand.Add(new Card(Suit.Club, new Rank("6")));

        var dealerHand = new List<Card>();
        dealerHand.Add(new Card(Suit.Diamond, new Rank("Ace")));
        dealerHand.Add(new Card(Suit.Spade, new Rank("6")));
        dealerHand.Add(new Card(Suit.Heart, new Rank("10")));

        //Act
        bool shouldHit = game.Soft17(playerHand);

        bool shouldStand = game.Soft17(dealerHand);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(shouldHit, Is.True, "Player hand with Ace and 6 should be considered a soft 17 and should hit.");
            Assert.That(shouldStand, Is.False, "Dealer hand with Ace, 6, and 10 should not be considered a soft 17 and should stand.");
        });
    }

}