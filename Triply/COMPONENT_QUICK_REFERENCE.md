# Triply Component Quick Reference

## 🔍 **SearchBar**
```razor
<SearchBar Label="Search Items"
           Placeholder="Type to search..."
           OnSearch="@HandleSearchAsync"
           ShowFilters="true">
    <FilterContent>
        <!-- Your custom filters -->
        <MudSelect @bind-Value="_filter" Label="Category">
            <MudSelectItem Value="@("")">All</MudSelectItem>
        </MudSelect>
    </FilterContent>
</SearchBar>

@code {
    private async Task HandleSearchAsync(string searchText)
    {
        // Filter your data
        _filtered = _items.Where(x => x.Name.Contains(searchText)).ToList();
    }
}
```

---

## 🔄 **PullToRefresh**
```razor
<PullToRefresh OnRefresh="@LoadDataAsync">
    <MudList>
        @foreach (var item in _items)
        {
            <MudListItem>@item.Name</MudListItem>
        }
    </MudList>
</PullToRefresh>

@code {
    private async Task LoadDataAsync()
    {
        _items = await _service.GetItemsAsync();
    }
}
```

---

## ⌨️ **Keyboard Shortcuts**
```razor
<!-- Already in MainLayout - just works! -->

@code {
    // To handle shortcuts on specific page:
    protected override void OnInitialized()
    {
        // Listen for global shortcuts if needed
    }
}
```

**Available Shortcuts:**
- `Ctrl+N` - New (context-aware)
- `Ctrl+S` - Save
- `Ctrl+P` - Print
- `Ctrl+F` - Search
- `ESC` - Close dialogs

---

## ⏳ **LoadingStateService**
```razor
@inject ILoadingStateService LoadingState

@code {
    private async Task LoadDataAsync()
    {
        LoadingState.ShowLoading("Loading invoices...");
        try
        {
            _data = await _service.GetDataAsync();
        }
        finally
        {
            LoadingState.HideLoading();
        }
    }
}
```

---

## 📱 **ResponsiveLayout**
```razor
<ResponsiveLayout>
    <MobileContent>
        <!-- Card-based layout for mobile -->
        <MudStack Spacing="2">
            @foreach (var item in _items)
            {
                <MudCard>
                    <MudCardContent>
                        <MudText>@item.Name</MudText>
                    </MudCardContent>
                </MudCard>
            }
        </MudStack>
    </MobileContent>
    
    <TabletContent>
        <!-- 2-column grid for tablet (optional) -->
        <MudGrid>
            @foreach (var item in _items)
            {
                <MudItem xs="12" md="6">
                    <MudCard>
                        <MudCardContent>
                            <MudText>@item.Name</MudText>
                        </MudCardContent>
                    </MudCard>
                </MudItem>
            }
        </MudGrid>
    </TabletContent>
    
    <DesktopContent>
        <!-- Full data grid for desktop -->
        <MudDataGrid Items="@_items" Hover="true">
            <Columns>
                <PropertyColumn Property="x => x.Name" />
                <PropertyColumn Property="x => x.Amount" Format="C2" />
            </Columns>
        </MudDataGrid>
    </DesktopContent>
</ResponsiveLayout>
```

---

## 🎨 **Theme Colors**

**Use semantic colors:**
```razor
<!-- Success (delivered, completed) -->
<MudChip Color="Color.Success">Delivered</MudChip>

<!-- Warning (due soon, pending) -->
<MudChip Color="Color.Warning">Due Soon</MudChip>

<!-- Error (overdue, failed) -->
<MudChip Color="Color.Error">Overdue</MudChip>

<!-- Primary (main actions) -->
<MudButton Color="Color.Primary">Create Invoice</MudButton>

<!-- Secondary (alternate actions) -->
<MudButton Color="Color.Secondary">View Details</MudButton>
```

**Current Theme:**
- Primary: `#FF6B35` (Trucking Orange)
- Secondary: `#004E89` (Deep Blue)
- Success: `#2E7D32` (Green)
- Warning: `#F57C00` (Amber)
- Error: `#C62828` (Red)

---

## 📤 **ExportButtons**
```razor
<ExportButtons PdfBytes="@_pdfBytes"
               FileName="report.pdf"
               CsvFileName="data.csv"
               OnExportCsv="@ExportCsvAsync"
               OnRefresh="@GeneratePdfAsync" />

@code {
    private byte[]? _pdfBytes;
    
    private async Task GeneratePdfAsync()
    {
        _pdfBytes = await _pdfService.GenerateReportAsync(...);
    }
    
    private async Task ExportCsvAsync()
    {
        var csv = CsvExportHelper.ToCsv(_items);
        await CsvExportHelper.SaveCsvAsync(csv, "data.csv");
    }
}
```

---

## 🔔 **NotificationBadge**
```razor
<NotificationBadge ShowCriticalOnly="true">
    <MudIconButton Icon="@Icons.Material.Filled.Notifications" />
</NotificationBadge>

<!-- Or filter by type -->
<NotificationBadge FilterType="NotificationType.InvoiceOverdue">
    <MudNavLink Href="/invoices">Invoices</MudNavLink>
</NotificationBadge>
```

---

## 📊 **Common Patterns**

### **List Page Template:**
```razor
@page "/items"
@inject ILoadingStateService LoadingState

<PageTitle>Items - Triply</PageTitle>

<SearchBar OnSearch="@SearchAsync" />

<PullToRefresh OnRefresh="@LoadItemsAsync">
    <ResponsiveLayout>
        <MobileContent>
            <!-- Cards -->
        </MobileContent>
        <DesktopContent>
            <!-- Data grid -->
        </DesktopContent>
    </ResponsiveLayout>
</PullToRefresh>

@code {
    protected override async Task OnInitializedAsync()
    {
        await LoadItemsAsync();
    }
    
    private async Task LoadItemsAsync()
    {
        LoadingState.ShowLoading("Loading items...");
        try
        {
            _items = await _service.GetAllAsync();
        }
        finally
        {
            LoadingState.HideLoading();
        }
    }
}
```

### **Detail Page Template:**
```razor
@page "/items/{Id:guid}"
@inject ILoadingStateService LoadingState
@inject IPdfGenerationService PdfService

<ExportButtons PdfBytes="@_pdfBytes" 
               FileName="@($"item_{_item.Name}.pdf")"
               OnRefresh="@GeneratePdfAsync" />

<MudCard>
    <MudCardContent>
        <!-- Item details -->
    </MudCardContent>
    <MudCardActions>
        <MudButton Color="Color.Primary" OnClick="@SaveAsync">Save</MudButton>
    </MudCardActions>
</MudCard>

@code {
    [Parameter]
    public Guid Id { get; set; }
    
    private async Task SaveAsync()
    {
        LoadingState.ShowLoading("Saving...");
        try
        {
            await _service.UpdateAsync(_item);
            _snackbar.Add("Saved successfully", Severity.Success);
        }
        finally
        {
            LoadingState.HideLoading();
        }
    }
}
```

---

## 🎯 **Tips**

1. **Always use LoadingStateService** for async operations > 500ms
2. **Add SearchBar** to all list pages with > 20 items
3. **Use ResponsiveLayout** for pages with different mobile/desktop needs
4. **Wrap mobile lists** in PullToRefresh for better UX
5. **Use semantic colors** (Success/Warning/Error) for status indicators
6. **Add keyboard shortcuts** to form pages (Ctrl+S to save)

---

## 📚 **Component Locations**

All reusable components are in:
```
Triply/Components/Shared/
├── SearchBar.razor
├── PullToRefresh.razor
├── ResponsiveLayout.razor
├── ExportButtons.razor
├── NotificationBadge.razor
├── GlobalLoadingOverlay.razor
├── KeyboardShortcuts.razor
└── OfflineIndicator.razor
```

Import in your page:
```razor
@using Triply.Components.Shared
```

Or globally in `_Imports.razor`:
```razor
@using Triply.Components.Shared
```

---

## 🚀 **Ready to Use!**

All components are registered and ready. Just import and use them in your pages!
