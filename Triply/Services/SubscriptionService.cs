using Triply.Core.Enums;
using Triply.Core.Interfaces;
using Triply.Core.Models;
using Microsoft.EntityFrameworkCore;
using Triply.Data;

namespace Triply.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly TriplyDbContext _context;
    private const string ADMIN_USER_ID = "admin@triply.app"; // Your admin account
    private const int TRIAL_DAYS = 14;

    public SubscriptionService(TriplyDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsAdminAsync()
    {
        // Check if current user is admin
        var userId = await GetCurrentUserIdAsync();
        return userId == ADMIN_USER_ID;
    }

    public async Task<SubscriptionTier> GetCurrentTierAsync()
    {
        // Admin always has full access
        if (await IsAdminAsync())
            return SubscriptionTier.Admin;

        var subscription = await GetCurrentSubscriptionAsync();
        
        if (subscription == null)
            return SubscriptionTier.Free;

        // Check if trial is active
        if (subscription.Status == SubscriptionStatus.TrialActive &&
            subscription.TrialEndDate.HasValue &&
            subscription.TrialEndDate.Value > DateTime.Now)
        {
            return SubscriptionTier.Trial;
        }

        // Check if pro subscription is active
        if (subscription.Status == SubscriptionStatus.Active &&
            subscription.SubscriptionEndDate.HasValue &&
            subscription.SubscriptionEndDate.Value > DateTime.Now)
        {
            return SubscriptionTier.Pro;
        }

        return SubscriptionTier.Free;
    }

    public async Task<SubscriptionStatus> GetSubscriptionStatusAsync()
    {
        if (await IsAdminAsync())
            return SubscriptionStatus.Active; // Admin always active

        var subscription = await GetCurrentSubscriptionAsync();
        
        if (subscription == null)
            return SubscriptionStatus.None;

        // Update status if trial expired
        if (subscription.Status == SubscriptionStatus.TrialActive &&
            subscription.TrialEndDate.HasValue &&
            subscription.TrialEndDate.Value < DateTime.Now)
        {
            subscription.Status = SubscriptionStatus.TrialExpired;
            await _context.SaveChangesAsync();
        }

        // Update status if subscription expired
        if (subscription.Status == SubscriptionStatus.Active &&
            subscription.SubscriptionEndDate.HasValue &&
            subscription.SubscriptionEndDate.Value < DateTime.Now)
        {
            subscription.Status = SubscriptionStatus.Expired;
            await _context.SaveChangesAsync();
        }

        return subscription.Status;
    }

    public async Task<bool> IsProFeatureAvailableAsync()
    {
        var tier = await GetCurrentTierAsync();
        return tier == SubscriptionTier.Trial || 
               tier == SubscriptionTier.Pro || 
               tier == SubscriptionTier.Admin;
    }

    public async Task<int> GetDaysRemainingInTrialAsync()
    {
        var subscription = await GetCurrentSubscriptionAsync();
        
        if (subscription?.TrialEndDate.HasValue == true)
        {
            var remaining = (subscription.TrialEndDate.Value - DateTime.Now).Days;
            return Math.Max(0, remaining);
        }

        return 0;
    }

    public async Task<bool> StartTrialAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        
        // Check if user already had a trial
        var existingSubscription = await GetCurrentSubscriptionAsync();
        if (existingSubscription?.TrialStartDate.HasValue == true)
        {
            return false; // Trial already used
        }

        var subscription = new Subscription
        {
            SubscriptionId = Guid.NewGuid(),
            UserId = userId,
            Tier = SubscriptionTier.Trial,
            Status = SubscriptionStatus.TrialActive,
            TrialStartDate = DateTime.Now,
            TrialEndDate = DateTime.Now.AddDays(TRIAL_DAYS),
            CreatedAt = DateTime.Now,
            IsAutoRenew = false
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PurchaseSubscriptionAsync(bool isYearly)
    {
        // This is a placeholder - actual implementation would use
        // Plugin.InAppBilling for Google Play billing
        
        var userId = await GetCurrentUserIdAsync();
        var subscription = await GetCurrentSubscriptionAsync();

        if (subscription == null)
        {
            subscription = new Subscription
            {
                SubscriptionId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.Now
            };
            _context.Subscriptions.Add(subscription);
        }

        subscription.Tier = SubscriptionTier.Pro;
        subscription.Status = SubscriptionStatus.Active;
        subscription.SubscriptionStartDate = DateTime.Now;
        subscription.SubscriptionEndDate = isYearly 
            ? DateTime.Now.AddYears(1) 
            : DateTime.Now.AddMonths(1);
        subscription.IsAutoRenew = true;
        subscription.LastCheckedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RestorePurchasesAsync()
    {
        // Placeholder for Google Play purchase restoration
        // Would query Google Play Billing API
        return false;
    }

    public async Task<DateTime?> GetSubscriptionExpiryAsync()
    {
        var subscription = await GetCurrentSubscriptionAsync();
        
        if (subscription?.Status == SubscriptionStatus.TrialActive)
            return subscription.TrialEndDate;
        
        if (subscription?.Status == SubscriptionStatus.Active)
            return subscription.SubscriptionEndDate;

        return null;
    }

    private async Task<Subscription?> GetCurrentSubscriptionAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<string> GetCurrentUserIdAsync()
    {
        // For now, use device ID or a stored user ID
        // In production, this would use authentication
        
        var deviceId = Preferences.Get("DeviceUserId", string.Empty);
        
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = Guid.NewGuid().ToString();
            Preferences.Set("DeviceUserId", deviceId);
        }

        return deviceId;
    }
}
