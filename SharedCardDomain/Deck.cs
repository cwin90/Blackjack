namespace SharedCardDomain;

public class Deck
{
    public Stack<Card> CardsInDeck { get; set; }
    
    public Deck()
    {
        CardsInDeck = new Stack<Card>();
    }

    public virtual int GetValue(Card card)
    {
        switch (card.Rank.Name)
        {
            case "Ace":
                return 14; 
            case "Jack":
                return 11;
            case "Queen":
                return 12;
            case "King":
                return 13;
            default:
                return int.Parse(card.Rank.Name);
        }
    }


    public static Deck StandardDeck()
    {
        Deck deck = new Deck();
        Suit[] suits = { Suit.Heart, Suit.Diamond, Suit.Club, Suit.Spade };
        string[] ranks = { "Ace", "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King" };

        foreach (Suit suit in suits)
        {
            foreach (string rank in ranks)
            {
                deck.CardsInDeck.Push(new Card(suit, new Rank(rank)));
            }
        }

        return deck;
    }


    public Deck Shuffle(Deck deck)
    {
        Random rng = new Random();
        List<Card> cardList = deck.CardsInDeck.ToList(); // Convert stack to list for shuffling
        Stack<Card> tempShuffledStack = new Stack<Card>(); // Use a temporary stack to build the shuffled deck

        // Fisher-Yates shuffle variant: pick random from remaining, add to new stack, remove from list
        while (cardList.Count > 0)
        {
            int randomIndex = rng.Next(cardList.Count);
            Card cardToMove = cardList[randomIndex];
            tempShuffledStack.Push(cardToMove);
            cardList.RemoveAt(randomIndex);
        }

        deck.CardsInDeck = tempShuffledStack; // Assign the newly shuffled stack back to the original deck's CardsInDeck.
        return deck; // Return the modified deck instance.
    }

    public Card DrawCard()
    {
        if (!CardsInDeck.Any())
            throw new InvalidOperationException("Deck is empty");
        
        Card card = CardsInDeck.Pop();
        return card;
    }

    public int RemainingCards => CardsInDeck.Count;
    

}
