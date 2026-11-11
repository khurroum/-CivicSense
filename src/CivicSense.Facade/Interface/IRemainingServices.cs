
using CivicSense.Entities;
using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Interface
{
    public interface IIssueWorkflowService
    {
        // ADMIN QUERY OPERATIONS

        Task<List<WorkflowQueueItemDto>>
            GetAdminQueueAsync(int adminId);

        Task<List<WorkflowStaffOptionDto>>
            GetActiveStaffAsync(int adminId);

        // STAFF QUERY OPERATIONS

        Task<List<WorkflowAssignmentDto>>
            GetMyAssignmentsAsync(int staffId);

        // RESTRICTED ISSUE HISTORY

        Task<List<WorkflowHistoryDto>>
            GetHistoryAsync(int issueId, int actorId);

        // EXISTING WORKFLOW OPERATIONS

        Task ReviewAsync(int issueId, int adminId);

        Task AssignAsync(
            int issueId, int staffId, int adminId);

        Task StartWorkAsync(int issueId, int staffId);

        Task ResolveAsync(int issueId, int staffId);

        Task RejectAsync(
            int issueId, int adminId, string reason);
    }

    public interface IIssueCommentService
    {
        Task<CommentDto> AddAsync(
            int issueId,
            int actorId,
            CommentCreateDto dto);

        Task<List<CommentDto>> GetByIssueAsync(
            int issueId, int actorId);
    }

    public interface IIssueConfirmationService
    {
        Task ConfirmAsync(
            int issueId,
            int actorId,
            bool confirmed);
    }

    public interface IFeedbackService
    {
        Task<FeedbackDto> SubmitAsync(
            int issueId,
            int actorId,
            FeedbackCreateDto dto);
    }

    public interface INotificationService
    {
        Task<List<NotificationDto>> GetMineAsync(
            int actorId);

        Task MarkReadAsync(int id, int actorId);
    }

    public interface IAIAnalysisService
    {
        // Trusted server-side AI integration only.
        Task<AIAnalysisDto> RecordSuggestionAsync(
            AIAnalysisCreateDto dto);

        Task ReviewAsync(
            int analysisId, int adminId);
    }

    public interface IServiceProviderService
    {
        Task<ServiceProviderDto> RegisterAsync(
            int actorId,
            ServiceProviderCreateDto dto);

        Task VerifyAsync(
            int providerId, int adminId);

        Task<List<ServiceProviderDto>> GetVerifiedAsync();
    }

    public interface IServiceRequestService
    {
        Task<ServiceRequestDto> CreateAsync(
            int actorId,
            ServiceRequestCreateDto dto);

        Task UpdateStatusAsync(
            int requestId,
            int actorId,
            ServiceRequestStatus newStatus);
    }

    public interface IPaymentService
    {
        Task<List<PaymentTransactionDto>> GetByRequestAsync(
            int serviceRequestId, int actorId);
    }
}
