using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IndustrialDeviceSystem.Core.Models;
using IndustrialDeviceSystem.Infrastructure.Data;

namespace IndustrialDeviceSystem.DataGenerator;

internal sealed record StudentRosterEntry(
    string StudentId,
    string GithubUser);

public sealed record BatchGenerationResult(
      int StudentCount,
      int PackageCount,
      string ManifestPath);

public sealed class BatchStudentPackageGenerator
{
    private static readonly UTF8Encoding Utf8WithoutBom =
        new(false);

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    private static readonly
        (string DeviceName, string DeviceType)[] DeviceCatalog =
        {
            ("数控加工设备", "TrainingDevice"),
            ("工业检测设备", "InspectionDevice"),
            ("液压试验设备", "HydraulicDevice"),
            ("环境测试设备", "EnvironmentDevice")
        };

    public BatchGenerationResult Generate(
        string rosterPath,
        string outputRoot)
    {
        List<StudentRosterEntry> roster =
            ReadRoster(rosterPath);

        Directory.CreateDirectory(outputRoot);
        string packagesDirectory = Path.Combine(
            outputRoot,
            "packages");

        Directory.CreateDirectory(packagesDirectory);

        var dataGenerator = new StudentDataGenerator();
        var manifest = new StringBuilder();

        manifest.AppendLine(
            "StudentId,GitHubUser,DeviceCode," +
            "SampleIntervalSeconds," +
            "MaximumCommunicationFailures," +
            "Warning,High,Emergency," +
            "ProfileSha256,PackageSha256");

        foreach (StudentRosterEntry entry in roster)
        {
            StudentProfile profile = CreateProfile(entry);

            string studentRoot = Path.Combine(
                outputRoot,
                entry.StudentId);

            string configDirectory = Path.Combine(
                studentRoot,
                "config");

            string dataDirectory = Path.Combine(
                studentRoot,
                "data");

            Directory.CreateDirectory(configDirectory);
            Directory.CreateDirectory(dataDirectory);

            string profilePath = Path.Combine(
                configDirectory,
                "student-profile.json");

            WriteProfile(profilePath, profile);

            dataGenerator.Generate(
                profile,
                dataDirectory);

            string packagePath = Path.Combine(
                packagesDirectory,
                $"{entry.StudentId}.zip");

            CreateDeterministicPackage(
                studentRoot,
                packagePath);

            string profileHash =
                CalculateSha256(profilePath);

            string packageHash =
                CalculateSha256(packagePath);

            StudentDeviceProfile device =
                profile.Devices[0];

            manifest
                .Append(profile.StudentId).Append(',')
                .Append(profile.GithubUser).Append(',')
                .Append(device.DeviceCode).Append(',')
                .Append(profile.SampleIntervalSeconds).Append(',')
                .Append(profile.MaximumCommunicationFailures).Append(',')
                .Append(profile.TemperatureThresholds.Warning).Append(',')
                .Append(profile.TemperatureThresholds.High).Append(',')
                .Append(profile.TemperatureThresholds.Emergency).Append(',')
                .Append(profileHash).Append(',')
                .Append(packageHash)
                .AppendLine();
        }

        string manifestPath = Path.Combine(
            outputRoot,
            "teacher-manifest.csv");

        File.WriteAllText(
            manifestPath,
            manifest.ToString(),
            Utf8WithoutBom);

        return new BatchGenerationResult(
            roster.Count,
            roster.Count,
            manifestPath);
    }

    private static void CreateDeterministicPackage(
    string studentRoot,
    string packagePath)
    {
        DateTimeOffset fixedTimestamp =
            new(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        using var fileStream = new FileStream(
            packagePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        using var archive = new ZipArchive(
            fileStream,
            ZipArchiveMode.Create);

        foreach (string relativePath in PackageFiles
                     .OrderBy(path => path,
                         StringComparer.Ordinal))
        {
            string localPath = Path.Combine(
                studentRoot,
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

            if (!File.Exists(localPath))
            {
                throw new FileNotFoundException(
                    "数据包缺少必交文件。",
                    localPath);
            }

            ZipArchiveEntry entry = archive.CreateEntry(
                relativePath,
                CompressionLevel.Optimal);

            entry.LastWriteTime = fixedTimestamp;

            using Stream input =
                File.OpenRead(localPath);

            using Stream output =
                entry.Open();

            input.CopyTo(output);
        }
    }

    private static List<StudentRosterEntry> ReadRoster(
        string rosterPath)
    {
        if (!File.Exists(rosterPath))
        {
            throw new FileNotFoundException(
                "未找到学生名单文件。",
                rosterPath);
        }

        string[] lines = File.ReadAllLines(
            rosterPath,
            Utf8WithoutBom);

        if (lines.Length < 2)
        {
            throw new InvalidDataException(
                "学生名单中没有学生记录。");
        }

        string header = string.Join(
            ",",
            lines[0]
                .Split(',')
                .Select(value =>
                    value.Trim().ToLowerInvariant()));

        if (header != "studentid,githubuser")
        {
            throw new InvalidDataException(
                "名单表头必须为studentId,githubUser。");
        }

        var entries = new List<StudentRosterEntry>();

        for (int index = 1; index < lines.Length; index++)
        {
            string line = lines[index].Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] fields = line.Split(
                ',',
                StringSplitOptions.TrimEntries);

            if (fields.Length != 2)
            {
                throw new InvalidDataException(
                    $"名单第{index + 1}行必须包含两列。");
            }

            ValidateIdentifier(
                fields[0],
                "studentId",
                index + 1);

            ValidateIdentifier(
                fields[1],
                "githubUser",
                index + 1);

            entries.Add(new StudentRosterEntry(
                fields[0],
                fields[1]));
        }

        if (entries.Count == 0)
        {
            throw new InvalidDataException(
                "学生名单中没有有效记录。");
        }

        bool duplicateStudentId = entries
            .GroupBy(
                entry => entry.StudentId,
                StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (duplicateStudentId)
        {
            throw new InvalidDataException(
                "学生名单中存在重复studentId。");
        }

        bool duplicateGithubUser = entries
            .GroupBy(
                entry => entry.GithubUser,
                StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1);

        if (duplicateGithubUser)
        {
            throw new InvalidDataException(
                "学生名单中存在重复githubUser。");
        }

        return entries;
    }

    private static StudentProfile CreateProfile(
        StudentRosterEntry entry)
    {
        uint seed = CreateStableSeed(entry.StudentId);

        int sampleIntervalSeconds =
            1 + (int)(seed % 3);

        int maximumCommunicationFailures =
            2 + (int)((seed / 3) % 3);

        double warning =
            55.0 + seed % 6;

        double high = warning + 20.0;
        double emergency = high + 10.0;

        int catalogIndex =
            (int)((seed / 7) %
                  (uint)DeviceCatalog.Length);

        var deviceDefinition =
            DeviceCatalog[catalogIndex];

        string deviceToken = new(
            entry.StudentId
                .Where(char.IsLetterOrDigit)
                .Select(char.ToUpperInvariant)
                .ToArray());

        if (deviceToken.Length > 6)
        {
            deviceToken = deviceToken[^6..];
        }

        string deviceCode =
            $"DEV-{deviceToken}-{seed % 1000:D3}";

        int bookingSlot =
            (int)((seed / 11) % 18);

        DateTime bookingStart = new DateTime(
                2026,
                10,
                10,
                8,
                0,
                0,
                DateTimeKind.Unspecified)
            .AddMinutes(bookingSlot * 30);

        var faultSequence = new List<bool>
        {
            false,
            false,
            false
        };

        faultSequence[(int)((seed / 17) % 3)] = true;

        return new StudentProfile
        {
            StudentId = entry.StudentId,
            GithubUser = entry.GithubUser,

            SampleIntervalSeconds =
                sampleIntervalSeconds,

            MaximumCommunicationFailures =
                maximumCommunicationFailures,

            TemperatureThresholds =
                new TemperatureThresholds
                {
                    Warning = warning,
                    High = high,
                    Emergency = emergency
                },

            Devices = new List<StudentDeviceProfile>
            {
                new()
                {
                    DeviceCode = deviceCode,
                    DeviceName =
                        deviceDefinition.DeviceName,
                    DeviceType =
                        deviceDefinition.DeviceType
                }
            },

            ExistingBooking =
                new ExistingBookingProfile
                {
                    Start = bookingStart,
                    End = bookingStart.AddHours(1)
                },

            FaultSequence = faultSequence
        };
    }

    private static void WriteProfile(
        string profilePath,
        StudentProfile profile)
    {
        string json = JsonSerializer.Serialize(
            profile,
            JsonOptions);

        File.WriteAllText(
            profilePath,
            json,
            Utf8WithoutBom);
    }

    private static uint CreateStableSeed(
        string studentId)
    {
        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(studentId));

        return BinaryPrimitives.ReadUInt32LittleEndian(
            hash.AsSpan(0, 4));
    }

    private static string CalculateSha256(
        string filePath)
    {
        byte[] hash = SHA256.HashData(
            File.ReadAllBytes(filePath));

        return Convert
            .ToHexString(hash)
            .ToLowerInvariant();
    }

    private static void ValidateIdentifier(
        string value,
        string fieldName,
        int lineNumber)
    {
        bool valid =
            !string.IsNullOrWhiteSpace(value) &&
            value.All(character =>
                char.IsLetterOrDigit(character) ||
                character is '-' or '_');

        if (!valid)
        {
            throw new InvalidDataException(
                $"名单第{lineNumber}行的" +
                $"{fieldName}包含非法字符。");
        }
    }

    private static readonly string[] PackageFiles ={
        "config/student-profile.json",
        "data/bookings.json",
        "data/devices.json",
        "data/telemetry.csv"
    };
}
