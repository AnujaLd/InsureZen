using System;
using InsureZen.Core.Enums;

namespace InsureZen.Core.Entities
{
    public class CheckerReview : BaseEntity
    {
        public Guid ClaimId { get; set; }
        public virtual Claim? Claim { get; set; }
        
        public Guid CheckerId { get; set; }
        public virtual User? Checker { get; set; }
        
        public Recommendation FinalDecision { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime ReviewedAt { get; set; }
        public bool IsFinal { get; set; }
    }
}