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

static class TypeExtensions
{
    public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces, [NotNullWhen(true)]out Type? match)
    {
        var current = type;

        while (current != null)
        {
            if (string.Equals(current.FullName, fullTypeName, StringComparison.Ordinal))
            {
                match = current;
                return true;
            }

            current = current.BaseType;
        }

        if (searchInterfaces)
        {
            foreach (var i in type.GetInterfaces())
            {
                if (string.Equals(i.Name, fullTypeName, StringComparison.Ordinal))
                {
                    match = type;
                    return true;
                }
            }
        }

        match = null;
        return false;
    }

    public static bool AssignableToTypeName(this Type type, string fullTypeName, bool searchInterfaces)
    {
        return type.AssignableToTypeName(fullTypeName, searchInterfaces, out _);
    }

    public static bool ImplementInterface(this Type type, Type interfaceType)
    {
        for (var currentType = type; currentType != null; currentType = currentType.BaseType)
        {
            IEnumerable<Type> interfaces = currentType.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i == interfaceType || (i != null && i.ImplementInterface(interfaceType)))
                {
                    return true;
                }
            }
        }

        return false;
    }
}