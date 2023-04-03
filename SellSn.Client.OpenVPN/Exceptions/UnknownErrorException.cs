#nullable enable
using System;

namespace SellSn.Client.OpenVPN.Exceptions;

/// <inheritdoc />
/// <summary>
///     Thrown when OpenVPN returns a 'Unknown error' output
/// </summary>
internal sealed class UnknownErrorException : Exception
{
    internal UnknownErrorException(string message) : base(message)
    {
    }
}