﻿namespace SharedCardDomain;

public class Card
{
    public Suit Suit { get; }
    public Rank Rank { get; }

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString()
    {
        return $"{Rank.Name} of {Suit.Name}";
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        Card other = (Card)obj;
        // Compare Suit and Rank for equality
        return Suit.Equals(other.Suit) && Rank.Equals(other.Rank);
    }

    public override int GetHashCode()
    {
        // Combine the hash codes of Suit and Rank
        return HashCode.Combine(Suit, Rank);
    }
}

public class Rank
{
    public string Name { get; }

    public Rank(string name)
    {
        Name = name;
    }

    // Override Equals and GetHashCode for Rank to compare by Name
    public override bool Equals(object obj) => obj is Rank other && Name == other.Name;
    public override int GetHashCode() => Name.GetHashCode();
}

public class Suit
{
    public string Name { get; }
    public static readonly Suit Diamond = new("Diamond");
    public static readonly Suit Club = new("Club");
    public static readonly Suit Heart = new("Heart");
    public static readonly Suit Spade = new("Spade");

    private Suit(string name)
    {
        Name = name;
    }

    // Suit instances are static readonly, so reference equality works.
    // No need to override Equals/GetHashCode here unless you want to compare by Name for some reason.
}