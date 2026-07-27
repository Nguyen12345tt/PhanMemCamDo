using PhanMemCamDo.Models.Entities;

namespace PhanMemCamDo.Models.ViewModels
{
    public class DashboardVM
    {
        // 1. Các con số tổng quan
        public int TotalCustomers { get; set; }        // Tổng khách
        public int ActiveContracts { get; set; }       // Hợp đồng đang vay
        public decimal TotalLoanAmount { get; set; }   // Tổng tiền đang cho vay (Tiền gốc)
        public decimal TotalProfit { get; set; }       // Tổng lãi đã thu (tạm tính từ PaymentHistory)

        // 2. Cảnh báo quan trọng
        public int ContractsDueSoon { get; set; }      // Sắp hết hạn (còn 3 ngày)

        // 3. Danh sách mới nhất
        public List<PawnContract> RecentContracts { get; set; } = []; // 5 Hợp đồng mới nhất
    }
}