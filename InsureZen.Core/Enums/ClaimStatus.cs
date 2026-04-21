namespace InsureZen.Core.Enums
{
    public enum ClaimStatus
    {
        PendingMakerReview = 1,
        PendingCheckerReview = 2,
        Approved = 3,
        Rejected = 4,
        ForwardedToInsurance = 5
    }
}