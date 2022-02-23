﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

namespace Argon;

/// <summary>
/// Converts a <see cref="DateTime"/> to and from Unix epoch time
/// </summary>
public class UnixDateTimeConverter : DateTimeConverterBase
{
    internal static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        long seconds;

        if (value is DateTime dateTime)
        {
            seconds = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
        else if (value is DateTimeOffset dateTimeOffset)
        {
            seconds = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }
        else
        {
            throw new JsonSerializationException("Expected date object value.");
        }

        if (seconds < 0)
        {
            throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
        }

        writer.WriteValue(seconds);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var nullable = ReflectionUtils.IsNullable(type);
        if (reader.TokenType == JsonToken.Null)
        {
            if (!nullable)
            {
                throw JsonSerializationException.Create(reader, $"Cannot convert null value to {type}.");
            }

            return null;
        }

        long seconds;

        if (reader.TokenType == JsonToken.Integer)
        {
            seconds = (long)reader.Value!;
        }
        else if (reader.TokenType == JsonToken.String)
        {
            if (!long.TryParse((string)reader.Value!, out seconds))
            {
                throw JsonSerializationException.Create(reader, $"Cannot convert invalid value to {type}.");
            }
        }
        else
        {
            throw JsonSerializationException.Create(reader, $"Unexpected token parsing date. Expected Integer or String, got {reader.TokenType}.");
        }

        if (seconds >= 0)
        {
            var d = UnixEpoch.AddSeconds(seconds);

            var t = nullable
                ? Nullable.GetUnderlyingType(type)
                : type;
            if (t == typeof(DateTimeOffset))
            {
                return new DateTimeOffset(d, TimeSpan.Zero);
            }
            return d;
        }

        throw JsonSerializationException.Create(reader, $"Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {type}.");
    }
}