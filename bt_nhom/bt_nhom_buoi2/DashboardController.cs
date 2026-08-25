using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;

// ... [giữ các using như cũ]

[ApiController]
[Route("api/[controller]/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly MongoDbContext _context;
    public DashboardController(MongoDbContext context) { _context = context; }

    [HttpGet("kpi")]
    public async Task<IActionResult> GetKpi()
    {
        var total = await _context.Students.CountDocumentsAsync(_ => true);

        // Tổng số lớp
        var totalClass = await _context.Students.DistinctAsync<string>("Malop", FilterDefinition<StudentModel>.Empty);
        var totalClassList = await totalClass.ToListAsync();

        // % Nam/Nữ
        var genderGrp = await _context.Students.Aggregate()
            .Group(new BsonDocument {
                { "_id", "$phai" },
                { "count", new BsonDocument("$sum", 1) }
            }).ToListAsync();
        var totalNam = genderGrp.FirstOrDefault(x => x["_id"] == "Nam")?["count"].AsInt32 ?? 0;
        var totalNu = genderGrp.FirstOrDefault(x => x["_id"] == "Nữ")?["count"].AsInt32 ?? 0;

        // TB điểm toàn trường
        var avgAgg = await _context.Students.Aggregate()
            .Unwind<StudentModel>("Monhoc")
            .Group(new BsonDocument {
                { "_id", BsonNull.Value },
                { "avgScore", new BsonDocument("$avg", "$Monhoc.Diem") }
            }).FirstOrDefaultAsync();
        double avgScore = avgAgg == null ? 0 : avgAgg["avgScore"].ToDouble();

        return Ok(new {
            totalStudent = total,
            totalClass = totalClassList.Count,
            avgScore,
            malePct = total == 0 ? 0 : (double)totalNam / total * 100,
            femalePct = total == 0 ? 0 : (double)totalNu / total * 100
        });
    }

    [HttpGet("class-stat")]
    public async Task<IActionResult> StatByClass()
    {
        // Group theo lớp code dạng BsonDocument để get: mã lớp, sĩ số, điểm TB cao/thấp nhất
        var result = await _context.Students.Aggregate()
            .Project(new BsonDocument{
                {"Malop", "$Malop"},
                {"AvgDiem", new BsonDocument("$avg", "$Monhoc.Diem")}
            })
            .Group(new BsonDocument{
                {"_id", "$Malop"},
                {"Siso", new BsonDocument("$sum", 1)},
                {"MaxDTB", new BsonDocument("$max", "$AvgDiem")},
                {"MinDTB", new BsonDocument("$min", "$AvgDiem")}
            }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("lang-popular")]
    public async Task<IActionResult> LanguagePopular()
    {
        var result = await _context.Students.Aggregate()
            .Unwind<string>("Ngoaingu")
            .Group(new BsonDocument
            {
                { "_id", "$Ngoaingu" },
                { "count", new BsonDocument("$sum", 1) }
            })
            .Sort(new BsonDocument("count", -1))
            .ToListAsync();
        return Ok(result);
    }

    [HttpGet("top5-score")]
    public async Task<IActionResult> Top5Student()
    {
        var result = await _context.Students.Aggregate()
            .Project(new BsonDocument{
                {"Masv", "$Masv"},
                {"Hoten", "$Hoten"},
                {"Malop", "$Malop"},
                {"ScoreAvg", new BsonDocument("$avg", "$Monhoc.Diem")}
            })
            .Sort(new BsonDocument("ScoreAvg", -1))
            .Limit(5)
            .ToListAsync();
        return Ok(result);
    }

    [HttpGet("rank-stat")]
    public async Task<IActionResult> RankStat()
    {
        var result = await _context.Students.Aggregate()
            .Project(new BsonDocument{
                {"Masv", "$Masv"},
                {"Hoten", "$Hoten"},
                {"ScoreAvg", new BsonDocument("$avg", "$Monhoc.Diem")}
            })
            .Project(new BsonDocument{
                {"Masv", 1},
                {"Hoten", 1},
                {"Rank", new BsonDocument
                    {
                        { "$switch", new BsonDocument
                            {
                                { "branches", new BsonArray
                                    {
                                        new BsonDocument {
                                            { "case", new BsonDocument("$gte", new BsonArray{ "$ScoreAvg", 8.5 })},
                                            { "then", "Xuất sắc"}
                                        },
                                        new BsonDocument {
                                            { "case", new BsonDocument("$and", new BsonArray{
                                                new BsonDocument("$gte", new BsonArray{ "$ScoreAvg", 7.0 }),
                                                new BsonDocument("$lt", new BsonArray{ "$ScoreAvg", 8.5 })
                                            })},
                                            { "then", "Giỏi"}
                                        },
                                        new BsonDocument {
                                            { "case", new BsonDocument("$and", new BsonArray{
                                                new BsonDocument("$gte", new BsonArray{ "$ScoreAvg", 5.5 }),
                                                new BsonDocument("$lt", new BsonArray{ "$ScoreAvg", 7.0 })
                                            })},
                                            { "then", "Khá"}
                                        }
                                    } 
                                },
                                { "default", "Trung bình/Yếu"}
                            }
                        }
                    }
                }
            }).ToListAsync();
        return Ok(result);
    }
}