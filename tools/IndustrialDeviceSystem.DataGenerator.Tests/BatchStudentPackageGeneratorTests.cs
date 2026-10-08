using System.IO.Compression;
using System.Text;

namespace IndustrialDeviceSystem.DataGenerator.Tests;

public sealed class BatchStudentPackageGeneratorTests
{
    [Fact]
    public void Generate_ValidRoster_CreatesPackagesAndManifest()
    {
        string root = CreateTemporaryDirectory();

        try
        {
            string rosterPath = WriteRoster(
                root,
                """
                studentId,githubUser
                20260001,student-a
                20260002,student-b
                """);

            string outputRoot =
                Path.Combine(root, "output");

            var generator =
                new BatchStudentPackageGenerator();

            BatchGenerationResult result =
                generator.Generate(
                    rosterPath,
                    outputRoot);

            Assert.Equal(2, result.StudentCount);
            Assert.Equal(2, result.PackageCount);
            Assert.True(File.Exists(result.ManifestPath));

            string firstZip = Path.Combine(
                outputRoot,
                "packages",
                "20260001.zip");

            Assert.True(File.Exists(firstZip));

            using ZipArchive archive =
                ZipFile.OpenRead(firstZip);

            string[] actualEntries = archive.Entries
                .Select(entry => entry.FullName)
                .OrderBy(name => name)
                .ToArray();

            string[] expectedEntries =
            {
                "config/student-profile.json",
                "data/bookings.json",
                "data/devices.json",
                "data/telemetry.csv"
            };

            Assert.Equal(
                expectedEntries.OrderBy(name => name),
                actualEntries);

            string[] manifestLines =
                File.ReadAllLines(result.ManifestPath);

            Assert.Equal(3, manifestLines.Length);
            Assert.Contains(
                "ProfileSha256,PackageSha256",
                manifestLines[0]);
        }
        finally
        {
            DeleteTemporaryDirectory(root);
        }
    }

    [Fact]
    public void Generate_SameRosterTwice_ProducesIdenticalZip()
    {
        string root = CreateTemporaryDirectory();

        try
        {
            string rosterPath = WriteRoster(
                root,
                """
                studentId,githubUser
                20260001,student-a
                """);

            string firstOutput =
                Path.Combine(root, "first");

            string secondOutput =
                Path.Combine(root, "second");

            var generator =
                new BatchStudentPackageGenerator();

            generator.Generate(rosterPath, firstOutput);
            generator.Generate(rosterPath, secondOutput);

            byte[] firstBytes = File.ReadAllBytes(
                Path.Combine(
                    firstOutput,
                    "packages",
                    "20260001.zip"));

            byte[] secondBytes = File.ReadAllBytes(
                Path.Combine(
                    secondOutput,
                    "packages",
                    "20260001.zip"));

            Assert.True(
                firstBytes.SequenceEqual(secondBytes));
        }
        finally
        {
            DeleteTemporaryDirectory(root);
        }
    }

    [Fact]
    public void Generate_DuplicateStudentId_ThrowsException()
    {
        string root = CreateTemporaryDirectory();

        try
        {
            string rosterPath = WriteRoster(
                root,
                """
                studentId,githubUser
                20260001,student-a
                20260001,student-b
                """);

            var generator =
                new BatchStudentPackageGenerator();

            Assert.Throws<InvalidDataException>(
                () => generator.Generate(
                    rosterPath,
                    Path.Combine(root, "output")));
        }
        finally
        {
            DeleteTemporaryDirectory(root);
        }
    }

    private static string WriteRoster(
        string root,
        string content)
    {
        string path = Path.Combine(
            root,
            "student-roster.csv");

        File.WriteAllText(
            path,
            content,
            new UTF8Encoding(false));

        return path;
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(
            Path.GetTempPath(),
            $"data-generator-test-{Guid.NewGuid():N}");

        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteTemporaryDirectory(
        string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
}
