using SharedCardDomain;
namespace BlackJackDomain;

public class BlackJackPlayer : Player
{
    public List<BlackJackHand> Hands { get; set; }
    public int Chips { get; set; }
    public BlackJackStats Stats { get; set; }
    public BlackJackPlayer(string name) : base(name)
    {
        Hands = new List<BlackJackHand>();
        Stats = new BlackJackStats();
        Chips = 100; // Starting chips, can be adjusted as needed
    }

    public void PlaceBet(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Bet amount cannot be negative.");
        }
        if (amount > Chips)
        {
            throw new InvalidOperationException("Cannot bet more chips than you have.");
        }
        Hands.Where(h => h.IsActive).LastOrDefault()?.Bet = amount; // Set the bet for the active hand
        Chips -= amount; // Deduct the bet from the player's chips
    }

    public void WinBet(BlackJackHand hand, bool isBlackJack = false)
    {

        int payout = 0;
        if (isBlackJack)
        {
            payout = (int)(hand.Bet * 2.5); // Blackjack pays 3:2
        }
        else
        {
            payout = hand.Bet * 2; // Regular win pays 1:1
        }
        Chips += payout; // Add the winnings to the player's chips
        Stats.RecordWin();
        if(isBlackJack)
        {
            Stats.BlackJacks++;
        }
    }

    public void LoseBet()
    {
        Stats.RecordLoss();
    }

    public void PushBet(BlackJackHand handToPush)
    {
        Chips += handToPush.Bet; // Return the bet to the player's chips
        Stats.RecordTie();
    }
}

public class BlackJackStats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public int TotalGames => Wins + Losses + Ties;
    public int BlackJacks { get; set; }

    public void RecordWin() => Wins++;
    public void RecordLoss() => Losses++;
    public void RecordTie() => Ties++;
}

