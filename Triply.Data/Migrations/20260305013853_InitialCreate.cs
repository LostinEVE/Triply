using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Triply.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DBA = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DOTNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MCNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EIN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    LogoImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    InvoicePrefix = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    NextInvoiceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultPaymentTerms = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FederalTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    StateTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SelfEmploymentTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FiscalYearStart = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    BillingAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BillingCity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BillingState = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    BillingZip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PaymentTerms = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "TaxPayments",
                columns: table => new
                {
                    TaxPaymentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaxType = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxYear = table.Column<int>(type: "INTEGER", nullable: false),
                    TaxQuarter = table.Column<int>(type: "INTEGER", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxPayments", x => x.TaxPaymentId);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    VIN = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LicensePlateState = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentOdometer = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.TruckId);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CDLNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CDLState = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    CDLExpiration = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AssignedTruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PayRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PayType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverId);
                    table.ForeignKey(
                        name: "FK_Drivers_Trucks_AssignedTruckId",
                        column: x => x.AssignedTruckId,
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    ExpenseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiptImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    IsDeductible = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaxCategory = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.ExpenseId);
                    table.ForeignKey(
                        name: "FK_Expenses_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    MaintenanceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MaintenanceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Odometer = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Vendor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PartsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsWarranty = table.Column<bool>(type: "INTEGER", nullable: false),
                    NextDueOdometer = table.Column<int>(type: "INTEGER", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Documents = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.MaintenanceId);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FuelEntries",
                columns: table => new
                {
                    FuelEntryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FuelDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Odometer = table.Column<int>(type: "INTEGER", nullable: false),
                    Gallons = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PricePerGallon = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: false),
                    TruckStop = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    FuelCardLast4 = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    ReceiptImage = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IFTA_Quarter = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelEntries", x => x.FuelEntryId);
                    table.ForeignKey(
                        name: "FK_FuelEntries_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FuelEntries_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loads",
                columns: table => new
                {
                    LoadId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoadNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TruckId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PickupAddress = table.Column<string>(type: "TEXT", nullable: true),
                    PickupCity = table.Column<string>(type: "TEXT", nullable: true),
                    PickupState = table.Column<string>(type: "TEXT", nullable: true),
                    PickupZip = table.Column<string>(type: "TEXT", nullable: true),
                    PickupDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PickupTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "TEXT", nullable: true),
                    DeliveryCity = table.Column<string>(type: "TEXT", nullable: true),
                    DeliveryState = table.Column<string>(type: "TEXT", nullable: true),
                    DeliveryZip = table.Column<string>(type: "TEXT", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeliveryTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Miles = table.Column<int>(type: "INTEGER", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RateType = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PODReceived = table.Column<bool>(type: "INTEGER", nullable: false),
                    PODDocument = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loads", x => x.LoadId);
                    table.ForeignKey(
                        name: "FK_Loads_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loads_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "DriverId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Loads_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "TruckId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    LineItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InvoiceId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoadId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.LineItemId);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "LoadId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_AssignedTruckId",
                table: "Drivers",
                column: "AssignedTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TruckId",
                table: "Expenses",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_DriverId",
                table: "FuelEntries",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_TruckId",
                table: "FuelEntries",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_LoadId",
                table: "InvoiceLineItems",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_CustomerId",
                table: "Loads",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_DriverId",
                table: "Loads",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_TruckId",
                table: "Loads",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_TruckId",
                table: "MaintenanceRecords",
                column: "TruckId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "FuelEntries");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "TaxPayments");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Loads");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "Trucks");
        }
    }
}
