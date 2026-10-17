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

## Xem trên điện thoại

Giao diện Web tự chuyển sang bố cục điện thoại (thanh điều hướng dưới đáy, chuồng hiển thị dạng thẻ) khi màn hình rộng không quá 820px. Để mở từ điện thoại cùng mạng Wi-Fi:

1. Chạy `SmartLivestock.Web` bằng profile **lan** (thay cho **http**) để Web lắng nghe trên mọi địa chỉ mạng. API và Simulator giữ nguyên, vì Web gọi API từ phía máy chủ.
2. Cho phép cổng 5297 qua Windows Firewall (PowerShell chạy bằng quyền Administrator, chỉ cần làm một lần):

   ```powershell
   New-NetFirewallRule -DisplayName 'SmartLivestock Web 5297' -Direction Inbound -Protocol TCP -LocalPort 5297 -Action Allow -Profile Private
   ```

3. Trên điện thoại mở `http://<IP máy tính>:5297` (xem IP bằng `ipconfig`, mục IPv4 Address).

Chỉ dùng cho mạng nội bộ tin cậy: hệ thống chưa có đăng nhập, ai vào được địa chỉ này đều xem và sửa được dữ liệu.

## Mô phỏng dữ liệu chạm ngưỡng cảnh báo

Simulator có kịch bản `DEMO_ALERTS` (mặc định khi chạy bằng F5, qua profile `demo-alerts` trong `simulators/SmartLivestock.IoTSimulator/Properties/launchSettings.json`): mỗi chuồng thứ 3 (3, 6, 9, 12, 15, 18) tăng dần một chỉ số qua mức cảnh báo, mức nguy hiểm rồi hồi phục, chu kỳ 3 phút và lệch pha giữa các chuồng; chuồng 7 mất cảm biến; các chuồng còn lại bình thường. Chọn profile `normal` để về dữ liệu bình thường. Mức cảnh báo/nguy hiểm trong `DemoAlertProfile` chỉ là số mô phỏng, không phải chuẩn an toàn cho vật nuôi. Cơ chế hộp thoại cảnh báo được nghiên cứu tại [docs/ALERT_DESIGN.md](docs/ALERT_DESIGN.md), chưa được xây dựng.

## Cảnh báo realtime và lịch sử lỗi

- **Phát hiện ở máy chủ:** mỗi tin nhắn cảm biến được so với `DemoAlertProfile`. Một "đợt lỗi" (từ lúc vượt ngưỡng đến lúc hết) của mỗi chuồng và chỉ số được lưu thành một bản ghi (bảng `Alerts`), không bị trùng khi còn đang diễn ra. Có độ trễ chống nhấp nháy: cảnh báo chỉ hết khi giá trị xuống thấp hơn ngưỡng một khoảng nhỏ. Cảm biến báo dữ liệu không hợp lệ tạo một đợt "Cảm biến lỗi" riêng cho chuồng.
- **Hộp thoại tóm tắt lỗi không chen ngang công việc.** Trang **Cài đặt** (lưu theo trình duyệt) cho chọn khi nào hộp thoại được hiện:
  - **Một lần mỗi ngày vào giờ cố định**, mặc định **23:59**.
  - **Lặp lại theo chu kỳ**: 15, 30 phút, 1, 2 hoặc 4 giờ.
  - **Ngay lập tức** khi có cảnh báo nguy hiểm hoặc lỗi cảm biến (hành vi gián đoạn nhất).
  - **Không bao giờ hiện hộp thoại.**
  - Tùy chọn **Cũng hiện khi mở phần mềm** (mặc định tắt).
  Đến giờ, hộp thoại tóm tắt các chuồng lỗi từ lần báo trước đến lúc đó; nếu không có lỗi nào thì không hiện gì. Giờ cố định đã trôi qua lúc tab đóng sẽ hiện một lần ở lần mở kế tiếp. Nút "Xem thử ngay" trong Cài đặt cho xem trước (không tính là đã xem lỗi). Giờ tính theo giờ của máy chủ.
- **Trang Lịch sử lỗi:** lọc theo khoảng thời gian (1 giờ, 24 giờ, 7 ngày, tất cả), chuồng, mức độ, trạng thái; có phân trang và tự cập nhật. Nút "Xem lịch sử lỗi" trong hộp thoại mở trang này với đúng khung giờ.
- **Đã đọc:** mỗi cảnh báo có trạng thái đã đọc / chưa đọc, lưu ở máy chủ (dùng chung, vì chưa có tài khoản). Trang Lịch sử lỗi hiện dấu chấm và chữ đậm cho cảnh báo chưa đọc; mỗi dòng có nút "Đã đọc" / "Bỏ đã đọc"; nút "Đánh dấu tất cả kết quả đã đọc" áp dụng cho mọi cảnh báo khớp bộ lọc hiện tại (kể cả các trang khác); có bộ lọc "Đọc". Huy hiệu trên menu đếm số chưa đọc. Hộp thoại tóm tắt chỉ liệt kê cảnh báo chưa đọc và có nút "Đánh dấu đã đọc". Cảnh báo leo thang (ví dụ cảnh báo lên nguy hiểm) tự quay về chưa đọc.
- **Phím Esc hoặc nút Đóng** chỉ đóng hộp thoại, không đánh dấu đã đọc và không "xác nhận" cảnh báo (xác nhận có ghi người thực hiện cần đăng nhập, F08).
- **Cập nhật cơ sở dữ liệu:** chạy `dotnet ef database update` (bước 3 phần "Chạy bằng Visual Studio") để có bảng `Alerts`.

## Trang Giám sát: số chuồng mỗi trang

Mặc định hiển thị **10 chuồng mỗi trang**. Ô "Số chuồng/trang" ở cuối bảng cho chọn gợi ý (5, 10, 20, 50) hoặc **gõ tay** một số từ 1 đến 50 (số lớn hơn được đưa về 50, nhỏ hơn về 1). Giá trị được nhớ theo từng trình duyệt.

Trên màn hình máy tính, chiều cao mỗi dòng của bảng tự co giãn theo chiều cao cửa sổ để **đủ 10 dòng, phân trang và ô chọn chuồng nằm gọn trong một màn hình, không phải cuộn trang** (từ khoảng 640px chiều cao trở lên; cửa sổ thấp hơn sẽ gọn hơn nữa: bỏ dòng mô tả, bỏ icon thẻ thống kê, mã chuồng nằm cùng dòng tên). Nếu đặt nhiều hơn 10 chuồng mỗi trang thì trang cuộn như bình thường. Khổ điện thoại giữ nguyên dạng thẻ.

Cụm phân trang (dùng chung cho trang Giám sát và Lịch sử lỗi) gồm nút mũi tên trước/sau, số trang với trang đang chọn tô màu, dấu "…" kèm nhảy nhanh về trang đầu/cuối khi có nhiều trang. Ở trang Giám sát, thông tin "Hiển thị 1–10 trong 19 chuồng" nằm bên trái, số trang ở giữa, ô "Số chuồng/trang" bên phải.

Trang **Lịch sử lỗi** và **Quản lý chuồng** dùng cùng cách phân trang và cùng chế độ vừa khít: mặc định 10 dòng mỗi trang, ô "Số lỗi/trang" và "Số chuồng/trang" cho chọn gợi ý hoặc gõ tay 1 đến 50 (nhớ theo từng trình duyệt và từng trang), 10 dòng nằm gọn trong một màn hình từ khổ khoảng 1366px chiều rộng và 640px chiều cao trở lên. Thông báo trạng thái (đã thêm, đã lưu, lỗi) hiện nổi ở góc trên bên phải và tự ẩn, không đẩy bảng xuống. Quản lý chuồng phân trang ở phía giao diện vì API trả cả danh sách. Mỗi trang có hằng số riêng (`--chrome-tall`, `--chrome-short` trong `app.css`) bằng chiều cao phần quanh bảng; nếu thêm thành phần mới phía trên bảng thì đo lại và chỉnh các hằng số này.
