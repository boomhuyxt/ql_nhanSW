# 🏢 HRM System - Phần mềm Quản lý Nhân sự tích hợp AI

[cite_start]Hệ thống quản trị nhân sự hiện đại giúp doanh nghiệp tối ưu hóa quy trình vận hành thông qua tự động hóa và trí tuệ nhân tạo[cite: 36, 38].

## 🌟 Tính năng chính

* [cite_start]**Chấm công FaceID**: Tự động ghi nhận thời gian vào/ra thông qua nhận diện khuôn mặt[cite: 45, 89].
* [cite_start]**Trợ lý ảo AI (Gemini)**: Hỗ trợ tra cứu nội bộ, tạo nhanh đơn nghỉ phép/tăng ca qua hội thoại và tóm tắt báo cáo nhân sự[cite: 106, 108, 109].
* [cite_start]**Quản lý lương tự động**: Tính toán thu nhập dựa trên ngày công thực tế, KPI, phụ cấp và các khoản khấu trừ (BHXH, thuế TNCN)[cite: 93, 209].
* [cite_start]**Quản trị đơn từ**: Quy trình tạo, gửi và phê duyệt đơn trực tuyến với trạng thái cập nhật thời gian thực[cite: 96, 97].
* [cite_start]**Hồ sơ nhân sự**: Lưu trữ thông tin chi tiết, lịch sử công tác, hợp đồng lao động và quản lý file minh chứng[cite: 86, 126, 127].

## 🛠 Công nghệ sử dụng

* [cite_start]**Ngôn ngữ**: C#[cite: 53].
* **Framework**: .NET Ecosystem (WPF, ASP.NET Core).
* [cite_start]**Cơ sở dữ liệu**: SQL Server / MySQL[cite: 169, 196].
* [cite_start]**AI Integration**: Google Gemini API (Phản hồi < 15s)[cite: 106, 107].
* [cite_start]**Bảo mật**: Cơ chế mã hóa mật khẩu SHA-256 và phân quyền người dùng (Role-based)[cite: 73, 79, 101].

## 📊 Kiến trúc dữ liệu

[cite_start]Hệ thống được thiết kế theo mô hình quan hệ xoay quanh thực thể **NhanVien**, bao gồm các phân hệ chính[cite: 124, 125]:
* **Quản lý Tổ chức**: `PhongBan`, `LichSuCongTac`.
* **Quản lý Nghiệp vụ**: `ChamCong`, `HopDongLaoDong`, `NghiPhep`.
* **Quản lý Tài chính**: `BangLuong`.
* **Quản lý Hệ thống**: `TaiKhoan`, `VaiTro`, `TepTin`.

## 💻 Giao diện tiêu biểu

* [cite_start]**Bảng điều khiển**: Dashboard cá nhân hiển thị lương tháng gần nhất và trạng thái chấm công[cite: 199, 201].
* [cite_start]**Hệ thống FaceID**: Giao diện quét khuôn mặt thời gian thực[cite: 205].
* [cite_start]**Quản trị & Phê duyệt**: Giao diện dành cho Admin để quản lý phòng ban và duyệt nhân sự mới[cite: 211, 213].

---
[cite_start]© 2026 - Phát triển bởi nhóm **FIGHT CLUB**[cite: 56, 152].
