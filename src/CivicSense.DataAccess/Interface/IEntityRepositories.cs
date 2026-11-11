
using CivicSense.Entities;

namespace CivicSense.DataAccess.Interface
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetActiveAsync();
    }

    public interface IIssueRepository : IRepository<Issue>
    {
        Task<List<Issue>> GetByReporterAsync(int reporterId);
        Task<Issue?> GetDetailsAsync(int issueId);
    }

    public interface IIssueImageRepository : IRepository<IssueImage>
    {
    }

    public interface IIssueAssignmentRepository
        : IRepository<IssueAssignment>
    {
        Task<IssueAssignment?> GetActiveByIssueAsync(int issueId);
    }

    public interface IIssueStatusHistoryRepository
        : IRepository<IssueStatusHistory>
    {
    }

    public interface IIssueCommentRepository
        : IRepository<IssueComment>
    {
        Task<List<IssueComment>> GetByIssueAsync(int issueId);
    }

    public interface IIssueConfirmationRepository
        : IRepository<IssueConfirmation>
    {
        Task<IssueConfirmation?> GetByIssueAndUserAsync(
            int issueId, int userId);
    }

    public interface INotificationRepository
        : IRepository<Notification>
    {
        Task<List<Notification>> GetUnreadByUserAsync(int userId);
        Task<List<Notification>> GetByUserAsync(int userId);
    }

    public interface IAIAnalysisRepository
        : IRepository<AIAnalysis>
    {
    }

    public interface IFeedbackRepository : IRepository<Feedback>
    {
        Task<Feedback?> GetByIssueAndUserAsync(
            int issueId, int userId);
    }

    public interface IServiceProviderRepository
        : IRepository<ServiceProvider>
    {
        Task<List<ServiceProvider>> GetVerifiedAsync();
        Task<ServiceProvider?> GetByUserIdAsync(int userId);
    }

    public interface IServiceRequestRepository
        : IRepository<ServiceRequest>
    {
    }

    public interface IPaymentTransactionRepository
        : IRepository<PaymentTransaction>
    {
        Task<List<PaymentTransaction>> GetByRequestAsync(
            int serviceRequestId);
    }
}
