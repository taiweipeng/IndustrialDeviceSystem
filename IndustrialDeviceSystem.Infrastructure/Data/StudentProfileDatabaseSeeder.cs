using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Infrastructure.Data;

public sealed class StudentProfileDatabaseSeeder
{
    public void Apply(
        StudentProfile profile,
        IDeviceRepository deviceRepository,
        IBookingRepository bookingRepository)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(deviceRepository);
        ArgumentNullException.ThrowIfNull(bookingRepository);

        foreach (var profileDevice in profile.Devices)
        {
            var device = deviceRepository.GetByCode(
                profileDevice.DeviceCode);

            if (device is null)
            {
                deviceRepository.Add(
                    new Device
                    {
                        DeviceCode =
                            profileDevice.DeviceCode,

                        DeviceName =
                            profileDevice.DeviceName,

                        DeviceType =
                            profileDevice.DeviceType,

                        Status = DeviceStatus.Idle,
                        Enabled = true
                    });

                continue;
            }

            // 保留设备当前运行状态，只同步教师分配的基础信息
            device.DeviceName =
                profileDevice.DeviceName;

            device.DeviceType =
                profileDevice.DeviceType;

            device.Enabled = true;

            deviceRepository.Update(device);
        }

        SeedExistingBooking(
            profile,
            deviceRepository,
            bookingRepository);
    }

    private static void SeedExistingBooking(
        StudentProfile profile,
        IDeviceRepository deviceRepository,
        IBookingRepository bookingRepository)
    {
        var firstProfileDevice =
            profile.Devices.First();

        var device = deviceRepository.GetByCode(
            firstProfileDevice.DeviceCode)
            ?? throw new InvalidOperationException(
                "个性化设备写入数据库失败。");

        var purpose =
            $"{profile.StudentId}-设备预约实验";

        var alreadyExists =
            bookingRepository
                .GetActiveBookings(device.DeviceId)
                .Any(booking =>
                    booking.StartTime ==
                        profile.ExistingBooking.Start
                    &&
                    booking.EndTime ==
                        profile.ExistingBooking.End
                    &&
                    string.Equals(
                        booking.Purpose,
                        purpose,
                        StringComparison.Ordinal));

        if (alreadyExists)
        {
            return;
        }

        bookingRepository.TryAddIfNoConflict(
            new Booking
            {
                DeviceId = device.DeviceId,
                UserId = 2,
                StartTime =
                    profile.ExistingBooking.Start,
                EndTime =
                    profile.ExistingBooking.End,
                Purpose = purpose,
                Status =
                    BookingStatus.Confirmed,
                CreatedAt =
                    profile.ExistingBooking.Start
                        .AddDays(-1)
            });
    }
}
