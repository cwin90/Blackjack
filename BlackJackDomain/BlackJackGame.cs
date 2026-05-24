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
    public List<BlackJackPlayer> InactivePlayers { get; set; }

    public List<Card> DealerHand { get; set; }
    int tableMinBet = 5;
    int minPlayers = 1;
    int maxPlayers = 5;
    int maxHandsPerPlayer = 4; // This is to prevent infinite splits and keep the game manageable. In a real casino, this can vary.
    public BlackJackGame(BlackJackDeck? deck = null)
    {
        Deck = deck ?? new BlackJackDeck(6); // Allow injecting a deck for testing
        DealerHand = new List<Card>();
        Players = new List<BlackJackPlayer>();
        InactivePlayers = new List<BlackJackPlayer>();
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
            player.Hands.Add(new BlackJackHand()
            {
                IsActive = true
            }); // Start with one hand for each player
            player.Hands[0].Cards.Add(Deck.DrawCard()); //TODO: Change to Active laters
            player.Hands[0].Cards.Add(Deck.DrawCard());
            player.PlaceBet(tableMinBet); // Place minimum bet for each player at the start of the game
        }
        DealerHand.Add(Deck.DrawCard());
        DealerHand.Add(Deck.DrawCard());
        return ShowHands(true); // Show hands with the dealer's hole card hidden
    }

    public void NextHand()
    {
        InactivePlayers.Clear();
        foreach(var player in Players)
        {
            player.Hands = new List<BlackJackHand>(); // Reset to one empty hand
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
    public List<string> CalculateGameResults()
    {
        var results = new List<string>();
        int dealerValue = CalculateHandValue(DealerHand, out _);
        foreach(var player in Players.Where(p => !InactivePlayers.Contains(p)))
        {
            foreach(var hand in player.Hands)
            {
                int playerValue = CalculateHandValue(hand.Cards, out _);
                if(dealerValue > 21)
                {
                    results.Add($"Dealer busted! Player {player.Name} wins with {playerValue}.");
                    player.WinBet(hand);
                }
                 else if(playerValue > 21)
                {
                    results.Add($"Player {player.Name} busted! Dealer wins with {dealerValue} against player's {playerValue}.");
                    player.LoseBet();
                }
                else if (playerValue > dealerValue)
                {
                    results.Add($"Player {player.Name} wins with {playerValue} against dealer's {dealerValue}.");
                    player.WinBet(hand);
                }
                else if (playerValue < dealerValue)
                {
                    results.Add($"Dealer wins against Player {player.Name} with {dealerValue} against player's {playerValue}.");
                    player.LoseBet();
                }
                else
                {
                    results.Add($"Push between Player {player.Name} and Dealer with both at {playerValue}.");
                    player.PushBet(hand);
                }
            }
        }
        return results;
    }
    public void DoubleUp(BlackJackPlayer player)
    {
        Card newCard = Hit(player.Hands.LastOrDefault(h => h.IsActive).Cards); // Double the bet on the active hand
        Console.WriteLine($"You drew: {newCard}");
        Console.WriteLine(ShowHands());
        player.PlaceBet(player.Hands.LastOrDefault(h => h.IsActive)?.Bet ?? 0); // Double the bet
    }

    public void Split(BlackJackPlayer player)
    {
        var activeHand = player.Hands.LastOrDefault(h => h.IsActive);
        if (activeHand == null || activeHand.Cards.Count != 2 || activeHand.Cards[0].Rank.Name != activeHand.Cards[1].Rank.Name || player.Hands.Count >= maxHandsPerPlayer)
        {
            Console.WriteLine("Cannot split. You must have exactly two cards of the same rank to split, and you cannot exceed the maximum number of hands.");
            return;
        }

        // Create a new hand for the split
        BlackJackHand newHand = new BlackJackHand();
        newHand.Cards.Add(activeHand.Cards[1]); // Move the second card to the new hand
        activeHand.Cards.RemoveAt(1); // Remove the second card from the original hand

        // Add a new card to each hand
        activeHand.Cards.Add(Deck.DrawCard());
        newHand.Cards.Add(Deck.DrawCard());
        newHand.Bet = activeHand.Bet; // Match the bet on the new hand
        player.Chips -= newHand.Bet; // Deduct the bet for the new hand from the player's chips
        player.Hands.Add(newHand); // Add the new hand to the player's hands
        //Playout first hand
        Console.WriteLine($"Playing first hand: {string.Join(", ", activeHand.Cards)}");
        PlayHand(player);
        AddLinespace();

    }

    public void ActivateNextHand(BlackJackPlayer player)
    {
        var nextHand = player.Hands.Where(h => !h.IsActive).FirstOrDefault();
        if (nextHand != null)
        {
            nextHand.IsActive = true;
            Console.WriteLine($"Activating next hand for Player {player.Name}: {string.Join(", ", nextHand.Cards)}");
            PlayHand(player);
        }
        else
        {
            Console.WriteLine($"No more hands to activate for Player {player.Name}.");
        }
    }

    public bool CheckForBust(List<Card> hand)
    {
        int handValue = CalculateHandValue(hand, out _);
        if (handValue > 21)
        {
            Console.WriteLine($"Busted with {handValue}! Hand value exceeds 21.");
            return true;
        }
        return false;
    }
    
    public void AddLinespace()
    {
        Console.WriteLine("\n-----------------------------\n");
    }
    public bool CheckForInitialBlackJacks(List<Card>? dealerHand = null, List<BlackJackPlayer>? players = null)
    {
        bool dealerHasBlackJack = CalculateHandValue(dealerHand ?? DealerHand, out _) == 21;
        foreach(var player in players ?? Players)
        {
            var hand = player.Hands.Where(h => h.IsActive).FirstOrDefault();
            bool playerHasBlackJack = CalculateHandValue(hand.Cards, out _) == 21;
            if (dealerHasBlackJack && playerHasBlackJack)
            {
                Console.WriteLine($"Both Dealer and Player {player.Name} have Blackjack! It's a push.");
                player.PushBet(hand);
                InactivePlayers.Add(player); // Mark player as inactive for the rest of the hand since it's a push
            }
            else if (dealerHasBlackJack)
            {
                Console.WriteLine($"Dealer has Blackjack! Player {player.Name} loses.");
                player.LoseBet();
                InactivePlayers.Add(player);
            }
            else if (playerHasBlackJack)
            {
                Console.WriteLine($"Player {player.Name} has Blackjack! Player wins.");
                player.WinBet(hand, true);
                InactivePlayers.Add(player);
            }
        }
        return dealerHasBlackJack;
    }

    //Assume Last Active hand is always the hand being played for simplicity. In a more complex implementation, we would want to track which hand is currently active for the player.
    //Assume if a player has no more inactive hands, they are done for the round and we can move on to the next player or dealer turn.
    public void ProcessPlayerAction(BlackJackPlayer player, string action)
    {
        if (action == "hit")
        {
            Card newCard = Hit(player.Hands.Where(h => h.IsActive).LastOrDefault().Cards);
            if (CheckForBust(player.Hands.Where(h => h.IsActive).LastOrDefault().Cards))
            {
                player.LoseBet();
                if (player.Hands.Where(h => !h.IsActive).FirstOrDefault() == null)
                {
                    InactivePlayers.Add(player);
                }
            }
        }
        else if (action == "double")
        {
            DoubleUp(player);
            if (CheckForBust(player.Hands.Where(h => h.IsActive).LastOrDefault().Cards))
            {
                player.LoseBet();
                if (player.Hands.Where(h => !h.IsActive).FirstOrDefault() == null)
                {
                    InactivePlayers.Add(player);
                }
            }
        }
        else if (action == "split")
        {
            Split(player);
        }
    }

    public void PlayHand(BlackJackPlayer player)
    {
        Console.WriteLine("Playing hand...");
        while(true)
        {
            Console.WriteLine("\nEnter 'hit' to draw another card or 'double' to double your bet or 'stand' to end your turn:");
            var input = Console.ReadLine();
            if (input == "stand") break;
            
            ProcessPlayerAction(player, input ?? "");
            ShowPlayersActiveHands(player);

            if (InactivePlayers.Contains(player) || input == "double") break;
        }
        AddLinespace();
        //Update hand or player status
        if (player.Hands.Where(h => !h.IsActive).FirstOrDefault() != null)
        {
            Console.WriteLine($"Ending hand with: {string.Join(", ", player.Hands.Where(h => h.IsActive).LastOrDefault().Cards)}");
            Console.WriteLine($"Player {player.Name} has another hand to play. Activating next hand...");
            ActivateNextHand(player);
        }
        else
        {
            Console.WriteLine($"Player {player.Name} is done for the round.");
        }
    }


    public void ShowPlayersActiveHands(BlackJackPlayer player)
    {
        Console.WriteLine($"Player {player.Name}'s Hand: {string.Join(", ", player.Hands.Where(h => h.IsActive).LastOrDefault().Cards)}");
        AddLinespace();
    }

    public void ExectuteGame()
    {
        Console.WriteLine("Welcome to Blackjack!");
        Console.WriteLine("How many players are joining? (Enter a number between 1 and 5)");
        string? input = Console.ReadLine();
        int playerCount = int.Parse(input ?? "1");
        Deck = new BlackJackDeck(6); // Reset the deck at the start of the game
        for (int i = 1; i <= playerCount; i++)
        {
            Console.WriteLine($"Enter name for Player {i}:");
            string? playerName = Console.ReadLine();
            AddPlayer(playerName ?? $"Player{i}");
        }
        while (true)
        {
            AddLinespace();
            Console.WriteLine(StartGame());
            AddLinespace();
            //Check for Player Blackjack
            var dealerBlackJackFound = CheckForInitialBlackJacks();
            if(!dealerBlackJackFound)
            {
                Console.WriteLine("No dealer blackjack found. Proceeding with player turns...");

                foreach(var player in Players.Where(p => !InactivePlayers.Contains(p)))
                {
                    Console.WriteLine($"Player {player.Name}'s turn:");
                    PlayHand(player);
                    AddLinespace();
                }

                if (Players.All(p => p.Hands.All(h => h.IsActive && (CalculateHandValue(h.Cards, out _) > 21))))
                {
                    Console.WriteLine("All players have busted. Skipping dealer's turn.");
                }
                else
                {
                    Console.WriteLine("At least one player is still in the game. Proceeding with dealer's turn...");
                    Console.WriteLine("Dealer's turn:");
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
            }
            else
            {
                Console.WriteLine("Dealer has Blackjack. Skipping player turns and proceeding to calculate results...");
            }

            CalculateGameResults();
            Console.WriteLine("Game over. Enter 'reset' to play another hand or stats to see current player stats, or any other key to exit.");
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
            else if(input == "stats")
            {
                foreach(var player in Players)
                {
                    Console.WriteLine($"Player {player.Name} Stats:");
                    Console.WriteLine($"  Chips: {player.Chips}");
                    Console.WriteLine($"  Wins: {player.Stats.Wins}");
                    Console.WriteLine($"  Losses: {player.Stats.Losses}");
                    Console.WriteLine($"  Ties: {player.Stats.Ties}");
                    Console.WriteLine($"  Total Games: {player.Stats.TotalGames}");
                    Console.WriteLine($"  Blackjacks: {player.Stats.BlackJacks}");
                }
                break;
            }
            else
            {
                Console.WriteLine("Thank you for playing! Goodbye.");
                break;
            }
        }
    }
}