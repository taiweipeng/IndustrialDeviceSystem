using System.Text.Json;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class StudentDataGeneratorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Generate_ValidProfile_CreatesRequiredFiles()
    {
        string outputDirectory = CreateTemporaryDirectory();

        try
        {
            StudentProfile profile = CreateProfile();
            var generator = new StudentDataGenerator();

            GeneratedDataSummary summary =
                generator.Generate(profile, outputDirectory);

            Assert.Equal(1, summary.DeviceCount);
            Assert.Equal(1, summary.BookingCount);
            Assert.Equal(12, summary.TelemetryCount);

            string devicesPath = Path.Combine(
                outputDirectory,
                "devices.json");

            string bookingsPath = Path.Combine(
                outputDirectory,
                "bookings.json");

            string telemetryPath = Path.Combine(
                outputDirectory,
                "telemetry.csv");

            Assert.True(File.Exists(devicesPath));
            Assert.True(File.Exists(bookingsPath));
            Assert.True(File.Exists(telemetryPath));

            List<Device>? devices =
                JsonSerializer.Deserialize<List<Device>>(
                    File.ReadAllText(devicesPath),
                    JsonOptions);

            List<Booking>? bookings =
                JsonSerializer.Deserialize<List<Booking>>(
                    File.ReadAllText(bookingsPath),
                    JsonOptions);

            Assert.NotNull(devices);
            Assert.NotNull(bookings);

            Device device = Assert.Single(devices);
            Booking booking = Assert.Single(bookings);

            Assert.Equal("DEV-001-A", device.DeviceCode);
            Assert.Equal(device.DeviceId, booking.DeviceId);

            string[] csvLines =
                File.ReadAllLines(telemetryPath);

            Assert.Equal(13, csvLines.Length);

            Assert.Equal(
                "TelemetryId,DeviceId,CollectedAt," +
                "Temperature,Pressure,Vibration," +
                "IsRunning,FaultSignal,EmergencyStop,Quality",
                csvLines[0]);
        }
        finally
        {
            DeleteTemporaryDirectory(outputDirectory);
        }
    }

    [Fact]
    public void Generate_SameProfileTwice_ProducesIdenticalFiles()
    {
        string firstDirectory = CreateTemporaryDirectory();
        string secondDirectory = CreateTemporaryDirectory();

        try
        {
            StudentProfile profile = CreateProfile();
            var generator = new StudentDataGenerator();

            generator.Generate(profile, firstDirectory);
            generator.Generate(profile, secondDirectory);

            string[] fileNames =
            {
                "devices.json",
                "bookings.json",
                "telemetry.csv"
            };

            foreach (string fileName in fileNames)
            {
                string firstContent = File.ReadAllText(
                    Path.Combine(firstDirectory, fileName));

                string secondContent = File.ReadAllText(
                    Path.Combine(secondDirectory, fileName));

                Assert.Equal(firstContent, secondContent);
            }
        }
        finally
        {
            DeleteTemporaryDirectory(firstDirectory);
            DeleteTemporaryDirectory(secondDirectory);
        }
    }

    [Fact]
    public void Generate_TwoDevices_CreatesTelemetryForEachDevice()
    {
        string outputDirectory = CreateTemporaryDirectory();

        try
        {
            StudentProfile profile = CreateProfile();

            profile.Devices.Add(new StudentDeviceProfile
            {
                DeviceCode = "DEV-002-A",
                DeviceName = "工业检测设备",
                DeviceType = "InspectionDevice"
            });

            var generator = new StudentDataGenerator();

            GeneratedDataSummary summary =
                generator.Generate(profile, outputDirectory);

            Assert.Equal(2, summary.DeviceCount);
            Assert.Equal(1, summary.BookingCount);
            Assert.Equal(24, summary.TelemetryCount);

            string telemetryPath = Path.Combine(
                outputDirectory,
                "telemetry.csv");

            string[] dataLines = File
                .ReadAllLines(telemetryPath)
                .Skip(1)
                .ToArray();

            Assert.Equal(24, dataLines.Length);

            int firstDeviceCount = dataLines.Count(
                line => line.Split(',')[1] == "1");

            int secondDeviceCount = dataLines.Count(
                line => line.Split(',')[1] == "2");

            Assert.Equal(12, firstDeviceCount);
            Assert.Equal(12, secondDeviceCount);
        }
        finally
        {
            DeleteTemporaryDirectory(outputDirectory);
        }
    }

    private static StudentProfile CreateProfile()
    {
        return new StudentProfile
        {
            StudentId = "20260001",
            GithubUser = "student-a",
            SampleIntervalSeconds = 2,
            MaximumCommunicationFailures = 3,

            TemperatureThresholds = new TemperatureThresholds
            {
                Warning = 60.0,
                High = 80.0,
                Emergency = 90.0
            },

            Devices = new List<StudentDeviceProfile>
            {
                new()
                {
                    DeviceCode = "DEV-001-A",
                    DeviceName = "数控加工设备",
                    DeviceType = "TrainingDevice"
                }
            },

            ExistingBooking = new ExistingBookingProfile
            {
                Start = new DateTime(
                    2026, 10, 10, 9, 0, 0,
                    DateTimeKind.Local),

                End = new DateTime(
                    2026, 10, 10, 10, 0, 0,
                    DateTimeKind.Local)
            },

            FaultSequence = new List<bool>
            {
                false,
                false,
                true
            }
        };
    }

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"industrial-device-data-{Guid.NewGuid():N}");

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteTemporaryDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}
