namespace IndustrialDeviceSystem.Core.Enums;

public enum DeviceStatus
{
    Offline = 0,
    Idle = 1,
    Running = 2,
    Maintenance = 3,
    Fault = 4
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3,
    Rejected = 4
}

public enum AlarmLevel
{
    Normal = 0,
    Warning = 1,
    High = 2,
    Emergency = 3
}

public enum AlarmStatus
{
    Active = 0,
    Acknowledged = 1,
    Cleared = 2
}

public enum MaintenanceStatus
{
    Created = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Closed = 4,
    Cancelled = 5
}

public enum UserRole
{
    Administrator = 0,
    Operator = 1,
    Maintainer = 2,
    Auditor = 3
}

public enum DataQuality
{
    Good = 0,
    Uncertain = 1,
    Bad = 2,
    CommunicationLost = 3
}

public enum BookingErrorCode
{
    None = 0,
    UserNotAllowed = 1,
    DeviceDisabled = 2,
    DeviceUnavailable = 3,
    InvalidTimeRange = 4,
    TimeConflict = 5,
    MaintenanceConflict = 6
}
