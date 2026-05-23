using SharedCardDomain;
namespace BlackJackDomain;

public class BlackJackPlayer : Player
{
    public List<List<Card>> Hands { get; set; }
    public int Chips { get; set; }
    public BlackJackStats Stats { get; set; }
    public BlackJackPlayer(string name) : base(name)
    {
        Hands = new List<List<Card>>();
        Stats = new BlackJackStats();
        Chips = 100; // Starting chips, can be adjusted as needed
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