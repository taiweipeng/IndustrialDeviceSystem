namespace IndustrialDeviceSystem.IntegrationTests;

public sealed class OpenPlcArtifactContractTests
{
    [Fact]
    public void OpenPlcRequiredFiles_Exist()
    {
        string root = FindRepositoryRoot();

        string programPath = Path.Combine(
            root,
            "openplc",
            "IndustrialDevice.st");

        string mapPath = Path.Combine(
            root,
            "openplc",
            "register-map.md");

        Assert.True(
            File.Exists(programPath),
            $"缺少OpenPLC程序文件：{programPath}");

        Assert.True(
            File.Exists(mapPath),
            $"缺少寄存器说明文件：{mapPath}");
    }

    [Fact]
    public void IndustrialDeviceProgram_ContainsFixedAddressesAndScenarios()
    {
        string root = FindRepositoryRoot();

        string programPath = Path.Combine(
            root,
            "openplc",
            "IndustrialDevice.st");

        string content = File.ReadAllText(programPath);

        string[] requiredTokens =
        [
            "TemperatureRaw AT %QW0",
            "PressureRaw AT %QW1",
            "VibrationRaw AT %QW2",
            "Scenario AT %QW10",
            "IsRunning AT %QX0.0",
            "FaultSignal AT %QX0.1",
            "EmergencyStop AT %QX0.2",
            "CASE Scenario OF",
            "UINT#0:",
            "UINT#1:",
            "UINT#2:",
            "UINT#3:"
        ];

        foreach (string token in requiredTokens)
        {
            Assert.Contains(token, content);
        }

        Assert.DoesNotContain("%MW0", content);
    }

    [Fact]
    public void RegisterMap_ContainsConnectionAndConversionContract()
    {
        string root = FindRepositoryRoot();

        string mapPath = Path.Combine(
            root,
            "openplc",
            "register-map.md");

        string content = File.ReadAllText(mapPath);

        string[] requiredTokens =
        [
            "127.0.0.1",
            "5020",
            "%QW0",
            "%QW1",
            "%QW2",
            "%QW10",
            "%QX0.0",
            "%QX0.1",
            "%QX0.2",
            "raw × 0.1",
            "raw × 0.01",
            "ReadHoldingRegisters(1, 0, 3)",
            "ReadCoils(1, 0, 3)",
            "CommunicationLost"
        ];

        foreach (string token in requiredTokens)
        {
            Assert.Contains(token, content);
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory =
            new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string solutionPath = Path.Combine(
                directory.FullName,
                "IndustrialDeviceSystem.sln");

            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "没有找到IndustrialDeviceSystem.sln所在目录。");
    }
}
