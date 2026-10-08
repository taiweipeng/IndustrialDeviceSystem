using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Simulator;

public sealed class SimulatorDataSource : IDeviceDataSource
{
    private static readonly double[] NormalTemperatureOffsets =
    {
        -0.4,
        0.0,
        0.3,
        0.6,
        0.2
    };

    private readonly Dictionary<string, DeviceSimulationState> _states;
    private readonly IClock _clock;
    private readonly object _syncRoot = new();

    public SimulatorDataSource(
        IEnumerable<SimulatorDeviceProfile> profiles,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(clock);

        _clock = clock;

        _states = new Dictionary<string, DeviceSimulationState>(
            StringComparer.OrdinalIgnoreCase);

        foreach (SimulatorDeviceProfile profile in profiles)
        {
            ArgumentNullException.ThrowIfNull(profile);

            if (string.IsNullOrWhiteSpace(profile.DeviceCode))
            {
                throw new ArgumentException(
                    "设备编码不能为空。",
                    nameof(profiles));
            }

            var profileCopy = new SimulatorDeviceProfile
            {
                DeviceCode = profile.DeviceCode.Trim(),
                BaseTemperature = profile.BaseTemperature,
                BasePressure = profile.BasePressure,
                BaseVibration = profile.BaseVibration,
                NormallyRunning = profile.NormallyRunning
            };

            bool added = _states.TryAdd(
                profileCopy.DeviceCode,
                new DeviceSimulationState(profileCopy));

            if (!added)
            {
                throw new ArgumentException(
                    $"设备编码重复：{profileCopy.DeviceCode}",
                    nameof(profiles));
            }
        }

        if (_states.Count == 0)
        {
            throw new ArgumentException(
                "至少需要配置一台模拟设备。",
                nameof(profiles));
        }
    }

    public void SetScenario(
        string deviceCode,
        SimulationScenario scenario)
    {
        if (!Enum.IsDefined(
                typeof(SimulationScenario),
                scenario))
        {
            throw new ArgumentOutOfRangeException(
                nameof(scenario),
                "未定义的模拟场景。");
        }

        lock (_syncRoot)
        {
            DeviceSimulationState state =
                FindState(deviceCode);

            state.Scenario = scenario;
            state.SampleIndex = 0;
        }
    }

    public DeviceSnapshot ReadSnapshot(string deviceCode)
    {
        lock (_syncRoot)
        {
            DeviceSimulationState state =
                FindState(deviceCode);

            int sampleIndex = state.SampleIndex;
            state.SampleIndex++;

            return CreateSnapshot(
                state,
                sampleIndex);
        }
    }

    private DeviceSimulationState FindState(
        string deviceCode)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            throw new ArgumentException(
                "设备编码不能为空。",
                nameof(deviceCode));
        }

        if (!_states.TryGetValue(
                deviceCode.Trim(),
                out DeviceSimulationState? state))
        {
            throw new KeyNotFoundException(
                $"没有找到模拟设备：{deviceCode}");
        }

        return state;
    }

    private DeviceSnapshot CreateSnapshot(
        DeviceSimulationState state,
        int sampleIndex)
    {
        SimulatorDeviceProfile profile = state.Profile;

        return state.Scenario switch
        {
            SimulationScenario.Normal =>
                CreateGoodSnapshot(
                    profile,
                    temperature:
                        profile.BaseTemperature +
                        GetNormalOffset(sampleIndex),
                    pressure:
                        profile.BasePressure +
                        GetNormalOffset(sampleIndex) * 0.01,
                    vibration:
                        Math.Max(
                            0.0,
                            profile.BaseVibration +
                            Math.Abs(GetNormalOffset(sampleIndex)) *
                            0.01),
                    isRunning: profile.NormallyRunning,
                    faultSignal: false,
                    emergencyStop: false),

            SimulationScenario.TemperatureRise =>
                CreateGoodSnapshot(
                    profile,
                    temperature:
                        profile.BaseTemperature +
                        Math.Min(
                            64.0,
                            (sampleIndex + 1) * 8.0),
                    pressure:
                        profile.BasePressure + 0.02,
                    vibration:
                        profile.BaseVibration + 0.01,
                    isRunning: profile.NormallyRunning,
                    faultSignal: false,
                    emergencyStop: false),

            SimulationScenario.Fault =>
                CreateGoodSnapshot(
                    profile,
                    temperature:
                        profile.BaseTemperature + 12.0,
                    pressure:
                        profile.BasePressure,
                    vibration:
                        profile.BaseVibration + 0.15,
                    isRunning: false,
                    faultSignal: true,
                    emergencyStop: false),

            SimulationScenario.EmergencyStop =>
                CreateGoodSnapshot(
                    profile,
                    temperature:
                        profile.BaseTemperature,
                    pressure:
                        profile.BasePressure,
                    vibration: 0.0,
                    isRunning: false,
                    faultSignal: false,
                    emergencyStop: true),

            SimulationScenario.CommunicationLost =>
                CreateCommunicationLostSnapshot(profile),

            _ => throw new InvalidOperationException(
                "无法处理当前模拟场景。")
        };
    }

    private DeviceSnapshot CreateGoodSnapshot(
        SimulatorDeviceProfile profile,
        double temperature,
        double pressure,
        double vibration,
        bool isRunning,
        bool faultSignal,
        bool emergencyStop)
    {
        return new DeviceSnapshot
        {
            DeviceCode = profile.DeviceCode,
            CollectedAt = _clock.Now,
            Temperature = temperature,
            Pressure = pressure,
            Vibration = vibration,
            IsRunning = isRunning,
            FaultSignal = faultSignal,
            EmergencyStop = emergencyStop,
            Quality = DataQuality.Good
        };
    }

    private DeviceSnapshot CreateCommunicationLostSnapshot(
        SimulatorDeviceProfile profile)
    {
        return new DeviceSnapshot
        {
            DeviceCode = profile.DeviceCode,
            CollectedAt = _clock.Now,

            // 保留模拟基准值，但上层必须依据Quality拒绝使用。
            Temperature = profile.BaseTemperature,
            Pressure = profile.BasePressure,
            Vibration = profile.BaseVibration,

            IsRunning = false,
            FaultSignal = false,
            EmergencyStop = false,
            Quality = DataQuality.CommunicationLost
        };
    }

    private static double GetNormalOffset(int sampleIndex)
    {
        int index =
            sampleIndex % NormalTemperatureOffsets.Length;

        return NormalTemperatureOffsets[index];
    }

    private sealed class DeviceSimulationState
    {
        public DeviceSimulationState(
            SimulatorDeviceProfile profile)
        {
            Profile = profile;
        }

        public SimulatorDeviceProfile Profile { get; }

        public SimulationScenario Scenario { get; set; } =
            SimulationScenario.Normal;

        public int SampleIndex { get; set; }
    }
}
