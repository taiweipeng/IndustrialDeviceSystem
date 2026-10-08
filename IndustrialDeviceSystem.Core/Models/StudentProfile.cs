namespace IndustrialDeviceSystem.Core.Models;

public sealed class StudentProfile
{
    public string StudentId { get; set; } = string.Empty;
    public string GithubUser { get; set; } = string.Empty;
    public int SampleIntervalSeconds { get; set; } = 2;
    public int MaximumCommunicationFailures { get; set; } = 3;

    public TemperatureThresholds TemperatureThresholds { get; set; } = new();

    public List<StudentDeviceProfile> Devices { get; set; } = new();

    public ExistingBookingProfile ExistingBooking { get; set; } = new();

    public List<bool> FaultSequence { get; set; } = new();
}

public sealed class StudentDeviceProfile
{
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
}

public sealed class ExistingBookingProfile
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
