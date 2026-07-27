namespace PhanMemCamDo.Models.Enums
{
    public enum InterestType
    {
        TheoNgay = 0,   // Tính tiền theo từng ngày (VD: 3k/ngày)
        TheoTuan = 1,   // Tính theo block tuần (VD: 1 tuần 20k, lố 1 ngày tính sang tuần 2)
        TheoThang = 2   // Tính theo block tháng
    }

    public enum ContractStatus
    {
        Active = 0,     // Đang cầm (Hợp đồng đang chạy bình thường)
        Redeemed = 1,   // Đã chuộc (Khách đã trả đủ tiền gốc + lãi và lấy lại đồ)
        Overdue = 2,    // Quá hạn (Đã hết ngày hợp đồng nhưng khách chưa qua, đang chờ xử lý)
        Liquidated = 3  // Đã thanh lý (Khách bỏ đồ/bùng, tiệm đã bán món đồ này thu hồi vốn)
    }
}