using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using CommunityToolkit.Maui;
using Triply.Data;
using Triply.Data.Repositories;
using Triply.Services;
using Triply.Core.Interfaces;
using Triply.Core.Validators;
using FluentValidation;

namespace Triply
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Configure SQLite database
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "triply.db");
            builder.Services.AddDbContext<TriplyDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Add DbContextFactory for services that need to create contexts
            builder.Services.AddDbContextFactory<TriplyDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register Unit of Work
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IUnitOfWork, UnitOfWork>(builder.Services);

            // Register validators
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Truck>, TruckValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Driver>, DriverValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Customer>, CustomerValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Load>, LoadValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Invoice>, InvoiceValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.Expense>, ExpenseValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.FuelEntry>, FuelEntryValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.MaintenanceRecord>, MaintenanceRecordValidator>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IValidator<Triply.Core.Models.CompanySettings>, CompanySettingsValidator>(builder.Services);

            // Register repositories (for backward compatibility with existing code)
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<ITruckRepository, TruckRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IDriverRepository, DriverRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<ILoadRepository, LoadRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IInvoiceRepository, InvoiceRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<CustomerRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<ExpenseRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<FuelEntryRepository>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<MaintenanceRepository>(builder.Services);

            // Register error handling and toast services
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IErrorLogger, ErrorLoggerService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IToastService, MudBlazorToastService>(builder.Services);

            // Register services
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IGeolocationService, GeolocationService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IConnectivityService, ConnectivityService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<ICameraService, CameraService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<INotificationService, NotificationService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<IPrintService, PrintService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddSingleton<ILoadingStateService, LoadingStateService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IOfflineQueueService, OfflineQueueService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IPdfGenerationService, PdfGenerationService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<EmailService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IEmailService, OfflineAwareEmailService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<LoadManagementService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IFTAReportService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<TruckingBusinessService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<Triply.Services.CostPerMileService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<Triply.Services.TaxEstimatorService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<Triply.Services.AccountingService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<Triply.Services.InvoiceService>(builder.Services);

            // Register subscription service
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<ISubscriptionService, SubscriptionService>(builder.Services);

            // Register enhanced services with validation and error handling
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<TruckService>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<InvoiceServiceEnhanced>(builder.Services);
            Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddScoped<IDataImportExportService, DataImportExportService>(builder.Services);

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<TriplyDbContext>();
                DatabaseInitializer.InitializeAsync(context).Wait();

                // Initialize and start notification service
                var notificationService = app.Services.GetRequiredService<INotificationService>();
                notificationService.CheckAndNotifyAsync().Wait();
                notificationService.StartPeriodicCheck();
            }

            return app;
        }
    }
}
