namespace bt_tuan4.Models;

public class Invoice
{
    public Guid BookingId { get; set; }
    public Guid InvoiceId { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public string HotelId { get; set; } = string.Empty;
    public decimal RoomCharge { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty; // UNPAID, PAID, REFUNDED
    public DateTime IssuedAt { get; set; }
}
