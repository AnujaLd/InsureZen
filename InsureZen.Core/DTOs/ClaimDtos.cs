using System;
using System.Collections.Generic;
using InsureZen.Core.Enums;

namespace InsureZen.Core.DTOs
{
    public class CreateClaimDto
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
        public Dictionary<string, object>? ExtractedData { get; set; }
    }

    public class ClaimResponseDto
    {
        public Guid Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public DateTime DateOfService { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Procedure { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ClaimStatus Status { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public MakerReviewDto? MakerReview { get; set; }
        public CheckerReviewDto? CheckerReview { get; set; }
        public bool IsLocked { get; set; }
        public string? LockedBy { get; set; }
    }

    public class MakerReviewDto
    {
        public Recommendation Recommendation { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime ReviewedAt { get; set; }
        public string MakerName { get; set; } = string.Empty;
    }

    public class CheckerReviewDto
    {
        public Recommendation FinalDecision { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public DateTime ReviewedAt { get; set; }
        public string CheckerName { get; set; } = string.Empty;
    }

    public class ClaimFilterDto
    {
        public ClaimStatus? Status { get; set; }
        public string? InsuranceCompany { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}