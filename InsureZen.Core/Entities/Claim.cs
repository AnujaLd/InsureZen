using System;
using System.Collections.Generic;
using InsureZen.Core.Enums;

namespace InsureZen.Core.Entities
{
    public class Claim : BaseEntity
    {
        public string ClaimNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime DateOfService { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Procedure { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ExtractedData { get; set; } = "{}";
        public ClaimStatus Status { get; set; }
        public Guid? MakerId { get; set; }
        public virtual User? Maker { get; set; }
        public Guid? CheckerId { get; set; }
        public virtual User? Checker { get; set; }
        public string FinalDecision { get; set; } = string.Empty;
        public DateTime? ForwardedToInsuranceAt { get; set; }
        public virtual MakerReview? MakerReview { get; set; }
        public virtual CheckerReview? CheckerReview { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Guid? LockedByUserId { get; set; }
        public DateTime? LockedAt { get; set; }
    }
}