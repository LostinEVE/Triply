# Quick Start: Adding Validation to Your Forms

## Step 1: Create a Validator (if not exists)

```csharp
// Triply.Core/Validators/YourModelValidator.cs
using FluentValidation;
using Triply.Core.Models;

namespace Triply.Core.Validators;

public class YourModelValidator : AbstractValidator<YourModel>
{
    public YourModelValidator()
    {
        RuleFor(m => m.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
            
        RuleFor(m => m.Email)
            .EmailAddress().WithMessage("Must be a valid email address")
            .When(m => !string.IsNullOrEmpty(m.Email));
            
        RuleFor(m => m.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}
```

## Step 2: Register Validator in MauiProgram.cs

```csharp
builder.Services.AddScoped<IValidator<YourModel>, YourModelValidator>();
```

## Step 3: Create/Update Service with Error Handling

```csharp
// Triply.Services/YourModelService.cs
public class YourModelService : BaseService
{
    private readonly IRepository<YourModel> _repository;
    private readonly IValidator<YourModel> _validator;
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    
    public YourModelService(
        IRepository<YourModel> repository,
        IValidator<YourModel> validator,
        IErrorLogger errorLogger,
        IToastService toastService,
        IDialogService dialogService) : base(errorLogger)
    {
        _repository = repository;
        _validator = validator;
        _toastService = toastService;
        _dialogService = dialogService;
    }
    
    public async Task<OperationResult<YourModel>> SaveAsync(YourModel model)
    {
        var result = await ValidateAndExecuteAsync(
            model,
            _validator,
            async () =>
            {
                await _repository.AddAsync(model);
                await _repository.SaveChangesAsync();
                return model;
            },
            "saving data");
            
        if (result.Success)
        {
            _toastService.ShowSuccess("Saved successfully");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }
        
        return result;
    }
    
    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var confirmed = await _dialogService.ConfirmAsync(
            "Confirm Delete",
            "Are you sure you want to delete this item?");
            
        if (!confirmed)
        {
            return OperationResult.FailureResult("Cancelled");
        }
        
        var result = await ExecuteWithErrorHandlingAsync(
            async () =>
            {
                var item = await _repository.GetByIdAsync(id);
                if (item != null)
                {
                    await _repository.DeleteAsync(item);
                    await _repository.SaveChangesAsync();
                }
            },
            "deleting data");
            
        if (result.Success)
        {
            _toastService.ShowSuccess("Deleted successfully");
        }
        
        return result;
    }
}
```

## Step 4: Update Blazor Page

```razor
@page "/your-page"
@using Triply.Core.Models
@using FluentValidation
@inject YourModelService Service
@inject NavigationManager Navigation

<PageTitle>Your Page</PageTitle>

<MudText Typo="Typo.h4">Add/Edit Item</MudText>

<ValidationSummary ValidationErrors="@_validationErrors" />

<MudForm @ref="_form" @bind-IsValid="@_formValid">
    <MudTextField @bind-Value="_model.Name" 
                  Label="Name" 
                  Required="true"
                  MaxLength="100" />
                  
    <MudTextField @bind-Value="_model.Email" 
                  Label="Email" 
                  InputType="InputType.Email" />
                  
    <MudNumericField @bind-Value="_model.Amount" 
                     Label="Amount" 
                     Min="0"
                     Required="true" />
</MudForm>

<MudButton OnClick="@SaveItem" 
           Disabled="@(!_formValid || _saving)"
           Variant="Variant.Filled"
           Color="Color.Primary">
    @if (_saving)
    {
        <MudProgressCircular Size="Size.Small" Indeterminate="true" />
        <MudText Class="ml-2">Saving...</MudText>
    }
    else
    {
        <MudText>Save</MudText>
    }
</MudButton>

@code {
    private YourModel _model = new();
    private MudForm _form = null!;
    private bool _formValid;
    private bool _saving;
    private List<string> _validationErrors = new();
    
    private async Task SaveItem()
    {
        _validationErrors.Clear();
        _saving = true;
        
        try
        {
            var result = await Service.SaveAsync(_model);
            
            if (result.Success)
            {
                Navigation.NavigateTo("/your-list");
            }
            else
            {
                _validationErrors = result.Errors;
            }
        }
        finally
        {
            _saving = false;
        }
    }
}
```

## Step 5: Add Delete Confirmation

```razor
<MudButton OnClick="@(() => DeleteItem(_model.Id))" 
           Color="Color.Error"
           Variant="Variant.Outlined">
    Delete
</MudButton>

@code {
    private async Task DeleteItem(Guid id)
    {
        var result = await Service.DeleteAsync(id);
        
        if (result.Success)
        {
            Navigation.NavigateTo("/your-list");
        }
    }
}
```

## Common Validation Rules

### Required Fields
```csharp
RuleFor(x => x.Field).NotEmpty().WithMessage("Field is required");
```

### String Length
```csharp
RuleFor(x => x.Field)
    .MaximumLength(100).WithMessage("Cannot exceed 100 characters")
    .MinimumLength(3).WithMessage("Must be at least 3 characters");
```

### Email
```csharp
RuleFor(x => x.Email)
    .EmailAddress().WithMessage("Must be a valid email")
    .When(x => !string.IsNullOrEmpty(x.Email));
```

### Phone (US Format)
```csharp
RuleFor(x => x.Phone)
    .Matches(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$")
    .WithMessage("Must be a valid phone number")
    .When(x => !string.IsNullOrEmpty(x.Phone));
```

### Numbers
```csharp
RuleFor(x => x.Amount)
    .GreaterThan(0).WithMessage("Must be greater than 0")
    .LessThanOrEqualTo(1000000).WithMessage("Cannot exceed 1,000,000");
```

### Dates
```csharp
RuleFor(x => x.Date)
    .LessThanOrEqualTo(DateTime.Now).WithMessage("Cannot be in the future");
    
RuleFor(x => x.EndDate)
    .GreaterThanOrEqualTo(x => x.StartDate)
    .WithMessage("End date must be after start date");
```

### Conditional Validation
```csharp
RuleFor(x => x.FieldA)
    .NotEmpty()
    .When(x => x.FieldB == SomeValue);
```

### Custom Business Rules
```csharp
RuleFor(x => x.Field)
    .Must((model, field) => CustomValidationLogic(model, field))
    .WithMessage("Custom validation failed");
```

## MudBlazor Form Validation

### Basic Validation
```razor
<MudTextField @bind-Value="_model.Name" 
              Label="Name"
              Required="true"
              RequiredError="Name is required"
              MaxLength="100" />
```

### Email Validation
```razor
<MudTextField @bind-Value="_model.Email"
              Label="Email"
              InputType="InputType.Email"
              Validation="@(new EmailAddressAttribute())" />
```

### Number Validation
```razor
<MudNumericField @bind-Value="_model.Amount"
                 Label="Amount"
                 Min="0"
                 Max="1000000"
                 Required="true" />
```

### Date Validation
```razor
<MudDatePicker @bind-Date="_model.Date"
               Label="Date"
               MaxDate="@DateTime.Today"
               Required="true" />
```

## Toast Notifications

```csharp
// Success
_toastService.ShowSuccess("Operation completed");

// Error
_toastService.ShowError("Operation failed");

// Warning
_toastService.ShowWarning("CDL expires soon");

// Info
_toastService.ShowInfo("Processing...");
```

## Dialog Confirmations

```csharp
// Simple confirmation
var confirmed = await _dialogService.ConfirmAsync(
    "Delete Item",
    "Are you sure? This cannot be undone.");

if (confirmed)
{
    // Proceed with delete
}

// With custom buttons
var confirmed = await _dialogService.ConfirmAsync(
    "Warning",
    "This action has consequences. Continue?");
```

## Testing Validators

```csharp
[Fact]
public void Should_Require_Name()
{
    var validator = new YourModelValidator();
    var model = new YourModel { Name = "" };
    
    var result = validator.Validate(model);
    
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Name");
}

[Fact]
public void Should_Validate_Email_Format()
{
    var validator = new YourModelValidator();
    var model = new YourModel { Email = "invalid" };
    
    var result = validator.Validate(model);
    
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Email");
}
```

## Checklist for New Forms

- [ ] Create validator with all business rules
- [ ] Register validator in MauiProgram.cs
- [ ] Create service inheriting from BaseService
- [ ] Add ValidationSummary component to form
- [ ] Use MudForm with @bind-IsValid
- [ ] Add Required attributes to required fields
- [ ] Add confirmation dialogs for delete operations
- [ ] Show success toast on save
- [ ] Show error toast on failure
- [ ] Disable submit button while saving
- [ ] Clear validation errors before new submission
- [ ] Navigate away on success
- [ ] Test all validation rules
