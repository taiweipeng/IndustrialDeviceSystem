using System.IO;
using System.Net.Sockets;
using NModbus;

namespace IndustrialDeviceSystem.Infrastructure.DeviceCommunication;

public sealed class TcpModbusRegisterClient
    : IModbusRegisterClient, IDisposable
{
    private readonly TcpModbusClientOptions _options;
    private readonly object _syncRoot = new();

    private TcpClient? _tcpClient;
    private IModbusMaster? _master;
    private bool _disposed;

    public TcpModbusRegisterClient(
        TcpModbusClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ValidateOptions(options);
        _options = options;
    }

    public ushort[] ReadHoldingRegisters(
        byte unitId,
        ushort startAddress,
        ushort count)
    {
        if (count == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "读取数量必须大于0。");
        }

        return Execute(
            master => master.ReadHoldingRegisters(
                unitId,
                startAddress,
                count));
    }

    public bool[] ReadCoils(
        byte unitId,
        ushort startAddress,
        ushort count)
    {
        if (count == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                "读取数量必须大于0。");
        }

        return Execute(
            master => master.ReadCoils(
                unitId,
                startAddress,
                count));
    }

    private TResult Execute<TResult>(
        Func<IModbusMaster, TResult> operation)
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();

            try
            {
                IModbusMaster master = EnsureConnected();
                return operation(master);
            }
            catch (Exception exception)
                when (IsCommunicationFailure(exception))
            {
                Disconnect();

                throw new ModbusCommunicationException(
                    $"无法连接Modbus TCP设备" +
                    $" {_options.Host}:{_options.Port}。",
                    exception);
            }
        }
    }

    private IModbusMaster EnsureConnected()
    {
        if (_tcpClient is { Connected: true } &&
            _master is not null)
        {
            return _master;
        }

        Disconnect();

        var tcpClient = new TcpClient
        {
            NoDelay = true
        };

        IModbusMaster? master = null;

        try
        {
            using var cancellationTokenSource =
                new CancellationTokenSource(
                    _options.ConnectTimeoutMilliseconds);

            tcpClient
                .ConnectAsync(
                    _options.Host,
                    _options.Port,
                    cancellationTokenSource.Token)
                .AsTask()
                .GetAwaiter()
                .GetResult();

            var factory = new ModbusFactory();
            master = factory.CreateMaster(tcpClient);

            master.Transport.ReadTimeout =
                _options.ReadTimeoutMilliseconds;

            master.Transport.WriteTimeout =
                _options.WriteTimeoutMilliseconds;

            master.Transport.Retries =
                _options.Retries;

            _tcpClient = tcpClient;
            _master = master;

            return master;
        }
        catch
        {
            master?.Dispose();
            tcpClient.Dispose();
            throw;
        }
    }

    private static bool IsCommunicationFailure(
        Exception exception)
    {
        return exception is SocketException
            or IOException
            or TimeoutException
            or OperationCanceledException;
    }

    private static void ValidateOptions(
        TcpModbusClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            throw new ArgumentException(
                "Modbus主机地址不能为空。",
                nameof(options));
        }

        if (options.Port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "Modbus端口必须在1至65535之间。");
        }

        if (options.ConnectTimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "连接超时必须大于0。");
        }

        if (options.ReadTimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "读取超时必须大于0。");
        }

        if (options.WriteTimeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "写入超时必须大于0。");
        }

        if (options.Retries < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "重试次数不能小于0。");
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(
                nameof(TcpModbusRegisterClient));
        }
    }

    private void Disconnect()
    {
        IModbusMaster? master = _master;
        TcpClient? tcpClient = _tcpClient;

        _master = null;
        _tcpClient = null;

        try
        {
            master?.Dispose();
        }
        finally
        {
            tcpClient?.Dispose();
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Disconnect();
        }
    }
}
