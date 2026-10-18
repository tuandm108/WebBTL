using SmartLivestock.Contracts;

namespace SmartLivestock.UnitTests;

public class LivestockSpeciesTests
{
    [Fact]
    public void Catalog_has_distinct_species_codes()
    {
        Assert.Equal(12, LivestockSpecies.All.Count);
        Assert.Equal(LivestockSpecies.All.Count,
            LivestockSpecies.All.Select(x => x.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData(" HeO ", "pig", "Heo (lợn)")]
    [InlineData("lợn", "pig", "Heo (lợn)")]
    [InlineData("CATTLE", "cattle", "Bò")]
    [InlineData("bò", "cattle", "Bò")]
    public void Legacy_names_resolve_to_catalog_codes(string input, string code, string label)
    {
        Assert.Equal(code, LivestockSpecies.Normalize(input));
        Assert.Equal(label, LivestockSpecies.NameOf(input));
    }

    [Fact]
    public void Unknown_legacy_species_keeps_its_label()
    {
        Assert.Null(LivestockSpecies.Normalize("Đà điểu"));
        Assert.Equal("Đà điểu", LivestockSpecies.NameOf("Đà điểu"));
    }
}
