using System.Text;
using IndustrialDeviceSystem.Infrastructure.Data;

namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class StudentProfileLoaderTests
{
    [Fact]
    public void Load_ValidProfile_ReturnsConfiguration()
    {
        string path = WriteTemporaryProfile(ValidJson);

        try
        {
            var loader = new StudentProfileLoader();

            var profile = loader.Load(path);

            Assert.Equal("20260001", profile.StudentId);
            Assert.Equal("student-a", profile.GithubUser);
            Assert.Equal(2, profile.SampleIntervalSeconds);
            Assert.Equal(3, profile.MaximumCommunicationFailures);

            Assert.Equal(
                60.0,
                profile.TemperatureThresholds.Warning);

            Assert.Single(profile.Devices);
            Assert.Equal(
                "DEV-001-A",
                profile.Devices[0].DeviceCode);

            Assert.Equal(3, profile.FaultSequence.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_InvalidThresholdOrder_ThrowsInvalidDataException()
    {
        string invalidJson = ValidJson.Replace(
            "\"warning\": 60.0",
            "\"warning\": 85.0");

        string path = WriteTemporaryProfile(invalidJson);

        try
        {
            var loader = new StudentProfileLoader();

            Assert.Throws<InvalidDataException>(
                () => loader.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_ThrowsFileNotFoundException()
    {
        var loader = new StudentProfileLoader();

        string missingPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.json");

        Assert.Throws<FileNotFoundException>(
            () => loader.Load(missingPath));
    }

    private static string WriteTemporaryProfile(string json)
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"student-profile-{Guid.NewGuid():N}.json");

        File.WriteAllText(
            path,
            json,
            new UTF8Encoding(false));

        return path;
    }

    private const string ValidJson = """
    {
      "studentId": "20260001",
      "githubUser": "student-a",
      "sampleIntervalSeconds": 2,
      "maximumCommunicationFailures": 3,
      "temperatureThresholds": {
        "warning": 60.0,
        "high": 80.0,
        "emergency": 90.0
      },
      "devices": [
        {
          "deviceCode": "DEV-001-A",
          "deviceName": "数控加工设备",
          "deviceType": "TrainingDevice"
        }
      ],
      "existingBooking": {
        "start": "2026-10-10T09:00:00",
        "end": "2026-10-10T10:00:00"
      },
      "faultSequence": [false, false, true]
    }
    """;
}
