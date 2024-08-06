using Amazon.S3.Model;
using BuildingManager.Helpers;
using BuildingManager.Models.Dto;
using BuildingManager.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildingManager.Contracts.Services
{
    public interface IPaymentRequestService
    {
        Task<SuccessResponse<PaymentRequestDto>> CreatePaymentRequestOtherPro (PaymentRequestReqDto model, string userId);
        Task<SuccessResponse<PaymentRequestDto>> CreatePaymentRequestPm (PaymentRequestPmReqDto model, string userId);
        Task<PageResponse<IList<PaymentRequestDto>>> GetPaymentRequestsOtherPro(PaymentRequestReqPagedDto model, string UserId);
        //Task<PageResponse<IList<PaymentRequestAndMemberDto>>> GetPaymentRequestsPM(PaymentRequestDtoPaged model);
        Task<PageResponse<IList<PaymentRequestAndMemberDto>>> GetPaymentRequestsPM(PaymentRequestReqPagedDto model);
        Task<SuccessResponse<IList<PayReqMonthlyDataDto>>> GetPayReqMonthlyData(string projectId, int year);
        Task<SuccessResponse<IList<PayReqWeeklyDataDto>>>  GetPayReqWeeklyData(string projectId, int year, int month);
        Task<SuccessResponse<IList<PayReqDailyDataDto>>> GetPayReqDailyData(string projectId, int year, int month, int week);
        Task<SuccessResponse<PaymentRequestDto>> PaymentRequestConfirmation(PaymentRequestStatusUpdateDto model);
        Task<SuccessResponse<PaymentRequestDto>> SendPaymentRequestForConfirmation(PaymentRequestStatusUpdateDto model, string userId);
        Task<SuccessResponse<PaymentRequestDto>> UpdatePendingPaymentRequest(UpdateGroupPaymentRequestDto model, string userId);
        Task<SuccessResponse<PaymentRequestDto>> UpdatePaymentRequestPm(UpdateGroupPaymentRequestPmDto model, string userId);
        Task DeletePaymentRequest(string projId, string paymentRequestId, string userId);
        Task DeletePaymentRequestPM(string projId, string paymentRequestId, string userId);
        Task<SuccessResponse<PaymentRequestDto>> UpdatePendingPaymentRequestFile(AddPaymentRequestFileReqDto model, string userId);
        Task<SuccessResponse<PaymentRequestDto>> UpdateConfirmationPaymentRequestFile(AddPaymentRequestFileReqDto model);
        Task<SuccessResponse<PaymentRequestDto>> DeletePaymentRequestFileOtherPro(PaymentRequestFileDto model, string userId);
        public Task<SuccessResponse<PaymentRequestDto>> DeleteConfirmationPaymentRequestFilePM(PaymentRequestFileDto model);
        Task<GetObjectResponse> DownloadPayReqFileOtherPro(PaymentRequestFileDto model, string userId);
        Task<GetObjectResponse> DownloadPayReqFilePM(PaymentRequestFileDto model);
        Task<GetObjectResponse> DownloadPayReqConfirmationFileOtherPro(PaymentRequestFileDto model, string userId);
        Task<GetObjectResponse> DownloadPayReqConfirmationFilePM(PaymentRequestFileDto model);

    }
}
