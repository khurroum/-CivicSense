
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
    }

    public interface IIssueConfirmationRepository
        : IRepository<IssueConfirmation>
    {
    }

    public interface INotificationRepository
        : IRepository<Notification>
    {
        Task<List<Notification>> GetUnreadByUserAsync(int userId);
    }

    public interface IAIAnalysisRepository
        : IRepository<AIAnalysis>
    {
    }

    public interface IFeedbackRepository : IRepository<Feedback>
    {
    }

    public interface IServiceProviderRepository
        : IRepository<ServiceProvider>
    {
        Task<List<ServiceProvider>> GetVerifiedAsync();
    }

    public interface IServiceRequestRepository
        : IRepository<ServiceRequest>
    {
    }

    public interface IPaymentTransactionRepository
        : IRepository<PaymentTransaction>
    {
    }
}
