namespace Deckster.Games;

public abstract class GameCreatedEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = "";
    public DateTimeOffset StartedTime { get; init; } = DateTimeOffset.UtcNow;
    public int InitialSeed { get; init; } = new Random().Next(0, int.MaxValue);
}
