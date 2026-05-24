namespace BlackJackDomain;

public interface IBlackJackUI
{
    void DisplayMessage(string message);
    string? GetInput(string? prompt = null);
    void DisplayLineSpace();
    void Clear();
}