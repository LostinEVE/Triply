namespace Triply.Core.Enums;

public enum SubscriptionTier
{
    Free = 0,
    Trial = 1,
    Pro = 2,
    Admin = 999  // For you - bypass all restrictions
}

public enum SubscriptionStatus
{
    None = 0,
    Active = 1,
    Expired = 2,
    Cancelled = 3,
    TrialActive = 4,
    TrialExpired = 5
}
