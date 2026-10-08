(******************************************************************************
 * Industrial Device Teaching Simulator
 *
 * Modbus TCP mapping:
 *   %QW0    Holding Register 0   Temperature raw value
 *   %QW1    Holding Register 1   Pressure raw value
 *   %QW2    Holding Register 2   Vibration raw value
 *   %QW10   Holding Register 10  Scenario command
 *   %QX0.0  Coil 0               Running signal
 *   %QX0.1  Coil 1               Fault signal
 *   %QX0.2  Coil 2               Emergency-stop signal
 *
 * Scenario command:
 *   0 = Normal
 *   1 = Temperature rise
 *   2 = Fault
 *   3 = Emergency stop
 ******************************************************************************)

PROGRAM IndustrialDevice
VAR
    TemperatureRaw AT %QW0 : UINT := UINT#350;
    PressureRaw AT %QW1 : UINT := UINT#100;
    VibrationRaw AT %QW2 : UINT := UINT#20;

    Scenario AT %QW10 : UINT := UINT#0;

    IsRunning AT %QX0.0 : BOOL := TRUE;
    FaultSignal AT %QX0.1 : BOOL := FALSE;
    EmergencyStop AT %QX0.2 : BOOL := FALSE;

    UpdateTimer : TON;
END_VAR

(* Generate a one-scan pulse approximately once per second. *)
UpdateTimer(IN := NOT UpdateTimer.Q, PT := T#1s);

CASE Scenario OF
    UINT#0:
        TemperatureRaw := UINT#350;
        PressureRaw := UINT#100;
        VibrationRaw := UINT#20;
        IsRunning := TRUE;
        FaultSignal := FALSE;
        EmergencyStop := FALSE;

    UINT#1:
        PressureRaw := UINT#100;
        VibrationRaw := UINT#20;
        IsRunning := TRUE;
        FaultSignal := FALSE;
        EmergencyStop := FALSE;

        IF TemperatureRaw < UINT#550 THEN
            TemperatureRaw := UINT#550;
        END_IF;

        IF UpdateTimer.Q THEN
            IF TemperatureRaw >= UINT#950 THEN
                TemperatureRaw := UINT#550;
            ELSE
                TemperatureRaw := TemperatureRaw + UINT#10;
            END_IF;
        END_IF;

    UINT#2:
        TemperatureRaw := UINT#400;
        PressureRaw := UINT#100;
        VibrationRaw := UINT#20;
        IsRunning := FALSE;
        FaultSignal := TRUE;
        EmergencyStop := FALSE;

    UINT#3:
        TemperatureRaw := UINT#400;
        PressureRaw := UINT#100;
        VibrationRaw := UINT#20;
        IsRunning := FALSE;
        FaultSignal := FALSE;
        EmergencyStop := TRUE;

ELSE
    (* Unknown commands return to a safe, non-fault teaching state. *)
    TemperatureRaw := UINT#350;
    PressureRaw := UINT#100;
    VibrationRaw := UINT#20;
    IsRunning := TRUE;
    FaultSignal := FALSE;
    EmergencyStop := FALSE;
END_CASE;

END_PROGRAM
