// See https://aka.ms/new-console-template for more information
using BlackJackDomain;
using System;

Console.WriteLine("Hello, World!");
BlackJackGame game = new BlackJackGame(new ConsoleBlackJackUI());
 // Process command-line arguments
if (args.Length > 0)
{
    Console.WriteLine("Command-line arguments:");
    foreach (var arg in args)
    {
        Console.WriteLine(arg);
    }
}
else
{
    Console.WriteLine("No command-line arguments provided.");
}

string? input;
bool gameInProgress = false;

game.ExectuteGame();