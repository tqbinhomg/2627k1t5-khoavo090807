namespace bt_tuan4.Models;

public class Room
{
    public string HotelId { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string Status { get; set; } = string.Empty; // AVAILABLE, OCCUPIED, MAINTENANCE
}
