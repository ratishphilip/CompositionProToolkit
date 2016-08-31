// Copyright (c) 2016 Ratish Philip 
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
// CompositionProToolkit v0.4.5
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Windows.UI;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions
{
    /// <summary>
    /// Class which encapsulates KeyFrame data
    /// </summary>
    /// <typeparam name="T">Type of the KeyFrameAnimation</typeparam>
    public sealed class KeyFrame<T>
    {
        #region Properties

        public float Key { get; set; }
        public T Value { get; set; }
        public CompositionEasingFunction Easing { get; set; }

        #endregion

        #region Construction / Initialization

        public KeyFrame(float normalizedProgressKey, T value, CompositionEasingFunction easing = null)
        {
            Key = normalizedProgressKey;
            Value = value;
            Easing = easing;
        }

        #endregion
    }

    /// <summary>
    /// Helper class for KeyFrameAnimation&lt;T&gt;
    /// </summary>
    internal static class KeyFrameAnimationHelper
    {
        #region Fields

        private static readonly Dictionary<Type, Type> AnimationTypes;
        private static readonly Dictionary<Type, MethodInfo> InitMethods;

        #endregion

        #region Public Field(s)

        public static readonly Dictionary<Type, MethodInfo> InsertMethods;

        #endregion

        #region Construction / Initialization

        static KeyFrameAnimationHelper()
        {
            AnimationTypes = new Dictionary<Type, Type>()
            {
                [typeof(Color)] = typeof(ColorKeyFrameAnimation),
                [typeof(Quaternion)] = typeof(QuaternionKeyFrameAnimation),
                [typeof(float)] = typeof(ScalarKeyFrameAnimation),
                [typeof(Vector2)] = typeof(Vector2KeyFrameAnimation),
                [typeof(Vector3)] = typeof(Vector3KeyFrameAnimation),
                [typeof(Vector4)] = typeof(Vector4KeyFrameAnimation)
            };

            // Get all 
            InitMethods = typeof(Compositor).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                      .Where(m => m.Name.StartsWith("Create") && m.Name.EndsWith("KeyFrameAnimation"))
                                      .ToDictionary(m => m.ReturnType, m => m);

            InsertMethods = new Dictionary<Type, MethodInfo>();

            foreach (var animationType in AnimationTypes)
            {
                // Get the InsertKeyFrame method (of the KeyFrameAnimation matching 
                // the animationType.Key) which take 3 parameters
                var methodInfo = animationType.Value.GetMethods(BindingFlags.Public |
                                                                BindingFlags.Instance |
                                                                BindingFlags.Static)
                                            .FirstOrDefault(m => m.Name.Equals("InsertKeyFrame") &&
                                                                 (m.GetParameters().Length == 3));

                InsertMethods[animationType.Key] = methodInfo;
            }
        }

        #endregion

        #region Internal APIs

        internal static KeyFrameAnimation CreateAnimation<T>(Compositor compositor)
        {
            if (compositor == null)
            {
                throw new ArgumentNullException(nameof(compositor));
            }

            var animationType = AnimationTypes[typeof(T)];
            if (InitMethods.ContainsKey(animationType))
            {
                return (KeyFrameAnimation)InitMethods[animationType].Invoke(compositor, null);
            }

            return null;
        }

        #endregion
    }

    /// <summary>
    /// Generic class which encapsulates a KeyFrameAnimation
    /// </summary>
    /// <typeparam name="T">Type of the KeyFrameAnimation</typeparam>
    public sealed class KeyFrameAnimation<T>
    {
        #region Properties

        /// <summary>
        /// The encapsulated KeyFrameAnimation object
        /// </summary>
        public KeyFrameAnimation Animation { get; }

        /// <summary>
        /// The duration by which the animation should be delayed
        /// </summary>
        public TimeSpan DelayTime
        {
            get { return Animation.DelayTime; }
            set { Animation.DelayTime = value; }
        }

        /// <summary>
        /// Direction of the Animation
        /// </summary>
        public AnimationDirection Direction
        {
            get { return Animation.Direction; }
            set { Animation.Direction = value; }
        }

        /// <summary>
        /// The duration of the animation. Minimum allowed value is 1ms and maximum allowed value is 24 days.
        /// </summary>
        public TimeSpan Duration
        {
            get { return Animation.Duration; }
            set { Animation.Duration = value; }
        }

        /// <summary>
        /// The iteration behavior for the key frame animation.
        /// </summary>
        public AnimationIterationBehavior IterationBehavior
        {
            get { return Animation.IterationBehavior; }
            set { Animation.IterationBehavior = value; }
        }

        /// <summary>
        /// The number of times to repeat the key frame animation. 
        /// A value of -1 causes the animation to repeat indefinitely.
        /// </summary>
        public int IterationCount
        {
            get { return Animation.IterationCount; }
            set { Animation.IterationCount = value; }
        }

        /// <summary>
        /// The number of key frames in the KeyFrameAnimation.
        /// </summary>
        public int KeyFrameCount => Animation.KeyFrameCount;

        /// <summary>
        /// Specifies how to set the property value when StopAnimation is called.
        /// </summary>
        public AnimationStopBehavior StopBehavior
        {
            get { return Animation.StopBehavior; }
            set { Animation.StopBehavior = value; }
        }

        /// <summary>
        /// Specifies the target for the animation
        /// </summary>
        public string Target
        {
            get { return Animation.Target; }
            set { Animation.Target = value; }
        }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="animation">The KeyFrameAnimation to encapsulate.</param>
        public KeyFrameAnimation(KeyFrameAnimation animation)
        {
            Animation = animation;
        }

        #endregion

        #region APIs

        /// <summary>
        /// Inserts a KeyFrame in the encapsulated KeyFrameAnimation object
        /// </summary>
        /// <param name="normalizedProgressKey">The time the key frame should occur at, expressed as 
        /// a percentage of the animation Duration. Allowed value is from 0.0 to 1.0.</param>
        /// <param name="value">The expression used to calculate the value of the KeyFrame.</param>
        /// <param name="easingFunction">The easing function to use when interpolating between frames.</param>
        public void InsertKeyFrame(float normalizedProgressKey, T value, CompositionEasingFunction easingFunction = null)
        {
            var type = typeof(T);
            // Invoke the appropriate InsertKeyFrame method for the encapsulated KeyFrameAnimation object
            if (KeyFrameAnimationHelper.InsertMethods.ContainsKey(type) && KeyFrameAnimationHelper.InsertMethods[type] != null)
            {
                KeyFrameAnimationHelper.InsertMethods[type].Invoke(Animation, new object[] {
                                                                                               normalizedProgressKey,
                                                                                               value,
                                                                                               easingFunction
                                                                                           });
            }
        }

        /// <summary>
        /// Inserts a KeyFrame in the encapsulated KeyFrameAnimation object
        /// </summary>
        /// <param name="keyFrame">KeyFrame&lt;T&gt; object encapsulating the normalizedProgressKey, the
        /// value of the KeyFrame and the Easing function></param>
        public void InsertKeyFrame(KeyFrame<T> keyFrame)
        {
            InsertKeyFrame(keyFrame.Key, keyFrame.Value, keyFrame.Easing);
        }

        /// <summary>
        /// Inserts a collection of KeyFrame&lt;T&gt; objects in the encapsulated KeyFrameAnimation object
        /// </summary>
        /// <param name="keyFrames">Array of KeyFrame&lt;T&gt; objects</param>
        public void InsertKeyFrames(params KeyFrame<T>[] keyFrames)
        {
            foreach (var keyFrame in keyFrames.Where(k => k != null))
            {
                InsertKeyFrame(keyFrame.Key, keyFrame.Value, keyFrame.Easing);
            }
        }

        /// <summary>
        /// Inserts a KeyFrame in the KeyFrameAnimation by converting
        /// the given Expression to appropriate string
        /// </summary>
        /// <param name="normalizedProgressKey">The time the key frame should occur at, expressed as 
        /// a percentage of the animation Duration. Allowed value is from 0.0 to 1.0.</param>
        /// <param name="expression">The expression which has to be converted to string to calculate the value of the KeyFrame.</param>
        /// <param name="easingFunction">The easing function to use when interpolating between frames.</param>
        public void InsertExpressionKeyFrame(float normalizedProgressKey, Expression<CompositionLambda<T>> expression,
            CompositionEasingFunction easingFunction = null)
        {
            Animation.InsertExpressionKeyFrame(normalizedProgressKey, expression, easingFunction);
        }

        #endregion

        #region Chaining APIs

        /// <summary>
        /// Sets the Animation Direction of encapsulated KeyFrameAnimation object.
        /// </summary>
        /// <param name="direction">Animation Direction</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> InTheDirection(AnimationDirection direction)
        {
            Animation.Direction = direction;
            return this;
        }

        /// <summary>
        /// Sets the duration by which the animation, defined by
        /// the encapsulated KeyFrameAnimation object, should be delayed.
        /// </summary>
        /// <param name="delayTime">Delay Duration</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> DelayBy(TimeSpan delayTime)
        {
            Animation.DelayTime = delayTime;
            return this;
        }

        /// <summary>
        /// Sets the animation Duration of encapsulated KeyFrameAnimation object.
        /// </summary>
        /// <param name="duration">Duration</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> HavingDuration(TimeSpan duration)
        {
            Animation.Duration = duration;
            return this;
        }

        /// <summary>
        /// Sets the number of times the animation, defined by the
        /// encapsulated KeyFrameAnimation object, should be repeated.
        /// </summary>
        /// <param name="count">Number of repetitions</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> Repeats(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Repetition count should have a value greater than or equal to 0. " + 
                                                                     "Use RepeatsForever() method to repeat the animation indefinitely.");
            Animation.IterationBehavior = AnimationIterationBehavior.Count;
            Animation.IterationCount = count;
            return this;
        }

        /// <summary>
        /// Causes the animation defined by the encapsulated 
        /// KeyFrameAnimation object to repeat indefinitely
        /// </summary>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> RepeatsForever()
        {
            Animation.IterationBehavior = AnimationIterationBehavior.Forever;
            Animation.IterationCount = -1;
            return this;
        }

        /// <summary>
        /// Specifies how to set the property value when StopAnimation 
        /// is called on the encapsulated KeyFrameAnimation object.
        /// </summary>
        /// <param name="stopBehavior">AnimationStopBehavior</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> OnStop(AnimationStopBehavior stopBehavior)
        {
            Animation.StopBehavior = stopBehavior;
            return this;
        }

        /// <summary>
        /// Sets the target for the encapsulated KeyFrameAnimation object
        /// </summary>
        /// <param name="targetExpression">Expression for the target</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public KeyFrameAnimation<T> ForTarget(Expression<Func<object>> targetExpression)
        {
            Animation.Target = CompositionExpressionEngine.ParseExpression(targetExpression);
            return this;
        }

        #endregion
    }
}
