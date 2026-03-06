using Triply.Core.Enums;

namespace Triply.Core.Models;

public class Subscription
{
    public Guid SubscriptionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public string? PurchaseToken { get; set; }
    public bool IsAutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
}
