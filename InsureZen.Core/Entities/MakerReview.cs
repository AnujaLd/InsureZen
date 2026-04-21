using System;
using InsureZen.Core.Enums;

namespace InsureZen.Core.Entities
{
    public class MakerReview : BaseEntity
    {
        public Guid ClaimId { get; set; }
        public virtual Claim? Claim { get; set; }
        
        public Guid MakerId { get; set; }
        public virtual User? Maker { get; set; }
        
        public Recommendation Recommendation { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime ReviewedAt { get; set; }
    }
}