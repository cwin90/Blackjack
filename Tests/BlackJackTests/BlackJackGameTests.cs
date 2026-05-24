namespace BlackJackTests;

using System.Reflection;
using BlackJackDomain;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharedCardDomain;

[TestFixture]
public class BlackJackGameTests
{
    BlackJackGame game;
    MockBlackJackUI mockUI;

    [SetUp]
    public void Setup()
    {
        mockUI = new MockBlackJackUI();
        game = new BlackJackGame(mockUI);
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

    [Test]
    public void CalculateHandValueTest()
    {
        // Arrange
        var hand = new List<Card>
        {
            new Card(Suit.Heart, new Rank("10")),
            new Card(Suit.Club, new Rank("5"))
        };

        // Act
        int value = game.CalculateHandValue(hand, out int aceCount);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(value, Is.EqualTo(15));
            Assert.That(aceCount, Is.EqualTo(0));
        });
    }

    [Test]
    public void PlayHand_PlayerHitsUntilBust_UpdatesPlayerStatusAndDisplaysMessage()
    {
        // Arrange
        var player = new BlackJackPlayer("BustPlayer");
        var hand = new BlackJackHand { IsActive = true, Bet = 10 };
        hand.Cards.Add(new Card(Suit.Heart, new Rank("10")));
        hand.Cards.Add(new Card(Suit.Club, new Rank("10")));
        player.Hands.Add(hand);
        game.Session.Players.Add(player);

        // Rig the deck: next card is a 5, making the hand value 25 (Bust)
        game.Session.Deck.CardsInDeck.Clear();
        game.Session.Deck.CardsInDeck.Push(new Card(Suit.Spade, new Rank("5")));

        // Simulate the user inputting 'hit'
        mockUI.InputQueue.Enqueue("hit");

        // Act
        game.PlayHand(player);

        // Assert
        Assert.Multiple(() =>
        {
            int finalValue = game.CalculateHandValue(hand.Cards, out _);
            Assert.That(finalValue, Is.EqualTo(25), "Hand value should be 25.");
            Assert.That(game.Session.InactivePlayers, Contains.Item(player), "Player should be moved to the inactive list after busting.");
            Assert.That(mockUI.DisplayedMessages.Any(m => m.Contains("Busted")), Is.True, "A bust message should be displayed to the UI.");
        });
    }
}