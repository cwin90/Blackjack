using System;
using System.Collections.Generic;
using System.Security;
using System.Xml;
using SharedCardDomain;

namespace BlackJackDomain;



public class BlackJackGame
{
    public BlackJackDeck Deck { get; set; }
    public List<BlackJackPlayer> Players { get; set; }

    public List<Card> DealerHand { get; set; }
    int minPlayers = 1;
    int maxPlayers = 5;
    public BlackJackGame()
    {
        Deck = new BlackJackDeck(6); // Using 6 decks for a more realistic casino experience
        DealerHand = new List<Card>();
        Players = new List<BlackJackPlayer>();
    }

    public string StartGame()
    {
        // Deal two cards to the player and two cards to the dealer
        if(Players.Count == 0)
        {
            Console.WriteLine("No players added. Please add at least one player to start the game.");
            return "No players added.";
        }
        foreach(var player in Players)
        {
            player.Hands.Add(new List<Card>()); // Start with one hand for each player
            player.Hands[0].Add(Deck.DrawCard());
            player.Hands[0].Add(Deck.DrawCard());
        }
        DealerHand.Add(Deck.DrawCard());
        DealerHand.Add(Deck.DrawCard());
        return ShowHands(true); // Show hands with the dealer's hole card hidden
    }

    public void NextHand()
    {
        foreach(var player in Players)
        {
            player.Hands = new List<List<Card>>(); // Reset to one empty hand
            player.ActiveHand.Clear();
        }
        DealerHand.Clear();
    }

    public int CalculateHandValue(List<Card> hand, out int aceCount)
    {
        int value = 0;
        aceCount = 0;

        foreach (Card card in hand)
        {
            value += Deck.GetValue(card);
            if (card.Rank.Name == "Ace")
            {
                aceCount++;
            }
        }

        // Adjust for Aces if total value exceeds 21
        while (value > 21 && aceCount > 0)
        {
            value -= 10; // Treat one Ace as 1 instead of 11
            aceCount--;
        }

        return value;
    }

    public enum GameResult
    {
        PlayerWins,
        DealerWins,
        PlayerBlackJack,
        DealerBlackJack,
        DealerBust,
        PlayerBust,
        Push
    }

    public Card Hit(List<Card> playerHand)
    {
        Card newCard = Deck.DrawCard();
        playerHand.Add(newCard);
        return newCard;
    }

    public bool Soft17(List<Card> hand)
    {
        int dealerValue = CalculateHandValue(hand, out int aceCount);
        bool hasAce = aceCount > 0;

        return dealerValue == 17 && hasAce;
    }
    
    public void AddPlayer(string name)
    {
        if (Players.Count >= maxPlayers)
        {
            Console.WriteLine("Maximum number of players reached. Cannot add more players.");
            return;
        }
        Players.Add(new BlackJackPlayer(name));
    }
    
    public string ShowHands(bool hideDealerHoleCard = true)
    {
        string handOutput = "";
        string dealersCards = "";
        foreach(var player in Players)
        {
            string playerHand = $"Player's {player.Name} Hands:\n";
            int handNumber = 1;
            foreach(var hand in player.Hands)
            {
                playerHand += $"  Hand {handNumber++}: {string.Join(", ", hand)}\n";
            }
            handOutput += playerHand;
        }
        if(hideDealerHoleCard && DealerHand.Count > 0)
        {  
            dealersCards = $"{DealerHand[0].ToString()}, [Hidden Card]";
        }
        else
        {
            dealersCards = string.Join(", ", DealerHand.Select(c => $"{c.ToString()}"));
        }
        handOutput += $"Dealer's Hand: {dealersCards}\n";
        return handOutput;
    }
    public void CalculateGameResults()
    {
        int dealerValue = CalculateHandValue(DealerHand, out _);
        foreach(var player in Players)
        {
            foreach(var hand in player.Hands)
            {
                int playerValue = CalculateHandValue(hand, out _);
                if (playerValue > 21)
                {
                    Console.WriteLine($"Player {player.Name} busts with {playerValue}. Dealer wins.");
                }
                else if (dealerValue > 21)
                {
                    Console.WriteLine($"Dealer busts with {dealerValue}. Player {player.Name} wins.");
                }
                else if (playerValue > dealerValue)
                {
                    Console.WriteLine($"Player {player.Name} wins with {playerValue} against dealer's {dealerValue}.");
                }
                else if (playerValue < dealerValue)
                {
                    Console.WriteLine($"Dealer wins against Player {player.Name} with {dealerValue} against player's {playerValue}.");
                }
                else
                {
                    Console.WriteLine($"Push between Player {player.Name} and Dealer with both at {playerValue}.");
                }
            }
        }
    }

    public void ExectuteGame()
    {
        Console.WriteLine("Welcome to Blackjack!");
        Console.WriteLine("How many players are joining? (Enter a number between 1 and 5)");
        string? input = Console.ReadLine();
        int playerCount = int.Parse(input ?? "1");
        for (int i = 1; i <= playerCount; i++)
        {
            Console.WriteLine($"Enter name for Player {i}:");
            string? playerName = Console.ReadLine();
            AddPlayer(playerName ?? $"Player{i}");
        }
        while (true)
        {
            
            Console.WriteLine(StartGame());

            //Check for Dealer Blackjack
            if (CalculateHandValue(DealerHand, out _) == 21)
            {
                Console.WriteLine("Dealer has Blackjack! Dealer wins.");
                continue;
            }
            else
            {
                Console.WriteLine("Dealer does not have Blackjack.");
                foreach(var player in Players)
                {
                    Console.WriteLine($"Player {player.Name}'s turn:");
                    while(true)
                    {
                        Console.WriteLine("\nEnter 'hit' to draw another card or 'stand' to end your turn:");
                        input = Console.ReadLine();

                        if (input == "hit")
                        {
                            Card newCard = Hit(player.Hands[0]);
                            Console.WriteLine($"You drew: {newCard}");
                            Console.WriteLine(ShowHands());
                            if (CalculateHandValue(player.Hands[0], out _) > 21)
                            {
                                Console.WriteLine("You busted! Dealer wins.");
                                break;
                            }
                        }
                        else if (input == "stand")
                        {
                            Console.WriteLine("You chose to stand. Your turn is over.");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Unknown command. Please enter 'hit', 'stand', or 'reset'.");
                        }
                    }
                }
            }

            //Dealer's turn logic would go here (not implemented in this snippet)
            Console.WriteLine("Dealer's turn:");

            //If Every Player Busts, skip dealer's turn and calculate results immediately
            if (Players.All(p => CalculateHandValue(p.Hands[0], out _) > 21))
            {
                Console.WriteLine("All players have busted. Skipping dealer's turn.");
            }
            else
            {
                while(true)
                {
                    int dealerValue = CalculateHandValue(DealerHand, out _);
                    Console.WriteLine($"Dealer's hand value: {dealerValue}");
                    if (dealerValue < 17 || Soft17(DealerHand))
                    {
                        Card newCard = Hit(DealerHand);
                        Console.WriteLine($"Dealer hits and draws: {newCard}");
                        if (CalculateHandValue(DealerHand, out _) > 21)
                        {
                            Console.WriteLine("Dealer busted! Players win.");
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Dealer stands.");
                        break;
                    }
                }
            }

            CalculateGameResults();
            Console.WriteLine("Game over. Enter 'reset' to play another hand or any other key to exit.");
            input = Console.ReadLine();
            if (input == "reset")
            {
                Console.WriteLine("Resetting game...");
                Console.WriteLine("Same Players? (Enter 'yes' to keep the same players or any other key to add new players)");
                input = Console.ReadLine();
                if (input == "yes")
                {
                    Console.WriteLine("Keeping same players...");
                }
                else
                {
                    Console.WriteLine("How many players are joining? (Enter a number between 1 and 5)");
                    input = Console.ReadLine();
                    playerCount = int.Parse(input ?? "1");
                    Players.Clear();
                    for (int i = 1; i <= playerCount; i++)
                    {
                        Console.WriteLine($"Enter name for Player {i}:");
                        string? playerName = Console.ReadLine();
                        AddPlayer(playerName ?? $"Player{i}");
                    }
                }
                NextHand();
                Console.WriteLine("Game reset. Starting new hand...");
                Console.WriteLine(ShowHands(true));
            }
            else
            {
                Console.WriteLine("Thank you for playing! Goodbye.");
                break;
            }
        }
    }
}