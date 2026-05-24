using System.Reflection;
using System.Linq;
using NUnit.Framework;
using SharedCardDomain;

namespace CardDomainTests;

[TestFixture]
public class CardTests
{
    [Test]
    public void ToStringTest()
    {
        // Arrange
        Card card = new Card(Suit.Heart, new Rank("Ace"));

        // Act
        string cardString = card.ToString();

        // Assert
        Assert.That(cardString, Is.EqualTo("Ace of Hearts"), "The ToString method should return the correct format.");
    }
}