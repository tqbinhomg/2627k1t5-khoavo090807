using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bt_tuan4.Models;
using bt_tuan4.Services;

namespace bt_tuan4.Pages;

public class RoomsListModel : PageModel
{
    private readonly HotelService _service;

    public RoomsListModel(HotelService service) => _service = service;

    [BindProperty] public string HotelId { get; set; } = string.Empty;
    public IReadOnlyList<Room>? AllRooms { get; set; }
    public IReadOnlyList<Room>? Rooms { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActiveModal { get; set; }

    [BindProperty] public Room FormRoom { get; set; } = new();
    [BindProperty] public bool IsEditing { get; set; }
    [BindProperty] public string OriginalHotelId { get; set; } = string.Empty;
    [BindProperty] public int OriginalRoomNumber { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAllRoomsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync(string hotelId)
    {
        HotelId = hotelId;
        await LoadAllRoomsAsync();
        if (!string.IsNullOrEmpty(HotelId))
        {
            try { Rooms = await _service.GetRoomsAsync(HotelId); }
            catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ActiveModal = "roomModal";
        if (string.IsNullOrWhiteSpace(FormRoom.HotelId))
        {
            ErrorMessage = "Mã khách sạn không được để trống.";
            await LoadAllRoomsAsync();
            return Page();
        }
        if (await _service.GetHotelAsync(FormRoom.HotelId) == null)
        {
            ErrorMessage = $"Khách sạn '{FormRoom.HotelId}' không tồn tại.";
            await LoadAllRoomsAsync();
            return Page();
        }
        var existingRooms = await _service.GetRoomsAsync(FormRoom.HotelId);
        if (existingRooms.Any(room => room.RoomNumber == FormRoom.RoomNumber &&
            (!IsEditing || room.HotelId != OriginalHotelId || room.RoomNumber != OriginalRoomNumber)))
        {
            ErrorMessage = $"Phòng {FormRoom.RoomNumber} đã tồn tại tại khách sạn '{FormRoom.HotelId}'.";
            await LoadAllRoomsAsync();
            return Page();
        }
        try
        {
            await _service.UpsertRoomAsync(FormRoom);
            if (IsEditing && (!string.Equals(OriginalHotelId, FormRoom.HotelId, StringComparison.OrdinalIgnoreCase) || OriginalRoomNumber != FormRoom.RoomNumber))
                await _service.DeleteRoomAsync(OriginalHotelId, OriginalRoomNumber);
            SuccessMessage = "Lưu phòng thành công.";
            ActiveModal = null;
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllRoomsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string hotelId, int roomNumber)
    {
        try
        {
            await _service.DeleteRoomAsync(hotelId, roomNumber);
            SuccessMessage = $"Đã xóa phòng {roomNumber} của khách sạn {hotelId}.";
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllRoomsAsync();
        return Page();
    }

    private async Task LoadAllRoomsAsync()
    {
        try { AllRooms = await _service.GetAllRoomsAsync(); }
        catch (Exception ex) { ErrorMessage = "Lỗi tải danh sách: " + ex.Message; AllRooms = new List<Room>(); }
    }
}
