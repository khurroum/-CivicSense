
using CivicSense.DataAccess.Implementation;
using CivicSense.DataAccess.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace CivicSense.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessServices(
            this IServiceCollection services)
        {
            // Generic repository
            services.AddScoped(
                typeof(IRepository<>),
                typeof(Repository<>));

            // Entity repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IIssueRepository, IssueRepository>();

            services.AddScoped<IIssueImageRepository,
                IssueImageRepository>();

            services.AddScoped<IIssueAssignmentRepository,
                IssueAssignmentRepository>();

            services.AddScoped<IIssueStatusHistoryRepository,
                IssueStatusHistoryRepository>();

            services.AddScoped<IIssueCommentRepository,
                IssueCommentRepository>();

            services.AddScoped<IIssueConfirmationRepository,
                IssueConfirmationRepository>();

            services.AddScoped<INotificationRepository,
                NotificationRepository>();

            services.AddScoped<IAIAnalysisRepository,
                AIAnalysisRepository>();

            services.AddScoped<IFeedbackRepository,
                FeedbackRepository>();

            services.AddScoped<IServiceProviderRepository,
                ServiceProviderRepository>();

            services.AddScoped<IServiceRequestRepository,
                ServiceRequestRepository>();

            services.AddScoped<IPaymentTransactionRepository,
                PaymentTransactionRepository>();

            return services;
        }
    }
}
