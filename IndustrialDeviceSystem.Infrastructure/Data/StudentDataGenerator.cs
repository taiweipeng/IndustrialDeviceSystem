using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Infrastructure.Data;

public sealed class StudentDataGenerator
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GeneratedDataSummary Generate(
        StudentProfile profile,
        string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException(
                "输出目录不能为空。",
                nameof(outputDirectory));
        }

        Directory.CreateDirectory(outputDirectory);

        int seed = CreateStableSeed(profile.StudentId);
        int userId = 1000 + seed % 900000;

        List<Device> devices = CreateDevices(profile);
        List<Booking> bookings =
            CreateBookings(profile, devices, userId);

        List<TelemetryRecord> telemetry =
            CreateTelemetry(profile, devices, seed);

        WriteJson(
            Path.Combine(outputDirectory, "devices.json"),
            devices);

        WriteJson(
            Path.Combine(outputDirectory, "bookings.json"),
            bookings);

        WriteTelemetryCsv(
            Path.Combine(outputDirectory, "telemetry.csv"),
            telemetry);

        return new GeneratedDataSummary(
            devices.Count,
            bookings.Count,
            telemetry.Count);
    }

    private static List<Device> CreateDevices(
        StudentProfile profile)
    {
        DateTime lastCommunicationTime =
            profile.ExistingBooking.Start.AddSeconds(
                -profile.SampleIntervalSeconds);

        return profile.Devices
            .Select((item, index) => new Device
            {
                DeviceId = index + 1,
                DeviceCode = item.DeviceCode,
                DeviceName = item.DeviceName,
                DeviceType = item.DeviceType,
                Status = DeviceStatus.Running,
                Enabled = true,
                LastCommunicationTime = lastCommunicationTime
            })
            .ToList();
    }

    private static List<Booking> CreateBookings(
        StudentProfile profile,
        IReadOnlyList<Device> devices,
        int userId)
    {
        return new List<Booking>
        {
            new()
            {
                BookingId = 1,
                DeviceId = devices[0].DeviceId,
                UserId = userId,
                StartTime = profile.ExistingBooking.Start,
                EndTime = profile.ExistingBooking.End,
                Purpose =
                    $"{profile.StudentId}-设备预约实验",
                Status = BookingStatus.Confirmed,
                CreatedAt =
                    profile.ExistingBooking.Start.AddDays(-1)
            }
        };
    }

    private static List<TelemetryRecord> CreateTelemetry(
        StudentProfile profile,
        IReadOnlyList<Device> devices,
        int seed)
    {
        var records = new List<TelemetryRecord>();
        var random = new Random(seed);

        int samplesPerDevice = Math.Max(
            12,
            profile.FaultSequence.Count * 3);

        double warning =
            profile.TemperatureThresholds.Warning;
        double high =
            profile.TemperatureThresholds.High;
        double emergency =
            profile.TemperatureThresholds.Emergency;

        double[] temperatureCycle =
        {
            warning - 5.0,
            warning,
            (warning + high) / 2.0,
            high,
            (high + emergency) / 2.0,
            emergency
        };

        DateTime firstTime =
            profile.ExistingBooking.Start.AddSeconds(
                -samplesPerDevice *
                profile.SampleIntervalSeconds);

        int telemetryId = 1;

        foreach (Device device in devices)
        {
            for (int index = 0;
                 index < samplesPerDevice;
                 index++)
            {
                bool emergencyStop =
                    index == samplesPerDevice - 1;

                bool faultSignal =
                    !emergencyStop &&
                    profile.FaultSequence[
                        index % profile.FaultSequence.Count];

                records.Add(new TelemetryRecord
                {
                    TelemetryId = telemetryId++,
                    DeviceId = device.DeviceId,
                    CollectedAt = firstTime.AddSeconds(
                        index *
                        profile.SampleIntervalSeconds),

                    Temperature = temperatureCycle[
                        index % temperatureCycle.Length],

                    Pressure = Math.Round(
                        1.0 + random.Next(-5, 6) * 0.01,
                        2),

                    Vibration = Math.Round(
                        0.20 + random.Next(0, 6) * 0.01,
                        2),

                    IsRunning =
                        !faultSignal && !emergencyStop,

                    FaultSignal = faultSignal,
                    EmergencyStop = emergencyStop,
                    Quality = DataQuality.Good
                });
            }
        }

        return records;
    }

    private static void WriteJson<T>(
        string filePath,
        T value)
    {
        string json = JsonSerializer.Serialize(
            value,
            JsonOptions);

        File.WriteAllText(
            filePath,
            json,
            Utf8WithoutBom);
    }

    private static void WriteTelemetryCsv(
        string filePath,
        IEnumerable<TelemetryRecord> records)
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            "TelemetryId,DeviceId,CollectedAt," +
            "Temperature,Pressure,Vibration," +
            "IsRunning,FaultSignal,EmergencyStop,Quality");

        foreach (TelemetryRecord record in records)
        {
            builder
                .Append(record.TelemetryId)
                .Append(',')
                .Append(record.DeviceId)
                .Append(',')
                .Append(record.CollectedAt.ToString(
                    "O",
                    CultureInfo.InvariantCulture))
                .Append(',')
                .Append(record.Temperature.ToString(
                    "F2",
                    CultureInfo.InvariantCulture))
                .Append(',')
                .Append(record.Pressure.ToString(
                    "F2",
                    CultureInfo.InvariantCulture))
                .Append(',')
                .Append(record.Vibration.ToString(
                    "F2",
                    CultureInfo.InvariantCulture))
                .Append(',')
                .Append(record.IsRunning ? "1" : "0")
                .Append(',')
                .Append(record.FaultSignal ? "1" : "0")
                .Append(',')
                .Append(record.EmergencyStop ? "1" : "0")
                .Append(',')
                .Append((int)record.Quality)
                .AppendLine();
        }

        File.WriteAllText(
            filePath,
            builder.ToString(),
            Utf8WithoutBom);
    }

    private static int CreateStableSeed(string studentId)
    {
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(studentId));

        return BinaryPrimitives.ReadInt32LittleEndian(
                   hash.AsSpan(0, 4))
               & int.MaxValue;
    }
}

public sealed record GeneratedDataSummary(
    int DeviceCount,
    int BookingCount,
    int TelemetryCount);
