using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsureZen.Core.DTOs;
using InsureZen.Core.Enums;
using InsureZen.Core.Interfaces;

namespace InsureZen.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClaim([FromBody] CreateClaimDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _claimService.CreateClaimAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaim(Guid id)
        {
            try
            {
                var result = await _claimService.GetClaimByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetClaims([FromQuery] ClaimFilterDto filter)
        {
            try
            {
                var result = await _claimService.GetClaimsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("maker/review")]
        [Authorize(Roles = "Maker")]
        public async Task<IActionResult> SubmitMakerReview([FromBody] MakerReviewRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _claimService.SubmitMakerReviewAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("checker/review")]
        [Authorize(Roles = "Checker")]
        public async Task<IActionResult> SubmitCheckerReview([FromBody] CheckerReviewRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _claimService.SubmitCheckerReviewAsync(request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockClaim(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();
                var result = await _claimService.LockClaimForReviewAsync(id, userId, role);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> UnlockClaim(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _claimService.UnlockClaimAsync(id, userId);
                return Ok(new { message = "Claim unlocked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}