// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Source taken from https://github.com/dotnet/runtime/blob/v11.0.100/src/libraries/Common/src/System/Text/ValueStringBuilder.AppendSpanFormattable.cs

using System;

#nullable enable

namespace AvaloniaEdit.Utils
{
    internal ref partial struct ValueStringBuilder
    {
        internal void AppendSpanFormattable<T>(T value, string? format = null, IFormatProvider? provider = null) where T : ISpanFormattable
        {
            if (value.TryFormat(_chars[_pos..], out int charsWritten, format, provider))
            {
                _pos += charsWritten;
            }
            else
            {
                Append(value.ToString(format, provider));
            }
        }
    }
}
