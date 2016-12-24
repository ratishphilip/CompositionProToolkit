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
// CompositionProToolkit v0.5.0
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions
{
    public static class CompositionPropertySetExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> InsertMethods;
        private static readonly Dictionary<Type, MethodInfo> TryGetMethods;
        private static readonly Type[] Floatables;

        #endregion

        #region Static Constructor

        static CompositionPropertySetExtensions()
        {
            // Get all the Insertxxx methods
            InsertMethods = typeof(CompositionPropertySet)
                                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => m.Name.StartsWith("Insert"))
                                .ToDictionary(m => m.GetParameters()[1].ParameterType,
                                              m => m);
            // Get all the TryGetxxx methods
            TryGetMethods = typeof(CompositionPropertySet)
                                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(m => m.Name.StartsWith("TryGet"))
                                .ToDictionary(m => m.GetParameters()[1].ParameterType.GetElementType(),
                                              m => m);

            Floatables = new[]{
                                  typeof(short),
                                  typeof(ushort),
                                  typeof(int),
                                  typeof(uint),
                                  typeof(long),
                                  typeof(ulong),
                                  typeof(char),
                                  typeof(double),
                                  typeof(decimal)
                              };
        }

        #endregion

        #region Extension methods

        /// <summary>
        /// Inserts a key and its corresponding value in the CompositionPropertySet
        /// </summary>
        /// <typeparam name="T">Type of the object to be inserted in the CompositionPropertySet</typeparam>
        /// <param name="propertySet">CompositionPropertySet</param>
        /// <param name="key">Key for the object to be inserted in the CompositionPropertySet</param>
        /// <param name="input">Object to be inserted in the CompositionPropertySet</param>
        public static void Insert<T>(this CompositionPropertySet propertySet, string key, object input)
        {
            var type = typeof(T);
            while (!type.IsPublic())
            {
                type = type.BaseType();
            }

            MethodInfo methodInfo;
            // Find matching Insertxxx method for the given type
            if (InsertMethods.TryGetValue(type, out methodInfo) ||
                ((type.BaseType() != null) && InsertMethods.TryGetValue(type.BaseType(), out methodInfo)))
            {
                // Once a matching Insertxxx method is found, Invoke it!
                methodInfo.Invoke(propertySet, new[] { key, input });
            }

            // If no matching method is found, then raise an exception
            throw new ArgumentException($"Cannot set the key \'{key}\' for the value of type \'{type.FullName}\'");
        }

        /// <summary>
        /// Retrieves an object for the given key, from the CompositionPropertySet
        /// </summary>
        /// <typeparam name="T">Type of the object to retrieve</typeparam>
        /// <param name="propertySet">CompositionPropertySet</param>
        /// <param name="key">Key of the object to retrieve</param>
        /// <returns>The retrieved object</returns>
        public static T Get<T>(this CompositionPropertySet propertySet, string key)
        {
            var type = typeof(T);
            while (!type.IsPublic())
            {
                type = type.BaseType();
            }

            MethodInfo methodInfo;
            // Find matching TryGetxxx method for the given type
            if (TryGetMethods.TryGetValue(type, out methodInfo) ||
                ((type.BaseType() != null) && TryGetMethods.TryGetValue(type.BaseType(), out methodInfo)))
            {
                var result = default(T);
                // Once a matching TryGetxxx method is found, Invoke it!
                var methodResult = (CompositionGetValueStatus)methodInfo.Invoke(propertySet, new object[] { key, result });

                switch (methodResult)
                {
                    case CompositionGetValueStatus.Succeeded:
                        return result;
                    case CompositionGetValueStatus.TypeMismatch:
                        throw new ArgumentException($"The key \'{key}\' does not return data of type "
                                                    + $"\'{type.FullName}\' in the CompositionPropertySet!");
                    case CompositionGetValueStatus.NotFound:
                        throw new ArgumentException($"The key \'{key}\' was not found in the CompositionPropertySet!");
                }

            }

            // If no matching method is found, then raise an exception
            throw new ArgumentException($"The key \'{key}\' was not found in the CompositionPropertySet!");
        }

        #endregion

        #region APIs

        /// <summary>
        /// Converts given object into a CompositionPropertySet
        /// </summary>
        /// <param name="input">Object to convert</param>
        /// <param name="compositor">Compositor</param>
        /// <returns>CompositionPropertySet</returns>
        public static CompositionPropertySet ToPropertySet(object input, Compositor compositor)
        {
            var propertySet = compositor.CreatePropertySet();

            foreach (var property in input.GetType().GetTypeInfo().DeclaredProperties)
            {
                var type = property.PropertyType;
                var parameter = property.GetValue(input);

                // Can the type be converted to float?
                if (Floatables.Contains(type))
                {
                    type = typeof(float);
                    parameter = Convert.ToSingle(parameter);
                }

                while (!type.IsPublic())
                {
                    type = type.BaseType();
                }

                MethodInfo methodInfo;
                // Find matching Insertxxx method for the given type
                if (InsertMethods.TryGetValue(type, out methodInfo) ||
                    ((type.BaseType() != null) && InsertMethods.TryGetValue(type.BaseType(), out methodInfo)))
                {
                    // Once a matching Insertxxx method is found, Invoke it!
                    methodInfo.Invoke(propertySet, new[] { property.Name, parameter });
                }
                else
                {
                    // If no matching method is found, then raise an exception
                    throw new ArgumentException($"Cannot set the key \'{property.Name}\' " +
                                                $"for the value of type \'{type.FullName}\'");
                }
            }

            return propertySet;
        }

        #endregion
    }
}
