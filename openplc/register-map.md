# OpenPLC Modbus TCP地址表

## 1. 课程联调参数

| 参数 | 课程约定 |
|---|---|
| 协议 | Modbus TCP |
| 主机 | `127.0.0.1` |
| 端口 | `5020` |
| Unit ID | `1` |
| 地址写法 | C#代码使用从0开始的协议地址 |

> 部分Modbus软件把第一个Holding Register显示为`40001`，而NModbus代码中的起始地址是`0`。本项目统一在代码和测试中使用从0开始的地址，避免把显示编号误当成协议地址。

## 2. 采集数据映射

| 数据区 | C#地址 | OpenPLC变量 | 含义 | 工程量转换 | C#读取方法 |
|---|---:|---|---|---|---|
| Holding Register | 0 | `%QW0` | 温度原始值 | `raw × 0.1 ℃` | `ReadHoldingRegisters(1, 0, 3)` |
| Holding Register | 1 | `%QW1` | 压力原始值 | `raw × 0.01 MPa` | 同上 |
| Holding Register | 2 | `%QW2` | 振动原始值 | `raw × 0.01` | 同上 |
| Coil | 0 | `%QX0.0` | 运行信号 | `true`表示运行 | `ReadCoils(1, 0, 3)` |
| Coil | 1 | `%QX0.1` | 故障信号 | `true`表示故障 | 同上 |
| Coil | 2 | `%QX0.2` | 急停信号 | `true`表示急停 | 同上 |

## 3. 教学场景控制

Holding Register 10（OpenPLC变量`%QW10`）用于选择教学场景，不属于业务遥测数据。

| 值 | 场景 | 预期原始值 | 预期线圈 |
|---:|---|---|---|
| 0 | 正常 | 温度350、压力100、振动20 | 运行1、故障0、急停0 |
| 1 | 温度上升 | 温度从550递增到950并循环，压力100、振动20 | 运行1、故障0、急停0 |
| 2 | 设备故障 | 温度400、压力100、振动20 | 运行0、故障1、急停0 |
| 3 | 急停 | 温度400、压力100、振动20 | 运行0、故障0、急停1 |

可在OpenPLC监控界面中修改`Scenario`，后续也可以通过Modbus功能码06或16写入Holding Register 10。

通信中断不能通过PLC内部变量真实模拟。测试`CommunicationLost`时，应停止OpenPLC Runtime、关闭Modbus从站插件或临时使用错误端口。

## 4. 首次联调验收值

当`Scenario = 0`时，C#端应读取到：

```text
Temperature = 35.0
Pressure = 1.00
Vibration = 0.20
IsRunning = true
FaultSignal = false
EmergencyStop = false
Quality = Good
```

## 5. 边界说明

- OpenPLC只负责生成和公开设备信号。
- `TcpModbusRegisterClient`负责Modbus TCP通信。
- `OpenPlcDataSource`负责原始值到工程量的转换。
- `DeviceMonitoringApplicationService`负责保存遥测、处理报警和更新设备状态。
- OpenPLC程序不得直接访问SQLite，也不得承担预约、报警或维护业务规则。
