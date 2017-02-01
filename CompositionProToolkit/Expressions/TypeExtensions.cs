// Copyright (c) 2017 Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the CompositionProToolkit project: 
// https://github.com/ratishphilip/CompositionProToolkit
//
// CompositionProToolkit v0.5.1
//

using System;
using System.Reflection;

namespace CompositionProToolkit.Expressions
{
    /// <summary>
    /// Extension methods for System.Type
    /// </summary>
    public static class TypeExtensions
    {
        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
            return type.GetTypeInfo()?.GetDeclaredProperty(propertyName);
        }

        public static MethodInfo GetMethod(this Type type, string methodName)
        {
            return type.GetTypeInfo()?.GetDeclaredMethod(methodName);
        }

        public static bool IsSubclassOf(this Type type, Type parentType)
        {
            return type.GetTypeInfo()?.IsSubclassOf(parentType) ?? false;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo()?.IsClass ?? false;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo()?.IsEnum ?? false;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo()?.IsPrimitive ?? false;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo()?.BaseType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo()?.IsGenericType ?? false;
        }

        public static bool IsPublic(this Type type)
        {
            return type.GetTypeInfo()?.IsPublic ?? false;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo()?.GenericTypeArguments;
        }

        public static object GetPropertyValue(this object instance, string propertyValue)
        {
            return instance.GetType().GetTypeInfo()?.GetDeclaredProperty(propertyValue)?.GetValue(instance);
        }

        public static TypeInfo GetTypeInfo(this Type type)
        {
            var reflectableType = type as IReflectableType;
            return reflectableType?.GetTypeInfo();
        }


        internal static bool IsAnonymous(this Type type)
        {
            if (!string.IsNullOrEmpty(type.Namespace) || !type.IsGenericType()) return false;
            return IsAnonymous(type.Name);
        }

        internal static bool IsAnonymous(string typeName)
        {
            // Optimization to improve perf when called from UserCache
            return
                typeName.Length > 5 &&
                    (typeName[0] == '<' && typeName[1] == '>' && (typeName[5] == 'A' && typeName[6] == 'n' || typeName.IndexOf("anon", StringComparison.OrdinalIgnoreCase) > -1) ||
                    typeName[0] == 'V' && typeName[1] == 'B' && typeName[2] == '$' && typeName[3] == 'A' && typeName[4] == 'n');
        }

        internal static string FormattedName(this Type t, bool fullname = false)
        {
            return fullname ? t.FullName : t.Name;
        }
    }
}
