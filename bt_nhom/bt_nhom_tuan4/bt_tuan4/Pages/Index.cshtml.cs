using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bt_tuan4.Models;
using bt_tuan4.Services;

namespace bt_tuan4.Pages;

public class IndexModel : PageModel
{
    private readonly HotelService _service;

    public IndexModel(HotelService service)
    {
        _service = service;
    }

    public IReadOnlyList<Hotel>? AllHotels { get; set; }
    public IReadOnlyList<Room>? AllRooms { get; set; }
    public IReadOnlyList<BookingGuest>? AllGuestBookings { get; set; }
    public IReadOnlyList<BookingHotelDate>? AllHotelBookings { get; set; }
    public IReadOnlyList<Invoice>? AllInvoices { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            AllHotels = await _service.GetAllHotelsAsync();
            if (AllHotels == null)
                AllHotels = new List<Hotel>();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi khi tải danh sách khách sạn: " + ex.Message;
            AllHotels = new List<Hotel>();
        }

        try
        {
            AllRooms = await _service.GetAllRoomsAsync();
            if (AllRooms == null)
                AllRooms = new List<Room>();
        }
        catch (Exception ex)
        {
            ErrorMessage = (ErrorMessage ?? "") + " Lỗi khi tải danh sách phòng: " + ex.Message;
            AllRooms = new List<Room>();
        }

        try
        {
            AllGuestBookings = await _service.GetAllGuestBookingsAsync();
            AllHotelBookings = await _service.GetAllHotelBookingsByDateAsync();
            AllInvoices = await _service.GetAllInvoicesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = (ErrorMessage ?? "") + " Lỗi khi tải số liệu đặt phòng/hóa đơn: " + ex.Message;
            AllGuestBookings = new List<BookingGuest>();
            AllHotelBookings = new List<BookingHotelDate>();
            AllInvoices = new List<Invoice>();
        }

        return Page();
    }
}