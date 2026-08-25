using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

public class StudentModel
{
    [BsonId]
    public ObjectId Id { get; set; }
    
    [BsonElement("masv")]
    public string? Masv { get; set; }
    
    [BsonElement("hoten")]
    public string? Hoten { get; set; }
    
    [BsonElement("tuoi")]
    public int Tuoi { get; set; }
    
    [BsonElement("phai")]
    public string? Phai { get; set; }
    
    [BsonElement("malop")]
    public string? Malop { get; set; }
    
    [BsonElement("ngoaingu")]
    public List<string>? Ngoaingu { get; set; }
    
    [BsonElement("monhoc")]
    public List<SubjectModel>? Monhoc { get; set; }
}

public class SubjectModel
{
    [BsonElement("mamon")]
    public string? Mamon { get; set; }
    
    [BsonElement("tenmon")]
    public string? Tenmon { get; set; }
    
    [BsonElement("diem")]
    [BsonRepresentation(MongoDB.Bson.BsonType.Double, AllowTruncation = true)]
    public double Diem { get; set; }
}