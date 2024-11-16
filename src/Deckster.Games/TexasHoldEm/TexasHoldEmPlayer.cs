using Deckster.Core.Games.Common;
using System.Numerics;

namespace Deckster.Games.TexasHoldEm;

public class TexasHoldEmPlayer
{
    public TexasHoldEmPlayer(Guid id, string name, int stackSize)
    {
        Id = id;
        Name = name;
        StackSize = stackSize;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    public List<Card> Cards { get; init; } = [];

    public static readonly TexasHoldEmPlayer Null = new(Guid.Empty, "Ing. Kognito", 0);

    public int StackSize { get; private set; }
    public int CurrentRoundBet { get; private set; }
    public bool IsAllIn { get; private set; }
    public bool HasFolded { get; private set; }
    public bool HasChecked { get; private set; }
    public bool IsEliminated { get; private set; }

    public int HandScore() => new TexasHoldEmHandRating().HandRating(Cards);

    public void Check()
    {
        HasChecked = true;
    }

    public void Fold()
    {
        HasFolded = true;
    }

    public void Bet(int betSize)
    {
        if (StackSize > betSize)
        {
            StackSize -= betSize;
            CurrentRoundBet += betSize;
        }
        else
        {
            CurrentRoundBet += StackSize;
            StackSize = 0;
            IsAllIn = true;
        }
    }

    public void Call(int maxBet)
    {
        var bet = maxBet - CurrentRoundBet;
        Bet(bet);
    }

    public bool IsStillInRound() => !IsEliminated && !HasFolded;
    public void ResetRound(List<Card> cards, int blindSize)
    {
        CurrentRoundBet = 0;
        HasFolded = false;
        HasChecked = false;
        IsAllIn = false;
        if (StackSize < blindSize)
        {
            IsEliminated = true;
        }

        Cards.Clear();
        Cards.AddRange(cards);
    }

    public void GiveWinnings(int winnings)
    {
        StackSize += winnings;
    }
}
