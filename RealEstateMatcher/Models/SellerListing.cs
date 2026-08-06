namespace RealEstateMatcher.Models;

public sealed class SellerListing
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string LineId { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string ExclusionNotes { get; set; } = string.Empty;
    public int? Bedrooms { get; set; }
    public int? LivingRooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? MainArea { get; set; }
    public decimal? DeedArea { get; set; }
    public int? Floor { get; set; }
    public int? TotalFloors { get; set; }
    public decimal? BuildingAge { get; set; }
    public int? ParkingCount { get; set; }
    public AvailabilityStatus PetsAllowed { get; set; }
    public AvailabilityStatus HasElevator { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public ICollection<HouseType> HouseTypes { get; set; } = [];
    public ICollection<Layout> Layouts { get; set; } = [];
    public ICollection<RequirementOption> Options { get; set; } = [];
}
