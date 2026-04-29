namespace IMS_API_.Models.Domains;

public enum UserRole
{
    Admin = 1,
    Staff = 2,
    Customer = 3
}

public enum AppointmentStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4
}

public enum InvoicePaymentStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4
}

public enum PartRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Fulfilled = 4
}
