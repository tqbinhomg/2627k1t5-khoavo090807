using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bt_tuan4.Models;
using bt_tuan4.Services;

namespace bt_tuan4.Pages;

public class HotelDetailsModel : PageModel
{
    private readonly HotelService _service;

    public HotelDetailsModel(HotelService service) => _service = service;

    [BindProperty(SupportsGet = true)]
    public string HotelId { get; set; } = string.Empty;

    public Hotel? Hotel { get; set; }
    public IReadOnlyList<Room>? Rooms { get; set; }
    public IReadOnlyList<Hotel>? AllHotels { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActiveModal { get; set; }

    [BindProperty] public Hotel FormHotel { get; set; } = new();
    [BindProperty] public bool IsEditing { get; set; }
    [BindProperty] public string OriginalHotelId { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAllHotelsAsync();
        if (!string.IsNullOrEmpty(HotelId)) await LoadHotelDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync(string hotelId)
    {
        HotelId = hotelId;
        await LoadAllHotelsAsync();
        await LoadHotelDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ActiveModal = "hotelModal";
        if (string.IsNullOrWhiteSpace(FormHotel.HotelId))
        {
            ErrorMessage = "Mã khách sạn không được để trống.";
            await LoadAllHotelsAsync();
            return Page();
        }
        var existingHotel = await _service.GetHotelAsync(FormHotel.HotelId);
        if (existingHotel != null && (!IsEditing || !string.Equals(OriginalHotelId, FormHotel.HotelId, StringComparison.OrdinalIgnoreCase)))
        {
            ErrorMessage = $"Mã khách sạn '{FormHotel.HotelId}' đã tồn tại.";
            await LoadAllHotelsAsync();
            return Page();
        }
        try
        {
            await _service.UpsertHotelAsync(FormHotel);
            if (IsEditing && !string.IsNullOrWhiteSpace(OriginalHotelId) && !string.Equals(OriginalHotelId, FormHotel.HotelId, StringComparison.OrdinalIgnoreCase))
                await _service.DeleteHotelAsync(OriginalHotelId);
            SuccessMessage = "Lưu khách sạn thành công.";
            ActiveModal = null;
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllHotelsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string hotelId)
    {
        try
        {
            await _service.DeleteHotelAsync(hotelId);
            SuccessMessage = $"Đã xóa khách sạn {hotelId}.";
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllHotelsAsync();
        return Page();
    }

    private async Task LoadAllHotelsAsync()
    {
        try { AllHotels = await _service.GetAllHotelsAsync(); }
        catch (Exception ex) { ErrorMessage = "Lỗi tải danh sách: " + ex.Message; AllHotels = new List<Hotel>(); }
    }

    private async Task LoadHotelDataAsync()
    {
        try
        {
            Hotel = await _service.GetHotelAsync(HotelId);
            if (Hotel != null) Rooms = await _service.GetRoomsAsync(HotelId);
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
    }
}
