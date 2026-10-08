PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS UserAccount
(
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    UserName TEXT NOT NULL COLLATE NOCASE UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role INTEGER NOT NULL
        CHECK (Role BETWEEN 0 AND 3),
    IsEnabled INTEGER NOT NULL DEFAULT 1
        CHECK (IsEnabled IN (0, 1)),
    CreatedAt TEXT NOT NULL DEFAULT
        (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE TABLE IF NOT EXISTS Device
(
    DeviceId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceCode TEXT NOT NULL COLLATE NOCASE UNIQUE,
    DeviceName TEXT NOT NULL,
    DeviceType TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0
        CHECK (Status BETWEEN 0 AND 4),
    Enabled INTEGER NOT NULL DEFAULT 1
        CHECK (Enabled IN (0, 1)),
    LastCommunicationTime TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT
        (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE TABLE IF NOT EXISTS Booking
(
    BookingId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId INTEGER NOT NULL,
    UserId INTEGER NOT NULL,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    Purpose TEXT NOT NULL DEFAULT '',
    Status INTEGER NOT NULL DEFAULT 0
        CHECK (Status BETWEEN 0 AND 4),
    CreatedAt TEXT NOT NULL,

    CONSTRAINT CK_Booking_Time
        CHECK (StartTime < EndTime),

    CONSTRAINT FK_Booking_Device
        FOREIGN KEY (DeviceId)
        REFERENCES Device (DeviceId)
        ON DELETE RESTRICT,

    CONSTRAINT FK_Booking_User
        FOREIGN KEY (UserId)
        REFERENCES UserAccount (UserId)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS TelemetryData
(
    TelemetryId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId INTEGER NOT NULL,
    CollectedAt TEXT NOT NULL,
    Temperature REAL NOT NULL,
    Pressure REAL NOT NULL,
    Vibration REAL NOT NULL,
    IsRunning INTEGER NOT NULL
        CHECK (IsRunning IN (0, 1)),
    FaultSignal INTEGER NOT NULL
        CHECK (FaultSignal IN (0, 1)),
    EmergencyStop INTEGER NOT NULL
        CHECK (EmergencyStop IN (0, 1)),
    Quality INTEGER NOT NULL
        CHECK (Quality BETWEEN 0 AND 3),

    CONSTRAINT FK_Telemetry_Device
        FOREIGN KEY (DeviceId)
        REFERENCES Device (DeviceId)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS Alarm
(
    AlarmId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId INTEGER NOT NULL,
    AlarmType TEXT NOT NULL,
    Level INTEGER NOT NULL
        CHECK (Level BETWEEN 0 AND 3),
    Message TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0
        CHECK (Status BETWEEN 0 AND 2),
    OccurredAt TEXT NOT NULL,
    LastOccurredAt TEXT NOT NULL,
    OccurrenceCount INTEGER NOT NULL DEFAULT 1
        CHECK (OccurrenceCount >= 1),
    AcknowledgedBy INTEGER NULL,
    AcknowledgedAt TEXT NULL,
    ClearedAt TEXT NULL,

    CONSTRAINT FK_Alarm_Device
        FOREIGN KEY (DeviceId)
        REFERENCES Device (DeviceId)
        ON DELETE RESTRICT,

    CONSTRAINT FK_Alarm_AcknowledgedBy
        FOREIGN KEY (AcknowledgedBy)
        REFERENCES UserAccount (UserId)
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS MaintenanceOrder
(
    OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
    DeviceId INTEGER NOT NULL,
    SourceAlarmId INTEGER NULL,
    AssigneeId INTEGER NULL,
    Description TEXT NOT NULL,
    Result TEXT NOT NULL DEFAULT '',
    Status INTEGER NOT NULL DEFAULT 0
        CHECK (Status BETWEEN 0 AND 5),
    CreatedAt TEXT NOT NULL,
    PlannedStartTime TEXT NULL,
    PlannedEndTime TEXT NULL,
    StartedAt TEXT NULL,
    CompletedAt TEXT NULL,

    CONSTRAINT CK_Maintenance_PlannedTime
        CHECK
        (
            PlannedStartTime IS NULL
            OR PlannedEndTime IS NULL
            OR PlannedStartTime < PlannedEndTime
        ),

    CONSTRAINT FK_Maintenance_Device
        FOREIGN KEY (DeviceId)
        REFERENCES Device (DeviceId)
        ON DELETE RESTRICT,

    CONSTRAINT FK_Maintenance_Alarm
        FOREIGN KEY (SourceAlarmId)
        REFERENCES Alarm (AlarmId)
        ON DELETE SET NULL,

    CONSTRAINT FK_Maintenance_Assignee
        FOREIGN KEY (AssigneeId)
        REFERENCES UserAccount (UserId)
        ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS OperationLog
(
    LogId INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NULL,
    ActionType TEXT NOT NULL,
    TargetType TEXT NOT NULL,
    TargetId INTEGER NULL,
    Detail TEXT NOT NULL,
    OccurredAt TEXT NOT NULL,

    CONSTRAINT FK_Log_User
        FOREIGN KEY (UserId)
        REFERENCES UserAccount (UserId)
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS IX_Booking_Device_Time
ON Booking
(
    DeviceId,
    StartTime,
    EndTime,
    Status
);

CREATE INDEX IF NOT EXISTS IX_Telemetry_Device_Time
ON TelemetryData
(
    DeviceId,
    CollectedAt
);

CREATE INDEX IF NOT EXISTS IX_Alarm_Device_Time
ON Alarm
(
    DeviceId,
    OccurredAt
);

CREATE INDEX IF NOT EXISTS IX_Maintenance_Device_Status
ON MaintenanceOrder
(
    DeviceId,
    Status
);

CREATE INDEX IF NOT EXISTS IX_Log_Time
ON OperationLog
(
    OccurredAt
);

CREATE INDEX IF NOT EXISTS
IX_OperationLog_OccurredAt
ON OperationLog
(
    OccurredAt DESC,
    LogId DESC
);

CREATE INDEX IF NOT EXISTS
IX_OperationLog_User_Time
ON OperationLog
(
    UserId,
    OccurredAt DESC,
    LogId DESC
);

CREATE UNIQUE INDEX IF NOT EXISTS
    UX_Alarm_Active_Device_Type
ON Alarm
(
    DeviceId,
    AlarmType
)
WHERE Status IN (0, 1);
