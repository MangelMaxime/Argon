#region License
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
/// Converts an object to and from JSON.
/// </summary>
public abstract class JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public abstract void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public abstract object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer);

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool CanConvert(Type type);

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
    /// </summary>
    public virtual bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
    /// </summary>
    public virtual bool CanWrite => true;
}

/// <summary>
/// Converts an object to and from JSON.
/// </summary>
public abstract class JsonConverter<T> : JsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (!(value != null ? value is T : ReflectionUtils.IsNullable(typeof(T))))
        {
            throw new JsonSerializationException($"Converter cannot write specified value to JSON. {typeof(T)} is required.");
        }
        WriteJson(writer, (T?)value, serializer);
    }

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public sealed override object? ReadJson(JsonReader reader, Type type, object? existingValue, JsonSerializer serializer)
    {
        var existingIsNull = existingValue == null;
        if (!(existingIsNull || existingValue is T))
        {
            throw new JsonSerializationException($"Converter cannot read JSON with the specified existing value. {typeof(T)} is required.");
        }
        return ReadJson(reader, type, existingIsNull ? default : (T?)existingValue, !existingIsNull, serializer);
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    public abstract T? ReadJson(JsonReader reader, Type type, T? existingValue, bool hasExistingValue, JsonSerializer serializer);

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public sealed override bool CanConvert(Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }
}