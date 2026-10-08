using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Interfaces;

public interface IDeviceRepository
{
    Device? GetById(int deviceId);

    Device? GetByCode(string deviceCode);

    IReadOnlyList<Device> GetAll();

    void Add(Device device);

    void Update(Device device);
}

public interface IBookingRepository
{
    IReadOnlyList<Booking> GetActiveBookings(int deviceId);

    void Add(Booking booking);
    bool TryAddIfNoConflict(Booking booking);

    void Update(Booking booking);
}

public interface IAlarmRepository
{
    Alarm? GetActiveAlarm(int deviceId, string alarmType);

    void Add(Alarm alarm);

    void Update(Alarm alarm);
}

public interface IMaintenanceRepository
{
    IReadOnlyList<MaintenanceOrder> GetActiveOrders(int deviceId);

    void Add(MaintenanceOrder order);

    void Update(MaintenanceOrder order);
}

public interface ITelemetryRepository
{
    void Add(TelemetryRecord telemetry);

    IReadOnlyList<TelemetryRecord> GetRecent(
        int deviceId,
        int count);

    /// <summary>
    /// 查询指定设备在时间范围内的遥测数据。
    /// 返回时间范围内最新的 maximumCount 条记录，
    /// 结果按照采集时间从早到晚排列。
    /// </summary>
    IReadOnlyList<TelemetryRecord> GetByTimeRange(
        int deviceId,
        DateTime startTime,
        DateTime endTime,
        int maximumCount);
}

public interface IUserRepository
{
    UserAccount? GetById(int userId);

    UserAccount? GetByUserName(
        string userName);

    IReadOnlyList<UserAccount> GetAll();

    void Add(UserAccount user);

    void Update(UserAccount user);
}

public interface IOperationLogRepository
{
    void Add(OperationLog operationLog);

    IReadOnlyList<OperationLog> GetRecent(
        int count);

    IReadOnlyList<OperationLog> GetByUser(
        int userId,
        int count);
}
