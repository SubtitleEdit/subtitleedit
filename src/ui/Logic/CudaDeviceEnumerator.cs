using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>One device as CUDA numbers it: <c>Index</c> is the N in "--device cuda:N" and in CUDA_VISIBLE_DEVICES.</summary>
/// <param name="ComputeCapability">The device's compute capability (e.g. 8.9 for an RTX 4070), or null when not known.
/// CUDA 13 builds need 7.5 (Turing) or newer, CUDA 12 builds 5.0 (Maxwell) or newer.</param>
public sealed record CudaDevice(int Index, string Name, long MemoryBytes, Version? ComputeCapability = null);

/// <summary>What the installed NVIDIA driver reports.</summary>
/// <param name="CudaVersion">The newest CUDA version the driver supports, as the driver encodes it (13000 = 13.0), or 0 when not known.</param>
/// <param name="Devices">The devices in CUDA's order.</param>
public sealed record CudaDriverInfo(int CudaVersion, IReadOnlyList<CudaDevice> Devices)
{
    public static readonly CudaDriverInfo None = new(0, Array.Empty<CudaDevice>());
}

/// <summary>
/// Asks the installed NVIDIA driver how CUDA numbers the adapters. That order is CUDA's own
/// (fastest first unless CUDA_DEVICE_ORDER says otherwise) and differs from the slot order
/// nvidia-smi and the registry use, so it cannot be worked out from them - and it is the number
/// a user needs to put an engine on one particular card of several (#16944).
/// Only the driver's device list is read: cuInit creates no context and takes no video memory,
/// so it is safe while another program fills a card. Environment variables that change CUDA's
/// numbering apply here exactly as they do to the engines Subtitle Edit starts.
/// </summary>
public static class CudaDeviceEnumerator
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuInit(uint flags);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDeviceGetCount(out int count);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDeviceGet(out int device, int ordinal);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDeviceGetName(byte[] name, int length, int device);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDeviceTotalMem(out nuint bytes, int device);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDriverGetVersion(out int version);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int CuDeviceGetAttribute(out int value, int attribute, int device);

    private const int AttributeComputeCapabilityMajor = 75;
    private const int AttributeComputeCapabilityMinor = 76;

    private static CudaDriverInfo? _cached;

    /// <summary>
    /// The CUDA devices in CUDA's order, or an empty list when there is no CUDA driver or it
    /// cannot be asked. Read once per run; never throws.
    /// </summary>
    public static IReadOnlyList<CudaDevice> GetDevices()
    {
        return GetDriverInfo().Devices;
    }

    /// <summary>
    /// The driver's CUDA version and devices, or <see cref="CudaDriverInfo.None"/> when there is
    /// no CUDA driver or it cannot be asked. Read once per run; never throws. Loads the driver
    /// library, so call it off the UI thread (<see cref="GpuCapabilityHelper.DetectWithCudaAsync"/>).
    /// </summary>
    public static CudaDriverInfo GetDriverInfo()
    {
        return _cached ??= Enumerate();
    }

    private static CudaDriverInfo Enumerate()
    {
        var devices = new List<CudaDevice>();
        var cudaVersion = 0;
        try
        {
            var library = OperatingSystem.IsWindows() ? "nvcuda.dll" : OperatingSystem.IsLinux() ? "libcuda.so.1" : null;
            if (library == null || !NativeLibrary.TryLoad(library, out var handle))
            {
                return CudaDriverInfo.None;
            }

            // cuDriverGetVersion needs no cuInit, so the CUDA version is known even when
            // initialising fails (no device, or a driver/kernel module mismatch on Linux).
            if (TryGet<CuDriverGetVersion>(handle, "cuDriverGetVersion", out var driverVersion) &&
                driverVersion(out var version) == 0)
            {
                cudaVersion = version;
            }

            // The driver library is left loaded: it is the system's own copy, and unloading a
            // driver library that has been initialised is not something it promises to survive.
            if (!TryGet<CuInit>(handle, "cuInit", out var init) ||
                !TryGet<CuDeviceGetCount>(handle, "cuDeviceGetCount", out var getCount) ||
                !TryGet<CuDeviceGet>(handle, "cuDeviceGet", out var get) ||
                !TryGet<CuDeviceGetName>(handle, "cuDeviceGetName", out var getName) ||
                !TryGet<CuDeviceTotalMem>(handle, "cuDeviceTotalMem_v2", out var totalMem))
            {
                return new CudaDriverInfo(cudaVersion, devices);
            }

            TryGet<CuDeviceGetAttribute>(handle, "cuDeviceGetAttribute", out var getAttribute);
            if (init(0) != 0 || getCount(out var count) != 0)
            {
                return new CudaDriverInfo(cudaVersion, devices);
            }

            for (var i = 0; i < count && i < 64; i++)
            {
                if (get(out var device, i) != 0)
                {
                    continue;
                }

                var buffer = new byte[256];
                var name = getName(buffer, buffer.Length, device) == 0 ? ReadName(buffer) : string.Empty;
                var memory = totalMem(out var bytes, device) == 0 ? (long)bytes : 0;
                Version? compute = null;
                if (getAttribute != null &&
                    getAttribute(out var major, AttributeComputeCapabilityMajor, device) == 0 &&
                    getAttribute(out var minor, AttributeComputeCapabilityMinor, device) == 0 &&
                    major > 0)
                {
                    compute = new Version(major, minor);
                }

                devices.Add(new CudaDevice(i, name, memory, compute));
            }
        }
        catch
        {
            // A driver that cannot be loaded or asked just means no device numbers are shown.
        }

        return new CudaDriverInfo(cudaVersion, devices);
    }

    private static bool TryGet<T>(IntPtr library, string name, out T function) where T : Delegate
    {
        if (NativeLibrary.TryGetExport(library, name, out var address))
        {
            function = Marshal.GetDelegateForFunctionPointer<T>(address);
            return true;
        }

        function = null!;
        return false;
    }

    private static string ReadName(byte[] buffer)
    {
        var length = Array.IndexOf(buffer, (byte)0);
        return Encoding.UTF8.GetString(buffer, 0, length < 0 ? buffer.Length : length).Trim();
    }
}
