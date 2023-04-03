using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SellSn.Client.OpenVPN.Models;

namespace SellSn.Client.OpenVPN.Utils;

/// <summary>
///     Allows processes to be automatically killed if this parent process unexpectedly quits.
///     This feature requires Windows 8 or greater. On Windows 7, nothing is done.
/// </summary>
/// <remarks>
///     References:
///     https://stackoverflow.com/a/4657392/386091
///     https://stackoverflow.com/a/9164742/386091
/// </remarks>
internal static class ChildProcessTracker
{
    // Windows will automatically close any open job handles when our process terminates.
    //  This can be verified by using SysInternals' Handle utility. When the job handle
    //  is closed, the child processes will be killed.
    private static readonly nint SJobHandle;

    static ChildProcessTracker()
    {
        // This feature requires Windows 8 or later. To support Windows 7 requires
        //  registry settings to be added if you are using Visual Studio plus an
        //  app.manifest change.
        //  https://stackoverflow.com/a/4232259/386091
        //  https://stackoverflow.com/a/9507862/386091
        if (Environment.OSVersion.Version < new Version(6, 2))
            return;

        // The job name is optional (and can be null) but it helps with diagnostics.
        //  If it's not null, it has to be unique. Use SysInternals' Handle command-line
        //  utility: handle -a ChildProcessTracker
        var jobName = "ChildProcessTracker" + Environment.ProcessId;
        SJobHandle = CreateJobObject(nint.Zero, jobName);

        var info = new JobobjectBasicLimitInformation
        {
            // This is the key flag. When our process is killed, Windows will automatically
            //  close the job handle, and when that happens, we want the child processes to
            //  be killed, too.
            LimitFlags = Jobobjectlimit.JobObjectLimitKillOnJobClose
        };

        var extendedInfo = new JobobjectExtendedLimitInformation
        {
            BasicLimitInformation = info
        };

        var length = Marshal.SizeOf(typeof(JobobjectExtendedLimitInformation));
        var extendedInfoPtr = Marshal.AllocHGlobal(length);
        try
        {
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!SetInformationJobObject(SJobHandle,
                    JobObjectInfoType.ExtendedLimitInformation,
                    extendedInfoPtr, (uint)length))
                throw new Win32Exception();
        }
        finally
        {
            Marshal.FreeHGlobal(extendedInfoPtr);
        }
    }

    /// <summary>
    ///     Add the process to be tracked. If our current process is killed, the child processes
    ///     that we are tracking will be automatically killed, too. If the child process terminates
    ///     first, that's fine, too.
    /// </summary>
    /// <param name="process"></param>
    public static void AddProcess(Process process)
    {
        if (SJobHandle == nint.Zero) return;

        var success = AssignProcessToJobObject(SJobHandle, process.Handle);
        if (!success && !process.HasExited)
            throw new Win32Exception();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateJobObject(nint lpJobAttributes, string name);

    [DllImport("kernel32.dll")]
    private static extern bool SetInformationJobObject(nint job, JobObjectInfoType infoType,
        nint lpJobObjectInfo, uint cbJobObjectInfoLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(nint job, nint process);
}

internal enum JobObjectInfoType
{
    ExtendedLimitInformation = 9
}