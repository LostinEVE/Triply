using Triply.Core.Enums;

namespace Triply.Core.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionTier> GetCurrentTierAsync();
    Task<SubscriptionStatus> GetSubscriptionStatusAsync();
    Task<bool> IsProFeatureAvailableAsync();
    Task<int> GetDaysRemainingInTrialAsync();
    Task<bool> StartTrialAsync();
    Task<bool> PurchaseSubscriptionAsync(bool isYearly);
    Task<bool> RestorePurchasesAsync();
    Task<DateTime?> GetSubscriptionExpiryAsync();
    Task<bool> IsAdminAsync();
}
