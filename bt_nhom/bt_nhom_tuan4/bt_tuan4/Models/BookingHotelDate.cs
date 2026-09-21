namespace bt_tuan4.Models;

public class BookingHotelDate
{
    public string HotelId { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public Guid BookingId { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
