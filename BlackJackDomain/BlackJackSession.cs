using System.Collections.Generic;
using SharedCardDomain;

namespace BlackJackDomain;

public class BlackJackSession
{
    public BlackJackDeck Deck { get; set; }
    public List<BlackJackPlayer> Players { get; set; } = new();
    public List<BlackJackPlayer> InactivePlayers { get; set; } = new();
    public List<Card> DealerHand { get; set; } = new();

    public BlackJackSession(BlackJackDeck deck)
    {
        Deck = deck;
    }
}