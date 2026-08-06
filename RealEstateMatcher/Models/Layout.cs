namespace RealEstateMatcher.Models;

public sealed class Layout
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Buyer> Buyers { get; set; } = [];
    public ICollection<SellerListing> SellerListings { get; set; } = [];
}
