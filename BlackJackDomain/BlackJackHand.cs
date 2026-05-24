namespace BlackJackDomain;

using SharedCardDomain;

public class BlackJackHand
{
    public List<Card> Cards { get; set; }
    public bool IsActive { get; set; }

    public int Bet { get; set; }

    public BlackJackHand()
    {
        Cards = new List<Card>();
        IsActive = true;
        Bet = 0;
    }

    public override string ToString()
    {
        return string.Join(", ", Cards.Select(c => c.ToString()));
    }

}