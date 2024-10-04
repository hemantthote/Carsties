using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Entities;

[Table("Items")]
public class Auction
{
    public Guid Id { get; set; }
    public int ReservePrice { get; set; } = 0;
    public string Seller { get; set; } // username from claim
    public string Winner { get; set; } // username of winner, nullable
    public int? SoldAmount { get; set; } // nullable
    public int? CurrentHighBid { get; set; } // nullable
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime AuctionEnd { get; set; }
    public Status Status { get; set; } = Status.Live;
    public Item Item { get; set; }
}
