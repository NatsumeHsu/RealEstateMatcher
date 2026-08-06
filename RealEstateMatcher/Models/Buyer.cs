namespace RealEstateMatcher.Models;

public sealed class Buyer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string DesiredAddress { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string ExclusionNotes { get; set; } = string.Empty;
    public int? MinBedrooms { get; set; }
    public int? MaxBedrooms { get; set; }
    public int? MinLivingRooms { get; set; }
    public int? MaxLivingRooms { get; set; }
    public int? MinBathrooms { get; set; }
    public int? MaxBathrooms { get; set; }
    public decimal? MinMainArea { get; set; }
    public decimal? MaxMainArea { get; set; }
    public decimal? MinDeedArea { get; set; }
    public decimal? MaxDeedArea { get; set; }
    public int? MinFloor { get; set; }
    public int? MaxFloor { get; set; }
    public int? MinTotalFloors { get; set; }
    public int? MaxTotalFloors { get; set; }
    public decimal? MinBuildingAge { get; set; }
    public decimal? MaxBuildingAge { get; set; }
    public int? MinParkingCount { get; set; }
    public RequirementLevel PetRequirement { get; set; }
    public RequirementLevel ElevatorRequirement { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<HouseType> HouseTypes { get; set; } = [];
    public ICollection<Layout> Layouts { get; set; } = [];
    public ICollection<RequirementOption> Options { get; set; } = [];
}
