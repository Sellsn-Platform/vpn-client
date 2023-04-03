using System;

namespace SellSn.Client.OpenVPN.Exceptions;

/// <inheritdoc />
internal sealed class TapException : Exception
{
    internal TapException(string message) : base(message)
    {
    }
}