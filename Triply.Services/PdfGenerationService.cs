using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Triply.Core.Interfaces;
using Triply.Core.Models;

namespace Triply.Services;

public class PdfGenerationService : IPdfGenerationService
{
    public PdfGenerationService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region Invoice PDF

    public async Task<byte[]> GenerateInvoiceAsync(Invoice invoice, CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(settings.CompanyName).SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                                col.Item().Text($"DOT# {settings.DOTNumber} | MC# {settings.MCNumber}").FontSize(10);
                                col.Item().Text($"{settings.Address}").FontSize(10);
                                col.Item().Text($"{settings.City}, {settings.State} {settings.Zip}").FontSize(10);
                                col.Item().Text($"Phone: {settings.Phone}").FontSize(10);
                                col.Item().Text($"Email: {settings.Email}").FontSize(10);
                            });

                            row.ConstantItem(100).AlignRight().Text("INVOICE")
                                .SemiBold().FontSize(28).FontColor(Colors.Blue.Darken2);
                        });

                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                    {
                        column.Spacing(15);

                        // Invoice Info & Bill To
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(innerCol =>
                                {
                                    innerCol.Item().Text("Invoice Details").SemiBold().FontSize(12);
                                    innerCol.Item().Text($"Invoice #: {invoice.InvoiceNumber}").FontSize(10);
                                    innerCol.Item().Text($"Invoice Date: {invoice.InvoiceDate:MM/dd/yyyy}").FontSize(10);
                                    innerCol.Item().Text($"Due Date: {invoice.DueDate:MM/dd/yyyy}").FontSize(10);
                                    innerCol.Item().Text($"Payment Terms: {invoice.Customer.PaymentTerms}").FontSize(10);
                                });
                            });

                            row.ConstantItem(20);

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(innerCol =>
                                {
                                    innerCol.Item().Text("Bill To:").SemiBold().FontSize(12);
                                    innerCol.Item().Text(invoice.Customer.CompanyName).SemiBold().FontSize(11);
                                    if (!string.IsNullOrEmpty(invoice.Customer.ContactName))
                                        innerCol.Item().Text(invoice.Customer.ContactName).FontSize(10);
                                    if (!string.IsNullOrEmpty(invoice.Customer.BillingAddress))
                                    {
                                        innerCol.Item().Text(invoice.Customer.BillingAddress).FontSize(10);
                                        innerCol.Item().Text($"{invoice.Customer.BillingCity}, {invoice.Customer.BillingState} {invoice.Customer.BillingZip}").FontSize(10);
                                    }
                                    if (!string.IsNullOrEmpty(invoice.Customer.ContactEmail))
                                        innerCol.Item().Text($"Email: {invoice.Customer.ContactEmail}").FontSize(10);
                                    if (!string.IsNullOrEmpty(invoice.Customer.ContactPhone))
                                        innerCol.Item().Text($"Phone: {invoice.Customer.ContactPhone}").FontSize(10);
                                });
                            });
                        });

                        // Line Items Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Description");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Quantity");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Rate");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Amount");

                                static IContainer HeaderStyle(IContainer container)
                                {
                                    return container
                                        .Background(Colors.Blue.Darken2)
                                        .Padding(8)
                                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White));
                                }
                            });

                            foreach (var item in invoice.LineItems)
                            {
                                table.Cell().Element(CellStyle).Text(item.Description);
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("N2"));
                                table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice.ToString("C2"));
                                table.Cell().Element(CellStyle).AlignRight().Text(item.LineTotal.ToString("C2"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(8);
                                }
                            }
                        });

                        // Totals Section
                        column.Item().AlignRight().Width(250).Column(col =>
                        {
                            col.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(totalCol =>
                            {
                                totalCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Subtotal:");
                                    row.RelativeItem().AlignRight().Text(invoice.Subtotal.ToString("C2"));
                                });

                                if (invoice.TaxAmount > 0)
                                {
                                    totalCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Tax:");
                                        row.RelativeItem().AlignRight().Text(invoice.TaxAmount.ToString("C2"));
                                    });
                                }

                                totalCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                totalCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text("Total:").SemiBold().FontSize(13);
                                    row.RelativeItem().AlignRight().Text(invoice.TotalAmount.ToString("C2"))
                                        .SemiBold().FontSize(13).FontColor(Colors.Blue.Darken2);
                                });

                                if (invoice.AmountPaid > 0)
                                {
                                    totalCol.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text("Amount Paid:");
                                        row.RelativeItem().AlignRight().Text(invoice.AmountPaid.ToString("C2"));
                                    });

                                    totalCol.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text("Balance Due:").SemiBold();
                                        row.RelativeItem().AlignRight().Text(invoice.Balance.ToString("C2"))
                                            .SemiBold().FontColor(Colors.Red.Darken1);
                                    });
                                }
                            });
                        });

                        // Notes
                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            column.Item().PaddingTop(10).Column(col =>
                            {
                                col.Item().Text("Notes:").SemiBold();
                                col.Item().Background(Colors.Grey.Lighten4).Padding(10)
                                    .Text(invoice.Notes).FontSize(10);
                            });
                        }

                        // Payment Instructions
                        column.Item().PaddingTop(20).Column(col =>
                        {
                            col.Item().Text("Payment Instructions").SemiBold().FontSize(12);
                            col.Item().Background(Colors.Blue.Lighten5).Padding(10).Column(innerCol =>
                            {
                                innerCol.Item().Text($"Please make checks payable to: {settings.CompanyName}").FontSize(10);
                                innerCol.Item().Text($"Mail to: {settings.Address}, {settings.City}, {settings.State} {settings.Zip}").FontSize(10);
                                innerCol.Item().Text($"For questions, contact: {settings.Email} or {settings.Phone}").FontSize(10);
                            });
                        });

                        // Thank You
                        column.Item().PaddingTop(20).AlignCenter()
                            .Text("Thank you for your business!")
                            .FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                        footer.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().AlignLeft()
                                .Text($"Generated: {DateTime.Now:MM/dd/yyyy hh:mm tt}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                            row.RelativeItem().AlignCenter().Text(x =>
                            {
                                x.DefaultTextStyle(style => style.FontSize(8));
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                            row.RelativeItem().AlignRight()
                                .Text($"Invoice: {invoice.InvoiceNumber}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region Profit & Loss Report

    public async Task<byte[]> GenerateProfitLossReportAsync(
        IncomeStatement incomeStatement,
        CompanySettings settings,
        IncomeStatement? priorPeriod = null)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("PROFIT & LOSS STATEMENT")
                            .SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text(incomeStatement.PeriodLabel).FontSize(12);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(10);

                        // Revenue Section
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("REVENUE").SemiBold().FontColor(Colors.White);

                            foreach (var account in incomeStatement.RevenueAccounts)
                            {
                                col.Item().PaddingLeft(15).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(100).AlignRight().Text(account.Balance.ToString("C0"));
                                    if (priorPeriod != null)
                                    {
                                        var priorAmount = priorPeriod.RevenueAccounts
                                            .FirstOrDefault(a => a.AccountId == account.AccountId)?.Balance ?? 0;
                                        var change = account.Balance - priorAmount;
                                        var changePercent = priorAmount > 0 ? (change / priorAmount) * 100 : 0;
                                        row.ConstantItem(80).AlignRight()
                                            .Text($"{changePercent:+0.0;-0.0;0}%")
                                            .FontSize(9)
                                            .FontColor(change >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                                    }
                                });
                            }

                            col.Item().PaddingTop(5).PaddingLeft(15).Row(row =>
                            {
                                row.RelativeItem().Text("Total Revenue").SemiBold();
                                row.ConstantItem(100).AlignRight()
                                    .Text(incomeStatement.TotalRevenue.ToString("C0")).SemiBold();
                            });
                        });

                        // Gross Profit (if COGS exists)
                        if (incomeStatement.TotalCOGS > 0)
                        {
                            column.Item().Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten2).Padding(5)
                                    .Text("COST OF GOODS SOLD").SemiBold();

                                foreach (var account in incomeStatement.COGSAccounts)
                                {
                                    col.Item().PaddingLeft(15).Row(row =>
                                    {
                                        row.RelativeItem().Text(account.AccountName);
                                        row.ConstantItem(100).AlignRight().Text(account.Balance.ToString("C0"));
                                    });
                                }

                                col.Item().PaddingTop(5).PaddingLeft(15).Row(row =>
                                {
                                    row.RelativeItem().Text("Total COGS").SemiBold();
                                    row.ConstantItem(100).AlignRight()
                                        .Text(incomeStatement.TotalCOGS.ToString("C0")).SemiBold();
                                });

                                col.Item().PaddingTop(5).Background(Colors.Blue.Lighten4).Padding(5).Row(row =>
                                {
                                    row.RelativeItem().Text("GROSS PROFIT").SemiBold();
                                    row.ConstantItem(100).AlignRight()
                                        .Text(incomeStatement.GrossProfit.ToString("C0")).SemiBold();
                                    row.ConstantItem(80).AlignRight()
                                        .Text($"{incomeStatement.GrossProfitMargin:N1}%").FontSize(9);
                                });
                            });
                        }

                        // Operating Expenses
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Red.Lighten3).Padding(5)
                                .Text("OPERATING EXPENSES").SemiBold();

                            foreach (var account in incomeStatement.OperatingExpenses.OrderByDescending(a => a.Balance))
                            {
                                var percent = incomeStatement.TotalRevenue > 0
                                    ? (account.Balance / incomeStatement.TotalRevenue) * 100
                                    : 0;

                                col.Item().PaddingLeft(15).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(100).AlignRight().Text(account.Balance.ToString("C0"));
                                    row.ConstantItem(80).AlignRight().Text($"{percent:N1}%").FontSize(9);
                                });
                            }

                            col.Item().PaddingTop(5).PaddingLeft(15).Row(row =>
                            {
                                row.RelativeItem().Text("Total Operating Expenses").SemiBold();
                                row.ConstantItem(100).AlignRight()
                                    .Text(incomeStatement.TotalOperatingExpenses.ToString("C0")).SemiBold();
                            });

                            col.Item().PaddingTop(5).Background(Colors.Blue.Lighten4).Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("OPERATING INCOME").SemiBold();
                                row.ConstantItem(100).AlignRight()
                                    .Text(incomeStatement.OperatingIncome.ToString("C0")).SemiBold();
                                row.ConstantItem(80).AlignRight()
                                    .Text($"{incomeStatement.OperatingMargin:N1}%").FontSize(9);
                            });
                        });

                        // Other Income/Expenses
                        if (incomeStatement.TotalOtherIncome > 0 || incomeStatement.TotalOtherExpenses > 0)
                        {
                            column.Item().Column(col =>
                            {
                                if (incomeStatement.TotalOtherIncome > 0)
                                {
                                    col.Item().PaddingLeft(15).Row(row =>
                                    {
                                        row.RelativeItem().Text("Other Income");
                                        row.ConstantItem(100).AlignRight()
                                            .Text(incomeStatement.TotalOtherIncome.ToString("C0"));
                                    });
                                }

                                if (incomeStatement.TotalOtherExpenses > 0)
                                {
                                    col.Item().PaddingLeft(15).Row(row =>
                                    {
                                        row.RelativeItem().Text("Other Expenses");
                                        row.ConstantItem(100).AlignRight()
                                            .Text($"({incomeStatement.TotalOtherExpenses:C0})");
                                    });
                                }
                            });
                        }

                        // Net Income
                        column.Item().PaddingTop(10).Background(Colors.Green.Lighten3).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text("NET INCOME").SemiBold().FontSize(14);
                            row.ConstantItem(100).AlignRight()
                                .Text(incomeStatement.NetIncome.ToString("C0"))
                                .SemiBold().FontSize(14)
                                .FontColor(incomeStatement.NetIncome >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            row.ConstantItem(80).AlignRight()
                                .Text($"{incomeStatement.NetProfitMargin:N1}%")
                                .SemiBold().FontSize(12);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region Balance Sheet

    public async Task<byte[]> GenerateBalanceSheetAsync(
        BalanceSheet balanceSheet,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("BALANCE SHEET").SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text(balanceSheet.DateLabel).FontSize(12);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(15);

                        // ASSETS
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Blue.Darken2).Padding(5)
                                .Text("ASSETS").SemiBold().FontColor(Colors.White).FontSize(12);

                            // Current Assets
                            col.Item().PaddingLeft(10).PaddingTop(5)
                                .Text("Current Assets").SemiBold();
                            foreach (var account in balanceSheet.CurrentAssets)
                            {
                                col.Item().PaddingLeft(20).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                });
                            }
                            col.Item().PaddingLeft(20).PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text("Total Current Assets").Italic();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalCurrentAssets.ToString("C0")).Italic();
                            });

                            // Fixed Assets
                            col.Item().PaddingLeft(10).PaddingTop(8)
                                .Text("Fixed Assets").SemiBold();
                            foreach (var account in balanceSheet.FixedAssets)
                            {
                                col.Item().PaddingLeft(20).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                });
                            }
                            col.Item().PaddingLeft(20).PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text("Total Fixed Assets").Italic();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalFixedAssets.ToString("C0")).Italic();
                            });

                            // Other Assets
                            if (balanceSheet.OtherAssets.Any())
                            {
                                col.Item().PaddingLeft(10).PaddingTop(8)
                                    .Text("Other Assets").SemiBold();
                                foreach (var account in balanceSheet.OtherAssets)
                                {
                                    col.Item().PaddingLeft(20).Row(row =>
                                    {
                                        row.RelativeItem().Text(account.AccountName);
                                        row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                    });
                                }
                                col.Item().PaddingLeft(20).PaddingTop(3).Row(row =>
                                {
                                    row.RelativeItem().Text("Total Other Assets").Italic();
                                    row.ConstantItem(120).AlignRight()
                                        .Text(balanceSheet.TotalOtherAssets.ToString("C0")).Italic();
                                });
                            }

                            col.Item().PaddingTop(8).Background(Colors.Blue.Lighten4).Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL ASSETS").SemiBold();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalAssets.ToString("C0")).SemiBold();
                            });
                        });

                        // LIABILITIES & EQUITY
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Red.Lighten3).Padding(5)
                                .Text("LIABILITIES").SemiBold().FontSize(12);

                            // Current Liabilities
                            col.Item().PaddingLeft(10).PaddingTop(5)
                                .Text("Current Liabilities").SemiBold();
                            foreach (var account in balanceSheet.CurrentLiabilities)
                            {
                                col.Item().PaddingLeft(20).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                });
                            }
                            col.Item().PaddingLeft(20).PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text("Total Current Liabilities").Italic();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalCurrentLiabilities.ToString("C0")).Italic();
                            });

                            // Long-Term Liabilities
                            if (balanceSheet.LongTermLiabilities.Any())
                            {
                                col.Item().PaddingLeft(10).PaddingTop(8)
                                    .Text("Long-Term Liabilities").SemiBold();
                                foreach (var account in balanceSheet.LongTermLiabilities)
                                {
                                    col.Item().PaddingLeft(20).Row(row =>
                                    {
                                        row.RelativeItem().Text(account.AccountName);
                                        row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                    });
                                }
                                col.Item().PaddingLeft(20).PaddingTop(3).Row(row =>
                                {
                                    row.RelativeItem().Text("Total Long-Term Liabilities").Italic();
                                    row.ConstantItem(120).AlignRight()
                                        .Text(balanceSheet.TotalLongTermLiabilities.ToString("C0")).Italic();
                                });
                            }

                            col.Item().PaddingTop(8).Background(Colors.Red.Lighten4).Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL LIABILITIES").SemiBold();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalLiabilities.ToString("C0")).SemiBold();
                            });
                        });

                        // EQUITY
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Green.Lighten3).Padding(5)
                                .Text("EQUITY").SemiBold().FontSize(12);

                            foreach (var account in balanceSheet.EquityAccounts)
                            {
                                col.Item().PaddingLeft(20).Row(row =>
                                {
                                    row.RelativeItem().Text(account.AccountName);
                                    row.ConstantItem(120).AlignRight().Text(account.Balance.ToString("C0"));
                                });
                            }

                            col.Item().PaddingLeft(20).Row(row =>
                            {
                                row.RelativeItem().Text("Retained Earnings");
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.RetainedEarnings.ToString("C0"));
                            });

                            col.Item().PaddingTop(8).Background(Colors.Green.Lighten4).Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL EQUITY").SemiBold();
                                row.ConstantItem(120).AlignRight()
                                    .Text(balanceSheet.TotalEquity.ToString("C0")).SemiBold();
                            });
                        });

                        // TOTAL LIABILITIES & EQUITY
                        column.Item().PaddingTop(10).Background(Colors.Grey.Lighten2).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL LIABILITIES & EQUITY").SemiBold().FontSize(12);
                            row.ConstantItem(120).AlignRight()
                                .Text(balanceSheet.TotalLiabilitiesAndEquity.ToString("C0"))
                                .SemiBold().FontSize(12);
                        });

                        // Balance Check
                        column.Item().AlignCenter().Text(
                            balanceSheet.IsBalanced
                                ? "✓ Balance Sheet is balanced"
                                : "⚠ Balance Sheet is out of balance!")
                            .FontSize(10)
                            .FontColor(balanceSheet.IsBalanced ? Colors.Green.Darken2 : Colors.Red.Darken2);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region Tax Reports

    public async Task<byte[]> GenerateTaxReportAsync(
        QuarterlyTaxEstimate taxEstimate,
        TaxDeductionSummary deductions,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("QUARTERLY TAX ESTIMATE")
                            .SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text(taxEstimate.QuarterLabel).FontSize(14);
                        column.Item().AlignCenter().Text($"{taxEstimate.StartDate:MM/dd/yyyy} - {taxEstimate.EndDate:MM/dd/yyyy}")
                            .FontSize(11);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(15);

                        // Income Summary
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Green.Lighten3).Padding(8)
                                .Text("INCOME SUMMARY").SemiBold().FontSize(12);

                            col.Item().PaddingLeft(15).PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Gross Income");
                                r.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.GrossIncome.ToString("C0")).SemiBold();
                            });

                            col.Item().PaddingLeft(15).Row(r =>
                            {
                                r.RelativeItem().Text("Total Deductions");
                                r.ConstantItem(120).AlignRight()
                                    .Text($"({deductions.TotalDeductions:C0})");
                            });     

                            col.Item().PaddingLeft(15).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                            col.Item().PaddingLeft(15).PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text("Net Profit").SemiBold();
                                r.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.NetProfit.ToString("C0")).SemiBold();
                            });
                        });

                        // Tax Deductions Breakdown
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Blue.Lighten4).Padding(8)
                                .Text("TAX DEDUCTIONS (Schedule C Format)").SemiBold().FontSize(11);

                            var breakdown = deductions.GetBreakdown();
                            foreach (var (category, (amount, percentage)) in breakdown.OrderByDescending(kvp => kvp.Value.Amount))
                            {
                                if (amount > 0)
                                {
                                    col.Item().PaddingLeft(15).Row(row =>
                                    {
                                        row.RelativeItem().Text(category);
                                        row.ConstantItem(100).AlignRight().Text(amount.ToString("C0"));
                                        row.ConstantItem(60).AlignRight()
                                            .Text($"{percentage:N1}%").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    });
                                }
                            }

                            col.Item().PaddingTop(5).PaddingLeft(15).Background(Colors.Grey.Lighten3)
                                .Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("Total Deductions").SemiBold();
                                row.ConstantItem(100).AlignRight()
                                    .Text(deductions.TotalDeductions.ToString("C0")).SemiBold();
                            });
                        });

                        // Tax Calculations
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Orange.Lighten3).Padding(8)
                                .Text("TAX CALCULATIONS").SemiBold().FontSize(12);

                            col.Item().PaddingLeft(15).PaddingTop(5)
                                .Text("Self-Employment Tax:").SemiBold().FontSize(11);
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("Net Profit × 92.35%");
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.SelfEmploymentTaxableIncome.ToString("C0"));
                            });
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("× 15.3% (SE Tax Rate)");
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.SelfEmploymentTax.ToString("C0")).SemiBold();
                            });

                            col.Item().PaddingLeft(15).PaddingTop(8)
                                .Text("Federal Income Tax:").SemiBold().FontSize(11);
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("Adjusted Gross Income");
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.AdjustedGrossIncome.ToString("C0"));
                            });
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("Less: Standard Deduction");
                                row.ConstantItem(120).AlignRight()
                                    .Text($"({taxEstimate.StandardDeduction:C0})");
                            });
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("Taxable Income");
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.TaxableIncome.ToString("C0"));
                            });
                            col.Item().PaddingLeft(25).Row(row =>
                            {
                                row.RelativeItem().Text("Federal Income Tax");
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.FederalIncomeTax.ToString("C0")).SemiBold();
                            });

                            if (taxEstimate.StateIncomeTax > 0)
                            {
                                col.Item().PaddingLeft(15).PaddingTop(8)
                                    .Text("State Income Tax:").SemiBold().FontSize(11);
                                col.Item().PaddingLeft(25).Row(row =>
                                {
                                    row.RelativeItem().Text("State Tax");
                                    row.ConstantItem(120).AlignRight()
                                        .Text(taxEstimate.StateIncomeTax.ToString("C0")).SemiBold();
                                });
                            }
                        });

                        // Quarterly Payment
                        column.Item().PaddingTop(10).Background(Colors.Red.Lighten3).Padding(10).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Total Annual Tax Liability").SemiBold().FontSize(12);
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.TotalTaxLiability.ToString("C0"))
                                    .SemiBold().FontSize(12);
                            });

                            col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Red.Darken1);

                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("QUARTERLY ESTIMATED PAYMENT").SemiBold().FontSize(14);
                                row.ConstantItem(120).AlignRight()
                                    .Text(taxEstimate.QuarterlyEstimatedPayment.ToString("C0"))
                                    .SemiBold().FontSize(14).FontColor(Colors.Red.Darken2);
                            });

                            col.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Text("Effective Tax Rate");
                                row.ConstantItem(120).AlignRight()
                                    .Text($"{taxEstimate.EffectiveTaxRate:N1}%").SemiBold();
                            });
                        });

                        // Safe Harbor
                        column.Item().Background(Colors.Blue.Lighten5).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(taxEstimate.SafeHarborMessage).SemiBold()
                                    .FontColor(taxEstimate.MeetsSafeHarbor ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                                col.Item().Text("Safe harbor protects from underpayment penalties").FontSize(9);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    public async Task<byte[]> GenerateAnnualTaxSummaryAsync(
        AnnualTaxProjection projection,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text($"{projection.Year} ANNUAL TAX PROJECTION")
                            .SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text($"Based on {projection.MonthsElapsed} months YTD")
                            .FontSize(11);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(15);

                        // YTD vs Projected
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Background(Colors.Blue.Lighten4).Padding(8)
                                    .Text("YEAR-TO-DATE ACTUALS").SemiBold().FontSize(11);
                                col.Item().PaddingLeft(15).PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Gross Income");
                                    r.ConstantItem(100).AlignRight()
                                        .Text(projection.YTDGrossIncome.ToString("C0"));
                                });
                                col.Item().PaddingLeft(15).Row(r =>
                                {
                                    r.RelativeItem().Text("Deductions");
                                    r.ConstantItem(100).AlignRight()
                                        .Text($"({projection.YTDDeductions:C0})");
                                });
                                col.Item().PaddingLeft(15).Row(r =>
                                {
                                    r.RelativeItem().Text("Net Profit").SemiBold();
                                    r.ConstantItem(100).AlignRight()
                                        .Text(projection.YTDNetProfit.ToString("C0")).SemiBold();
                                });
                            });

                            row.ConstantItem(20);

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Background(Colors.Green.Lighten4).Padding(8)
                                    .Text("PROJECTED ANNUAL").SemiBold().FontSize(11);
                                col.Item().PaddingLeft(15).PaddingTop(5).Row(r =>
                                {
                                    r.RelativeItem().Text("Income");
                                    r.ConstantItem(100).AlignRight()
                                        .Text(projection.ProjectedAnnualIncome.ToString("C0"));
                                });
                                col.Item().PaddingLeft(15).Row(r =>
                                {
                                    r.RelativeItem().Text("Deductions");
                                    r.ConstantItem(100).AlignRight()
                                        .Text($"({projection.ProjectedAnnualDeductions:C0})");
                                });
                                col.Item().PaddingLeft(15).Row(r =>
                                {
                                    r.RelativeItem().Text("Net Profit").SemiBold();
                                    r.ConstantItem(100).AlignRight()
                                        .Text(projection.ProjectedAnnualNetProfit.ToString("C0")).SemiBold();
                                });
                            });
                        });

                        // Projected Tax
                        column.Item().Background(Colors.Orange.Lighten3).Padding(10).Column(col =>
                        {
                            col.Item().Text("PROJECTED TAX LIABILITY").SemiBold().FontSize(12);
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Self-Employment Tax");
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.ProjectedSelfEmploymentTax.ToString("C0"));
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Federal Income Tax");
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.ProjectedFederalIncomeTax.ToString("C0"));
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("State Income Tax");
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.ProjectedStateIncomeTax.ToString("C0"));
                            });
                            col.Item().PaddingTop(5).LineHorizontal(2).LineColor(Colors.Orange.Darken1);
                            col.Item().PaddingTop(3).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL PROJECTED TAX").SemiBold().FontSize(13);
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.ProjectedTotalTax.ToString("C0"))
                                    .SemiBold().FontSize(13).FontColor(Colors.Red.Darken2);
                            });
                        });

                        // Payment Status
                        column.Item().Background(Colors.Blue.Lighten5).Padding(10).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Estimated Payments Made");
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.EstimatedPaymentsMade.ToString("C0"));
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Remaining Tax Liability").SemiBold();
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.RemainingTaxLiability.ToString("C0")).SemiBold();
                            });
                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Recommended Next Quarterly Payment");
                                row.ConstantItem(120).AlignRight()
                                    .Text(projection.RecommendedNextQuarterlyPayment.ToString("C0")).SemiBold();
                            });
                        });

                        // Quarterly Breakdown
                        if (projection.QuarterlyEstimates.Any())
                        {
                            column.Item().Column(col =>
                            {
                                col.Item().Background(Colors.Grey.Lighten2).Padding(8)
                                    .Text("QUARTERLY BREAKDOWN").SemiBold().FontSize(11);

                                col.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Quarter").SemiBold();
                                        header.Cell().AlignRight().Text("Income").SemiBold();
                                        header.Cell().AlignRight().Text("Tax").SemiBold();
                                        header.Cell().AlignRight().Text("Payment").SemiBold();
                                    });

                                    foreach (var q in projection.QuarterlyEstimates.Take(4))
                                    {
                                        table.Cell().Text(q.QuarterLabel);
                                        table.Cell().AlignRight().Text(q.GrossIncome.ToString("C0"));
                                        table.Cell().AlignRight().Text(q.TotalTaxLiability.ToString("C0"));
                                        table.Cell().AlignRight().Text(q.QuarterlyEstimatedPayment.ToString("C0"));
                                    }
                                });
                            });
                        }

                        // Tax Saving Tips
                        if (projection.TaxSavingTips.Any())
                        {
                            column.Item().Background(Colors.Yellow.Lighten3).Padding(10).Column(col =>
                            {
                                col.Item().Text("💡 TAX PLANNING RECOMMENDATIONS").SemiBold().FontSize(11);
                                foreach (var tip in projection.TaxSavingTips)
                                {
                                    col.Item().PaddingLeft(10).PaddingTop(3)
                                        .Text($"• {tip}").FontSize(9);
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region IFTA Report

    public async Task<byte[]> GenerateIFTAReportAsync(
        IFTAQuarterlyReport iftaReport,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(settings.CompanyName).SemiBold().FontSize(16);
                                col.Item().Text($"DOT# {settings.DOTNumber}").FontSize(10);
                                col.Item().Text(iftaReport.VIN != null ? $"VIN: {iftaReport.VIN}" : "Fleet Report")
                                    .FontSize(10);
                            });
                            row.RelativeItem().AlignCenter().Column(col =>
                            {
                                col.Item().Text("IFTA QUARTERLY REPORT").SemiBold().FontSize(18);
                                col.Item().Text(iftaReport.QuarterLabel).FontSize(14);
                                col.Item().Text($"{iftaReport.StartDate:MM/dd/yyyy} - {iftaReport.EndDate:MM/dd/yyyy}")
                                    .FontSize(11);
                            });
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Total Miles: {iftaReport.TotalMiles:N0}");
                                col.Item().Text($"Total Gallons: {iftaReport.TotalGallons:N1}");
                                col.Item().Text($"Fleet MPG: {iftaReport.FleetMPG:N2}");
                            });
                        });
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);  // State
                            columns.RelativeColumn();     // Miles
                            columns.RelativeColumn();     // Taxable Gal
                            columns.RelativeColumn();     // Gal Purchased
                            columns.RelativeColumn();     // Tax Rate
                            columns.RelativeColumn();     // Tax Owed
                            columns.RelativeColumn();     // Tax Paid
                            columns.RelativeColumn();     // Net Tax
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("State");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Miles\nDriven");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Taxable\nGallons");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Gallons\nPurchased");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Tax Rate\n(¢/gal)");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Tax\nOwed");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Tax\nPaid");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Net Tax\nDue/(Credit)");

                            static IContainer HeaderStyle(IContainer container)
                            {
                                return container.Background(Colors.Blue.Darken2).Padding(5)
                                    .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White).FontSize(8));
                            }
                        });

                        foreach (var state in iftaReport.StateData.OrderBy(s => s.State))
                        {
                            table.Cell().Element(CellStyle).Text(state.StateFullName);
                            table.Cell().Element(CellStyle).AlignRight().Text(state.MilesDriven.ToString("N0"));
                            table.Cell().Element(CellStyle).AlignRight().Text(state.TaxableGallons.ToString("N1"));
                            table.Cell().Element(CellStyle).AlignRight().Text(state.GallonsPurchased.ToString("N1"));
                            table.Cell().Element(CellStyle).AlignRight().Text(state.StateFuelTaxRate.ToString("N2"));
                            table.Cell().Element(CellStyle).AlignRight().Text(state.TaxOwed.ToString("C2"));
                            table.Cell().Element(CellStyle).AlignRight().Text(state.TaxPaid.ToString("C2"));

                            var netTax = state.NetTaxOwed - state.NetTaxCredit;
                            table.Cell().Element(CellStyle).AlignRight()
                                .Text(netTax.ToString("C2"))
                                .FontColor(netTax > 0 ? Colors.Red.Darken1 : Colors.Green.Darken1)
                                .SemiBold();

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(5);
                            }
                        }

                        // Totals Row
                        table.Cell().Element(TotalStyle).Text("TOTALS").SemiBold();
                        table.Cell().Element(TotalStyle).AlignRight()
                            .Text(iftaReport.TotalMiles.ToString("N0")).SemiBold();
                        table.Cell().Element(TotalStyle).AlignRight()
                            .Text(iftaReport.TotalGallons.ToString("N1")).SemiBold();
                        table.Cell().Element(TotalStyle);
                        table.Cell().Element(TotalStyle);
                        table.Cell().Element(TotalStyle).AlignRight()
                            .Text(iftaReport.TotalTaxOwed.ToString("C2")).SemiBold();
                        table.Cell().Element(TotalStyle).AlignRight()
                            .Text(iftaReport.TotalTaxCredit.ToString("C2")).SemiBold();
                        table.Cell().Element(TotalStyle).AlignRight()
                            .Text(iftaReport.NetIFTATax.ToString("C2"))
                            .SemiBold().FontSize(11)
                            .FontColor(iftaReport.NetIFTATax > 0 ? Colors.Red.Darken1 : Colors.Green.Darken1);

                        static IContainer TotalStyle(IContainer container)
                        {
                            return container.Background(Colors.Grey.Lighten2)
                                .BorderTop(2).BorderColor(Colors.Black)
                                .Padding(8);
                        }
                    });

                    page.Footer().Column(footer =>
                    {
                        footer.Item().PaddingTop(10).Background(Colors.Blue.Lighten5).Padding(8).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"NET IFTA TAX: {iftaReport.NetIFTATax:C2}")
                                    .SemiBold().FontSize(14)
                                    .FontColor(iftaReport.NetIFTATax > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                                col.Item().Text(
                                    iftaReport.NetIFTATax > 0
                                        ? "Amount to remit with IFTA return"
                                        : "Credit/refund expected")
                                    .FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Filing Deadline: {iftaReport.FilingDeadline:MMMM d, yyyy}")
                                    .SemiBold().FontSize(11);
                                var daysUntil = (iftaReport.FilingDeadline - DateTime.UtcNow).Days;
                                col.Item().Text($"({daysUntil} days remaining)")
                                    .FontSize(10)
                                    .FontColor(daysUntil < 7 ? Colors.Red.Darken1 : Colors.Grey.Darken1);
                            });
                        });

                        footer.Item().PaddingTop(5).AlignCenter().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region Maintenance Report

    public async Task<byte[]> GenerateMaintenanceReportAsync(
        List<MaintenanceRecord> maintenanceRecords,
        Truck truck,
        DateTime startDate,
        DateTime endDate,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text(settings.CompanyName).SemiBold().FontSize(16);
                                col.Item().Text("MAINTENANCE REPORT").SemiBold().FontSize(14);
                                col.Item().Text($"{startDate:MM/dd/yyyy} - {endDate:MM/dd/yyyy}").FontSize(11);
                            });
                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text($"Truck: {truck.TruckId}").SemiBold().FontSize(12);
                                col.Item().Text($"VIN: {truck.VIN}").FontSize(10);
                                col.Item().Text($"Make/Model: {truck.Make} {truck.Model}").FontSize(10);
                                col.Item().Text($"Year: {truck.Year}").FontSize(10);
                            });
                        });
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(15);

                        // Summary
                        column.Item().Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Maintenance Events").SemiBold();
                                col.Item().Text(maintenanceRecords.Count.ToString()).FontSize(14);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Cost").SemiBold();
                                col.Item().Text(maintenanceRecords.Sum(m => m.TotalCost).ToString("C0"))
                                    .FontSize(14).FontColor(Colors.Red.Darken1);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Average Cost per Event").SemiBold();
                                var avgCost = maintenanceRecords.Any()
                                    ? maintenanceRecords.Average(m => m.TotalCost)
                                    : 0;
                                col.Item().Text(avgCost.ToString("C0")).FontSize(14);
                            });
                        });

                        // Maintenance History Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);   // Date
                                columns.ConstantColumn(60);   // Odometer
                                columns.RelativeColumn(2);    // Description
                                columns.ConstantColumn(80);   // Cost
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Date");
                                header.Cell().Element(HeaderStyle).Text("Odometer");
                                header.Cell().Element(HeaderStyle).Text("Description");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Cost");

                                static IContainer HeaderStyle(IContainer container)
                                {
                                    return container.Background(Colors.Blue.Darken2).Padding(5)
                                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White).FontSize(9));
                                }
                            });

                            foreach (var record in maintenanceRecords.OrderBy(m => m.MaintenanceDate))
                            {
                                table.Cell().Element(CellStyle).Text(record.MaintenanceDate.ToString("MM/dd/yyyy"));
                                table.Cell().Element(CellStyle).Text(record.Odometer > 0 ? record.Odometer.ToString("N0") : "-");
                                table.Cell().Element(CellStyle).Text(record.Description ?? "");
                                table.Cell().Element(CellStyle).AlignRight().Text(record.TotalCost.ToString("C2"));

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5);
                                }
                            }
                        });

                        // Cost by Type
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Grey.Lighten2).Padding(8)
                                .Text("COST BY MAINTENANCE TYPE").SemiBold().FontSize(11);

                            var byType = maintenanceRecords
                                .GroupBy(m => m.Type)
                                .Select(g => new
                                {
                                    Type = g.Key.ToString(),
                                    Count = g.Count(),
                                    TotalCost = g.Sum(m => m.TotalCost)
                                })
                                .OrderByDescending(x => x.TotalCost)
                                .ToList();

                            foreach (var type in byType)
                            {
                                col.Item().PaddingLeft(15).Row(row =>
                                {
                                    row.RelativeItem().Text($"{type.Type} ({type.Count})");
                                    row.ConstantItem(100).AlignRight().Text(type.TotalCost.ToString("C0"));
                                    var percent = maintenanceRecords.Sum(m => m.TotalCost) > 0
                                        ? (type.TotalCost / maintenanceRecords.Sum(m => m.TotalCost)) * 100
                                        : 0;
                                    row.ConstantItem(60).AlignRight()
                                        .Text($"{percent:N1}%").FontSize(9);
                                });
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region Aging Report

    public async Task<byte[]> GenerateAgingReportAsync(
        InvoiceAgingReport agingReport,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("ACCOUNTS RECEIVABLE AGING REPORT")
                            .SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text($"As of {agingReport.ReportDate:MMMM d, yyyy}")
                            .FontSize(12);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(15);

                        // Summary by Bucket
                        column.Item().Row(row =>
                        {
                            foreach (var bucket in agingReport.AgingBuckets)
                            {
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(8).Column(col =>
                                {
                                    col.Item().Text(bucket.Label).SemiBold().FontSize(10);
                                    col.Item().Text(bucket.TotalAmount.ToString("C0"))
                                        .FontSize(13).FontColor(Colors.Blue.Darken2);
                                    col.Item().Text($"{bucket.InvoiceCount} invoices").FontSize(8);
                                });
                            }
                        });

                        column.Item().Background(Colors.Grey.Lighten2).Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text("Total Outstanding").SemiBold().FontSize(12);
                            row.RelativeItem().AlignRight().Text(agingReport.TotalOutstanding.ToString("C0"))
                                .SemiBold().FontSize(13).FontColor(Colors.Red.Darken2);
                        });

                        // Customer Summary Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);    // Customer
                                columns.RelativeColumn();     // Current
                                columns.RelativeColumn();     // 1-30
                                columns.RelativeColumn();     // 31-60
                                columns.RelativeColumn();     // 61-90
                                columns.RelativeColumn();     // 90+
                                columns.RelativeColumn();     // Total
                                columns.ConstantColumn(60);   // Risk
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Customer");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Current");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("1-30");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("31-60");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("61-90");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("90+");
                                header.Cell().Element(HeaderStyle).AlignRight().Text("Total");
                                header.Cell().Element(HeaderStyle).Text("Risk");

                                static IContainer HeaderStyle(IContainer container)
                                {
                                    return container.Background(Colors.Blue.Darken2).Padding(5)
                                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White).FontSize(8));
                                }
                            });

                            foreach (var customer in agingReport.CustomerSummaries)
                            {
                                table.Cell().Element(CellStyle).Text(customer.CustomerName);
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.Current.ToString("C0"));
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.Days1to30.ToString("C0"));
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.Days31to60.ToString("C0"));
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.Days61to90.ToString("C0"));
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.Over90Days.ToString("C0"))
                                    .FontColor(customer.Over90Days > 0 ? Colors.Red.Darken1 : Colors.Black);
                                table.Cell().Element(CellStyle).AlignRight().Text(customer.TotalOutstanding.ToString("C0"))
                                    .SemiBold();
                                table.Cell().Element(CellStyle).Text(customer.RiskLevel)
                                    .FontSize(8)
                                    .FontColor(customer.RiskLevel == "High Risk" ? Colors.Red.Darken1 :
                                              customer.RiskLevel == "Medium Risk" ? Colors.Orange.Darken1 :
                                              Colors.Green.Darken1);

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion

    #region CPM Report

    public async Task<byte[]> GenerateCPMReportAsync(
        CPMReport cpmReport,
        CompanySettings settings)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().AlignCenter().Text(settings.CompanyName)
                            .SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);
                        column.Item().AlignCenter().Text("COST PER MILE ANALYSIS")
                            .SemiBold().FontSize(16);
                        column.Item().AlignCenter().Text($"{cpmReport.StartDate:MM/dd/yyyy} - {cpmReport.EndDate:MM/dd/yyyy}")
                            .FontSize(12);
                        if (!string.IsNullOrEmpty(cpmReport.TruckId))
                            column.Item().AlignCenter().Text($"Truck: {cpmReport.TruckId}").FontSize(11);
                        column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Spacing(10);

                        // Summary
                        column.Item().Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Miles").SemiBold();
                                col.Item().Text(cpmReport.TotalMiles.ToString("N0")).FontSize(14);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Revenue").SemiBold();
                                col.Item().Text(cpmReport.TotalRevenue.ToString("C0"))
                                    .FontSize(14).FontColor(Colors.Green.Darken2);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Expenses").SemiBold();
                                col.Item().Text(cpmReport.TotalExpenses.ToString("C0"))
                                    .FontSize(14).FontColor(Colors.Red.Darken1);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Profit").SemiBold();
                                col.Item().Text(cpmReport.TotalProfit.ToString("C0"))
                                    .FontSize(14)
                                    .FontColor(cpmReport.TotalProfit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            });
                        });

                        // Operating Costs
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Red.Lighten3).Padding(8)
                                .Text("OPERATING COSTS").SemiBold().FontSize(12);

                            var costs = new[]
                            {
                                ("Fuel", cpmReport.FuelCost, cpmReport.FuelCPM),
                                ("Maintenance", cpmReport.MaintenanceCost, cpmReport.MaintenanceCPM),
                                ("Insurance", cpmReport.InsuranceCost, cpmReport.InsuranceCPM),
                                ("Permits & Licenses", cpmReport.PermitsCost, cpmReport.PermitsCPM),
                                ("Tolls", cpmReport.TollsCost, cpmReport.TollsCPM),
                                ("Truck Payment", cpmReport.TruckPaymentCost, cpmReport.TruckPaymentCPM),
                                ("Driver Pay", cpmReport.DriverPayCost, cpmReport.DriverPayCPM),
                                ("Tires", cpmReport.TiresCost, cpmReport.TiresCPM),
                                ("Other", cpmReport.OtherExpenses, cpmReport.OtherCPM)
                            };

                            foreach (var (label, cost, cpm) in costs)
                            {
                                if (cost > 0)
                                {
                                    col.Item().PaddingLeft(15).Row(row =>
                                    {
                                        row.RelativeItem().Text(label);
                                        row.ConstantItem(100).AlignRight().Text(cost.ToString("C0"));
                                        row.ConstantItem(80).AlignRight().Text(cpm.ToString("C3")).FontSize(9);
                                    });
                                }
                            }

                            col.Item().PaddingTop(5).PaddingLeft(15).Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("Total Operating Cost").SemiBold();
                                row.ConstantItem(100).AlignRight().Text(cpmReport.TotalExpenses.ToString("C0")).SemiBold();
                                row.ConstantItem(80).AlignRight().Text(cpmReport.TotalCPM.ToString("C3")).SemiBold();
                            });
                        });

                        // Fixed vs Variable
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("FIXED COSTS").SemiBold().FontSize(11);
                                col.Item().Text(cpmReport.FixedCosts.ToString("C0")).FontSize(13);
                                col.Item().Text(cpmReport.FixedCPM.ToString("C3")).FontSize(11);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(10).Column(col =>
                            {
                                col.Item().Text("VARIABLE COSTS").SemiBold().FontSize(11);
                                col.Item().Text(cpmReport.VariableCosts.ToString("C0")).FontSize(13);
                                col.Item().Text(cpmReport.VariableCPM.ToString("C3")).FontSize(11);
                            });
                        });

                        // Revenue & Profitability
                        column.Item().Column(col =>
                        {
                            col.Item().Background(Colors.Green.Lighten3).Padding(8)
                                .Text("REVENUE & PROFITABILITY").SemiBold().FontSize(12);

                            col.Item().PaddingLeft(15).PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text("Revenue per Mile");
                                row.ConstantItem(100).AlignRight().Text(cpmReport.RevenuePerMile.ToString("C3"));
                            });
                            col.Item().PaddingLeft(15).Row(row =>
                            {
                                row.RelativeItem().Text("Cost per Mile");
                                row.ConstantItem(100).AlignRight().Text(cpmReport.TotalCPM.ToString("C3"));
                            });
                            col.Item().PaddingLeft(15).Row(row =>
                            {
                                row.RelativeItem().Text("Profit per Mile").SemiBold();
                                row.ConstantItem(100).AlignRight()
                                    .Text(cpmReport.ProfitPerMile.ToString("C3"))
                                    .SemiBold()
                                    .FontColor(cpmReport.ProfitPerMile >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            });

                            col.Item().PaddingTop(5).PaddingLeft(15).Row(row =>
                            {
                                row.RelativeItem().Text("Profit Margin");
                                row.ConstantItem(100).AlignRight().Text($"{cpmReport.ProfitMargin:N1}%").SemiBold();
                            });
                        });

                        // Break-Even Analysis
                        column.Item().Background(Colors.Yellow.Lighten3).Padding(10).Column(col =>
                        {
                            col.Item().Text("BREAK-EVEN ANALYSIS").SemiBold().FontSize(11);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Break-Even Rate");
                                row.ConstantItem(100).AlignRight().Text(cpmReport.BreakEvenRatePerMile.ToString("C3"));
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Target Rate (15% margin)");
                                row.ConstantItem(100).AlignRight().Text(cpmReport.TargetRatePerMile.ToString("C3")).SemiBold();
                            });
                        });

                        // Efficiency Metrics
                        column.Item().Background(Colors.Blue.Lighten5).Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Average MPG").SemiBold();
                                col.Item().Text(cpmReport.AverageMPG.ToString("N2")).FontSize(13);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Total Loads").SemiBold();
                                col.Item().Text(cpmReport.TotalLoads.ToString("N0")).FontSize(13);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Avg Revenue/Load").SemiBold();
                                col.Item().Text(cpmReport.AverageRevenuePerLoad.ToString("C0")).FontSize(13);
                            });
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Avg Miles/Load").SemiBold();
                                col.Item().Text(cpmReport.AverageMilesPerLoad.ToString("N0")).FontSize(13);
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(s => s.FontSize(8));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    #endregion
}
