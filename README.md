# Smart Livestock

Mẫu nghiên cứu hệ thống giám sát chăn nuôi dùng .NET 8, SQL Server và MQTT. Có thể chạy trên Windows bằng Visual Studio mà không cần Docker. Tiến độ chức năng được ghi tại [docs/IMPLEMENTATION_PROGRESS.md](docs/IMPLEMENTATION_PROGRESS.md).

## Yêu cầu trên máy phát triển

- Visual Studio có thành phần **ASP.NET and web development** và .NET SDK 8.0.426 (hoặc bản vá 8.0 tương thích với `global.json`).
- SQL Server LocalDB, SQL Server Express hoặc SQL Server. Cấu hình mặc định dùng LocalDB `MSSQLLocalDB`.
- Eclipse Mosquitto được cài trực tiếp trên Windows tại `C:\Program Files\mosquitto`.

Broker của dự án chỉ lắng nghe tại `127.0.0.1:1884`. Dịch vụ Mosquitto mặc định có thể dùng cổng 1883; hai cổng này tách biệt.

## Chạy bằng Visual Studio

1. Mở PowerShell tại thư mục chứa `SmartLivestock.sln` và tạo cấu hình MQTT cục bộ (chỉ cần lần đầu):

   ```powershell
   Set-Location D:\demoweb
   .\scripts\setup-local-mqtt.ps1
   ```

   Lệnh này tạo `deploy/native/.env` và cấu hình broker; các tệp này được Git bỏ qua. Mở `SmartLivestock.sln` trực tiếp trong Visual Studio. Khi chạy bằng F5, API và simulator tự đọc thông tin đăng nhập từ tệp này. Nếu PowerShell chặn chạy script, dùng `powershell -ExecutionPolicy Bypass -File .\scripts\setup-local-mqtt.ps1` cho riêng lần chạy đó.

2. Mở **View > Terminal** trong Visual Studio. Tại `D:\demoweb`, khởi động broker và để cửa sổ terminal này tiếp tục chạy:

   ```powershell
   & 'C:\Program Files\mosquitto\mosquitto.exe' -c '.\deploy\native\mosquitto.conf' -v
   ```

3. Mở một terminal khác trong Visual Studio. **Chỉ cần làm lần đầu** hoặc khi có migration mới:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = 'Development'
   dotnet tool restore
   dotnet ef database update --project src/SmartLivestock.Infrastructure --startup-project src/SmartLivestock.Api
   ```

   Trước khi chạy migration, dừng mọi phiên API cũ bằng **Shift+F5** hoặc đóng terminal đang chạy API; tiến trình API có thể khóa DLL và làm lệnh build thất bại. Nếu dùng SQL Server khác LocalDB, sửa `ConnectionStrings:Database` trong `src/SmartLivestock.Api/appsettings.Development.json` trước khi chạy migration và API. Cơ sở dữ liệu cần cho phép tài khoản đang chạy Visual Studio tạo hoặc cập nhật schema.

4. Trong **Solution Explorer**, bấm chuột phải vào dòng đầu tiên **Solution 'SmartLivestock'** > **Properties** (hoặc **Configure Startup Projects...**). Chọn **Multiple startup projects**, rồi đặt:

   | Project | Action |
   | --- | --- |
   | `SmartLivestock.Api` | `Start` |
   | `SmartLivestock.IoTSimulator` | `Start` |
   | `SmartLivestock.Web` | `Start` |
   | Các project còn lại | `None` |

   Xếp API trước simulator và Web nếu Visual Studio cho đổi thứ tự. Chọn profile **http** cho API và Web. `SmartLivestock.Domain` trong ảnh là thư viện, vì vậy không chọn nó làm startup project. Xem thêm [hướng dẫn nhiều startup project của Microsoft](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects).

5. Nhấn **F5**. Mở `http://localhost:5297` để xem Web; API có Swagger tại `http://localhost:5215/swagger` và kiểm tra kết nối SQL tại `http://localhost:5215/health/ready`. Simulator gửi dữ liệu mẫu mỗi 5 giây; đợi vài giây rồi làm mới trang Web nếu chưa thấy dữ liệu. Dùng **Shift+F5** để dừng các project, rồi dừng broker bằng **Ctrl+C** trong terminal.

Nếu F5 báo lỗi MQTT, kiểm tra `deploy/native/.env` đã được tạo và broker đang chạy ở cổng 1884. Nếu Web hoặc API báo cổng đã được dùng, đóng phiên chạy cũ trước khi F5 lại.

## Kiểm tra bản build

Trong terminal tại thư mục solution:

```powershell
dotnet restore SmartLivestock.sln
dotnet build SmartLivestock.sln --no-restore -m:1
dotnet test SmartLivestock.sln --no-build -m:1
```

## Chạy bằng dòng lệnh

Có thể chạy từng tiến trình bằng `dotnet run` thay cho F5. Sau khi thực hiện bước 2 và 3 ở trên, mở ba terminal độc lập tại thư mục solution:

```powershell
# Terminal API
$mqttVars = ConvertFrom-StringData (Get-Content 'deploy/native/.env' -Raw)
$env:Mqtt__User = $mqttVars.MQTT_USER
$env:Mqtt__Password = $mqttVars.MQTT_PASSWORD
dotnet run --project src/SmartLivestock.Api --launch-profile http
```

```powershell
# Terminal simulator
$mqttVars = ConvertFrom-StringData (Get-Content 'deploy/native/.env' -Raw)
$env:MQTT_USER = $mqttVars.MQTT_USER
$env:MQTT_PASSWORD = $mqttVars.MQTT_PASSWORD
$env:MQTT_PORT = '1884'
dotnet run --project simulators/SmartLivestock.IoTSimulator
```

```powershell
# Terminal Web
dotnet run --project src/SmartLivestock.Web --launch-profile http
```

Simulator hỗ trợ biến `SCENARIO`: `NORMAL`, `HIGH_TEMPERATURE`, `HIGH_HUMIDITY`, `HIGH_AMMONIA`, `HIGH_H2S`, `SENSOR_FAILURE`.

Đây là bản mẫu nghiên cứu; các phần đăng nhập, liên động điều khiển và một số chức năng trong yêu cầu tổng thể vẫn đang được phát triển. Khi triển khai thực tế cần cấu hình SQL Server, TLS, tài khoản riêng cho thiết bị và kiểm định an toàn vận hành.
