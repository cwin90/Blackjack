using System.Collections.Generic;
using BlackJackDomain;

namespace BlackJackTests;

public class MockBlackJackUI : IBlackJackUI
{
    public Queue<string?> InputQueue { get; } = new Queue<string?>();
    public List<string> DisplayedMessages { get; } = new List<string>();
    public bool ClearCalled { get; private set; }
    public int LineSpacesCalled { get; private set; }

    public void DisplayMessage(string message)
    {
        DisplayedMessages.Add(message);
    }

    public string? GetInput(string? prompt = null)
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            DisplayedMessages.Add(prompt);
        }

        return InputQueue.Count > 0 ? InputQueue.Dequeue() : null;
    }

    public void DisplayLineSpace()
    {
        LineSpacesCalled++;
    }

    public void Clear()
    {
        ClearCalled = true;
    }
}