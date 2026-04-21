using System;
using System.Threading.Tasks;
using InsureZen.Core.DTOs;

namespace InsureZen.Core.Interfaces
{
    public interface IClaimService
    {
        Task<ClaimResponseDto> CreateClaimAsync(CreateClaimDto createDto, Guid userId);
        Task<ClaimResponseDto> GetClaimByIdAsync(Guid id);
        Task<PaginatedResponseDto<ClaimResponseDto>> GetClaimsAsync(ClaimFilterDto filter);
        Task<ClaimResponseDto> SubmitMakerReviewAsync(MakerReviewRequestDto reviewDto, Guid makerId);
        Task<ClaimResponseDto> SubmitCheckerReviewAsync(CheckerReviewRequestDto reviewDto, Guid checkerId);
        Task<ClaimLockDto> LockClaimForReviewAsync(Guid claimId, Guid userId, Enums.UserRole role);
        Task UnlockClaimAsync(Guid claimId, Guid userId);
    }
}