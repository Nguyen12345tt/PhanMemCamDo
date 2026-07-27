using PhanMemCamDo.Models;
using PhanMemCamDo.Models.Entities;
using PhanMemCamDo.Models.Enums;
using System;

namespace PhanMemCamDo.Services
{
    public class PawnCalculator
    {
        // Cấu hình lãi phạt chung (VD: 150% lãi thường hoặc cố định)
        private const decimal HE_SO_PHAT = 1.5m;

        /// <summary>
        /// Hàm tính toán đa năng: Hỗ trợ Ngày, Tuần, Tháng
        /// </summary>
        public decimal CalculateTotalPayment(PawnContract contract, DateTime? returnDate = null)
        {
            if (contract == null) return 0;

            DateTime payDate = returnDate ?? DateTime.Now;

            // 1. Tính số ngày thực tế khách đã cầm
            TimeSpan duration = payDate - contract.StartDate;
            int totalDays = (int)Math.Ceiling(duration.TotalDays);
            if (totalDays < 1) totalDays = 1; // Tối thiểu là 1 ngày

            decimal totalInterest = 0;

            // 2. Phân loại cách tính dựa trên InterestType lưu trong DB
            switch (contract.InterestType)
            {
                case InterestType.TheoNgay:
                    // Công thức: Tiền cầm * Lãi/ngày * Số ngày
                    // Lưu ý: InterestRate ở đây hiểu là % hoặc số tiền/1tr/ngày tùy quy ước. 
                    // Giả sử InterestRate lưu dạng % (VD: 0.3% = 0.003)
                    totalInterest = contract.PawnAmount * (contract.InterestRate / 100) * totalDays;
                    break;

                case InterestType.TheoTuan:
                    // Quy tắc: 1 tuần = 7 ngày. Lố 1 ngày tính sang tuần mới.
                    int weeks = (int)Math.Ceiling((double)totalDays / 7);
                    totalInterest = contract.PawnAmount * (contract.InterestRate / 100) * weeks;
                    break;

                case InterestType.TheoThang:
                    // Quy tắc: 1 tháng = 30 ngày. Lố 1 ngày tính sang tháng mới.
                    int months = (int)Math.Ceiling((double)totalDays / 30);
                    totalInterest = contract.PawnAmount * (contract.InterestRate / 100) * months;
                    break;
            }

            // 3. Xử lý Phạt Quá Hạn (Nếu ngày trả > ngày hết hạn)
            if (payDate > contract.EndDate)
            {
                TimeSpan overdueDuration = payDate - contract.EndDate;
                int overdueDays = (int)Math.Ceiling(overdueDuration.TotalDays);

                // Tính thêm tiền phạt cho những ngày quá hạn
                // Logic: Lãi những ngày quá hạn sẽ nhân hệ số phạt (VD: gấp 1.5 lần)
                decimal dailyPenaltyRate = (contract.InterestRate / 100) * HE_SO_PHAT;

                // Nếu đang tính theo tuần/tháng thì phải quy đổi về lãi ngày để phạt cho chính xác từng ngày
                if (contract.InterestType == InterestType.TheoTuan) dailyPenaltyRate /= 7;
                if (contract.InterestType == InterestType.TheoThang) dailyPenaltyRate /= 30;

                decimal penaltyAmount = contract.PawnAmount * dailyPenaltyRate * overdueDays;

                totalInterest += penaltyAmount;
            }

            return contract.PawnAmount + totalInterest;
        }
    }
}