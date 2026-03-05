# Validation and Error Handling Guide

## Overview

This project implements comprehensive validation and error handling using:
- **FluentValidation** for model validation
- **OperationResult** pattern for service responses
- **BaseService** for consistent error handling
- **Toast notifications** for user feedback
- **Dialog confirmations** for destructive actions
- **Error logging** to local files

## 1. FluentValidation Validators

### Available Validators

All validators are located in `Triply.Core/Validators/`:

- `TruckValidator` - Validates truck data
- `DriverValidator` - Validates driver information
- `CustomerValidator` - Validates customer data
- `LoadValidator` - Validates loads with business rules
- `InvoiceValidator` - Validates invoices and calculations
- `ExpenseValidator` - Validates expense entries
- `FuelEntryValidator` - Validates fuel entries
- `MaintenanceRecordValidator` - Validates maintenance records
- `CompanySettingsValidator` - Validates company settings

### Example: Using a Validator

```csharp
// Inject the validator
public class TruckPage
{
    private readonly IValidator<Truck> _truckValidator;
    
    public TruckPage(IValidator<Truck> truckValidator)
    {
        _truckValidator = truckValidator;
    }
    
    private async Task ValidateTruck(Truck truck)
    {
        var result = await _truckValidator.ValidateAsync(truck);
        
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                // Display error: error.ErrorMessage
            }
        }
    }
}
```

### Business Rules in Validators

Some validators include business rules beyond simple field validation:

**LoadValidator Example:**
```csharp
// Cannot invoice a load that hasn't been picked up
RuleFor(l => l.Status)
    .Must((load, status) => status != LoadStatus.Pending)
    .WithMessage("Cannot create invoice for a load that hasn't been picked up yet")
    .When(l => l.InvoiceLineItems.Any());
```

## 2. Service Layer Error Handling

### BaseService

All service classes should inherit from `BaseService` which provides:
- Automatic error logging
- Try-catch error handling
- Validation integration
- User-friendly error messages

### Using BaseService

```csharp
public class TruckService : BaseService
{
    private readonly ITruckRepository _repository;
    private readonly IValidator<Truck> _validator;
    private readonly IToastService _toastService;
    
    public TruckService(
        ITruckRepository repository,
        IValidator<Truck> validator,
        IErrorLogger errorLogger,
        IToastService toastService) : base(errorLogger)
    {
        _repository = repository;
        _validator = validator;
        _toastService = toastService;
    }
    
    public async Task<OperationResult<Truck>> AddTruckAsync(Truck truck)
    {
        var result = await ValidateAndExecuteAsync(
            truck,
            _validator,
            async () => await _repository.AddTruckAsync(truck),
            "adding truck");
            
        if (result.Success)
        {
            _toastService.ShowSuccess($"Truck {truck.TruckId} added successfully");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
        
        return result;
    }
}
```

### OperationResult Pattern

All service methods should return `OperationResult` or `OperationResult<T>`:

```csharp
// Success
return OperationResult.SuccessResult("Operation completed");

// Success with data
return OperationResult<Truck>.SuccessResult(truck, "Truck added");

// Failure
return OperationResult.FailureResult("Validation failed", validationErrors);

// Exception
return OperationResult.ExceptionResult(ex, "loading data");
```

## 3. Blazor Component Integration

### Validation Summary Component

Use `ValidationSummary` to display validation errors:

```razor
<ValidationSummary ValidationErrors="@_validationErrors" />

@code {
    private List<string> _validationErrors = new();
    
    private async Task ValidateForm()
    {
        var result = await _validator.ValidateAsync(_model);
        if (!result.IsValid)
        {
            _validationErrors = result.Errors.Select(e => e.ErrorMessage).ToList();
        }
    }
}
```

### MudForm Validation

MudBlazor forms have built-in validation:

```razor
<MudForm @ref="_form" @bind-IsValid="@_formValid">
    <MudTextField @bind-Value="_truck.TruckId" 
                  Label="Truck ID" 
                  Required="true"
                  RequiredError="Truck ID is required"
                  MaxLength="50" />
                  
    <MudNumericField @bind-Value="_truck.CurrentOdometer" 
                     Label="Odometer" 
                     Min="0"
                     Required="true" />
</MudForm>

<MudButton Disabled="@(!_formValid)" OnClick="@SaveTruck">Save</MudButton>
```

### Using ValidationErrors in Razor

```razor
@if (_validationErrors.Any())
{
    <MudAlert Severity="Severity.Error">
        <ul>
            @foreach (var error in _validationErrors)
            {
                <li>@error</li>
            }
        </ul>
    </MudAlert>
}
```

## 4. Toast Notifications

### Success Notifications

Show success toasts for completed operations:

```csharp
_toastService.ShowSuccess("Truck added successfully");
_toastService.ShowSuccess($"Invoice {invoiceNumber} created");
```

### Error Notifications

```csharp
_toastService.ShowError("Failed to save truck");
_toastService.ShowError(result.Message);
```

### Warning and Info

```csharp
_toastService.ShowWarning("CDL expires in 30 days");
_toastService.ShowInfo("Syncing data...");
```

## 5. Confirmation Dialogs

### Delete Confirmations

Always confirm destructive actions:

```csharp
var confirmed = await _dialogService.ConfirmAsync(
    "Delete Truck",
    $"Are you sure you want to delete truck {truckId}? This action cannot be undone.");
    
if (confirmed)
{
    await _truckService.DeleteTruckAsync(truckId);
}
```

### Using MudDialog for Confirmations

```razor
<MudButton OnClick="@ShowDeleteDialog">Delete</MudButton>

@code {
    private async Task ShowDeleteDialog()
    {
        var parameters = new DialogParameters<ConfirmDeleteDialog>();
        parameters.Add(x => x.Message, "Are you sure you want to delete this item?");
        
        var dialog = await DialogService.ShowAsync<ConfirmDeleteDialog>("Confirm Delete", parameters);
        var result = await dialog.Result;
        
        if (!result.Canceled)
        {
            await DeleteItem();
        }
    }
}
```

## 6. Error Logging

### Automatic Logging

All errors are automatically logged when using `BaseService`:

```csharp
await ExecuteWithErrorHandlingAsync(
    async () => await _repository.SaveAsync(),
    "saving data");  // This context appears in logs
```

### Manual Logging

```csharp
await _errorLogger.LogErrorAsync(exception, "TruckService.AddTruck");
await _errorLogger.LogWarningAsync("Odometer reading seems low", "TruckService");
await _errorLogger.LogInfoAsync("Truck added successfully", "TruckService");
```

### Viewing Logs

Logs are stored in: `{AppDataDirectory}/Logs/triply_{date}.log`

```csharp
var recentLogs = await _errorLogger.GetRecentLogsAsync(100);
await _errorLogger.ClearLogsAsync();
```

## 7. Complete Example

### Service with Full Error Handling

```csharp
public class ExpenseService : BaseService
{
    private readonly ExpenseRepository _repository;
    private readonly IValidator<Expense> _validator;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    
    public ExpenseService(
        ExpenseRepository repository,
        IValidator<Expense> validator,
        IErrorLogger errorLogger,
        IToastService toastService,
        IDialogService dialogService) : base(errorLogger)
    {
        _repository = repository;
        _validator = validator;
        _toastService = toastService;
        _dialogService = dialogService;
    }
    
    public async Task<OperationResult<Expense>> AddExpenseAsync(Expense expense)
    {
        var result = await ValidateAndExecuteAsync(
            expense,
            _validator,
            async () => await _repository.AddExpenseAsync(expense),
            "adding expense");
            
        if (result.Success && result.Data != null)
        {
            _toastService.ShowSuccess($"Expense ${expense.Amount:N2} added successfully");
            await _errorLogger.LogInfoAsync($"Expense {expense.ExpenseId} added", "ExpenseService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
        
        return result;
    }
    
    public async Task<OperationResult> DeleteExpenseAsync(Guid expenseId)
    {
        var confirmed = await _dialogService.ConfirmAsync(
            "Delete Expense",
            "Are you sure you want to delete this expense? This action cannot be undone.");
            
        if (!confirmed)
        {
            return OperationResult.FailureResult("Delete cancelled");
        }
        
        var result = await ExecuteWithErrorHandlingAsync(
            async () => await _repository.DeleteExpenseAsync(expenseId),
            "deleting expense");
            
        if (result.Success)
        {
            _toastService.ShowSuccess("Expense deleted successfully");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
        
        return result;
    }
}
```

### Blazor Component with Full Validation

```razor
@page "/expense/add"
@inject IValidator<Expense> ExpenseValidator
@inject ExpenseService ExpenseService
@inject NavigationManager Navigation

<ValidationSummary ValidationErrors="@_validationErrors" />

<MudForm @ref="_form" @bind-IsValid="@_formValid">
    <MudNumericField @bind-Value="_expense.Amount" 
                     Label="Amount" 
                     Required="true"
                     Min="0" />
    
    <MudDatePicker @bind-Date="_expenseDate" 
                   Label="Date" 
                   Required="true" />
</MudForm>

<MudButton OnClick="@SaveExpense" Disabled="@(!_formValid)">Save</MudButton>

@code {
    private Expense _expense = new();
    private MudForm _form = null!;
    private bool _formValid;
    private List<string> _validationErrors = new();
    private DateTime? _expenseDate = DateTime.Now;
    
    private async Task SaveExpense()
    {
        _validationErrors.Clear();
        _expense.ExpenseDate = _expenseDate ?? DateTime.Now;
        
        var result = await ExpenseService.AddExpenseAsync(_expense);
        
        if (result.Success)
        {
            Navigation.NavigateTo("/expenses");
        }
        else
        {
            _validationErrors = result.Errors;
        }
    }
}
```

## Best Practices

1. **Always validate** user input before saving
2. **Always confirm** destructive actions (delete, void)
3. **Always show feedback** (success/error toasts)
4. **Always log errors** for debugging
5. **Always use OperationResult** for service methods
6. **Keep validators focused** on validation, not business logic
7. **Provide helpful error messages** - user-friendly, not technical
8. **Test validation rules** with unit tests

## Validation Rules Summary

### Format Validations
- **Email**: Valid email format
- **Phone**: US phone format (555-555-5555)
- **EIN**: XX-XXXXXXX format
- **DOT Number**: 7-8 digits
- **MC Number**: MC-XXXXXX format
- **VIN**: Exactly 17 characters
- **ZIP Code**: XXXXX or XXXXX-XXXX
- **State**: 2 character codes

### Range Validations
- **Dates**: Not in the future (for historical records)
- **Amounts**: Greater than 0
- **Odometer**: Cannot decrease
- **Tax Rates**: 0-100%

### Business Rules
- **Load Status**: Cannot invoice pending/cancelled loads
- **Invoice Calculations**: Totals must match
- **Payment**: Cannot exceed balance
- **CDL Expiration**: Must be in future
- **Delivery Date**: Must be after pickup date
