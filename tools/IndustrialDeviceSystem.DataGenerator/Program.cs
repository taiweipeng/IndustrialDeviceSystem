using IndustrialDeviceSystem.DataGenerator;
using IndustrialDeviceSystem.Infrastructure.Data;

if (args.Length == 2)
{
    return RunSingle(args[0], args[1]);
}

if (args.Length == 3 &&
    args[0].Equals(
        "batch",
        StringComparison.OrdinalIgnoreCase))
{
    return RunBatch(args[1], args[2]);
}

PrintUsage();
return 1;

static int RunSingle(
    string profileArgument,
    string outputArgument)
{
    try
    {
        string profilePath =
            Path.GetFullPath(profileArgument);

        string outputDirectory =
            Path.GetFullPath(outputArgument);

        var loader = new StudentProfileLoader();
        var generator = new StudentDataGenerator();

        var profile = loader.Load(profilePath);

        GeneratedDataSummary summary =
            generator.Generate(
                profile,
                outputDirectory);

        Console.WriteLine("个性化测试数据生成成功。");
        Console.WriteLine($"学生：{profile.StudentId}");
        Console.WriteLine($"设备：{summary.DeviceCount}");
        Console.WriteLine($"预约：{summary.BookingCount}");
        Console.WriteLine($"遥测：{summary.TelemetryCount}");
        Console.WriteLine($"输出：{outputDirectory}");

        return 0;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine(
            $"生成失败：{exception.Message}");

        return 1;
    }
}

static int RunBatch(
    string rosterArgument,
    string outputArgument)
{
    try
    {
        string rosterPath =
            Path.GetFullPath(rosterArgument);

        string outputRoot =
            Path.GetFullPath(outputArgument);

        var generator =
            new BatchStudentPackageGenerator();

        BatchGenerationResult result =
            generator.Generate(
                rosterPath,
                outputRoot);

        Console.WriteLine("全班个性化数据包生成成功。");
        Console.WriteLine(
            $"学生人数：{result.StudentCount}");

        Console.WriteLine(
            $"教师总表：{result.ManifestPath}");

        Console.WriteLine(
            $"输出目录：{outputRoot}");

        Console.WriteLine(
            $"ZIP数据包：{result.PackageCount}");

        return 0;
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine(
            $"批量生成失败：{exception.Message}");

        return 1;
    }
}

static void PrintUsage()
{
    Console.WriteLine("单个学生：");
    Console.WriteLine(
        "  DataGenerator <profile.json> <data-directory>");

    Console.WriteLine("全班批量：");
    Console.WriteLine(
        "  DataGenerator batch <roster.csv> <output-root>");

}
