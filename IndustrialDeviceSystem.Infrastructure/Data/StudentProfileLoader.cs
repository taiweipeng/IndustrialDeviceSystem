using System.Text;
using System.Text.Json;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Infrastructure.Data;

public sealed class StudentProfileLoader
{
    private static readonly UTF8Encoding StrictUtf8 =
        new(encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public StudentProfile Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "配置文件路径不能为空。",
                nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "未找到学生个性化配置文件。",
                filePath);
        }

        try
        {
            string json = File.ReadAllText(filePath, StrictUtf8);

            StudentProfile? profile =
                JsonSerializer.Deserialize<StudentProfile>(
                    json,
                    JsonOptions);

            if (profile is null)
            {
                throw new InvalidDataException("配置文件内容为空。");
            }

            Validate(profile);
            return profile;
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException(
                "配置文件不是有效的UTF-8编码。",
                exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "配置文件JSON格式错误。",
                exception);
        }
    }

    private static void Validate(StudentProfile profile)
    {
        if (string.IsNullOrWhiteSpace(profile.StudentId))
        {
            throw new InvalidDataException("studentId不能为空。");
        }

        if (string.IsNullOrWhiteSpace(profile.GithubUser))
        {
            throw new InvalidDataException("githubUser不能为空。");
        }

        if (profile.SampleIntervalSeconds <= 0)
        {
            throw new InvalidDataException(
                "sampleIntervalSeconds必须大于0。");
        }

        if (profile.MaximumCommunicationFailures <= 0)
        {
            throw new InvalidDataException(
                "maximumCommunicationFailures必须大于0。");
        }

        TemperatureThresholds thresholds =
            profile.TemperatureThresholds;

        if (!(thresholds.Warning < thresholds.High &&
              thresholds.High < thresholds.Emergency))
        {
            throw new InvalidDataException(
                "温度阈值必须满足 warning < high < emergency。");
        }

        if (profile.Devices.Count == 0)
        {
            throw new InvalidDataException(
                "至少需要配置一台设备。");
        }

        if (profile.Devices.Any(device =>
                string.IsNullOrWhiteSpace(device.DeviceCode) ||
                string.IsNullOrWhiteSpace(device.DeviceName) ||
                string.IsNullOrWhiteSpace(device.DeviceType)))
        {
            throw new InvalidDataException(
                "设备编码、名称和类型均不能为空。");
        }

        bool hasDuplicateDeviceCode = profile.Devices
            .GroupBy(
                device => device.DeviceCode,
                StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (hasDuplicateDeviceCode)
        {
            throw new InvalidDataException(
                "设备编码不能重复。");
        }

        if (profile.ExistingBooking.Start >=
            profile.ExistingBooking.End)
        {
            throw new InvalidDataException(
                "已有预约的开始时间必须早于结束时间。");
        }

        if (profile.FaultSequence.Count == 0)
        {
            throw new InvalidDataException(
                "faultSequence不能为空。");
        }
    }
}
