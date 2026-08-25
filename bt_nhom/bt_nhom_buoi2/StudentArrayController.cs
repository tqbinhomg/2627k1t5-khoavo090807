using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

[ApiController]
[Route("api/[controller]/array")]
public class StudentArrayController : ControllerBase
{
    private readonly MongoDbContext _context;
    public StudentArrayController(MongoDbContext context) { _context = context; }

    [HttpPost("add-language/{masv}")]
    public async Task<IActionResult> AddLanguage(string masv, [FromBody] string lang)
    {
        var result = await _context.Students.UpdateOneAsync(
            s => s.Masv == masv,
            Builders<StudentModel>.Update.Push("Ngoaingu", lang)
        );
        return Ok(result.ModifiedCount);
    }

    [HttpPost("add-subject/{masv}")]
    public async Task<IActionResult> AddSubject(string masv, [FromBody] SubjectModel subject)
    {
        var result = await _context.Students.UpdateOneAsync(
            s => s.Masv == masv,
            Builders<StudentModel>.Update.Push("Monhoc", subject)
        );
        return Ok(result.ModifiedCount);
    }

    [HttpPut("update-score/{masv}/{mamon}")]
    public async Task<IActionResult> UpdateScore(string masv, string mamon, [FromBody] double diem)
    {
        var result = await _context.Students.UpdateOneAsync(
            s => s.Masv == masv && s.Monhoc != null && s.Monhoc.Any(m => m.Mamon == mamon),
            Builders<StudentModel>.Update.Set("Monhoc.$.Diem", diem)
        );
        return Ok(result.ModifiedCount);
    }

    [HttpPut("replace/{id}")]
    public async Task<IActionResult> Replace([FromRoute] string id, [FromBody] StudentModel doc)
    {
        var result = await _context.Students.ReplaceOneAsync(x => x.Id == ObjectId.Parse(id), doc);
        return Ok(result.ModifiedCount);
    }
}
