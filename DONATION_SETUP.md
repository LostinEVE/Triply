# 💝 Setting Up PayPal Donations

## **What You Need:**

1. **Your PayPal.Me Link**
   - Go to: https://www.paypal.com/paypalme/
   - Sign in to your PayPal account
   - Create your custom link (e.g., `paypal.me/YourName`)
   - Copy the full URL

2. **Replace Placeholder in Code**

Find and replace `YOUR_PAYPAL_USERNAME` in these files:
- `Triply/Components/Pages/About.razor` (line ~115)
- `Triply/Components/Pages/HelpSupport.razor` (line ~181)

**Example:**
```
Before: https://www.paypal.com/paypalme/YOUR_PAYPAL_USERNAME
After:  https://www.paypal.com/paypalme/JohnDoe123
```

---

## **How to Get Your PayPal.Me Link:**

### **If You Have PayPal:**
1. Go to: https://www.paypal.com/paypalme/
2. Log in
3. Click "Create Your PayPal.Me Link"
4. Choose your custom URL: `paypal.me/YourChosenName`
5. Copy the full link

### **If You Don't Have PayPal:**
1. Sign up at: https://www.paypal.com/
2. Verify your account
3. Then follow steps above

---

## **Alternative Donation Methods:**

You can also add other donation options by editing the About.razor file:

### **Venmo:**
```html
<MudButton Href="https://venmo.com/YOUR-VENMO-USERNAME" Target="_blank">
    Venmo
</MudButton>
```

### **Cash App:**
```html
<MudButton Href="https://cash.app/$YOUR-CASHTAG" Target="_blank">
    Cash App
</MudButton>
```

### **Buy Me a Coffee:**
```html
<MudButton Href="https://www.buymeacoffee.com/YOUR-USERNAME" Target="_blank">
    Buy Me a Coffee
</MudButton>
```

---

## **Update README.md:**

Add this section to your main README.md file:

```markdown
## 💝 Support the Project

Triply is **100% free and open source**. Built by an owner-operator with 29 years 
of trucking experience to help fellow truckers manage their businesses better.

If Triply saves you time and helps your business, consider supporting development:

[![Donate with PayPal](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/paypalme/YOUR_PAYPAL_USERNAME)

Every donation helps continue improving Triply for the trucking community! ☕🚛
```

---

## **Current Setup:**

✅ **Donation buttons added to:**
- About page (prominent card with heart icon)
- Help & Support page (in contact section)

✅ **Message:**
- "100% free and open source"
- "If it helps you, consider supporting"
- "Suggested amounts" (coffee, lunch, fuel tank)
- "100% optional • No pressure"

✅ **Your admin access:**
- Still intact in Settings
- You use for free
- Others can donate if they want

---

## **Next Steps:**

1. Get your PayPal.Me link
2. Replace `YOUR_PAYPAL_USERNAME` in the two files
3. Push to GitHub
4. Build Android APK
5. Share with the trucking community!

---

**Once you have your PayPal.Me link, just tell me and I'll update the files with your actual link!** 💰
