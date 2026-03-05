using FluentValidation;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class TruckService : BaseService
{
    private readonly ITruckRepository _truckRepository;
    private readonly IValidator<Truck> _truckValidator;
    private readonly IToastService _toastService;

    public TruckService(
        ITruckRepository truckRepository,
        IValidator<Truck> truckValidator,
        IErrorLogger errorLogger,
        IToastService toastService) : base(errorLogger)
    {
        _truckRepository = truckRepository;
        _truckValidator = truckValidator;
        _toastService = toastService;
    }

    public async Task<OperationResult<IEnumerable<Truck>>> GetAllTrucksAsync()
    {
        return await ExecuteWithErrorHandlingAsync(
            async () => await _truckRepository.GetAllTrucksAsync(),
            "loading trucks");
    }

    public async Task<OperationResult<Truck?>> GetTruckByIdAsync(string truckId)
    {
        return await ExecuteWithErrorHandlingAsync(
            async () => await _truckRepository.GetTruckByIdAsync(truckId),
            $"loading truck {truckId}");
    }

    public async Task<OperationResult<Truck>> AddTruckAsync(Truck truck)
    {
        var result = await ValidateAndExecuteAsync(
            truck,
            _truckValidator,
            async () => await _truckRepository.AddTruckAsync(truck),
            "adding truck");

        if (result.Success)
        {
            _toastService.ShowSuccess($"Truck {truck.TruckId} added successfully");
            await _errorLogger.LogInfoAsync($"Truck {truck.TruckId} added", "TruckService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult> UpdateTruckAsync(Truck truck)
    {
        var result = await ValidateAndExecuteAsync(
            truck,
            _truckValidator,
            async () => await _truckRepository.UpdateTruckAsync(truck),
            "updating truck");

        if (result.Success)
        {
            _toastService.ShowSuccess($"Truck {truck.TruckId} updated successfully");
            await _errorLogger.LogInfoAsync($"Truck {truck.TruckId} updated", "TruckService");
        }
        else
        {
            _toastService.ShowError(result.Message);
        }

        return result;
    }

    public async Task<OperationResult> DeleteTruckAsync(string truckId)
    {
        // Note: Confirmation dialog should be handled in the UI layer
        var operationResult = await ExecuteWithErrorHandlingAsync(
            async () => await _truckRepository.DeleteTruckAsync(truckId),
            "deleting truck");

        if (operationResult.Success)
        {
            _toastService.ShowSuccess($"Truck {truckId} deleted successfully");
            await _errorLogger.LogInfoAsync($"Truck {truckId} deleted", "TruckService");
        }
        else
        {
            _toastService.ShowError(operationResult.Message);
        }

        return operationResult;
    }
}
