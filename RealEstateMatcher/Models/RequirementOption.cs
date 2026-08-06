namespace RealEstateMatcher.Models;

public enum OptionCategory
{
    City, District, ParkingType, Orientation, Decoration, School, Transport
}

public sealed class RequirementOption
{
    public int Id { get; set; }
    public OptionCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Buyer> Buyers { get; set; } = [];
    public ICollection<SellerListing> SellerListings { get; set; } = [];
}

public enum RequirementLevel { NoPreference, Required, Excluded }
public enum AvailabilityStatus { Unknown, No, Yes }
