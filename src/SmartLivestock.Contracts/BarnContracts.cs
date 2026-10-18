namespace SmartLivestock.Contracts;

public sealed record BarnInput(string Code, string Name, int Capacity, bool Monitored = true);

public sealed record MonitoredInput(IReadOnlyList<Guid> Ids, bool Monitored);

public sealed record BarnDto(Guid Id, string Code, string Name, int Capacity, int DeviceCount, bool Monitored, int AnimalCount);

public sealed record SpeciesCountDto(string Code, string Name, int Count);

public sealed record MonitoredBarnDto(BarnDto Barn, IReadOnlyList<ReadingDto> Readings,
    IReadOnlyList<SpeciesCountDto> Species);

public sealed record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);
