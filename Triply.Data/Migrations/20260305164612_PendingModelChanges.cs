using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Triply.Data.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailSignature",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnableEmailNotifications",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FromEmail",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FromName",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SMTPPassword",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SMTPPort",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SMTPServer",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SMTPUsername",
                table: "CompanySettings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSSL",
                table: "CompanySettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AccountingPeriods",
                columns: table => new
                {
                    PeriodId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Month = table.Column<int>(type: "INTEGER", nullable: true),
                    Quarter = table.Column<int>(type: "INTEGER", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingPeriods", x => x.PeriodId);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    AccountType = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ParentAccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailOutboxQueue",
                columns: table => new
                {
                    QueueId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ToEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ToName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    BodyHtml = table.Column<string>(type: "TEXT", nullable: false),
                    BodyText = table.Column<string>(type: "TEXT", nullable: true),
                    AttachmentData = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AttachmentFileName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AttachmentContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InvoiceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EmailType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOutboxQueue", x => x.QueueId);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    JournalEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntryNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EntryType = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceDocument = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    IsPosted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsReversed = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReversedByEntryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.JournalEntryId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationSettings",
                columns: table => new
                {
                    NotificationSettingsId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnableNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableInvoiceAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableMaintenanceAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableDocumentAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableTaxAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableIFTAAlerts = table.Column<bool>(type: "INTEGER", nullable: false),
                    InvoiceDueWarningDays = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceDueWarningDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DocumentExpiryWarningDays = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    EnableLocalNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    QuietHoursStart = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationSettings", x => x.NotificationSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "QueuedOperations",
                columns: table => new
                {
                    QueuedOperationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperationType = table.Column<int>(type: "INTEGER", nullable: false),
                    OperationData = table.Column<string>(type: "TEXT", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxRetries = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedOperations", x => x.QueuedOperationId);
                });

            migrationBuilder.CreateTable(
                name: "BankReconciliations",
                columns: table => new
                {
                    ReconciliationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatementDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatementBeginningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StatementEndingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookBeginningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookEndingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsReconciled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReconciledDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankReconciliations", x => x.ReconciliationId);
                    table.ForeignKey(
                        name: "FK_BankReconciliations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryLines",
                columns: table => new
                {
                    JournalEntryLineId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JournalEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsReconciled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReconciledDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryLines", x => x.JournalEntryLineId);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "JournalEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriods_Year_Month_Quarter",
                table: "AccountingPeriods",
                columns: new[] { "Year", "Month", "Quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: "Accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ParentAccountId",
                table: "Accounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankReconciliations_AccountId",
                table: "BankReconciliations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxQueue_Status",
                table: "EmailOutboxQueue",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailOutboxQueue_Status_Priority_CreatedDate",
                table: "EmailOutboxQueue",
                columns: new[] { "Status", "Priority", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryDate",
                table: "JournalEntries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_AccountId",
                table: "JournalEntryLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryLines_JournalEntryId",
                table: "JournalEntryLines",
                column: "JournalEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingPeriods");

            migrationBuilder.DropTable(
                name: "BankReconciliations");

            migrationBuilder.DropTable(
                name: "EmailOutboxQueue");

            migrationBuilder.DropTable(
                name: "JournalEntryLines");

            migrationBuilder.DropTable(
                name: "NotificationSettings");

            migrationBuilder.DropTable(
                name: "QueuedOperations");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "EmailSignature",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "EnableEmailNotifications",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "FromEmail",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "FromName",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SMTPPassword",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SMTPPort",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SMTPServer",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "SMTPUsername",
                table: "CompanySettings");

            migrationBuilder.DropColumn(
                name: "UseSSL",
                table: "CompanySettings");
        }
    }
}
