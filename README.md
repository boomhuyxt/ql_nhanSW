Dưới đây là phiên bản đã được làm sạch (xóa `[cite: ...]`, chuẩn hóa format) để bạn copy dễ dàng:

---

# 🏢 HRM System - Phần mềm Quản lý Nhân sự tích hợp AI

Hệ thống quản trị nhân sự hiện đại giúp doanh nghiệp tối ưu hóa quy trình vận hành thông qua tự động hóa và trí tuệ nhân tạo.

## 🌟 Tính năng chính

* **Chấm công FaceID**: Tự động ghi nhận thời gian vào/ra thông qua nhận diện khuôn mặt.
* **Trợ lý ảo AI (Gemini)**: Hỗ trợ tra cứu nội bộ, tạo nhanh đơn nghỉ phép/tăng ca qua hội thoại và tóm tắt báo cáo nhân sự.
* **Quản lý lương tự động**: Tính toán thu nhập dựa trên ngày công thực tế, KPI, phụ cấp và các khoản khấu trừ (BHXH, thuế TNCN).
* **Quản trị đơn từ**: Quy trình tạo, gửi và phê duyệt đơn trực tuyến với trạng thái cập nhật thời gian thực.
* **Hồ sơ nhân sự**: Lưu trữ thông tin chi tiết, lịch sử công tác, hợp đồng lao động và quản lý file minh chứng.

## 🛠 Công nghệ sử dụng

* **Ngôn ngữ**: C#
* **Framework**: .NET Ecosystem (WPF, ASP.NET Core)
* **Cơ sở dữ liệu**: SQL Server / MySQL
* **AI Integration**: Google Gemini API (Phản hồi < 15s)
* **Bảo mật**: Mã hóa mật khẩu SHA-256 và phân quyền người dùng (Role-based)

## 📊 Kiến trúc dữ liệu

Hệ thống được thiết kế theo mô hình quan hệ xoay quanh thực thể **NhanVien**, bao gồm các phân hệ chính:

* **Quản lý Tổ chức**: `PhongBan`, `LichSuCongTac`
* **Quản lý Nghiệp vụ**: `ChamCong`, `HopDongLaoDong`, `NghiPhep`
* **Quản lý Tài chính**: `BangLuong`
* **Quản lý Hệ thống**: `TaiKhoan`, `VaiTro`, `TepTin`

## 💻 Giao diện tiêu biểu

* **Bảng điều khiển**: Dashboard cá nhân hiển thị lương tháng gần nhất và trạng thái chấm công
* **Hệ thống FaceID**: Giao diện quét khuôn mặt thời gian thực
* **Quản trị & Phê duyệt**: Giao diện dành cho Admin để quản lý phòng ban và duyệt nhân sự mới

---

© 2026 - Phát triển bởi nhóm **FIGHT CLUB**

---
