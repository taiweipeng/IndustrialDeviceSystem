using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Repositories;
using Xunit;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class DeviceRepositoryTests
{
    [Fact]
    public void GetAll_ShouldReturnSeedDevices()
    {
        using var database =
            new TemporaryDatabase(
                includeSeed: true);

        var repository =
            new DeviceRepository(
                database.ConnectionFactory);

        IReadOnlyList<Device> devices =
            repository.GetAll();

        Assert.Equal(3, devices.Count);

        Assert.Contains(
            devices,
            device =>
                device.DeviceCode ==
                "DEV-001");
    }

    [Fact]
    public void AddAndGetByCode_ShouldRoundTrip()
    {
        using var database =
            new TemporaryDatabase(
                includeSeed: false);

        var repository =
            new DeviceRepository(
                database.ConnectionFactory);

        var device = new Device
        {
            DeviceCode = "TEST-001",
            DeviceName = "自动化测试设备",
            DeviceType = "TestDevice",
            Status = DeviceStatus.Idle,
            Enabled = true
        };

        repository.Add(device);

        Device? saved =
            repository.GetByCode(
                "TEST-001");

        Assert.True(device.DeviceId > 0);
        Assert.NotNull(saved);
        Assert.Equal(
            "自动化测试设备",
            saved.DeviceName);
        Assert.Equal(
            DeviceStatus.Idle,
            saved.Status);
        Assert.True(saved.Enabled);
    }

    [Fact]
    public void Update_ShouldPersistChanges()
    {
        using var database =
            new TemporaryDatabase(
                includeSeed: false);

        var repository =
            new DeviceRepository(
                database.ConnectionFactory);

        var device = new Device
        {
            DeviceCode = "TEST-002",
            DeviceName = "修改前名称",
            DeviceType = "TestDevice",
            Status = DeviceStatus.Idle,
            Enabled = true
        };

        repository.Add(device);

        DateTime communicationTime =
            new(
                2026,
                10,
                10,
                8,
                30,
                0,
                DateTimeKind.Utc);

        device.DeviceName = "修改后名称";
        device.Status = DeviceStatus.Running;
        device.LastCommunicationTime =
            communicationTime;

        repository.Update(device);

        Device? updated =
            repository.GetById(
                device.DeviceId);

        Assert.NotNull(updated);
        Assert.Equal(
            "修改后名称",
            updated.DeviceName);
        Assert.Equal(
            DeviceStatus.Running,
            updated.Status);
        Assert.Equal(
            communicationTime,
            updated.LastCommunicationTime);
    }
}
