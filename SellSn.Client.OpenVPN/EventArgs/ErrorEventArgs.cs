using System;

namespace SellSn.Client.OpenVPN.EventArgs;

/// <inheritdoc />
public sealed class ErrorEventArgs : System.EventArgs
{
    internal ErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
}