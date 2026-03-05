namespace Triply.Core.Enums;

public enum TruckStatus
{
    Active = 0,
    InShop = 1,
    Sold = 2,
    Inactive = 3
}

public enum PayType
{
    PerMile = 0,
    Percentage = 1,
    Hourly = 2,
    Salary = 3
}

public enum LoadStatus
{
    Booked = 0,
    InTransit = 1,
    Delivered = 2,
    Invoiced = 3,
    Paid = 4
}

public enum RateType
{
    FlatRate = 0,
    PerMile = 1
}

public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4,
    Void = 5,
    Cancelled = 6
}

public enum ExpenseCategory
{
    Fuel = 0,
    Maintenance = 1,
    Insurance = 2,
    Permits = 3,
    Tolls = 4,
    Parking = 5,
    Scales = 6,
    Lumper = 7,
    DriverPay = 8,
    OfficeExpense = 9,
    TruckPayment = 10,
    Trailer = 11,
    Tires = 12,
    Other = 13
}

public enum PaymentMethod
{
    Cash = 0,
    CreditCard = 1,
    DebitCard = 2,
    FuelCard = 3,
    Check = 4,
    ACH = 5
}

public enum FuelType
{
    Diesel = 0,
    DEF = 1
}

public enum MaintenanceType
{
    OilChange = 0,
    Tires = 1,
    Brakes = 2,
    Transmission = 3,
    Engine = 4,
    Electrical = 5,
    HVAC = 6,
    DOTInspection = 7,
    AnnualInspection = 8,
    PreventiveMaintenance = 9,
    Repair = 10,
    Other = 11
}

public enum TaxType
{
    FederalQuarterly = 0,
    StateQuarterly = 1,
    IFTA = 2,
    HeavyVehicleUse2290 = 3,
    UCR = 4,
    SelfEmployment = 5
}

public enum TaxPaymentStatus
{
    Pending = 0,
    Paid = 1,
    Late = 2,
    Overdue = 3
}
