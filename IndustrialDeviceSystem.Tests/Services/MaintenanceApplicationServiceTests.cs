using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;
using Xunit;

namespace IndustrialDeviceSystem.Tests.Services;

public sealed class MaintenanceApplicationServiceTests
{
    private static readonly DateTime CurrentTime =
        new(2026, 10, 4, 11, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateFromAlarm_WithAssignee_ShouldCreateAssignedOrder()
    {
        TestContext context = CreateContext();

        MaintenanceOrder order =
            context.Service.CreateFromAlarm(
                CreateAlarm(),
                assigneeId: 3,
                description: "检查设备温度传感器",
                plannedStartTime: CurrentTime.AddHours(1),
                plannedEndTime: CurrentTime.AddHours(3));

        Assert.True(order.OrderId > 0);
        Assert.Equal(1, order.DeviceId);
        Assert.Equal(10, order.SourceAlarmId);
        Assert.Equal(3, order.AssigneeId);
        Assert.Equal(
            MaintenanceStatus.Assigned,
            order.Status);
        Assert.Equal(CurrentTime, order.CreatedAt);
        Assert.Equal(1, context.Repository.AddCallCount);
    }

    [Fact]
    public void CreateFromAlarm_DuplicateSourceAlarm_ShouldReturnExisting()
    {
        TestContext context = CreateContext();

        var existing = new MaintenanceOrder
        {
            OrderId = 20,
            DeviceId = 1,
            SourceAlarmId = 10,
            AssigneeId = 3,
            Description = "已有维护工单",
            Status = MaintenanceStatus.InProgress,
            CreatedAt = CurrentTime.AddHours(-2),
            StartedAt = CurrentTime.AddHours(-1)
        };

        context.Repository.Seed(existing);

        MaintenanceOrder result =
            context.Service.CreateFromAlarm(
                CreateAlarm(),
                assigneeId: 4,
                description: "重复请求");

        Assert.Same(existing, result);
        Assert.Equal(0, context.Repository.AddCallCount);
    }

    [Theory]
    [InlineData(AlarmLevel.Warning, AlarmStatus.Active)]
    [InlineData(AlarmLevel.High, AlarmStatus.Cleared)]
    public void CreateFromAlarm_InvalidAlarm_ShouldFail(
        AlarmLevel level,
        AlarmStatus status)
    {
        TestContext context = CreateContext();
        Alarm alarm = CreateAlarm();

        alarm.Level = level;
        alarm.Status = status;

        Assert.Throws<InvalidOperationException>(() =>
            context.Service.CreateFromAlarm(
                alarm,
                assigneeId: null,
                description: "测试维护"));

        Assert.Equal(0, context.Repository.AddCallCount);
    }

    [Fact]
    public void CreateFromAlarm_IncompletePlannedTime_ShouldFail()
    {
        TestContext context = CreateContext();

        Assert.Throws<ArgumentException>(() =>
            context.Service.CreateFromAlarm(
                CreateAlarm(),
                assigneeId: null,
                description: "测试维护",
                plannedStartTime: CurrentTime.AddHours(1),
                plannedEndTime: null));

        Assert.Equal(0, context.Repository.AddCallCount);
    }

    [Fact]
    public void MaintenanceOrder_ShouldFollowCompleteLifecycle()
    {
        TestContext context = CreateContext();

        MaintenanceOrder order =
            context.Service.CreateFromAlarm(
                CreateAlarm(),
                assigneeId: null,
                description: "处理温度高报警");

        Assert.Equal(
            MaintenanceStatus.Created,
            order.Status);

        bool assigned =
            context.Service.Assign(order, assigneeId: 3);

        Assert.True(assigned);
        Assert.Equal(
            MaintenanceStatus.Assigned,
            order.Status);
        Assert.Equal(3, order.AssigneeId);

        context.Clock.Now =
            CurrentTime.AddMinutes(10);

        bool started =
            context.Service.Start(order);

        Assert.True(started);
        Assert.Equal(
            MaintenanceStatus.InProgress,
            order.Status);
        Assert.Equal(
            context.Clock.Now,
            order.StartedAt);

        context.Clock.Now =
            CurrentTime.AddHours(1);

        bool completed =
            context.Service.Complete(
                order,
                "已更换温度传感器并完成测试");

        Assert.True(completed);
        Assert.Equal(
            MaintenanceStatus.Completed,
            order.Status);
        Assert.Equal(
            "已更换温度传感器并完成测试",
            order.Result);
        Assert.Equal(
            context.Clock.Now,
            order.CompletedAt);

        bool closed =
            context.Service.Close(
                order,
                verificationPassed: true);

        Assert.True(closed);
        Assert.Equal(
            MaintenanceStatus.Closed,
            order.Status);

        Assert.Equal(4, context.Repository.UpdateCallCount);
    }

    [Fact]
    public void IllegalStateTransitions_ShouldNotUpdateRepository()
    {
        TestContext context = CreateContext();

        var order = new MaintenanceOrder
        {
            OrderId = 30,
            DeviceId = 1,
            Status = MaintenanceStatus.Created,
            Description = "尚未指派的工单",
            CreatedAt = CurrentTime
        };

        context.Repository.Seed(order);

        bool started =
            context.Service.Start(order);

        bool completed =
            context.Service.Complete(
                order,
                "不应允许完成");

        bool closed =
            context.Service.Close(
                order,
                verificationPassed: true);

        Assert.False(started);
        Assert.False(completed);
        Assert.False(closed);

        Assert.Equal(
            MaintenanceStatus.Created,
            order.Status);
        Assert.Equal(0, context.Repository.UpdateCallCount);
    }

    [Fact]
    public void Close_VerificationFailed_ShouldRemainCompleted()
    {
        TestContext context = CreateContext();

        var order = new MaintenanceOrder
        {
            OrderId = 40,
            DeviceId = 1,
            AssigneeId = 3,
            Description = "待验收工单",
            Result = "维护已完成",
            Status = MaintenanceStatus.Completed,
            CreatedAt = CurrentTime.AddHours(-2),
            CompletedAt = CurrentTime.AddMinutes(-10)
        };

        context.Repository.Seed(order);

        bool closed =
            context.Service.Close(
                order,
                verificationPassed: false);

        Assert.False(closed);
        Assert.Equal(
            MaintenanceStatus.Completed,
            order.Status);
        Assert.Equal(0, context.Repository.UpdateCallCount);
    }

    private static TestContext CreateContext()
    {
        var repository =
            new FakeMaintenanceRepository();

        var clock = new MutableClock
        {
            Now = CurrentTime
        };

        var service =
            new MaintenanceApplicationService(
                repository,
                new MaintenanceRuleService(),
                clock);

        return new TestContext(
            service,
            repository,
            clock);
    }

    private static Alarm CreateAlarm()
    {
        return new Alarm
        {
            AlarmId = 10,
            DeviceId = 1,
            AlarmType = "Temperature",
            Level = AlarmLevel.High,
            Message = "设备温度超过高限",
            Status = AlarmStatus.Active,
            OccurredAt = CurrentTime.AddMinutes(-5),
            LastOccurredAt = CurrentTime.AddMinutes(-1),
            OccurrenceCount = 2
        };
    }

    private sealed record TestContext(
        MaintenanceApplicationService Service,
        FakeMaintenanceRepository Repository,
        MutableClock Clock);

    private sealed class MutableClock : IClock
    {
        public DateTime Now { get; set; }
    }

    private sealed class FakeMaintenanceRepository
        : IMaintenanceRepository
    {
        public List<MaintenanceOrder> Orders { get; } = new();

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public void Seed(MaintenanceOrder order)
        {
            Orders.Add(order);
        }

        public IReadOnlyList<MaintenanceOrder> GetActiveOrders(
            int deviceId)
        {
            return Orders
                .Where(order =>
                    order.DeviceId == deviceId &&
                    order.Status is MaintenanceStatus.Created
                        or MaintenanceStatus.Assigned
                        or MaintenanceStatus.InProgress
                        or MaintenanceStatus.Completed)
                .ToList();
        }

        public void Add(MaintenanceOrder order)
        {
            AddCallCount++;

            if (order.OrderId == 0)
            {
                order.OrderId = 100 + AddCallCount;
            }

            Orders.Add(order);
        }

        public void Update(MaintenanceOrder order)
        {
            int index = Orders.FindIndex(
                item => item.OrderId == order.OrderId);

            if (index < 0)
            {
                throw new InvalidOperationException(
                    "维护工单不存在。");
            }

            UpdateCallCount++;
            Orders[index] = order;
        }
    }
}
