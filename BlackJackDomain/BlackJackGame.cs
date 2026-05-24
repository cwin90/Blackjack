using System;
using System.Collections.Generic;
using System.Security;
using System.Xml;
using SharedCardDomain;

namespace BlackJackDomain;



public class BlackJackGame
{
    private readonly IBlackJackUI _ui;
    public BlackJackSession Session { get; private set; }

    int tableMinBet = 5;
    int minPlayers = 1;
    int maxPlayers = 5;
    int maxHandsPerPlayer = 4; // This is to prevent infinite splits and keep the game manageable. In a real casino, this can vary.
    public BlackJackGame(IBlackJackUI ui, BlackJackDeck? deck = null)
    {
        _ui = ui;
        Session = new BlackJackSession(deck ?? new BlackJackDeck(6));
    }

    public string StartGame()
    {
        // Deal two cards to the player and two cards to the dealer
        if(Session.Players.Count == 0)
        {
            _ui.DisplayMessage("No players added. Please add at least one player to start the game.");
            return "No players added.";
        }
        foreach(var player in Session.Players)
        {
            player.Hands.Add(new BlackJackHand()
            {
                IsActive = true
            }); // Start with one hand for each player
            player.Hands[0].Cards.Add(Session.Deck.DrawCard()); //TODO: Change to Active laters
            player.Hands[0].Cards.Add(Session.Deck.DrawCard());
            player.PlaceBet(tableMinBet); // Place minimum bet for each player at the start of the game
        }
        Session.DealerHand.Add(Session.Deck.DrawCard());
        Session.DealerHand.Add(Session.Deck.DrawCard());
        return ShowHands(true); // Show hands with the dealer's hole card hidden
    }

    public void NextHand()
    {
        Session.InactivePlayers.Clear();
        foreach(var player in Session.Players)
        {
            player.Hands = new List<BlackJackHand>(); // Reset to one empty hand
        }
        Session.DealerHand.Clear();
    }

    public int CalculateHandValue(List<Card> hand, out int aceCount)
    {
        int value = 0;
        aceCount = 0;

        foreach (Card card in hand)
        {
            value += Session.Deck.GetValue(card);
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
        Card newCard = Session.Deck.DrawCard();
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
        if (Session.Players.Count >= maxPlayers)
        {
            _ui.DisplayMessage("Maximum number of players reached. Cannot add more players.");
            return;
        }
        Session.Players.Add(new BlackJackPlayer(name));
    }

    public string ShowHands(bool hideDealerHoleCard = true)
    {
        string handOutput = "";
        string dealersCards = "";
        foreach(var player in Session.Players)
        {
            string playerHand = $"Player's {player.Name} Hands:\n";
            int handNumber = 1;
            foreach(var hand in player.Hands)
            {
                playerHand += $"  Hand {handNumber++}: {string.Join(", ", hand)}\n";
            }
            handOutput += playerHand;
        }
        if(hideDealerHoleCard && Session.DealerHand.Count > 0)
        {  
            dealersCards = $"{Session.DealerHand[0].ToString()}, [Hidden Card]";
        }
        else
        {
            dealersCards = string.Join(", ", Session.DealerHand.Select(c => $"{c.ToString()}"));
        }
        handOutput += $"Dealer's Hand: {dealersCards}\n";
        return handOutput;
    }
    public List<string> CalculateGameResults()
    {
        var results = new List<string>();
        int dealerValue = CalculateHandValue(Session.DealerHand, out _);
        foreach(var player in Session.Players.Where(p => !Session.InactivePlayers.Contains(p)))
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
        _ui.DisplayMessage($"You drew: {newCard}");
        _ui.DisplayMessage(ShowHands());
        player.PlaceBet(player.Hands.LastOrDefault(h => h.IsActive)?.Bet ?? 0); // Double the bet
    }

    public void Split(BlackJackPlayer player)
    {
        var activeHand = player.Hands.LastOrDefault(h => h.IsActive);
        if (activeHand == null || activeHand.Cards.Count != 2 || activeHand.Cards[0].Rank.Name != activeHand.Cards[1].Rank.Name || player.Hands.Count >= maxHandsPerPlayer)
        {
            _ui.DisplayMessage("Cannot split. You must have exactly two cards of the same rank to split, and you cannot exceed the maximum number of hands.");
            return;
        }

        // Create a new hand for the split
        BlackJackHand newHand = new BlackJackHand();
        newHand.Cards.Add(activeHand.Cards[1]); // Move the second card to the new hand
        activeHand.Cards.RemoveAt(1); // Remove the second card from the original hand

        // Add a new card to each hand
        activeHand.Cards.Add(Session.Deck.DrawCard());
        newHand.Cards.Add(Session.Deck.DrawCard());
        newHand.Bet = activeHand.Bet; // Match the bet on the new hand
        player.Chips -= newHand.Bet; // Deduct the bet for the new hand from the player's chips
        player.Hands.Add(newHand); // Add the new hand to the player's hands
        //Playout first hand
        _ui.DisplayMessage($"Playing first hand: {string.Join(", ", activeHand.Cards)}");
        PlayHand(player);
        AddLinespace();

    }

    public void ActivateNextHand(BlackJackPlayer player)
    {
        var nextHand = player.Hands.Where(h => !h.IsActive).FirstOrDefault();
        if (nextHand != null)
        {
            nextHand.IsActive = true;
            _ui.DisplayMessage($"Activating next hand for Player {player.Name}: {string.Join(", ", nextHand.Cards)}");
            PlayHand(player);
        }
        else
        {
            _ui.DisplayMessage($"No more hands to activate for Player {player.Name}.");
        }
    }

    public bool CheckForBust(List<Card> hand)
    {
        int handValue = CalculateHandValue(hand, out _);
        if (handValue > 21)
        {
            _ui.DisplayMessage($"Busted with {handValue}! Hand value exceeds 21.");
            return true;
        }
        return false;
    }
    
    public void AddLinespace()
    {
        _ui.DisplayLineSpace();
    }
    public bool CheckForInitialBlackJacks(List<Card>? dealerHand = null, List<BlackJackPlayer>? players = null)
    {
        bool dealerHasBlackJack = CalculateHandValue(dealerHand ?? Session.DealerHand, out _) == 21;
        foreach(var player in players ?? Session.Players)
        {
            var hand = player.Hands.Where(h => h.IsActive).FirstOrDefault();
            bool playerHasBlackJack = CalculateHandValue(hand.Cards, out _) == 21;
            if (dealerHasBlackJack && playerHasBlackJack)
            {
                _ui.DisplayMessage($"Both Dealer and Player {player.Name} have Blackjack! It's a push.");
                player.PushBet(hand);
                Session.InactivePlayers.Add(player); // Mark player as inactive for the rest of the hand since it's a push
            }
            else if (dealerHasBlackJack)
            {
                _ui.DisplayMessage($"Dealer has Blackjack! Player {player.Name} loses.");
                player.LoseBet();
                Session.InactivePlayers.Add(player);
            }
            else if (playerHasBlackJack)
            {
                _ui.DisplayMessage($"Player {player.Name} has Blackjack! Player wins.");
                player.WinBet(hand, true);
                Session.InactivePlayers.Add(player);
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
                    Session.InactivePlayers.Add(player);
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
                    Session.InactivePlayers.Add(player);
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
        _ui.DisplayMessage("Playing hand...");
        while(true)
        {
            var input = _ui.GetInput("\nEnter 'hit' to draw another card or 'double' to double your bet or 'stand' to end your turn:");
            if (input == "stand") break;
            
            ProcessPlayerAction(player, input ?? "");
            ShowPlayersActiveHands(player);

            if (Session.InactivePlayers.Contains(player) || input == "double") break;
        }
        AddLinespace();
        //Update hand or player status
        if (player.Hands.Where(h => !h.IsActive).FirstOrDefault() != null)
        {
            _ui.DisplayMessage($"Ending hand with: {string.Join(", ", player.Hands.Where(h => h.IsActive).LastOrDefault().Cards)}");
            _ui.DisplayMessage($"Player {player.Name} has another hand to play. Activating next hand...");
            ActivateNextHand(player);
        }
        else
        {
            _ui.DisplayMessage($"Player {player.Name} is done for the round.");
        }
    }


    public void ShowPlayersActiveHands(BlackJackPlayer player)
    {
        _ui.DisplayMessage($"Player {player.Name}'s Hand: {string.Join(", ", player.Hands.Where(h => h.IsActive).LastOrDefault().Cards)}");
        AddLinespace();
    }

    public void ExectuteGame()
    {
        Session.Deck = new BlackJackDeck(6); // Reset the deck at the start of the game
        _ui.DisplayMessage("Welcome to Blackjack!");
        string? input = _ui.GetInput("How many players are joining? (Enter a number between 1 and 5)");
        int playerCount = int.Parse(input ?? "1");
        
        for (int i = 1; i <= playerCount; i++)
        {
            string? playerName = _ui.GetInput($"Enter name for Player {i}:");
            AddPlayer(playerName ?? $"Player{i}");
        }
        while (true)
        {
            AddLinespace();
            _ui.DisplayMessage(StartGame());
            AddLinespace();
            //Check for Player Blackjack
            var dealerBlackJackFound = CheckForInitialBlackJacks();
            if(!dealerBlackJackFound)
            {
                _ui.DisplayMessage("No dealer blackjack found. Proceeding with player turns...");

                foreach(var player in Session.Players.Where(p => !Session.InactivePlayers.Contains(p)))
                {
                    _ui.DisplayMessage($"Player {player.Name}'s turn:");
                    PlayHand(player);
                    AddLinespace();
                }

                if (Session.Players.All(p => p.Hands.All(h => h.IsActive && (CalculateHandValue(h.Cards, out _) > 21))))
                {
                    _ui.DisplayMessage("All players have busted. Skipping dealer's turn.");
                }
                else
                {
                    _ui.DisplayMessage("At least one player is still in the game. Proceeding with dealer's turn...");
                    _ui.DisplayMessage("Dealer's turn:");
                    while(true)
                    {
                        int dealerValue = CalculateHandValue(Session.DealerHand, out _);
                        _ui.DisplayMessage($"Dealer's hand value: {dealerValue}");
                        if (dealerValue < 17 || Soft17(Session.DealerHand))
                        {
                            Card newCard = Hit(Session.DealerHand);
                            _ui.DisplayMessage($"Dealer hits and draws: {newCard}");
                            if (CalculateHandValue(Session.DealerHand, out _) > 21)
                            {
                                _ui.DisplayMessage("Dealer busted! Players win.");
                                break;
                            }
                        }
                        else
                        {
                            _ui.DisplayMessage("Dealer stands.");
                            break;
                        }
                    }
                }
            }
            else
            {
                _ui.DisplayMessage("Dealer has Blackjack. Skipping player turns and proceeding to calculate results...");
            }

            CalculateGameResults();
            input = _ui.GetInput("Game over. Enter 'reset' to play another hand or stats to see current player stats, or any other key to exit.");
            if (input == "reset")
            {
                _ui.DisplayMessage("Resetting game...");
                input = _ui.GetInput("Same Players? (Enter 'yes' to keep the same players or any other key to add new players)");
                if (input == "yes")
                {
                    _ui.DisplayMessage("Keeping same players...");
                }
                else
                {
                    input = _ui.GetInput("How many players are joining? (Enter a number between 1 and 5)");
                    playerCount = int.Parse(input ?? "1");
                    Session.Players.Clear();
                    for (int i = 1; i <= playerCount; i++)
                    {
                        string? playerName = _ui.GetInput($"Enter name for Player {i}:");
                        AddPlayer(playerName ?? $"Player{i}");
                    }
                }
                if(Session.Deck.CardsInDeck.Count <= 52 * 3)
                {
                    _ui.DisplayMessage("Deck is running low. Resetting and shuffling new deck...");
                    Session.Deck = new BlackJackDeck(6);
                }
                NextHand();
                _ui.DisplayMessage("Game reset. Starting new hand...");
                _ui.DisplayMessage(ShowHands(true));
            }
            else if(input == "stats")
            {
                foreach(var player in Session.Players)
                {
                    _ui.DisplayMessage($"Player {player.Name} Stats:");
                    _ui.DisplayMessage($"  Chips: {player.Chips}");
                    _ui.DisplayMessage($"  Wins: {player.Stats.Wins}");
                    _ui.DisplayMessage($"  Losses: {player.Stats.Losses}");
                    _ui.DisplayMessage($"  Ties: {player.Stats.Ties}");
                    _ui.DisplayMessage($"  Total Games: {player.Stats.TotalGames}");
                    _ui.DisplayMessage($"  Blackjacks: {player.Stats.BlackJacks}");
                }
                break;
            }
            else
            {
                _ui.DisplayMessage("Thank you for playing! Goodbye.");
                break;
            }
        }
    }
}