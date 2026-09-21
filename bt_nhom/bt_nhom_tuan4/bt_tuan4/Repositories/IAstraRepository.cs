using System.Collections.Generic;
using System.Threading.Tasks;
using bt_tuan4.Models;

namespace bt_tuan4.Repositories
{
    public interface IAstraRepository
    {
        Task<Hotel?> GetHotelAsync(string hotelId);
        Task<IReadOnlyList<Room>> GetRoomsByHotelAsync(string hotelId);
        Task<IReadOnlyList<BookingGuest>> GetBookingsByGuestAsync(string guestId);
        Task<IReadOnlyList<BookingHotelDate>> GetBookingsByHotelDateAsync(string hotelId, DateTime startDate, DateTime endDate);
        Task<Invoice?> GetInvoiceByBookingAsync(Guid bookingId);
        Task<IReadOnlyList<Hotel>> GetAllHotelsAsync();
        Task<IReadOnlyList<Room>> GetAllRoomsAsync();
        Task<IReadOnlyList<BookingGuest>> GetAllBookingsByGuestAsync();
        Task<IReadOnlyList<BookingHotelDate>> GetAllBookingsByHotelDateAsync();
        Task<IReadOnlyList<Invoice>> GetAllInvoicesAsync();

        // CRUD Hotels
        Task UpsertHotelAsync(Hotel hotel);
        Task DeleteHotelAsync(string hotelId);

        // CRUD Rooms
        Task UpsertRoomAsync(Room room);
        Task DeleteRoomAsync(string hotelId, int roomNumber);

        // CRUD BookingGuest
        Task UpsertBookingGuestAsync(BookingGuest booking);
        Task DeleteBookingGuestAsync(string guestId, DateTime checkInDate, Guid bookingId);

        // CRUD BookingHotelDate
        Task UpsertBookingHotelDateAsync(BookingHotelDate booking);
        Task DeleteBookingHotelDateAsync(string hotelId, DateTime checkInDate, Guid bookingId);

        // CRUD Invoice
        Task UpsertInvoiceAsync(Invoice invoice);
        Task DeleteInvoiceAsync(Guid bookingId);
    }
}
