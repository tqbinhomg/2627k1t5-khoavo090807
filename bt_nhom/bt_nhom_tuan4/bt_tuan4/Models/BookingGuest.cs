namespace bt_tuan4.Models;

public class BookingGuest
{
    public string GuestId { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public Guid BookingId { get; set; }
    public string HotelId { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public int RoomNumber { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty; // CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED
}
