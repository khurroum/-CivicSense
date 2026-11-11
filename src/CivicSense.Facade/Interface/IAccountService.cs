
using CivicSense.Facade.DTOs;

namespace CivicSense.Facade.Interface
{
    public interface IAccountService
    {
        Task<AuthenticatedUserDto> RegisterAsync(
            RegisterDto dto);

        Task<AuthenticatedUserDto?> LoginAsync(
            LoginDto dto);
    }
}
