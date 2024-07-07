using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Repository
{
    public interface IPaymentRequestRepository
    {
        Task CreatePaymentRequest(PaymentRequest paymentRequest);
        Task<(int, IList<PaymentRequestDto>)> GetPaymentRequestsOtherPro(PaymentRequestDtoPaged model, string UserId);
        Task<(int, IList<PaymentRequestAndMemberDto>)> GetPaymentRequestsPM(PaymentRequestDtoPaged model);
        Task<IList<PayReqMonthlyDataDto>> GetPayReqMonthlyData(string projectId, int year);
        Task<IList<PayReqWeeklyDataDto>>  GetPayReqWeeklyData(string projectId, int year, int month);
        Task<IList<PayReqDailyDataDto>>  GetPayReqDailyData(string projectId, int year, int month, int week);
        Task<(int, int)> UpdatePaymentConfirmationStatus(PaymentRequestStatusUpdateDto model);
        Task<(int, int)> SendPayReqForConfirmation(PaymentRequestStatusUpdateDto model, string userId);
        Task<(int, int)> UpdatePendingPaymentRequest(UpdatePaymentRequestDto model, string userId);
        Task<PaymentRequest> GetPaymentRequestDetailsOtherPro(string projId, string paymentRequestId, string userId);
        Task<PaymentRequest> GetPaymentRequestDetailsPM(string projId, string paymentRequestId);
        Task<(int, int)> DeletePaymentRequest(string projId, string paymentRequestId, string userId);
        Task<(int, int)> AddPendingPaymentRequestFile(AddPaymentRequestFileDto model, string userId);
        Task<(int, int)> AddConfirmationPaymentRequestFile(AddPaymentRequestFileDto model);
        Task<(int, int)> RemovePaymentRequestFileDetailsOtherPro(string projId, string paymentRequestId, string userId);
        public Task<(int, int)> RemoveConfirmationPaymentRequestFileDetailsPM(string projId, string paymentRequestId);

    }

}
