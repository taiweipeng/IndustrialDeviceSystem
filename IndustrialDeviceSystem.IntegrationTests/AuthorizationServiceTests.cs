using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Core.Services;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class AuthorizationServiceTests
{
    private readonly AuthorizationService
        _authorizationService = new();

    [Fact]
    public void Administrator_ShouldHaveAllPermissions()
    {
        var session = CreateSession(
            1,
            "admin",
            UserRole.Administrator);

        foreach (var permission in
                 Enum.GetValues<SystemPermission>())
        {
            Assert.True(
                _authorizationService.HasPermission(
                    session,
                    permission),
                $"管理员缺少权限：{permission}");
        }
    }

    [Fact]
    public void Operator_ShouldHaveOnlyExpectedPermissions()
    {
        var expectedPermissions =
            new HashSet<SystemPermission>
            {
                SystemPermission.ViewDashboard,
                SystemPermission.MonitorDevice,
                SystemPermission.ControlSimulation,
                SystemPermission.CreateBooking,
                SystemPermission.ManageOwnBooking,
                SystemPermission.AcknowledgeAlarm,
                SystemPermission.CreateMaintenanceOrder,
                SystemPermission.ViewHistory,
                SystemPermission.ExportHistory,
                SystemPermission.ViewOwnOperationLog
            };

        AssertPermissionMatrix(
            UserRole.Operator,
            expectedPermissions);
    }

    [Fact]
    public void Maintainer_ShouldHaveOnlyExpectedPermissions()
    {
        var expectedPermissions =
            new HashSet<SystemPermission>
            {
                SystemPermission.ViewDashboard,
                SystemPermission.MonitorDevice,
                SystemPermission.AcknowledgeAlarm,
                SystemPermission.ClearAlarm,
                SystemPermission.CreateMaintenanceOrder,
                SystemPermission.ProcessMaintenanceOrder,
                SystemPermission.ViewHistory,
                SystemPermission.ExportHistory,
                SystemPermission.ViewOwnOperationLog
            };

        AssertPermissionMatrix(
            UserRole.Maintainer,
            expectedPermissions);
    }

    [Fact]
    public void Auditor_ShouldHaveOnlyExpectedPermissions()
    {
        var expectedPermissions =
            new HashSet<SystemPermission>
            {
                SystemPermission.ViewDashboard,
                SystemPermission.MonitorDevice,
                SystemPermission.ViewHistory,
                SystemPermission.ExportHistory,
                SystemPermission.ViewAllOperationLogs
            };

        AssertPermissionMatrix(
            UserRole.Auditor,
            expectedPermissions);
    }

    [Fact]
    public void Auditor_ShouldViewAllLogsButNotOwnLogPage()
    {
        var session = CreateSession(
            4,
            "auditor01",
            UserRole.Auditor);

        Assert.True(
            _authorizationService.HasPermission(
                session,
                SystemPermission.ViewAllOperationLogs));

        Assert.False(
            _authorizationService.HasPermission(
                session,
                SystemPermission.ViewOwnOperationLog));
    }

    [Fact]
    public void EnsurePermission_AllowedPermission_ShouldNotThrow()
    {
        var session = CreateSession(
            2,
            "operator01",
            UserRole.Operator);

        var exception = Record.Exception(
            () =>
                _authorizationService
                    .EnsurePermission(
                        session,
                        SystemPermission.CreateBooking));

        Assert.Null(exception);
    }

    [Fact]
    public void EnsurePermission_DeniedPermission_ShouldThrow()
    {
        var session = CreateSession(
            2,
            "operator01",
            UserRole.Operator);

        Assert.Throws<UnauthorizedAccessException>(
            () =>
                _authorizationService
                    .EnsurePermission(
                        session,
                        SystemPermission.ManageAllBookings));
    }

    [Fact]
    public void CanManageBooking_Administrator_ShouldManageAnyBooking()
    {
        var session = CreateSession(
            1,
            "admin",
            UserRole.Administrator);

        var booking = CreateBooking(
            bookingId: 100,
            userId: 2);

        Assert.True(
            _authorizationService
                .CanManageBooking(
                    session,
                    booking));
    }

    [Fact]
    public void CanManageBooking_Operator_ShouldManageOwnBookingOnly()
    {
        var session = CreateSession(
            2,
            "operator01",
            UserRole.Operator);

        var ownBooking = CreateBooking(
            bookingId: 100,
            userId: 2);

        var otherUsersBooking = CreateBooking(
            bookingId: 101,
            userId: 3);

        Assert.True(
            _authorizationService
                .CanManageBooking(
                    session,
                    ownBooking));

        Assert.False(
            _authorizationService
                .CanManageBooking(
                    session,
                    otherUsersBooking));
    }

    [Fact]
    public void CanManageBooking_Auditor_ShouldNotManageBooking()
    {
        var session = CreateSession(
            4,
            "auditor01",
            UserRole.Auditor);

        var booking = CreateBooking(
            bookingId: 100,
            userId: 4);

        Assert.False(
            _authorizationService
                .CanManageBooking(
                    session,
                    booking));
    }

    [Fact]
    public void EnsureCanManageBooking_OtherUsersBooking_ShouldThrow()
    {
        var session = CreateSession(
            2,
            "operator01",
            UserRole.Operator);

        var booking = CreateBooking(
            bookingId: 100,
            userId: 3);

        Assert.Throws<UnauthorizedAccessException>(
            () =>
                _authorizationService
                    .EnsureCanManageBooking(
                        session,
                        booking));
    }

    [Fact]
    public void HasPermission_InvalidPermission_ShouldThrow()
    {
        var session = CreateSession(
            1,
            "admin",
            UserRole.Administrator);

        var invalidPermission =
            (SystemPermission)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                _authorizationService
                    .HasPermission(
                        session,
                        invalidPermission));
    }

    [Fact]
    public void HasPermission_NullSession_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                _authorizationService
                    .HasPermission(
                        null!,
                        SystemPermission.ViewDashboard));
    }

    private void AssertPermissionMatrix(
        UserRole role,
        IReadOnlySet<SystemPermission>
            expectedPermissions)
    {
        var session = CreateSession(
            99,
            $"test-{role}",
            role);

        foreach (var permission in
                 Enum.GetValues<SystemPermission>())
        {
            var expected =
                expectedPermissions.Contains(
                    permission);

            var actual =
                _authorizationService.HasPermission(
                    session,
                    permission);

            Assert.True(
                expected == actual,
                $"角色 {role} 的权限 {permission}：" +
                $"期望 {expected}，实际 {actual}。");
        }
    }

    private static UserSession CreateSession(
        int userId,
        string userName,
        UserRole role)
    {
        return new UserSession(
            userId,
            userName,
            role);
    }

    private static Booking CreateBooking(
        int bookingId,
        int userId)
    {
        return new Booking
        {
            BookingId = bookingId,
            DeviceId = 1,
            UserId = userId,
            StartTime = new DateTime(
                2026,
                10,
                8,
                9,
                0,
                0),
            EndTime = new DateTime(
                2026,
                10,
                8,
                10,
                0,
                0),
            Purpose = "权限测试",
            CreatedAt = new DateTime(
                2026,
                10,
                8,
                8,
                0,
                0)
        };
    }
}
