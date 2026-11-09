namespace Staybnb.Enums
{
    public enum BookingStatus { Pending, Approved, Rejected, CheckedIn, Cancelled }

    public enum ApplicationStatus { Pending, Approved, Rejected }

    public enum PaymentStatus { Pending, Paid, Failed }

    public enum CheckInStatus { NotStarted, InProgress, Submitted, Verified }

    public enum DocumentStatus { Pending, Verified, Rejected }

    public enum NotificationType { BookingApproved, BookingRejected, NewMessage, HostApplication, General }

    public enum ActivityType { Login, Register, RoleChange, BookingStatus, Application, Other }
}