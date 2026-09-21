using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using bt_tuan4.Models;

namespace bt_tuan4.Repositories
{
    public class AstraRepository : IAstraRepository
    {
        private readonly Cassandra.ISession _session;
        public AstraRepository(Cassandra.ISession session)
        {
            _session = session;
        }

        public async Task<Hotel?> GetHotelAsync(string hotelId)
        {
            var stmt = _session.Prepare("SELECT hotel_id, hotel_name, city, address, star_rating, phone FROM hotels WHERE hotel_id = ?");
            var rs = await _session.ExecuteAsync(stmt.Bind(hotelId));
            var row = rs.FirstOrDefault();
            if (row == null) return null;
            return new Hotel
            {
                HotelId = row.GetValue<string>("hotel_id"),
                HotelName = row.GetValue<string>("hotel_name"),
                City = row.GetValue<string>("city"),
                Address = row.GetValue<string>("address"),
                StarRating = row.GetValue<int>("star_rating"),
                Phone = row.GetValue<string>("phone")
            };
        }

        public async Task<IReadOnlyList<Room>> GetRoomsByHotelAsync(string hotelId)
        {
            var stmt = _session.Prepare("SELECT hotel_id, room_number, room_type, price_per_night, status FROM rooms_by_hotel WHERE hotel_id = ?");
            var rs = await _session.ExecuteAsync(stmt.Bind(hotelId));
            var list = rs.Select(row => new Room
            {
                HotelId = row.GetValue<string>("hotel_id"),
                RoomNumber = row.GetValue<int>("room_number"),
                RoomType = row.GetValue<string>("room_type"),
                PricePerNight = row.GetValue<decimal>("price_per_night"),
                Status = row.GetValue<string>("status")
            }).ToList();
            return list;
        }

        private static DateTime GetDateFromRow(Row row, string columnName)
        {
            var localDate = row.GetValue<LocalDate>(columnName);
            return localDate != null ? new DateTime(localDate.Year, localDate.Month, localDate.Day) : DateTime.MinValue;
        }

        public async Task<IReadOnlyList<BookingGuest>> GetBookingsByGuestAsync(string guestId)
        {
            var stmt = _session.Prepare("SELECT guest_id, check_in_date, booking_id, hotel_id, hotel_name, room_number, check_out_date, total_amount, status FROM bookings_by_guest WHERE guest_id = ? ORDER BY check_in_date DESC, booking_id DESC");
            var rs = await _session.ExecuteAsync(stmt.Bind(guestId));
            var list = rs.Select(row => new BookingGuest
            {
                GuestId = row.GetValue<string>("guest_id"),
                CheckInDate = GetDateFromRow(row, "check_in_date"),
                BookingId = row.GetValue<Guid>("booking_id"),
                HotelId = row.GetValue<string>("hotel_id"),
                HotelName = row.GetValue<string>("hotel_name"),
                RoomNumber = row.GetValue<int>("room_number"),
                CheckOutDate = GetDateFromRow(row, "check_out_date"),
                TotalAmount = row.GetValue<decimal>("total_amount"),
                Status = row.GetValue<string>("status")
            }).ToList();
            return list;
        }

        public async Task<IReadOnlyList<BookingHotelDate>> GetBookingsByHotelDateAsync(string hotelId, DateTime startDate, DateTime endDate)
        {
            var stmt = _session.Prepare("SELECT hotel_id, check_in_date, booking_id, guest_id, guest_name, room_number, check_out_date, total_amount, status FROM bookings_by_hotel_date WHERE hotel_id = ? AND check_in_date >= ? AND check_in_date <= ? ORDER BY check_in_date ASC, booking_id ASC");
            var startLocalDate = new LocalDate(startDate.Year, startDate.Month, startDate.Day);
            var endLocalDate = new LocalDate(endDate.Year, endDate.Month, endDate.Day);
            var rs = await _session.ExecuteAsync(stmt.Bind(hotelId, startLocalDate, endLocalDate));
            var list = rs.Select(row => new BookingHotelDate
            {
                HotelId = row.GetValue<string>("hotel_id"),
                CheckInDate = GetDateFromRow(row, "check_in_date"),
                BookingId = row.GetValue<Guid>("booking_id"),
                GuestId = row.GetValue<string>("guest_id"),
                GuestName = row.GetValue<string>("guest_name"),
                RoomNumber = row.GetValue<int>("room_number"),
                CheckOutDate = GetDateFromRow(row, "check_out_date"),
                TotalAmount = row.GetValue<decimal>("total_amount"),
                Status = row.GetValue<string>("status")
            }).ToList();
            return list;
        }

        public async Task<Invoice?> GetInvoiceByBookingAsync(Guid bookingId)
        {
            var stmt = _session.Prepare("SELECT booking_id, invoice_id, guest_id, hotel_id, room_charge, service_charge, tax, total_amount, payment_status, issued_at FROM invoices_by_booking WHERE booking_id = ?");
            var rs = await _session.ExecuteAsync(stmt.Bind(bookingId));
            var row = rs.FirstOrDefault();
            if (row == null) return null;
            return new Invoice
            {
                BookingId = row.GetValue<Guid>("booking_id"),
                InvoiceId = row.GetValue<Guid>("invoice_id"),
                GuestId = row.GetValue<string>("guest_id"),
                HotelId = row.GetValue<string>("hotel_id"),
                RoomCharge = row.GetValue<decimal>("room_charge"),
                ServiceCharge = row.GetValue<decimal>("service_charge"),
                Tax = row.GetValue<decimal>("tax"),
                TotalAmount = row.GetValue<decimal>("total_amount"),
                PaymentStatus = row.GetValue<string>("payment_status"),
                IssuedAt = row.GetValue<DateTime>("issued_at")
            };
        }
        public async Task<IReadOnlyList<Hotel>> GetAllHotelsAsync()
        {
            var stmt = _session.Prepare("SELECT hotel_id, hotel_name, city, address, star_rating, phone FROM hotels");
            var rs = await _session.ExecuteAsync(stmt.Bind());
            var list = rs.Select(row => new Hotel
            {
                HotelId = row.GetValue<string>("hotel_id"),
                HotelName = row.GetValue<string>("hotel_name"),
                City = row.GetValue<string>("city"),
                Address = row.GetValue<string>("address"),
                StarRating = row.GetValue<int>("star_rating"),
                Phone = row.GetValue<string>("phone")
            }).ToList();
            return list;
        }

        public async Task<IReadOnlyList<Room>> GetAllRoomsAsync()
        {
            var allHotels = await GetAllHotelsAsync();
            var allRooms = new List<Room>();
            
            foreach (var hotel in allHotels)
            {
                try
                {
                    var rooms = await GetRoomsByHotelAsync(hotel.HotelId);
                    allRooms.AddRange(rooms);
                }
                catch
                {
                }
            }
            
            return allRooms;
        }

        public async Task<IReadOnlyList<BookingGuest>> GetAllBookingsByGuestAsync()
        {
            var stmt = _session.Prepare("SELECT guest_id, check_in_date, booking_id, hotel_id, hotel_name, room_number, check_out_date, total_amount, status FROM bookings_by_guest");
            var rs = await _session.ExecuteAsync(stmt.Bind());
            var list = rs.Select(row => new BookingGuest
            {
                GuestId = row.GetValue<string>("guest_id"),
                CheckInDate = GetDateFromRow(row, "check_in_date"),
                BookingId = row.GetValue<Guid>("booking_id"),
                HotelId = row.GetValue<string>("hotel_id"),
                HotelName = row.GetValue<string>("hotel_name"),
                RoomNumber = row.GetValue<int>("room_number"),
                CheckOutDate = GetDateFromRow(row, "check_out_date"),
                TotalAmount = row.GetValue<decimal>("total_amount"),
                Status = row.GetValue<string>("status")
            }).ToList();
            return list;
        }

        public async Task<IReadOnlyList<BookingHotelDate>> GetAllBookingsByHotelDateAsync()
        {
            var stmt = _session.Prepare("SELECT hotel_id, check_in_date, booking_id, guest_id, guest_name, room_number, check_out_date, total_amount, status FROM bookings_by_hotel_date");
            var rs = await _session.ExecuteAsync(stmt.Bind());
            var list = rs.Select(row => new BookingHotelDate
            {
                HotelId = row.GetValue<string>("hotel_id"),
                CheckInDate = GetDateFromRow(row, "check_in_date"),
                BookingId = row.GetValue<Guid>("booking_id"),
                GuestId = row.GetValue<string>("guest_id"),
                GuestName = row.GetValue<string>("guest_name"),
                RoomNumber = row.GetValue<int>("room_number"),
                CheckOutDate = GetDateFromRow(row, "check_out_date"),
                TotalAmount = row.GetValue<decimal>("total_amount"),
                Status = row.GetValue<string>("status")
            }).ToList();
            return list;
        }

        public async Task<IReadOnlyList<Invoice>> GetAllInvoicesAsync()
        {
            var allInvoices = new List<Invoice>();
            var allHotels = await GetAllHotelsAsync();
            foreach (var hotel in allHotels)
            {
                try
                {
                    var bookingStmt = _session.Prepare("SELECT hotel_id, check_in_date, booking_id FROM bookings_by_hotel_date WHERE hotel_id = ?");
                    var rs = await _session.ExecuteAsync(bookingStmt.Bind(hotel.HotelId));
                    var seenBookings = new HashSet<Guid>();
                    foreach (var row in rs)
                    {
                        var bookingId = row.GetValue<Guid>("booking_id");
                        if (seenBookings.Add(bookingId))
                        {
                            var invoice = await GetInvoiceByBookingAsync(bookingId);
                            if (invoice != null) allInvoices.Add(invoice);
                        }
                    }
                }
                catch { }
            }
            return allInvoices;
        }

        public async Task UpsertHotelAsync(Hotel hotel)
        {
            var stmt = _session.Prepare("INSERT INTO hotels (hotel_id, hotel_name, city, address, star_rating, phone) VALUES (?, ?, ?, ?, ?, ?)");
            await _session.ExecuteAsync(stmt.Bind(hotel.HotelId, hotel.HotelName, hotel.City, hotel.Address, hotel.StarRating, hotel.Phone));
        }

        public async Task DeleteHotelAsync(string hotelId)
        {
            var stmt = _session.Prepare("DELETE FROM hotels WHERE hotel_id = ?");
            await _session.ExecuteAsync(stmt.Bind(hotelId));
        }

        public async Task UpsertRoomAsync(Room room)
        {
            var stmt = _session.Prepare("INSERT INTO rooms_by_hotel (hotel_id, room_number, room_type, price_per_night, status) VALUES (?, ?, ?, ?, ?)");
            await _session.ExecuteAsync(stmt.Bind(room.HotelId, room.RoomNumber, room.RoomType, room.PricePerNight, room.Status));
        }

        public async Task DeleteRoomAsync(string hotelId, int roomNumber)
        {
            var stmt = _session.Prepare("DELETE FROM rooms_by_hotel WHERE hotel_id = ? AND room_number = ?");
            await _session.ExecuteAsync(stmt.Bind(hotelId, roomNumber));
        }

        public async Task UpsertBookingGuestAsync(BookingGuest b)
        {
            var stmt = _session.Prepare("INSERT INTO bookings_by_guest (guest_id, check_in_date, booking_id, hotel_id, hotel_name, room_number, check_out_date, total_amount, status) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
            var checkIn = new LocalDate(b.CheckInDate.Year, b.CheckInDate.Month, b.CheckInDate.Day);
            var checkOut = new LocalDate(b.CheckOutDate.Year, b.CheckOutDate.Month, b.CheckOutDate.Day);
            await _session.ExecuteAsync(stmt.Bind(b.GuestId, checkIn, b.BookingId, b.HotelId, b.HotelName, b.RoomNumber, checkOut, b.TotalAmount, b.Status));
        }

        public async Task DeleteBookingGuestAsync(string guestId, DateTime checkInDate, Guid bookingId)
        {
            var stmt = _session.Prepare("DELETE FROM bookings_by_guest WHERE guest_id = ? AND check_in_date = ? AND booking_id = ?");
            var checkIn = new LocalDate(checkInDate.Year, checkInDate.Month, checkInDate.Day);
            await _session.ExecuteAsync(stmt.Bind(guestId, checkIn, bookingId));
        }

        public async Task UpsertBookingHotelDateAsync(BookingHotelDate b)
        {
            var stmt = _session.Prepare("INSERT INTO bookings_by_hotel_date (hotel_id, check_in_date, booking_id, guest_id, guest_name, room_number, check_out_date, total_amount, status) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)");
            var checkIn = new LocalDate(b.CheckInDate.Year, b.CheckInDate.Month, b.CheckInDate.Day);
            var checkOut = new LocalDate(b.CheckOutDate.Year, b.CheckOutDate.Month, b.CheckOutDate.Day);
            await _session.ExecuteAsync(stmt.Bind(b.HotelId, checkIn, b.BookingId, b.GuestId, b.GuestName, b.RoomNumber, checkOut, b.TotalAmount, b.Status));
        }

        public async Task DeleteBookingHotelDateAsync(string hotelId, DateTime checkInDate, Guid bookingId)
        {
            var stmt = _session.Prepare("DELETE FROM bookings_by_hotel_date WHERE hotel_id = ? AND check_in_date = ? AND booking_id = ?");
            var checkIn = new LocalDate(checkInDate.Year, checkInDate.Month, checkInDate.Day);
            await _session.ExecuteAsync(stmt.Bind(hotelId, checkIn, bookingId));
        }

        public async Task UpsertInvoiceAsync(Invoice inv)
        {
            var stmt = _session.Prepare("INSERT INTO invoices_by_booking (booking_id, invoice_id, guest_id, hotel_id, room_charge, service_charge, tax, total_amount, payment_status, issued_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
            await _session.ExecuteAsync(stmt.Bind(inv.BookingId, inv.InvoiceId, inv.GuestId, inv.HotelId, inv.RoomCharge, inv.ServiceCharge, inv.Tax, inv.TotalAmount, inv.PaymentStatus, inv.IssuedAt));
        }

        public async Task DeleteInvoiceAsync(Guid bookingId)
        {
            var stmt = _session.Prepare("DELETE FROM invoices_by_booking WHERE booking_id = ?");
            await _session.ExecuteAsync(stmt.Bind(bookingId));
        }
    }
}
