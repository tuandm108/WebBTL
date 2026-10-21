# Smart Livestock

Mẫu nghiên cứu hệ thống giám sát chăn nuôi dùng .NET 8, SQL Server và MQTT. Có thể chạy trên Windows bằng Visual Studio mà không cần Docker. Tiến độ chức năng được ghi tại [docs/IMPLEMENTATION_PROGRESS.md](docs/IMPLEMENTATION_PROGRESS.md).

## Đăng nhập và quản lý tài khoản

Trước lần chạy đầu tiên của phiên bản có đăng nhập, tạo tài khoản admin và khóa riêng giữa Web và API:

```powershell
.\scripts\setup-local-auth.ps1 -AdminEmail 'admin@example.com' -AdminPassword (Read-Host 'Mật khẩu admin' -AsSecureString)
dotnet ef database update --project src/SmartLivestock.Infrastructure --startup-project src/SmartLivestock.Api
```

Chọn email của bạn và mật khẩu mạnh (12–128 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt). Script kiểm tra quy tắc trước khi lưu vào `deploy/native/auth.env`, tệp đã được Git bỏ qua. Nếu mật khẩu khởi tạo chưa hợp lệ và Web chưa tạo admin, chạy lại cùng lệnh để nhập mật khẩu khác; script giữ nguyên khóa API đã tạo. Web tạo admin khi khởi động lần đầu rồi xóa mật khẩu khởi tạo khỏi tệp; mật khẩu trong SQL Server được Identity băm. Không đặt mật khẩu hoặc khóa API vào `appsettings.json`. Với môi trường khác, cung cấp `Auth:ApiKey` cho cả Web và API và `BootstrapAdmin:Email`/`BootstrapAdmin:Password` cho Web qua biến môi trường hoặc kho bí mật. Hai tiến trình phải dùng cùng khóa API.

Mở Web tại `http://localhost:5297/login`. Người dùng khác chọn **Tạo tài khoản** và đăng ký email, mật khẩu. Họ chỉ đăng nhập được sau khi admin vào **Quản lý tài khoản** để chấp thuận. Trang này liệt kê các tài khoản và trạng thái theo từng trang, hỗ trợ tìm kiếm theo một phần email và xóa bộ lọc; admin có thể khóa/mở khóa tài khoản con, từ chối hoặc xóa tài khoản con, đổi mật khẩu của chính mình hoặc cấp lại mật khẩu cho tài khoản khác. Xóa vĩnh viễn yêu cầu nhập đúng email và mật khẩu admin hiện tại; tài khoản admin không thể bị khóa hoặc xóa tại đây. Tài khoản đã khóa không thể đăng nhập, phiên đang mở bị kiểm tra lại trong tối đa khoảng một phút. Khi đổi/cấp lại mật khẩu, admin phải nhập mật khẩu hiện tại của mình. Đổi mật khẩu admin sẽ đăng xuất phiên hiện tại; phiên có mật khẩu cũ được kiểm tra lại trong tối đa khoảng một phút. Hệ thống không gửi mật khẩu qua email, vì vậy admin cần chuyển mật khẩu được cấp lại cho người dùng qua kênh riêng. Chạy Web bằng HTTPS khi dùng ngoài máy phát triển; không truyền mật khẩu qua HTTP trên mạng LAN.

API và SignalR yêu cầu khóa riêng trong header `X-SmartLivestock-Api-Key`; Web gắn khóa ở phía máy chủ. Khóa này dành cho kết nối Web → API, không chia sẻ cho trình duyệt hoặc thiết bị IoT. Nếu trước đây dùng Swagger/API trực tiếp, cần chuyển sang xác thực phù hợp cho công cụ đó. Việc xác thực email bằng thư xác nhận và thu hồi quyền API theo từng người còn là bước tiếp theo.

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

   Kiểm tra `Test-NetConnection 127.0.0.1 -Port 1884`: kết quả cần có `TcpTestSucceeded : True`. Mosquitto chạy ở cổng 1883 không nhận dữ liệu của dự án.

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

5. Nhấn **F5**. Mở `http://localhost:5297` để xem Web; API có Swagger tại `http://localhost:5215/swagger` và kiểm tra trạng thái SQL cùng MQTT tại `http://localhost:5215/health/ready`. Simulator gửi dữ liệu mẫu mỗi 5 giây; đợi vài giây rồi làm mới trang Web nếu chưa thấy dữ liệu. Dùng **Shift+F5** để dừng các project, rồi dừng broker bằng **Ctrl+C** trong terminal.

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

Simulator hỗ trợ biến `SCENARIO`: `NORMAL`, `HIGH_TEMPERATURE`, `HIGH_HUMIDITY`, `HIGH_AMMONIA`, `HIGH_H2S`, `SENSOR_FAILURE`, `DEMO_ALERTS`.

Đây là bản mẫu nghiên cứu; quản lý tài khoản nâng cao, liên động điều khiển và một số chức năng trong yêu cầu tổng thể vẫn đang được phát triển. Khi triển khai thực tế cần cấu hình SQL Server, TLS, tài khoản riêng cho thiết bị và kiểm định an toàn vận hành.

## Xem trên điện thoại

Giao diện Web tự chuyển sang bố cục điện thoại (thanh điều hướng dưới đáy, chuồng hiển thị dạng thẻ) khi màn hình rộng không quá 820px. Để mở từ điện thoại cùng mạng Wi-Fi:

1. Chạy `SmartLivestock.Web` bằng profile **lan** (thay cho **http**) để Web lắng nghe trên mọi địa chỉ mạng. API và Simulator giữ nguyên, vì Web gọi API từ phía máy chủ.
2. Cho phép cổng 5297 qua Windows Firewall (PowerShell chạy bằng quyền Administrator, chỉ cần làm một lần):

   ```powershell
   New-NetFirewallRule -DisplayName 'SmartLivestock Web 5297' -Direction Inbound -Protocol TCP -LocalPort 5297 -Action Allow -Profile Private
   ```

3. Trên điện thoại mở `http://<IP máy tính>:5297` (xem IP bằng `ipconfig`, mục IPv4 Address).

Chỉ dùng trên mạng nội bộ tin cậy nếu chạy bằng HTTP: đăng nhập đã được bổ sung nhưng mật khẩu và cookie cần HTTPS khi dùng qua mạng LAN.

## Mô phỏng dữ liệu chạm ngưỡng cảnh báo

Simulator có kịch bản `DEMO_ALERTS` (mặc định khi chạy bằng F5, qua profile `demo-alerts` trong `simulators/SmartLivestock.IoTSimulator/Properties/launchSettings.json`): mỗi chuồng thứ 3 (3, 6, 9, 12, 15, 18) tăng dần một chỉ số qua mức cảnh báo, mức nguy hiểm rồi hồi phục, chu kỳ 3 phút và lệch pha giữa các chuồng; chuồng 7 mất cảm biến; các chuồng còn lại bình thường. Chọn profile `normal` để về dữ liệu bình thường. Mức cảnh báo/nguy hiểm trong `DemoAlertProfile` chỉ là số mô phỏng, không phải chuẩn an toàn cho vật nuôi. Thiết kế hộp thoại cảnh báo nằm tại [docs/ALERT_DESIGN.md](docs/ALERT_DESIGN.md).

## Gắn vật nuôi với chuồng

Mở **Vật nuôi** ở menu bên trái để thêm từng con với mã riêng, loài, chuồng hiện tại và tên (nếu có). Danh mục chọn sẵn gồm bò, trâu, heo, gà, vịt, ngan, ngỗng, dê, cừu, thỏ, chim cút và bồ câu. Chọn **Chi tiết / sửa** để mở hộp thoại chỉnh sửa hoặc chuyển chuồng. Có thể mở danh sách đã lọc từ nút **Vật nuôi** trên trang Quản lý chuồng hoặc trang Giám sát. Migration tạo 2 con **dữ liệu mẫu** cho mỗi chuồng mẫu có đủ sức chứa; chúng có mã `DEMO-xx-xx` và nhãn riêng, có thể sửa hoặc xóa. Dữ liệu người dùng đã nhập được giữ nguyên, kể cả tên loài cũ; màn hình giám sát gộp các tên cũ tương ứng với mã loài chuẩn. API không cho hai con trùng mã trong cùng trang trại, không cho số con vượt sức chứa của chuồng, và không cho xóa chuồng khi còn vật nuôi. Số con và cơ cấu loài hiển thị theo chuồng trên trang Giám sát. **Chỉ số cảm biến và cảnh báo hiện áp dụng cho chuồng; tên loài và từng con chưa ảnh hưởng đến ngưỡng cảnh báo, và hệ thống chưa đánh giá sức khỏe riêng từng con.** Sau khi cập nhật mã nguồn, chạy migration ở bước 3 rồi khởi động lại API và Web trong Visual Studio.

## Cảnh báo realtime và lịch sử lỗi

- **Phát hiện ở máy chủ:** mỗi tin nhắn cảm biến được so với `DemoAlertProfile`. Một "đợt lỗi" (từ lúc vượt ngưỡng đến lúc hết) của mỗi chuồng và chỉ số được lưu thành một bản ghi (bảng `Alerts`), không bị trùng khi còn đang diễn ra. Có độ trễ chống nhấp nháy: cảnh báo chỉ hết khi giá trị xuống thấp hơn ngưỡng một khoảng nhỏ. Cảm biến báo dữ liệu không hợp lệ tạo một đợt "Cảm biến lỗi" riêng cho chuồng.
- **Hộp thoại tóm tắt lỗi không chen ngang công việc.** Trang **Cài đặt** (lưu theo trình duyệt) cho chọn khi nào hộp thoại được hiện:
  - **Một lần mỗi ngày vào giờ cố định**, mặc định **23:59**.
  - **Lặp lại theo chu kỳ**: 15, 30 phút, 1, 2 hoặc 4 giờ.
  - **Ngay lập tức** khi có cảnh báo nguy hiểm hoặc lỗi cảm biến (hành vi gián đoạn nhất).
  - **Không bao giờ hiện hộp thoại.**
  - Tùy chọn **Cũng hiện khi mở phần mềm** (mặc định tắt).
  Đến giờ, hộp thoại tóm tắt các chuồng lỗi từ lần báo trước đến lúc đó; nếu không có lỗi nào thì không hiện gì. Giờ cố định đã trôi qua lúc tab đóng sẽ hiện một lần ở lần mở kế tiếp. Nút "Xem thử ngay" trong Cài đặt cho xem trước (không tính là đã xem lỗi). Giờ tính theo giờ của máy chủ.
- **Trang Lịch sử lỗi:** lọc theo khoảng thời gian (1 giờ, 24 giờ, 7 ngày, tất cả), chuồng, mức độ, trạng thái; có phân trang và tự cập nhật. Mỗi dòng hiển thị số con và cơ cấu loài **hiện tại** của chuồng cùng thời điểm đánh dấu đã đọc. Thông tin vật nuôi này không phải ảnh chụp tại lúc phát sinh cảnh báo; dữ liệu cũ không lưu ảnh chụp đó. Nút "Xem lịch sử lỗi" trong hộp thoại mở trang này với đúng khung giờ.
- **Đã đọc:** mỗi cảnh báo có trạng thái đã đọc / chưa đọc, hiện vẫn dùng chung giữa các tài khoản; chưa có xác nhận theo từng người. Trang Lịch sử lỗi hiện dấu chấm và chữ đậm cho cảnh báo chưa đọc; mỗi dòng có nút "Đã đọc" / "Bỏ đã đọc"; nút "Đánh dấu tất cả kết quả đã đọc" áp dụng cho mọi cảnh báo khớp bộ lọc hiện tại (kể cả các trang khác); có bộ lọc "Đọc". Huy hiệu trên menu đếm số chưa đọc. Hộp thoại tóm tắt chỉ liệt kê cảnh báo chưa đọc và có nút "Đánh dấu đã đọc". Cảnh báo leo thang (ví dụ cảnh báo lên nguy hiểm) tự quay về chưa đọc.
- **Phím Esc hoặc nút Đóng** chỉ đóng hộp thoại, không đánh dấu đã đọc và không "xác nhận" cảnh báo (xác nhận có ghi người thực hiện cần đăng nhập, F08).
- **Cập nhật cơ sở dữ liệu:** chạy `dotnet ef database update` (bước 3 phần "Chạy bằng Visual Studio") để có bảng `Alerts`.

## Trang Giám sát: số chuồng mỗi trang

Mặc định hiển thị **10 chuồng mỗi trang**. Ô "Số chuồng/trang" ở cuối bảng cho chọn gợi ý (5, 10, 20, 50) hoặc **gõ tay** một số từ 1 đến 50 (số lớn hơn được đưa về 50, nhỏ hơn về 1). Giá trị được nhớ theo từng trình duyệt.

Trên màn hình máy tính, chiều cao mỗi dòng của bảng tự co giãn theo chiều cao cửa sổ để **đủ 10 dòng, phân trang và ô chọn chuồng nằm gọn trong một màn hình, không phải cuộn trang** (từ khoảng 640px chiều cao trở lên; cửa sổ thấp hơn sẽ gọn hơn nữa: bỏ dòng mô tả, bỏ icon thẻ thống kê, mã chuồng nằm cùng dòng tên). Nếu đặt nhiều hơn 10 chuồng mỗi trang thì trang cuộn như bình thường. Khổ điện thoại giữ nguyên dạng thẻ.

Cụm phân trang (dùng chung cho trang Giám sát và Lịch sử lỗi) gồm nút mũi tên trước/sau, số trang với trang đang chọn tô màu, dấu "…" kèm nhảy nhanh về trang đầu/cuối khi có nhiều trang. Ở trang Giám sát, thông tin "Hiển thị 1–10 trong 19 chuồng" nằm bên trái, số trang ở giữa, ô "Số chuồng/trang" bên phải.

Trang **Lịch sử lỗi** và **Quản lý chuồng** dùng cùng cách phân trang: mặc định 10 dòng mỗi trang, ô "Số lỗi/trang" và "Số chuồng/trang" cho chọn gợi ý hoặc gõ tay 1 đến 50 (nhớ theo từng trình duyệt và từng trang). Trên desktop, 10 dòng mặc định được giãn theo chiều cao màn hình; khi chọn nhiều dòng hơn hoặc nội dung dài, trang sẽ cuộn khi cần. Thông báo trạng thái (đã thêm, đã lưu, lỗi) hiện nổi ở góc trên bên phải và tự ẩn, không đẩy bảng xuống. Quản lý chuồng phân trang ở phía giao diện vì API trả cả danh sách. Mỗi trang có hằng số riêng (`--chrome-tall`, `--chrome-short` trong `app.css`) bằng chiều cao phần quanh bảng; nếu thêm thành phần mới phía trên bảng thì đo lại và chỉnh các hằng số này.
