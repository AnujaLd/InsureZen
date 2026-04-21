using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InsureZen.Core.Entities;
using InsureZen.Core.Enums;
using InsureZen.Core.Interfaces;
using InsureZen.Core.DTOs;

namespace InsureZen.Infrastructure.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClaimService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ClaimResponseDto> CreateClaimAsync(CreateClaimDto createDto, Guid userId)
        {
            var claim = new Claim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = createDto.ClaimNumber,
                PatientName = createDto.PatientName,
                DateOfBirth = createDto.DateOfBirth,
                InsuranceCompany = createDto.InsuranceCompany,
                PolicyNumber = createDto.PolicyNumber,
                DateOfService = createDto.DateOfService,
                Diagnosis = createDto.Diagnosis ?? string.Empty,
                Procedure = createDto.Procedure ?? string.Empty,
                Amount = createDto.Amount,
                ExtractedData = JsonSerializer.Serialize(createDto.ExtractedData ?? new Dictionary<string, object>()),
                Status = ClaimStatus.PendingMakerReview,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Claims.AddAsync(claim);
            await _unitOfWork.CompleteAsync();

            return await MapToResponseDto(claim);
        }

        public async Task<ClaimResponseDto> GetClaimByIdAsync(Guid id)
        {
            var claim = await GetClaimWithIncludes(id);
            if (claim == null)
                throw new Exception("Claim not found");

            return await MapToResponseDto(claim);
        }

        public async Task<PaginatedResponseDto<ClaimResponseDto>> GetClaimsAsync(ClaimFilterDto filter)
        {
            var allClaims = await _unitOfWork.Claims.FindAsync(c => !c.IsDeleted);
            var query = allClaims.AsQueryable();

            // Apply filters
            if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status.Value);

            if (!string.IsNullOrEmpty(filter.InsuranceCompany))
                query = query.Where(c => c.InsuranceCompany.Contains(filter.InsuranceCompany));

            if (filter.StartDate.HasValue)
                query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(c => 
                    c.ClaimNumber.Contains(filter.SearchTerm) ||
                    c.PatientName.Contains(filter.SearchTerm) ||
                    c.PolicyNumber.Contains(filter.SearchTerm));
            }

            var totalCount = query.Count();

            var claims = query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var items = new List<ClaimResponseDto>();
            foreach (var claim in claims)
            {
                items.Add(await MapToResponseDto(claim));
            }

            return new PaginatedResponseDto<ClaimResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<ClaimResponseDto> SubmitMakerReviewAsync(MakerReviewRequestDto reviewDto, Guid makerId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var claim = await GetClaimWithIncludes(reviewDto.ClaimId);
                if (claim == null)
                    throw new Exception("Claim not found");

                if (claim.Status != ClaimStatus.PendingMakerReview)
                    throw new Exception("Claim is not in pending maker review state");

                if (claim.LockedByUserId != makerId)
                    throw new Exception("Claim is not locked by you");

                var makerReview = new MakerReview
                {
                    Id = Guid.NewGuid(),
                    ClaimId = claim.Id,
                    MakerId = makerId,
                    Recommendation = reviewDto.Recommendation,
                    Feedback = reviewDto.Feedback ?? string.Empty,
                    ReviewedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MakerReviews.AddAsync(makerReview);

                claim.Status = ClaimStatus.PendingCheckerReview;
                claim.MakerId = makerId;
                claim.LockedByUserId = null;
                claim.LockedAt = null;
                claim.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Claims.Update(claim);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await MapToResponseDto(claim);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ClaimResponseDto> SubmitCheckerReviewAsync(CheckerReviewRequestDto reviewDto, Guid checkerId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var claim = await GetClaimWithIncludes(reviewDto.ClaimId);
                if (claim == null)
                    throw new Exception("Claim not found");

                if (claim.Status != ClaimStatus.PendingCheckerReview)
                    throw new Exception("Claim is not in pending checker review state");

                if (claim.LockedByUserId != checkerId)
                    throw new Exception("Claim is not locked by you");

                var checkerReview = new CheckerReview
                {
                    Id = Guid.NewGuid(),
                    ClaimId = claim.Id,
                    CheckerId = checkerId,
                    FinalDecision = reviewDto.FinalDecision,
                    Feedback = reviewDto.Feedback ?? string.Empty,
                    ReviewedAt = DateTime.UtcNow,
                    IsFinal = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CheckerReviews.AddAsync(checkerReview);

                claim.Status = reviewDto.FinalDecision == Recommendation.Approve 
                    ? ClaimStatus.Approved 
                    : ClaimStatus.Rejected;
                claim.CheckerId = checkerId;
                claim.FinalDecision = reviewDto.FinalDecision.ToString();
                claim.LockedByUserId = null;
                claim.LockedAt = null;
                claim.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Claims.Update(claim);
                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Simulate forwarding to insurance company
                await ForwardToInsuranceCompany(claim.Id);

                return await MapToResponseDto(claim);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ClaimLockDto> LockClaimForReviewAsync(Guid claimId, Guid userId, UserRole role)
        {
            var claim = await _unitOfWork.Claims.GetByIdAsync(claimId);
            if (claim == null)
                throw new Exception("Claim not found");

            // Check if claim is already locked
            if (claim.LockedByUserId.HasValue && claim.LockedByUserId != userId)
            {
                var lockExpiry = claim.LockedAt?.AddMinutes(5);
                if (DateTime.UtcNow < lockExpiry)
                {
                    return new ClaimLockDto
                    {
                        ClaimId = claimId,
                        IsLocked = true,
                        LockExpiry = lockExpiry
                    };
                }
            }

            // Check if claim is in correct state for the role
            if (role == UserRole.Maker && claim.Status != ClaimStatus.PendingMakerReview)
                throw new Exception("Claim is not available for maker review");

            if (role == UserRole.Checker && claim.Status != ClaimStatus.PendingCheckerReview)
                throw new Exception("Claim is not available for checker review");

            claim.LockedByUserId = userId;
            claim.LockedAt = DateTime.UtcNow;
            claim.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Claims.Update(claim);
            await _unitOfWork.CompleteAsync();

            return new ClaimLockDto
            {
                ClaimId = claimId,
                IsLocked = true,
                LockExpiry = DateTime.UtcNow.AddMinutes(5)
            };
        }

        public async Task UnlockClaimAsync(Guid claimId, Guid userId)
        {
            var claim = await _unitOfWork.Claims.GetByIdAsync(claimId);
            if (claim != null && claim.LockedByUserId == userId)
            {
                claim.LockedByUserId = null;
                claim.LockedAt = null;
                claim.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Claims.Update(claim);
                await _unitOfWork.CompleteAsync();
            }
        }

        private async Task<Claim?> GetClaimWithIncludes(Guid id)
        {
            var claims = await _unitOfWork.Claims.FindAsync(c => c.Id == id);
            return claims.FirstOrDefault();
        }

        private async Task<ClaimResponseDto> MapToResponseDto(Claim claim)
        {
            // Get maker review
            var makerReviews = await _unitOfWork.MakerReviews.FindAsync(mr => mr.ClaimId == claim.Id);
            var makerReview = makerReviews.FirstOrDefault();
            
            var checkerReviews = await _unitOfWork.CheckerReviews.FindAsync(cr => cr.ClaimId == claim.Id);
            var checkerReview = checkerReviews.FirstOrDefault();
            
            var maker = makerReview != null ? await _unitOfWork.Users.GetByIdAsync(makerReview.MakerId) : null;
            var checker = checkerReview != null ? await _unitOfWork.Users.GetByIdAsync(checkerReview.CheckerId) : null;

            return new ClaimResponseDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                PatientName = claim.PatientName,
                DateOfBirth = claim.DateOfBirth,
                InsuranceCompany = claim.InsuranceCompany,
                PolicyNumber = claim.PolicyNumber,
                DateOfService = claim.DateOfService,
                Diagnosis = claim.Diagnosis,
                Procedure = claim.Procedure,
                Amount = claim.Amount,
                Status = claim.Status,
                StatusDescription = claim.Status.ToString(),
                CreatedAt = claim.CreatedAt,
                IsLocked = claim.LockedByUserId.HasValue && claim.LockedAt?.AddMinutes(5) > DateTime.UtcNow,
                LockedBy = claim.LockedByUserId?.ToString(),
                MakerReview = makerReview != null ? new MakerReviewDto
                {
                    Recommendation = makerReview.Recommendation,
                    Feedback = makerReview.Feedback,
                    ReviewedAt = makerReview.ReviewedAt,
                    MakerName = maker?.FullName ?? string.Empty
                } : null,
                CheckerReview = checkerReview != null ? new CheckerReviewDto
                {
                    FinalDecision = checkerReview.FinalDecision,
                    Feedback = checkerReview.Feedback,
                    ReviewedAt = checkerReview.ReviewedAt,
                    CheckerName = checker?.FullName ?? string.Empty
                } : null
            };
        }

        private async Task ForwardToInsuranceCompany(Guid claimId)
        {
            var claim = await _unitOfWork.Claims.GetByIdAsync(claimId);
            if (claim != null)
            {
                claim.Status = ClaimStatus.ForwardedToInsurance;
                claim.ForwardedToInsuranceAt = DateTime.UtcNow;
                claim.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Claims.Update(claim);
                await _unitOfWork.CompleteAsync();
                
                // Log the forwarding action
                Console.WriteLine($"[Forwarding] Claim {claim.ClaimNumber} forwarded to {claim.InsuranceCompany} at {DateTime.UtcNow}");
            }
        }
    }
}