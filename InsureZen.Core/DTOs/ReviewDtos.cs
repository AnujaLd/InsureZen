using System;
using InsureZen.Core.Enums;

namespace InsureZen.Core.DTOs
{
    public class MakerReviewRequestDto
    {
        public Guid ClaimId { get; set; }
        public Recommendation Recommendation { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class CheckerReviewRequestDto
    {
        public Guid ClaimId { get; set; }
        public Recommendation FinalDecision { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class ClaimLockDto
    {
        public Guid ClaimId { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockExpiry { get; set; }
    }
}