using System.Threading.Tasks;
using InsureZen.Core.DTOs;

namespace InsureZen.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto);
    }
}