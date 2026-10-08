using IndustrialDeviceSystem.Core.Enums;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class AlarmRuleService
{
    public AlarmLevel GetTemperatureLevel(
        double temperature,
        TemperatureThresholds thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        if (!double.IsFinite(thresholds.Warning) ||
            !double.IsFinite(thresholds.High) ||
            !double.IsFinite(thresholds.Emergency) ||
            thresholds.Warning >= thresholds.High ||
            thresholds.High >= thresholds.Emergency)
        {
            throw new ArgumentException(
                "温度阈值必须是有限数值，并满足Warning < High < Emergency。",
                nameof(thresholds));
        }

        if (!double.IsFinite(temperature))
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature),
                "温度必须是有限数值。");
        }

        if (temperature < thresholds.Warning)
        {
            return AlarmLevel.Normal;
        }

        if (temperature < thresholds.High)
        {
            return AlarmLevel.Warning;
        }

        if (temperature < thresholds.Emergency)
        {
            return AlarmLevel.High;
        }

        return AlarmLevel.Emergency;
    }
}
