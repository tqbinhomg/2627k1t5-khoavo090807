using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly MongoDbContext _context;
    public StudentController(MongoDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Students.Find(_ => true).ToListAsync());

    [HttpGet("{masv}")]
    public async Task<IActionResult> GetByMasv(string masv) =>
        Ok(await _context.Students.Find(s => s.Masv == masv).FirstOrDefaultAsync());

    [HttpGet("by-class/{malop}")]
    public async Task<IActionResult> GetByClass(string malop) =>
        Ok(await _context.Students.Find(s => s.Malop == malop).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(StudentModel student)
    {
        await _context.Students.InsertOneAsync(student);
        return Ok(student);
    }

    [HttpPut("{masv}")]
    public async Task<IActionResult> Update(string masv, StudentModel update)
    {
        var result = await _context.Students.UpdateOneAsync(s => s.Masv == masv,
            Builders<StudentModel>.Update
                .Set(s => s.Hoten, update.Hoten)
                .Set(s => s.Tuoi, update.Tuoi)
                .Set(s => s.Phai, update.Phai)
                .Set(s => s.Malop, update.Malop)
        );
        return Ok(result.ModifiedCount);
    }

    [HttpDelete("{masv}")]
    public async Task<IActionResult> Delete(string masv)
    {
        var result = await _context.Students.DeleteOneAsync(s => s.Masv == masv);
        return Ok(result.DeletedCount);
    }

    [HttpDelete("by-class/{malop}")]
    public async Task<IActionResult> DeleteByClass(string malop)
    {
        var result = await _context.Students.DeleteManyAsync(s => s.Malop == malop);
        return Ok(result.DeletedCount);
    }
}
