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
        Hands.Where(h => h.IsActive).ToList().ForEach(h => h.Bet += amount); // Place the bet on all active hands
        Chips -= amount; // Deduct the bet from the player's chips
    }

    public void WinBet(bool isBlackJack = false)
    {

        int payout = 0;
        Hands.Where(h => h.IsActive).ToList().ForEach(h => payout = isBlackJack ? (int)(h.Bet * 2.5) : h.Bet * 2); 
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
        Chips -= Hands.Where(h => h.IsActive).Sum(h => h.Bet); // Deduct the total bet from the player's chips
    }

    public void PushBet()
    {
        Chips += Hands.Where(h => h.IsActive).Sum(h => h.Bet); // Return the bet to the player's chips
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

