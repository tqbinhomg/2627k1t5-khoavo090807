using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

public class DashboardMvcController : Controller
{
    private readonly MongoDbContext _context;
    public DashboardMvcController(MongoDbContext context) { _context = context; }
    
    public async Task<IActionResult> Index()
    {
        var total = await _context.Students.CountDocumentsAsync(_ => true);
        
        var classes = await _context.Students.DistinctAsync<string>("malop", FilterDefinition<StudentModel>.Empty);
        var classList = await classes.ToListAsync();
        
        var totalScoreAgg = await _context.Students.Aggregate()
            .Unwind("monhoc")
            .Group(new BsonDocument
            {
                {"_id", BsonNull.Value},
                {"tongDiem", new BsonDocument("$sum", "$monhoc.diem")},
                {"soMon", new BsonDocument("$sum", 1)}
            }).FirstOrDefaultAsync();
        double avgScore = 0;
        if (totalScoreAgg != null && totalScoreAgg.Contains("tongDiem") && totalScoreAgg.Contains("soMon") && totalScoreAgg["soMon"].ToInt32() > 0)
            avgScore = totalScoreAgg["tongDiem"].ToDouble() / totalScoreAgg["soMon"].ToDouble();
        
        var genderAgg = await _context.Students.Aggregate()
            .Group(new BsonDocument { { "_id", "$phai" }, { "count", new BsonDocument("$sum", 1) } })
            .ToListAsync();

        long male = genderAgg.FirstOrDefault(x => x.Contains("_id") && x["_id"] != null && x["_id"].AsString == "Nam")?["count"].ToInt64() ?? 0;
        long female = genderAgg.FirstOrDefault(x => x.Contains("_id") && x["_id"] != null && x["_id"].AsString == "Nữ")?["count"].ToInt64() ?? 0;

        var classStats = await _context.Students.Aggregate()
            .Project(new BsonDocument {
                {"malop", "$malop"},
                {"diemTB", new BsonDocument("$cond", new BsonArray {
                    new BsonDocument("$gt", new BsonArray {new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$monhoc", new BsonArray() })), 0}),
                    new BsonDocument("$avg", "$monhoc.diem"),
                    BsonNull.Value
                })}
            })
            .Group(new BsonDocument{
                {"_id", "$malop"},
                {"Siso", new BsonDocument("$sum", 1)},
                {"MaxDTB", new BsonDocument("$max", "$diemTB")},
                {"MinDTB", new BsonDocument("$min", "$diemTB")}
            }).ToListAsync();
        
        // Làm tròn MaxDTB và MinDTB về 2 chữ số thập phân
        foreach(var item in classStats)
        {
            if(item.Contains("MaxDTB") && item["MaxDTB"] != BsonNull.Value)
            {
                item["MaxDTB"] = Math.Round(item["MaxDTB"].ToDouble(), 2);
            }
            if(item.Contains("MinDTB") && item["MinDTB"] != BsonNull.Value)
            {
                item["MinDTB"] = Math.Round(item["MinDTB"].ToDouble(), 2);
            }
        }
        
        var popLang = await _context.Students.Aggregate()
            .Unwind("ngoaingu")
            .Group(new BsonDocument { { "_id", "$ngoaingu" }, { "count", new BsonDocument("$sum", 1) } })
            .Sort(new BsonDocument("count", -1))
            .ToListAsync();
        
        var top5 = await _context.Students.Aggregate()
            .Project(new BsonDocument {
                {"masv", "$masv"}, {"hoten", "$hoten"}, {"malop", "$malop"},
                {"scoreAvg", new BsonDocument("$cond", new BsonArray {
                    new BsonDocument("$gt", new BsonArray { new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$monhoc", new BsonArray() })), 0 }),
                    new BsonDocument("$avg", "$monhoc.diem"),
                    BsonNull.Value
                })}
            })
            .Sort(new BsonDocument("scoreAvg", -1)).Limit(5)
            .ToListAsync();
        
        var allRanks = await _context.Students.Aggregate()
            .Project(new BsonDocument {
                {"scoreAvg", new BsonDocument("$cond", new BsonArray {
                    new BsonDocument("$gt", new BsonArray {new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray { "$monhoc", new BsonArray() })), 0}),
                    new BsonDocument("$avg", "$monhoc.diem"),
                    BsonNull.Value
                })}
            })
            .Project(new BsonDocument {
                {"Rank", new BsonDocument("$switch", new BsonDocument{
                    {"branches", new BsonArray{
                        new BsonDocument{{"case", new BsonDocument("$gte", new BsonArray{"$scoreAvg", 8.5})}, {"then", "Xuất sắc"}},
                        new BsonDocument{{"case", new BsonDocument("$gte", new BsonArray{"$scoreAvg", 7.0})}, {"then", "Giỏi"}},
                        new BsonDocument{{"case", new BsonDocument("$gte", new BsonArray{"$scoreAvg", 5.5})}, {"then", "Khá"}}
                    }},
                    {"default", "Trung bình/Yếu"}
                })}
            })
            .Group(new BsonDocument {{"_id", "$Rank"}, {"SoLuong", new BsonDocument("$sum", 1)}})
            .Sort(new BsonDocument("SoLuong", -1))
            .ToListAsync();
        
        ViewBag.Total = total;
        ViewBag.TotalClass = classList.Count;
        ViewBag.AvgScore = avgScore;
        ViewBag.PctMale = total == 0 ? 0 : Math.Round((double)male / total * 100, 2);
        ViewBag.PctFemale = total == 0 ? 0 : Math.Round((double)female / total * 100, 2);
        ViewBag.ByClass = classStats;
        ViewBag.PopularLang = popLang;
        ViewBag.Top5 = top5;
        ViewBag.RankStat = allRanks;
        return View();
    }
}