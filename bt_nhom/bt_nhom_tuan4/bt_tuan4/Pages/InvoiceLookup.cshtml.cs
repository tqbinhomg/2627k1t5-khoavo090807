using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using bt_tuan4.Models;
using bt_tuan4.Services;

namespace bt_tuan4.Pages;

public class InvoiceLookupModel : PageModel
{
    private readonly HotelService _service;

    public InvoiceLookupModel(HotelService service) => _service = service;

    [BindProperty] public string BookingIdInput { get; set; } = string.Empty;
    public Invoice? Invoice { get; set; }
    public IReadOnlyList<Invoice>? AllInvoices { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ActiveModal { get; set; }

    [BindProperty] public Invoice FormInvoice { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSearchAsync(string bookingIdInput)
    {
        BookingIdInput = bookingIdInput;
        await LoadAllAsync();
        if (!string.IsNullOrEmpty(BookingIdInput) && Guid.TryParse(BookingIdInput, out var id))
        {
            try { Invoice = await _service.GetInvoiceAsync(id); }
            catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
            if (Invoice == null) ErrorMessage = "Không tìm thấy hóa đơn.";
        }
        else if (!string.IsNullOrEmpty(BookingIdInput))
        {
            ErrorMessage = "Mã đặt phòng không hợp lệ.";
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        ActiveModal = "invoiceModal";
        if (string.IsNullOrWhiteSpace(FormInvoice.HotelId) || await _service.GetHotelAsync(FormInvoice.HotelId) == null)
        {
            ErrorMessage = $"Khách sạn '{FormInvoice.HotelId}' không tồn tại.";
            await LoadAllAsync();
            return Page();
        }
        if (FormInvoice.BookingId == Guid.Empty) FormInvoice.BookingId = Guid.NewGuid();
        if (FormInvoice.InvoiceId == Guid.Empty) FormInvoice.InvoiceId = Guid.NewGuid();
        if (FormInvoice.IssuedAt == default) FormInvoice.IssuedAt = DateTime.UtcNow;
        try
        {
            await _service.UpsertInvoiceAsync(FormInvoice);
            SuccessMessage = "Lưu hóa đơn thành công.";
            ActiveModal = null;
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid bookingId)
    {
        try
        {
            await _service.DeleteInvoiceAsync(bookingId);
            SuccessMessage = "Đã xóa hóa đơn.";
        }
        catch (Exception ex) { ErrorMessage = "Lỗi: " + ex.Message; }
        await LoadAllAsync();
        return Page();
    }

    private async Task LoadAllAsync()
    {
        try { AllInvoices = await _service.GetAllInvoicesAsync(); }
        catch (Exception ex) { ErrorMessage = "Lỗi tải danh sách: " + ex.Message; AllInvoices = new List<Invoice>(); }
    }
}
