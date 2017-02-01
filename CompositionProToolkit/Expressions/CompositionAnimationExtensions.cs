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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Windows.Foundation;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions
{
    /// <summary>
    /// Extension methods for Animations deriving from CompositionAnimation
    /// </summary>
    public static class CompositionAnimationExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> SetMethods;
        private static readonly Type[] Floatables;

        #endregion

        #region Static Constructor

        static CompositionAnimationExtensions()
        {
            SetMethods = typeof(CompositionAnimation)
                               .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                               .Where(m => m.Name.StartsWith("Set") && m.Name.EndsWith("Parameter"))
                               .ToDictionary(m => m.GetParameters()[1].ParameterType,
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

        #region Extensions

        /// <summary>
        /// Sets the Expression property of ExpressionAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="animation">ExpressionAnimation</param>
        /// <param name="expression">Expression</param>
        /// <returns>Dictionary of parameter names and the parameters</returns>
        public static Dictionary<string, object> SetExpression<T>(this ExpressionAnimation animation,
            Expression<CompositionLambda<T>> expression)
        {
            var result = CompositionExpressionEngine.CreateCompositionExpression(expression);
            animation.Expression = result.Expression;
            animation.SetParameters(result.Parameters);

            return result.Parameters;
        }

        /// <summary>
        /// Inserts a KeyFrame in the KeyFrameAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <typeparam name="T">Type of the Expression</typeparam>
        /// <param name="animation">KeyFrameAnimation</param>
        /// <param name="normalizedProgressKey">The time the key frame should occur at, expressed as 
        /// a percentage of the animation Duration. Allowed value is from 0.0 to 1.0.</param>
        /// <param name="expression">The expression used to calculate the value of the key frame.</param>
        /// <param name="easingFunction">Easing Function (optional)</param>
        /// <returns>KeyFrameAnimation</returns>
        public static KeyFrameAnimation InsertExpressionKeyFrame<T>(this KeyFrameAnimation animation, float normalizedProgressKey,
            Expression<CompositionLambda<T>> expression, CompositionEasingFunction easingFunction = null)
        {
            var result = CompositionExpressionEngine.CreateCompositionExpression(expression);
            animation.InsertExpressionKeyFrame(normalizedProgressKey, result.Expression, easingFunction);
            animation.SetParameters(result.Parameters);

            return animation;
        }

        /// <summary>
        /// Sets the parameter value in the animation for the given key
        /// </summary>
        /// <typeparam name="T">Type of Animation</typeparam>
        /// <param name="animation">Animation object into which the parameters must be set</param>
        /// <param name="key">Key for the input value</param>
        /// <param name="input">Value to set</param>
        /// <returns>True if successful, otherwise False</returns>
        public static bool SetParameter<T>(this T animation, string key, object input) where T : CompositionAnimation
        {
            if (String.IsNullOrWhiteSpace(key))
                return false;

            var parameter = input;
            var type = parameter.GetType();

            // Can the type be converted to float?
            if (Floatables.Contains(type))
            {
                type = typeof(float);
                parameter = Convert.ToSingle(parameter);
            }

            if (type == typeof(Point))
            {
                var point = (Point)parameter;
                parameter = new Vector3((float)point.X, (float)point.Y, 0);
                type = typeof(Vector3);
            }

            while (!type.IsPublic())
            {
                type = type.BaseType();
            }

            MethodInfo methodInfo;
            // Find matching Setxxx method for the given type
            if (SetMethods.TryGetValue(type, out methodInfo) ||
                ((type.BaseType() != null) && SetMethods.TryGetValue(type.BaseType(), out methodInfo)))
            {
                // Once a matching SetxxxParameter method is found, Invoke it!
                methodInfo.Invoke(animation, new[] { key, parameter });

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the parameters obtained from parsing the Expression to the CompositionAnimation
        /// </summary>
        /// <typeparam name="T">Type of Animation</typeparam>
        /// <param name="animation">Animation object into which the parameters must be set</param>
        /// <param name="parameters">Parameters to set</param>
        /// <returns>Animation</returns>
        public static T SetParameters<T>(this T animation, Dictionary<string, object> parameters) where T : CompositionAnimation
        {
            var newParameters = new Dictionary<string, object>();

            // Try to set the parameter for each of the keys
            foreach (var key in from key in parameters.Keys
                                let parameter = parameters[key]
                                where (!animation.SetParameter(key, parameter))
                                select key)
            {
                // If SetParameter fails, then convert the parameter into a CompositionPropertySet
                // Since we cannot modify the parameters dictionary while we are inside the loop, add the key and the
                // CompositionPropertySet to the newParameters dictionary, so that the parameters dictionary can be updated 
                // once the loop completes
                newParameters[key] = CompositionPropertySetExtensions.ToPropertySet(parameters[key], animation.Compositor);
            }

            // If any key value pairs exist in the newParameters dictionary, then update the parameters dictionary
            if (newParameters.Any())
            {
                foreach (var item in newParameters)
                {
                    parameters[item.Key] = item.Value;
                    // Set item.Value as the Reference Parameter for the animation
                    animation.SetReferenceParameter(item.Key, (CompositionObject)parameters[item.Key]);
                }
                // Clean up newParameters dictionary
                newParameters.Clear();
            }

            return animation;
        }

        #endregion
    }
}
