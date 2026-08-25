using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

public class SinhVienMvcController : Controller
{
    private readonly MongoDbContext _context;
    public SinhVienMvcController(MongoDbContext context) { _context = context; }

    public async Task<IActionResult> Index(string? searchMasv, string? filterMalop)
    {
        var studentsAgg = await _context.Students.Aggregate()
            .Project(new BsonDocument {
                {"_id", "$_id"},
                {"masv", "$masv"},
                {"hoten", "$hoten"},
                {"tuoi", "$tuoi"},
                {"phai", "$phai"},
                {"malop", "$malop"},
                {"ngoaingu", "$ngoaingu"},
                {"monhoc", "$monhoc"},
                {"scoreAvg", new BsonDocument("$cond", new BsonArray {
                    new BsonDocument("$gt", new BsonArray {new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$monhoc", new BsonArray() })), 0}),
                    new BsonDocument("$avg", "$monhoc.diem"),
                    BsonNull.Value
                })}
            }).ToListAsync();

        var students = new List<StudentModel>();
        var scoreDict = new Dictionary<string, (double score, string rank)>();
        
        foreach(var doc in studentsAgg)
        {
            var student = new StudentModel
            {
                Id = doc["_id"].IsObjectId ? doc["_id"].AsObjectId : ObjectId.Empty,
                Masv = doc.Contains("masv") ? doc["masv"].AsString : null,
                Hoten = doc.Contains("hoten") ? doc["hoten"].AsString : null,
                Tuoi = doc.Contains("tuoi") ? doc["tuoi"].ToInt32() : 0,
                Phai = doc.Contains("phai") ? doc["phai"].AsString : null,
                Malop = doc.Contains("malop") ? doc["malop"].AsString : null,
                Ngoaingu = doc.Contains("ngoaingu") && doc["ngoaingu"].IsBsonArray ? 
                    doc["ngoaingu"].AsBsonArray.Where(x => !x.IsBsonNull).Select(x => x.IsString ? x.AsString : x.ToString()).Where(s => s != null).Select(s => s!).ToList() : new List<string>()
            };
            
            // Tính xếp loại cho mỗi sinh viên
            double scoreAvg = 0;
            string rank = "Trung bình/Yếu";
            if(doc.Contains("scoreAvg") && doc["scoreAvg"] != BsonNull.Value)
            {
                scoreAvg = doc["scoreAvg"].ToDouble();
                if(scoreAvg >= 8.5 && scoreAvg <= 10.0) rank = "Xuất sắc";
                else if(scoreAvg >= 7.0 && scoreAvg < 8.5) rank = "Giỏi";
                else if(scoreAvg >= 5.5 && scoreAvg < 7.0) rank = "Khá";
                else rank = "Trung bình/Yếu";
            }
            
            if (!string.IsNullOrEmpty(student.Masv))
                scoreDict[student.Masv] = (scoreAvg, rank);
            
            if(doc.Contains("monhoc") && doc["monhoc"].IsBsonArray)
            {
                student.Monhoc = new List<SubjectModel>();
                foreach(var mon in doc["monhoc"].AsBsonArray)
                {
                    student.Monhoc.Add(new SubjectModel
                    {
                        Mamon = mon["mamon"].IsString ? mon["mamon"].AsString : null,
                        Tenmon = mon["tenmon"].IsString ? mon["tenmon"].AsString : null,
                        Diem = mon["diem"].IsDouble ? mon["diem"].ToDouble() : (mon["diem"].IsInt32 ? mon["diem"].ToInt32() : 0)
                    });
                }
            }
            
            students.Add(student);
        }

        if (!string.IsNullOrEmpty(searchMasv))
            students = students.Where(s => s.Masv == searchMasv).ToList();
        if (!string.IsNullOrEmpty(filterMalop))
            students = students.Where(s => s.Malop == filterMalop).ToList();

        // Lọc lại dictionary theo danh sách sau khi filter
        var masvList = students.Select(s => s.Masv).ToHashSet();
        ViewBag.StudentScoreAvg = scoreDict.Where(kv => masvList.Contains(kv.Key)).ToDictionary(kv => kv.Key, kv => kv.Value);

        return View(students);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(StudentModel model)
    {
        // Validation tuổi
        if (model.Tuoi <= 0 || model.Tuoi > 100)
        {
            ModelState.AddModelError("Tuoi", "Tuổi phải là số nguyên lớn hơn 0 và nhỏ hơn hoặc bằng 100");
            return View(model);
        }

        // Kiểm tra masv trùng
        var existing = await _context.Students.Find(s => s.Masv == model.Masv).FirstOrDefaultAsync();
        if (existing != null)
        {
            ModelState.AddModelError("Masv", "Mã sinh viên đã tồn tại. Vui lòng nhập mã khác.");
            return View(model);
        }

        try
        {
            await _context.Students.InsertOneAsync(model);
            TempData["SuccessMessage"] = "Thêm sinh viên thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi thêm sinh viên: " + ex.Message);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(string id)
    {
        var sv = await _context.Students.Find(s => s.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        if (sv == null) return NotFound();
        return View(sv);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(StudentModel m)
    {
        // Validation tuổi
        if (m.Tuoi <= 0 || m.Tuoi > 100)
        {
            ModelState.AddModelError("Tuoi", "Tuổi phải là số nguyên lớn hơn 0 và nhỏ hơn hoặc bằng 100");
            return View(m);
        }

        // Kiểm tra masv trùng với sinh viên khác (không phải chính nó)
        var existing = await _context.Students.Find(s => s.Masv == m.Masv && s.Id != m.Id).FirstOrDefaultAsync();
        if (existing != null)
        {
            ModelState.AddModelError("Masv", "Mã sinh viên đã tồn tại. Vui lòng nhập mã khác.");
            return View(m);
        }

        try
        {
            await _context.Students.ReplaceOneAsync(s => s.Id == m.Id, m);
            TempData["SuccessMessage"] = "Cập nhật sinh viên thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi khi cập nhật sinh viên: " + ex.Message);
            return View(m);
        }
    }

    public async Task<IActionResult> Delete(string id)
    {
        var sv = await _context.Students.Find(s => s.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        if (sv == null) return NotFound();
        return View(sv);
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _context.Students.DeleteOneAsync(s => s.Id == ObjectId.Parse(id));
        return RedirectToAction("Index");
    }
}
