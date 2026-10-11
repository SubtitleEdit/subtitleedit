using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace Nikse.SubtitleEdit.Logic.Platform.Progress;

internal sealed class LinuxLauncherProgress : IPlatformProgress
{
    private readonly Task<DBusConnection?> _connection = ConnectAsync();
    private long _version;
    private bool _disposed;

    public void Update(double? percentage, bool indeterminate)
    {
        if (!_disposed)
            _ = UpdateAsync(indeterminate ? null : percentage, ++_version);
    }

    private static async Task<DBusConnection?> ConnectAsync()
    {
        DBusConnection? connection = null;
        try
        {
            if (DBusAddress.Session is not { } address) return null;
            connection = new DBusConnection(address);
            await connection.ConnectAsync();
            return connection;
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Linux launcher progress unavailable: {exception.Message}");
            connection?.Dispose();
            return null;
        }
    }

    private async Task UpdateAsync(double? percentage, long version)
    {
        try
        {
            var connection = await _connection;
            // Publish only the latest state after connecting, including clear requests.
            if (connection is null || _disposed || version != _version) return;
            connection.TrySendMessage(CreateUpdate(connection, percentage));
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Linux launcher progress unavailable: {exception.Message}");
        }
    }

    internal static MessageBuffer CreateUpdate(DBusConnection connection, double? percentage)
    {
        using var writer = connection.GetMessageWriter();
        writer.WriteSignalHeader(
            path: "/com/canonical/unity/launcherentry",
            @interface: "com.canonical.Unity.LauncherEntry",
            member: "Update", signature: "sa{sv}");
        writer.WriteString("application://dk.nikse.subtitleedit.desktop");
        var dictionary = writer.WriteDictionaryStart();
        writer.WriteDictionaryEntryStart();
        writer.WriteString("progress");
        writer.WriteVariantDouble(Math.Clamp(double.IsFinite(percentage ?? 0) ? percentage ?? 0 : 0, 0, 100) / 100);
        writer.WriteDictionaryEntryStart();
        writer.WriteString("progress-visible");
        writer.WriteVariantBool(percentage.HasValue);
        writer.WriteDictionaryEnd(dictionary);
        return writer.CreateMessage();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ = DisconnectAsync();
    }

    private async Task DisconnectAsync()
    {
        var connection = await _connection;
        connection?.Dispose();
    }
}
