using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;

// QUAN TRỌNG: Ép culture bất biến để parse điểm "7.4" = 7.4 (không bị hiểu thành 74)
var invariantCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = invariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = invariantCulture;

ConventionRegistry.Register(
    "camelCase",
    new ConventionPack { new CamelCaseElementNameConvention() },
    _ => true);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(name: "default", pattern: "{controller=SinhVienMvc}/{action=Index}/{id?}");
app.MapControllers();

app.Run();