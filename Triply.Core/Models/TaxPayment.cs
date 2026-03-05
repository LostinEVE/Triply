using Triply.Core.Enums;

namespace Triply.Core.Models;

public class TaxPayment
{
    public int TaxPaymentId { get; set; }
    public TaxType TaxType { get; set; }
    public int TaxYear { get; set; }
    public int? TaxQuarter { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public TaxPaymentStatus Status { get; set; }
}
