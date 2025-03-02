Made with ❤️ by Conjuring0107

# Cảnh báo mực nước 🌊
**Theo dõi mực nước dễ dàng và chính xác**

![Banner](https://via.placeholder.com/800x200.png?text=WaterLevelMonitor)  
*Một dự án đơn giản nhưng mạnh mẽ để giám sát mực nước.*

---

## Giới thiệu
Cảnh báo mực nước là công cụ giúp bạn theo dõi mực nước dựa theo hệ thống quan trắc của Thủy điện, dùng để cảnh báo khi mực nước tăng cao và 
hạ thấp đột ngột (khi đang chạy máy) - Nhìn chung, nó giống một cái đồng hồ báo thức giúp bạn yên tâm nghỉ ngơi hoặc làm việc khác khi đang
trong ca trực.

### Tính năng
- Cảnh báo khi mực nước bất thường.
- Hỗ trợ giao diện đơn giản.
- Sẽ còn update thêm chức năng.

---

## Cài đặt
- Hệ điều hành: Windows
- Công cụ build: Microsoft Visual Studio Community (2022) - .NET desktop development.

### Hướng dẫn
1. Cài đặt Nuget gồm:
	+ Playwraight
	+ Serilog
	+ Serilog.Sinks
2. Thông qua python hoặc bất cứ gì cài đặt Playwraight. Ví dụ python:
	```pip install playwright
	playwright install```
	Sau đó tìm đường dẫn chứa ms-playwright và coppy nguyên folder (nếu muốn nó tương tích với cả firefox, ms edge, chronium)
hoặc chỉ ms-playwright\chromium_headless_shell-xxxx (nếu chỉ muốn dùng chronium) đưa vào folder Release (hoặc Debug). Nó sẽ có
đường dẫn dạng: 
	```bin\Release\ms-playwright\chromium_headless_shell-xxxx```
hoặc 
	```bin\Debug\ms-playwright\chromium_headless_shell-xxxx```
3. Trong thư mục ...\bin\Debug hoặc bin\Release cần tạo folder \config\credentials.txt dùng để chứa tài khoản và mật khẩu đăng nhập vào trang web quan trắc
encryption bởi AES.
	Trong CredentialManager.cs chứa khóa IV và Key, nếu muốn thay đổi thì sửa ở đây.
4. Tạo credentials.txt trong folder Encryption (nhớ đồng bộ khóa Key và IV), thay user và pass thành tài khoản và mật khẩu của bạn.
5. Chạy ở chế độ Debug sẽ có nhiều log hơn để tìm kiếm và sửa lỗi hoặc bug, chế độ Release chỉ giữ phần nhỏ logs, chủ yếu là quản lý networklog
dùng để giám sát khi có vấn đề về hệ thống mạng.
6. .......................