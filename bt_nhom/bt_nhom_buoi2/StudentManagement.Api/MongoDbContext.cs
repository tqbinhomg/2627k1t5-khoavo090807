using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class MongoDbContext
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        _mongoClient = new MongoClient(_settings.ConnectionString);
        _mongoDatabase = _mongoClient.GetDatabase(_settings.DatabaseName);
        CreateIndexes();
        CreateValidators();
    }

    private string StudentsCollectionName =>
        string.IsNullOrWhiteSpace(_settings.CollectionName) ? "sv" : _settings.CollectionName;

    public IMongoCollection<StudentModel> Students =>
        _mongoDatabase.GetCollection<StudentModel>(StudentsCollectionName);

    private void CreateIndexes()
    {
        // Unique index cho masv - ngăn chặn trùng lặp mã sinh viên
        var masvIndexOptions = new CreateIndexOptions { Unique = true, Name = "idx_masv_unique" };
        var masvIndex = new CreateIndexModel<StudentModel>(
            Builders<StudentModel>.IndexKeys.Ascending(s => s.Masv), 
            masvIndexOptions);
        
        // Compound index cho malop + hoten
        var compoundIndex = new CreateIndexModel<StudentModel>(
            Builders<StudentModel>.IndexKeys.Ascending(s => s.Malop).Ascending(s => s.Hoten),
            new CreateIndexOptions { Name = "idx_malop_hoten" });

        try
        {
            Students.Indexes.CreateOne(masvIndex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Index masv có thể đã tồn tại: {ex.Message}");
        }
        
        try
        {
            Students.Indexes.CreateOne(compoundIndex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Index compound có thể đã tồn tại: {ex.Message}");
        }
    }

    private void CreateValidators()
    {
        // Tạo validator schema document
        var schema = new BsonDocument();
        
        // Trường bắt buộc
        schema.Add("bsonType", "object");
        schema.Add("required", new BsonArray { "masv", "hoten", "phai", "malop" });
        
        // Properties
        var properties = new BsonDocument();
        
        // masv
        var masvProp = new BsonDocument();
        masvProp.Add("bsonType", "string");
        properties.Add("masv", masvProp);
        
        // hoten
        var hotenProp = new BsonDocument();
        hotenProp.Add("bsonType", "string");
        properties.Add("hoten", hotenProp);
        
        // phai
        var phaiProp = new BsonDocument();
        phaiProp.Add("enum", new BsonArray { "Nam", "Nữ" });
        properties.Add("phai", phaiProp);
        
        // malop
        var malopProp = new BsonDocument();
        malopProp.Add("bsonType", "string");
        properties.Add("malop", malopProp);
        
        // tuoi
        var tuoiProp = new BsonDocument();
        tuoiProp.Add("bsonType", "int");
        tuoiProp.Add("minimum", 0);
        tuoiProp.Add("maximum", 100);
        properties.Add("tuoi", tuoiProp);
        
        // ngoaingu - cho phép null hoặc array
        var ngoainguProp = new BsonDocument();
        ngoainguProp.Add("bsonType", new BsonArray { "array", "null" });
        properties.Add("ngoaingu", ngoainguProp);
        
        // monhoc - mảng với validation cho diem (0-10), cho phép null
        var monhocProp = new BsonDocument();
        monhocProp.Add("bsonType", new BsonArray { "array", "null" }); // Cho phép array hoặc null
        
        var items = new BsonDocument();
        items.Add("bsonType", "object");
        items.Add("required", new BsonArray { "mamon", "tenmon", "diem" });
        
        var itemProps = new BsonDocument();
        
        var mamonItem = new BsonDocument();
        mamonItem.Add("bsonType", "string");
        itemProps.Add("mamon", mamonItem);
        
        var tenmonItem = new BsonDocument();
        tenmonItem.Add("bsonType", "string");
        itemProps.Add("tenmon", tenmonItem);
        
        var diemItem = new BsonDocument();
        diemItem.Add("bsonType", new BsonArray { "double", "int", "long", "decimal" }); // Cho phép nhiều kiểu số
        diemItem.Add("minimum", 0.0);
        diemItem.Add("maximum", 10.0);
        itemProps.Add("diem", diemItem);
        
        items.Add("properties", itemProps);
        monhocProp.Add("items", items);
        properties.Add("monhoc", monhocProp);
        
        schema.Add("properties", properties);
        
        // Wrap in $jsonSchema
        var validator = new BsonDocument("$jsonSchema", schema);

        try
        {
            var cmd = new BsonDocument
            {
                { "create", StudentsCollectionName },
                { "validator", validator },
                { "validationLevel", "strict" }
            };
            _mongoDatabase.RunCommand<BsonDocument>(cmd);
            Console.WriteLine("Collection 'sv' da duoc tao voi validation");
        }
        catch (MongoDB.Driver.MongoCommandException ex) when (ex.Message.Contains("already exists") || ex.Code == 48)
        {
            try
            {
                var updateCmd = new BsonDocument
                {
                    { "collMod", StudentsCollectionName },
                    { "validator", validator },
                    { "validationLevel", "strict" }
                };
                _mongoDatabase.RunCommand<BsonDocument>(updateCmd);
                Console.WriteLine("Validation da duoc cap nhat");
            }
            catch (Exception updateEx)
            {
                Console.WriteLine("Khong the cap nhat validator: " + updateEx.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Loi tao validator: " + ex.Message);
        }
    }
}
