namespace SmartLivestock.Contracts;

public sealed record AnimalInput(string Tag, string? Name, string Species, Guid BarnId);

public sealed record AnimalDto(Guid Id, string Tag, string Name, string Species, Guid BarnId,
    string BarnCode, DateTimeOffset RegisteredAt, bool IsDemo);
