using System;
using System.Collections.Generic;
using SharedCardDomain;

namespace BlackJackDomain
{
    public class BlackJackDeck : Deck
    {

        public BlackJackDeck()
        {
            CardsInDeck = StandardDeck().CardsInDeck;
        }

        public BlackJackDeck(int numberOfDecks)
        {
            CardsInDeck = new Stack<Card>();

            for (int i = 0; i < numberOfDecks; i++)
            {
                foreach (Card card in StandardDeck().CardsInDeck)
                {
                    CardsInDeck.Push(card);
                }
            }
            Shuffle(this);
        }

        public override int GetValue(Card card)
        {
            switch (card.Rank.Name)
            {
                case "Ace":
                    return 11; // In Blackjack, Ace can be 1 or 11, but we will treat it as 11 here
                case "Jack":
                case "Queen":
                case "King":
                    return 10; // Face cards are worth 10 in Blackjack
                default:
                    return int.Parse(card.Rank.Name); // Number cards are worth their face value
            }
        }

        public BlackJackDeck ResetDeck(int decksToUse)
        {
            BlackJackDeck newDeck = new BlackJackDeck();
            Stack<Card> combinedCards = new Stack<Card>();

            for (int i = 0; i < decksToUse; i++)
            {
                foreach (Card card in newDeck.CardsInDeck)
                {
                    combinedCards.Push(card);
                }
            }

            this.CardsInDeck = combinedCards;
            this.Shuffle(this);
            return this;
        }

    }
}