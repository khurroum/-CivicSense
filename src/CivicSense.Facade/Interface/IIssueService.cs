
using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Interface
{
    public interface IIssueService
    {
        Task<IssueDto> CreateAsync(
            IssueCreateDto dto, int actorId);

        Task<List<IssueDto>> GetMyIssuesAsync(int actorId);

        Task<IssueDto?> GetByIdAsync(
            int issueId, int actorId);

        Task<bool> UpdateAsync(
            int issueId, IssueUpdateDto dto, int actorId);

        Task<bool> ArchiveAsync(
            int issueId, int actorId);
    }
}
