using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bt_tuan4.Models;
using bt_tuan4.Repositories;

namespace bt_tuan4.Services
{
    public class HotelService
    {
        private readonly IAstraRepository _repo;
        public HotelService(IAstraRepository repo)
        {
            _repo = repo;
        }

        public Task<Hotel?> GetHotelAsync(string hotelId) => _repo.GetHotelAsync(hotelId);
        public Task<IReadOnlyList<Room>> GetRoomsAsync(string hotelId) => _repo.GetRoomsByHotelAsync(hotelId);
        public Task<IReadOnlyList<BookingGuest>> GetGuestBookingsAsync(string guestId) => _repo.GetBookingsByGuestAsync(guestId);
        public Task<IReadOnlyList<BookingHotelDate>> GetHotelBookingsByDateAsync(string hotelId, DateTime start, DateTime end) => _repo.GetBookingsByHotelDateAsync(hotelId, start, end);
        public Task<Invoice?> GetInvoiceAsync(Guid bookingId) => _repo.GetInvoiceByBookingAsync(bookingId);
        
        public Task<IReadOnlyList<Hotel>> GetAllHotelsAsync() => _repo.GetAllHotelsAsync();
        public Task<IReadOnlyList<Room>> GetAllRoomsAsync() => _repo.GetAllRoomsAsync();
        public Task<IReadOnlyList<BookingGuest>> GetAllGuestBookingsAsync() => _repo.GetAllBookingsByGuestAsync();
        public Task<IReadOnlyList<BookingHotelDate>> GetAllHotelBookingsByDateAsync() => _repo.GetAllBookingsByHotelDateAsync();
        public Task<IReadOnlyList<Invoice>> GetAllInvoicesAsync() => _repo.GetAllInvoicesAsync();

        public Task UpsertHotelAsync(Hotel hotel) => _repo.UpsertHotelAsync(hotel);
        public Task DeleteHotelAsync(string hotelId) => _repo.DeleteHotelAsync(hotelId);
        public Task UpsertRoomAsync(Room room) => _repo.UpsertRoomAsync(room);
        public Task DeleteRoomAsync(string hotelId, int roomNumber) => _repo.DeleteRoomAsync(hotelId, roomNumber);
        public Task UpsertBookingGuestAsync(BookingGuest b) => _repo.UpsertBookingGuestAsync(b);
        public Task DeleteBookingGuestAsync(string guestId, DateTime checkInDate, Guid bookingId) => _repo.DeleteBookingGuestAsync(guestId, checkInDate, bookingId);
        public Task UpsertBookingHotelDateAsync(BookingHotelDate b) => _repo.UpsertBookingHotelDateAsync(b);
        public Task DeleteBookingHotelDateAsync(string hotelId, DateTime checkInDate, Guid bookingId) => _repo.DeleteBookingHotelDateAsync(hotelId, checkInDate, bookingId);
        public Task UpsertInvoiceAsync(Invoice inv) => _repo.UpsertInvoiceAsync(inv);
        public Task DeleteInvoiceAsync(Guid bookingId) => _repo.DeleteInvoiceAsync(bookingId);
    }
}
