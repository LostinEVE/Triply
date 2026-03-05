# Triply Blazor Layout & Navigation Guide

## Overview

The Triply app uses **MudBlazor** components for a professional, responsive layout with comprehensive navigation designed specifically for trucking business management.

## Layout Structure

### MainLayout.razor

The main application layout includes:

#### **Top App Bar (MudAppBar)**
- **Left Side:**
  - Hamburger menu button (toggles sidebar)
  - Triply logo with truck icon
  
- **Right Side:**
  - Quick Actions menu (+ button)
    - Add Expense
    - Add Fuel Entry
    - Add Load
    - Create Invoice
  - Dark/Light theme toggle
  - Settings button

#### **Sidebar Navigation (MudDrawer)**
- Responsive drawer that:
  - Stays open on desktop (>= 960px)
  - Closes automatically on mobile
  - Shows company branding in header
  
- **Navigation Sections:**
  1. **Dashboard** - Home overview
  2. **Operations**
     - Trucks
     - Loads
     - Customers
  3. **Financial**
     - Invoices
     - Expenses
     - Fuel Log
     - Maintenance
  4. **Reporting & Accounting**
     - Reports (expandable group)
       - Profit & Loss
       - Balance Sheet
       - Tax Reports
       - IFTA Report
       - Cost Per Mile
       - AR Aging
     - Accounting
  5. **System**
     - Settings
     - Help & Support

#### **Mobile Bottom Navigation**
- Appears only on mobile devices (< 960px)
- Quick access to:
  - Home
  - Loads
  - Quick Actions (FAB in center)
  - Expenses
  - Menu

#### **Floating Quick Actions (Mobile)**
- Appears when FAB is clicked
- Overlay background
- Action buttons:
  - Add Expense
  - Add Fuel
  - Create Invoice

---

## Navigation Menu (NavMenu.razor)

### Features

**Icon System:**
- Every menu item has a Material Design icon
- Color-coded by category:
  - Primary (blue) - Core operations
  - Success (green) - Financial income
  - Error (red) - Financial expenses
  - Warning (orange) - Fuel/resources
  - Info (light blue) - Maintenance/reports
  - Tertiary (purple) - Reports & accounting

**Grouped Navigation:**
- Reports are grouped under expandable section
- Sections separated by dividers
- Section headers (OPERATIONS, FINANCIAL, etc.)

### Usage

```razor
<NavMenu />
```

The menu automatically:
- Highlights active route
- Collapses on mobile after navigation
- Shows icons and text labels

---

## Breadcrumbs Component

Automatic breadcrumb generation based on current route.

### Features

- **Auto-generated** from URL path
- **Icon support** for common routes
- **Clickable** to navigate back
- **Smart formatting** of route segments

### Route Mapping

| Route | Breadcrumb | Icon |
|-------|-----------|------|
| `/` | Dashboard | Home |
| `/trucks` | Trucks | LocalShipping |
| `/loads` | Loads | Inventory |
| `/invoices` | Invoices | Description |
| `/expenses` | Expenses | Receipt |
| `/fuel` | Fuel Log | LocalGasStation |
| `/maintenance` | Maintenance | Build |
| `/reports` | Reports | Assessment |
| `/reports/profit-loss` | Reports > Profit & Loss | TrendingUp |
| `/reports/ifta` | Reports > IFTA Report | LocalGasStation |

### Usage

```razor
<Breadcrumbs />
```

Example output:
```
Dashboard > Loads > Add New
```

---

## Dashboard (Home.razor)

### Layout Sections

#### **1. Quick Stats Cards (4 cards)**
- Active Loads (Primary)
- Unpaid Invoices (Warning)
- Monthly Revenue (Success)
- Profit Margin (Info)

Each card shows:
- Label
- Large number/amount
- Color-coded icon

#### **2. Quick Actions Card**
Action buttons for common tasks:
- New Load (Primary)
- Create Invoice (Success)
- Log Fuel (Warning)
- Add Expense (Error)
- View Reports (Info, outlined)

#### **3. Recent Loads Card**
- Last 3-5 recent loads
- Load number, route, status
- Amount
- Icon color-coded by status
- "View All Loads" link

#### **4. Outstanding Invoices Card**
- Overdue invoices (red warning icon)
- Due soon invoices (orange schedule icon)
- Current invoices (green check icon)
- Days overdue/remaining
- Amount
- "View All Invoices" link

#### **5. Alerts & Reminders Card**
MudAlert components with severity levels:
- **Error** - Critical alerts (maintenance due)
- **Warning** - Important reminders (IFTA due)
- **Info** - General notifications (tax payment)

---

## Theme System

### Colors

**Light Mode:**
- Primary: `#1976d2` (Blue)
- Secondary: `#424242` (Dark Gray)
- Background: `#f5f5f5` (Light Gray)
- AppBar: `#1976d2` (Blue)
- Drawer: `#ffffff` (White)

**Dark Mode:**
- Primary: `#90caf9` (Light Blue)
- Secondary: `#424242` (Dark Gray)
- Background: `#121212` (Dark)
- AppBar: `#1e1e1e` (Dark Gray)
- Drawer: `#1e1e1e` (Dark Gray)

### Toggle Theme

User can toggle between light/dark mode:
- Icon button in top app bar
- Sun icon (light mode active)
- Moon icon (dark mode active)
- Persists across sessions (TODO: implement localStorage)

---

## Responsive Design

### Breakpoints

| Size | Width | Behavior |
|------|-------|----------|
| **Mobile** | < 600px | Bottom nav, collapsed drawer by default |
| **Tablet** | 600-960px | Side drawer toggleable |
| **Desktop** | >= 960px | Side drawer always visible |

### Mobile Optimizations

1. **Bottom Navigation Bar**
   - Always visible on mobile
   - 5 icon buttons
   - Center FAB for quick actions
   - Active route highlighted

2. **Drawer Behavior**
   - Closed by default on mobile
   - Auto-closes after navigation
   - Overlay mode (pushes content)

3. **Quick Actions**
   - Desktop: Dropdown menu in app bar
   - Mobile: Overlay modal with large buttons

4. **Cards & Layout**
   - Grid adjusts to screen size
   - Full-width cards on mobile
   - Stacked layout for narrow screens

---

## Navigation Patterns

### Common Navigations

```csharp
// From anywhere in the app
@inject NavigationManager NavigationManager

// Navigate to route
NavigationManager.NavigateTo("/loads");

// Navigate with state
NavigationManager.NavigateTo("/invoices/create?customerId=123");

// Check current route
var currentPath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

// Check if route is active
private bool IsActive(string route)
{
    var currentPath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
    return currentPath.StartsWith(route.TrimStart('/'));
}
```

### Quick Action Navigation

```csharp
// Add Expense
NavigationManager.NavigateTo("/expenses/add");

// Add Fuel
NavigationManager.NavigateTo("/fuel/add");

// Create Invoice
NavigationManager.NavigateTo("/invoices/create");

// Add Load
NavigationManager.NavigateTo("/loads/add");
```

---

## Best Practices

### 1. Use Consistent Icons

Always use Material Design icons for consistency:

```razor
<MudIcon Icon="@Icons.Material.Filled.LocalShipping" />
```

### 2. Color-Code by Function

- **Primary** (blue) - Main actions, trucks
- **Success** (green) - Income, completed
- **Error** (red) - Expenses, overdue
- **Warning** (orange) - Fuel, pending
- **Info** (light blue) - Informational

### 3. Mobile-First Design

Test all pages on mobile viewport:
```css
@media (max-width: 960px) {
    /* Mobile styles */
}
```

### 4. Breadcrumb Support

Add route mappings to `GetBreadcrumbInfo()` in Breadcrumbs.razor for new pages.

### 5. Quick Actions

Add frequently used actions to:
- App bar quick actions menu
- Mobile FAB overlay
- Dashboard quick actions card

---

## Customization

### Add New Navigation Item

1. **Update NavMenu.razor:**

```razor
<MudNavLink Href="/drivers" Icon="@Icons.Material.Filled.Person" IconColor="Color.Primary">
    Drivers
</MudNavLink>
```

2. **Add Breadcrumb Mapping:**

```csharp
// In Breadcrumbs.razor
"drivers" => ("Drivers", Icons.Material.Filled.Person),
```

3. **Create Page:**

```razor
@page "/drivers"

<PageTitle>Drivers - Triply</PageTitle>

<MudText Typo="Typo.h4">
    <MudIcon Icon="@Icons.Material.Filled.Person" Class="mr-2" />
    Drivers
</MudText>
```

### Add Dashboard Widget

```razor
<MudItem xs="12" md="6">
    <MudCard Elevation="3">
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.h6">
                    <MudIcon Icon="@Icons.Material.Filled.YourIcon" Class="mr-2" />
                    Widget Title
                </MudText>
            </CardHeaderContent>
        </MudCardHeader>
        <MudCardContent>
            <!-- Widget content -->
        </MudCardContent>
    </MudCard>
</MudItem>
```

### Customize Theme

Edit `MainLayout.razor`:

```csharp
private MudTheme _theme = new()
{
    Palette = new PaletteLight()
    {
        Primary = "#yourcolor",
        Secondary = "#yourcolor",
        // ... other colors
    }
};
```

---

## Troubleshooting

### Drawer Not Opening

**Check:**
1. `_drawerOpen` state variable
2. `DrawerToggle()` method wired to button
3. Breakpoint set correctly

### Navigation Not Highlighting

**Fix:** Ensure route matches exactly:
```csharp
Href="/exact/route/path"
```

### Mobile Bottom Nav Not Showing

**Check:**
1. `_isMobile` detection working
2. Class `d-flex d-md-none` applied
3. Viewport width < 960px

### Quick Actions Not Closing

**Ensure:**
```csharp
OnClick="@(() => { NavigateTo("/route"); _showQuickActions = false; })"
```

---

## Summary

The Triply layout provides:
- ✅ Professional MudBlazor design
- ✅ Responsive mobile/tablet/desktop
- ✅ Comprehensive navigation (12+ menu items)
- ✅ Quick actions for common tasks
- ✅ Automatic breadcrumbs
- ✅ Dark/light theme support
- ✅ Mobile bottom navigation
- ✅ Dashboard with widgets
- ✅ Color-coded sections
- ✅ Icon-based navigation

**Perfect for trucking business management!** 🚛
