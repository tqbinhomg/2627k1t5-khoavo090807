# Ứng dụng Quản lý Sinh viên - MongoDB

## Công nghệ
- ASP.NET Core 10.0
- MongoDB Driver 3.11.0
- Bootstrap 5.3.2

## Cài đặt
1. Cài MongoDB hoặc dùng MongoDB Atlas
2. Update connection string trong appsettings.json
3. Import data: `mongoimport --db BaiTapNhom --collection sv --file BaiTapNhom.sv.json --jsonArray`
4. Chạy: `dotnet run`

## Truy cập
- Web MVC: http://localhost:5181/SinhVienMvc
- Dashboard: http://localhost:5181/DashboardMvc
- Swagger API: http://localhost:5181/swagger