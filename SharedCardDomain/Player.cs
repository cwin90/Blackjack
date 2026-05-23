namespace SharedCardDomain;

public class Player
{
    public string Name { get; set; }
    public List<Card> ActiveHand { get; set; }

    public Player(string name)
    {
        Name = name;
        ActiveHand = new List<Card>();
    }
}