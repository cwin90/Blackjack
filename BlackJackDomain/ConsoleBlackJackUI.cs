using System;

namespace BlackJackDomain;

public class ConsoleBlackJackUI : IBlackJackUI
{
    public void DisplayMessage(string message)
    {
        Console.WriteLine(message);
    }

    public string? GetInput(string? prompt = null)
    {
        if (!string.IsNullOrEmpty(prompt))
            Console.WriteLine(prompt);
        return Console.ReadLine();
    }

    public void DisplayLineSpace()
    {
        Console.WriteLine("\n-----------------------------\n");
    }

    public void Clear()
    {
        Console.Clear();
    }
}