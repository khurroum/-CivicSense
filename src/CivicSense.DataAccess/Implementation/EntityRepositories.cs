
using CivicSense.DataAccess.Interface;
using CivicSense.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicSense.DataAccess.Implementation
{
    // 1. USER
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbContext.Users
                .AnyAsync(x => x.Email == email);
        }
    }

    // 2. CATEGORY
    public class CategoryRepository
        : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<Category>> GetActiveAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }

    // 3. ISSUE
    public class IssueRepository
        : Repository<Issue>, IIssueRepository
    {
        public IssueRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<Issue>> GetByReporterAsync(
            int reporterId)
        {
            return await _dbContext.Issues
                .AsNoTracking()
                .Where(x => x.ReporterId == reporterId
                         && !x.IsArchived)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Issue?> GetDetailsAsync(int issueId)
        {
            return await _dbContext.Issues
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Reporter)
                .Include(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(x => x.Id == issueId);
        }
    }

    // 4. ISSUE IMAGE
    public class IssueImageRepository
        : Repository<IssueImage>, IIssueImageRepository
    {
        public IssueImageRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // 5. ISSUE ASSIGNMENT
    public class IssueAssignmentRepository
        : Repository<IssueAssignment>, IIssueAssignmentRepository
    {
        public IssueAssignmentRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IssueAssignment?> GetActiveByIssueAsync(
            int issueId)
        {
            return await _dbContext.IssueAssignments
                .AsNoTracking()
                .Include(x => x.Staff)
                .FirstOrDefaultAsync(x =>
                    x.IssueId == issueId && x.IsActive);
        }
    }

    // 6. ISSUE STATUS HISTORY
    public class IssueStatusHistoryRepository
        : Repository<IssueStatusHistory>,
          IIssueStatusHistoryRepository
    {
        public IssueStatusHistoryRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // 7. ISSUE COMMENT
    public class IssueCommentRepository
        : Repository<IssueComment>, IIssueCommentRepository
    {
        public IssueCommentRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<IssueComment>> GetByIssueAsync(
            int issueId)
        {
            return await _dbContext.IssueComments
                .AsNoTracking()
                .Where(x => x.IssueId == issueId && !x.IsDeleted)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }

    // 8. ISSUE CONFIRMATION
    public class IssueConfirmationRepository
        : Repository<IssueConfirmation>,
          IIssueConfirmationRepository
    {
        public IssueConfirmationRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IssueConfirmation?>
            GetByIssueAndUserAsync(int issueId, int userId)
        {
            return await _dbContext.IssueConfirmations
                .FirstOrDefaultAsync(x =>
                    x.IssueId == issueId &&
                    x.UserId == userId);
        }
    }

    // 9. NOTIFICATION
    public class NotificationRepository
        : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<Notification>> GetUnreadByUserAsync(
            int userId)
        {
            return await _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsRead)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetByUserAsync(
            int userId)
        {
            return await _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }

    // 10. AI ANALYSIS
    public class AIAnalysisRepository
        : Repository<AIAnalysis>, IAIAnalysisRepository
    {
        public AIAnalysisRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // 11. FEEDBACK
    public class FeedbackRepository
        : Repository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<Feedback?> GetByIssueAndUserAsync(
            int issueId, int userId)
        {
            return await _dbContext.Feedbacks
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.IssueId == issueId &&
                    x.UserId == userId);
        }
    }

    // 12. SERVICE PROVIDER
    public class ServiceProviderRepository
        : Repository<ServiceProvider>,
          IServiceProviderRepository
    {
        public ServiceProviderRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<ServiceProvider>> GetVerifiedAsync()
        {
            return await _dbContext.ServiceProviders
                .AsNoTracking()
                .Where(x => x.IsVerified && x.IsActive)
                .OrderBy(x => x.BusinessName)
                .ToListAsync();
        }

        public async Task<ServiceProvider?> GetByUserIdAsync(
            int userId)
        {
            return await _dbContext.ServiceProviders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }
    }

    // 13. SERVICE REQUEST
    public class ServiceRequestRepository
        : Repository<ServiceRequest>, IServiceRequestRepository
    {
        public ServiceRequestRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }
    }

    // 14. PAYMENT TRANSACTION
    public class PaymentTransactionRepository
        : Repository<PaymentTransaction>,
          IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<List<PaymentTransaction>> GetByRequestAsync(
            int serviceRequestId)
        {
            return await _dbContext.PaymentTransactions
                .AsNoTracking()
                .Where(x => x.ServiceRequestId == serviceRequestId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
