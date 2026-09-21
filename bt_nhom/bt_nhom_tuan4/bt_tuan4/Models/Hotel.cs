namespace bt_tuan4.Models;

public class Hotel
{
    public string HotelId { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? StarRating { get; set; } // null if not available
    public string Phone { get; set; } = string.Empty;
}
