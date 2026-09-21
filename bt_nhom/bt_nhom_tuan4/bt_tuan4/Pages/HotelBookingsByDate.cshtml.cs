using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bt_tuan4.Models;
using bt_tuan4.Services;

namespace bt_tuan4.Pages;

public class HotelBookingsByDateModel : PageModel
{
    private readonly HotelService _service;

    public HotelBookingsByDateModel(HotelService service) => _service = service;

    [BindProperty] public string HotelId { get; set; } = string.Empty;
    [BindProperty] public DateTime StartDate { get; set; }
    [BindProperty] public DateTime EndDate { get; set; }

    public IReadOnlyList<BookingHotelDate>? Bookings { get; set; }
    public IReadOnlyList<BookingHotelDate>? AllBookings { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActiveModal { get; set; }

    [BindProperty] public BookingHotelDate FormBooking { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync(string hotelId, DateTime startDate, DateTime endDate)
    {
        HotelId = hotelId; StartDate = startDate; EndDate = endDate;
        await LoadAllAsync();
        if (!string.IsNullOrEmpty(HotelId) && StartDate != default && EndDate != default)
        {
            try { Bookings = await _service.GetHotelBookingsByDateAsync(HotelId, StartDate, EndDate); }
            catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ActiveModal = "bookingModal";
        if (FormBooking.CheckInDate == default || FormBooking.CheckOutDate == default || FormBooking.CheckInDate >= FormBooking.CheckOutDate)
        {
            ErrorMessage = "Ngày nhận phòng phải trước ngày trả phòng.";
            await LoadAllAsync();
            return Page();
        }
        var hotel = await _service.GetHotelAsync(FormBooking.HotelId);
        if (hotel == null)
        {
            ErrorMessage = $"Khách sạn '{FormBooking.HotelId}' không tồn tại.";
            await LoadAllAsync();
            return Page();
        }
        var room = (await _service.GetRoomsAsync(FormBooking.HotelId))
            .FirstOrDefault(item => item.RoomNumber == FormBooking.RoomNumber);
        if (room == null)
        {
            ErrorMessage = $"Phòng {FormBooking.RoomNumber} không tồn tại tại khách sạn '{FormBooking.HotelId}'.";
            await LoadAllAsync();
            return Page();
        }
        if (!string.Equals(room.Status, "AVAILABLE", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = $"Phòng {FormBooking.RoomNumber} hiện không thể đặt vì đang ở trạng thái '{room.Status}'.";
            await LoadAllAsync();
            return Page();
        }
        if (FormBooking.BookingId == Guid.Empty) FormBooking.BookingId = Guid.NewGuid();
        try
        {
            await _service.UpsertBookingHotelDateAsync(FormBooking);
            SuccessMessage = "Lưu đặt phòng thành công.";
            ActiveModal = null;
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string hotelId, DateTime checkInDate, Guid bookingId)
    {
        try
        {
            await _service.DeleteBookingHotelDateAsync(hotelId, checkInDate, bookingId);
            SuccessMessage = "Đã xóa đặt phòng.";
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllAsync();
        return Page();
    }

    private async Task LoadAllAsync()
    {
        try { AllBookings = await _service.GetAllHotelBookingsByDateAsync(); }
        catch (Exception ex) { ErrorMessage = "Lỗi tải danh sách: " + ex.Message; AllBookings = new List<BookingHotelDate>(); }
    }
}
