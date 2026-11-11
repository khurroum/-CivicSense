
using CivicSense.Entities;

namespace CivicSense.Facade.Interface
{
    public interface IIssueWorkflowService
    {
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
        Task<IssueComment> AddAsync(
            int issueId, int actorId,
            string content, bool isInternal);

        Task<List<IssueComment>> GetByIssueAsync(
            int issueId, int actorId);
    }

    public interface IIssueConfirmationService
    {
        Task ConfirmAsync(
            int issueId, int actorId, bool confirmed);
    }

    public interface IFeedbackService
    {
        Task<Feedback> SubmitAsync(
            int issueId, int actorId, int rating,
            bool resolutionConfirmed, string? comment);
    }

    public interface INotificationService
    {
        Task<List<Notification>> GetMineAsync(int actorId);

        Task MarkReadAsync(int id, int actorId);
    }

    public interface IAIAnalysisService
    {
        Task<AIAnalysis> RecordSuggestionAsync(
            AIAnalysis suggestion);

        Task ReviewAsync(int analysisId, int adminId);
    }

    public interface IServiceProviderService
    {
        Task<ServiceProvider> RegisterAsync(
            int actorId, string businessName,
            string serviceType, string contactNumber,
            string serviceArea);

        Task VerifyAsync(int providerId, int adminId);

        Task<List<ServiceProvider>> GetVerifiedAsync();
    }

    public interface IServiceRequestService
    {
        Task<ServiceRequest> CreateAsync(
            int actorId, int providerId,
            int? issueId, string description,
            string serviceAddress);

        Task UpdateStatusAsync(
            int requestId, int actorId,
            ServiceRequestStatus newStatus);
    }

    public interface IPaymentService
    {
        Task<List<PaymentTransaction>> GetByRequestAsync(
            int serviceRequestId, int actorId);
    }
}
