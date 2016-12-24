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
using System.Linq;
using System.Linq.Expressions;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI.Composition;

namespace CompositionProToolkit.Expressions
{
    /// <summary>
    /// Extension methods for Windows.UI.Composition.Compositor
    /// </summary>
    public static class CompositorExtensions
    {
        /// <summary>
        /// Creates an ExpressionAnimation based on the given CompositionLambda Expression
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <param name="expression">CompositionLambda Expression</param>
        /// <returns>ExpressionAnimation</returns>
        public static ExpressionAnimation CreateExpressionAnimation<T>(this Compositor compositor,
            Expression<CompositionLambda<T>> expression)
        {
            var result = CompositionExpressionEngine.CreateCompositionExpression(expression);
            var animation = compositor.CreateExpressionAnimation();
            animation.Expression = result.Expression;
            animation.SetParameters(result.Parameters);

            return animation;
        }

        /// <summary>
        /// Creates an instance of CompositionEffectFactory.
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="graphicsEffect">The type of effect to create.</param>
        /// <param name="animatablePropertyExpressions">List of Expression each specifying 
        /// an animatable property</param>
        /// <returns>The created CompositionEffectFactory object.</returns>
        public static CompositionEffectFactory CreateEffectFactory(this Compositor compositor,
            IGraphicsEffect graphicsEffect, params Expression<Func<object>>[] animatablePropertyExpressions)
        {
            var animatableProperties = animatablePropertyExpressions.Select(CompositionExpressionEngine.ParseExpression).ToArray();

            return compositor.CreateEffectFactory(graphicsEffect, animatableProperties);
        }

        /// <summary>
        /// Creates an instance of KeyFrameAnimation&lt;T&gt;
        /// </summary>
        /// <typeparam name="T">Type of the encapsulated KeyFrameAnimation</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>KeyFrameAnimation&lt;T&gt;</returns>
        public static KeyFrameAnimation<T> CreateKeyFrameAnimation<T>(this Compositor compositor)
        {
            return new KeyFrameAnimation<T>(KeyFrameAnimationHelper.CreateAnimation<T>(compositor));
        }

        /// <summary>
        /// Creates a CompositionLambda expression for 'c =&gt; c.StartingValue' for the given type
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>Expression&lt;CompositionLambda&lt;T&gt;&gt;</returns>
        public static Expression<CompositionLambda<T>> CreateStartingValueExpression<T>(this Compositor compositor)
        {
            Expression<CompositionLambda<T>> expression = c => c.StartingValue;
            return expression;
        }

        /// <summary>
        /// Creates a CompositionLambda expression for 'c =&gt; c.FinalValue' for the given type
        /// </summary>
        /// <typeparam name="T">Type of the CompositionLambda Expression</typeparam>
        /// <param name="compositor">Compositor</param>
        /// <returns>Expression&lt;CompositionLambda&lt;T&gt;&gt;</returns>
        public static Expression<CompositionLambda<T>> CreateFinalValueExpression<T>(this Compositor compositor)
        {
            Expression<CompositionLambda<T>> expression = c => c.FinalValue;
            return expression;
        }

        /// <summary>
        /// This extension method creates a scoped batch and handles the completed event
        /// the subscribing and unsubscribing process internally.
        /// 
        /// Example usage:
        /// _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
        ///        () => // Action
        ///        {
        ///            transitionVisual.StartAnimation("Scale.XY", _scaleUpAnimation);
        ///        },
        ///        () => // Post Action
        ///        {
        ///            BackBtn.IsEnabled = true;
        ///        });
        /// 
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="batchType">Composition Batch Type</param>
        /// <param name="action">Action to perform within the scoped batch</param>
        /// <param name="postAction">Action to perform once the batch completes</param>
        public static void CreateScopedBatch(this Compositor compositor, CompositionBatchTypes batchType, Action action, Action postAction = null)
        {
            if (action == null)
                throw new ArgumentException("Cannot create a scoped batch on an action with null value!", nameof(action));

            // Create ScopedBatch
            var batch = compositor.CreateScopedBatch(batchType);

            // Handler for the Completed Event
            TypedEventHandler<object, CompositionBatchCompletedEventArgs> handler = null;
            handler = (s, ea) =>
            {
                var scopedBatch = s as CompositionScopedBatch;

                // Unsubscribe the handler from the Completed event
                if (scopedBatch != null)
                {
                    scopedBatch.Completed -= handler;
                }

                try
                {
                    // Invoke the post action
                    postAction?.Invoke();
                }
                finally
                {
                    scopedBatch?.Dispose();
                }
            };

            // Subscribe to the Completed event
            batch.Completed += handler;

            // Invoke the action
            action();

            // End Batch
            batch.End();
        }

        /// <summary>
        /// This extension method creates a scoped batch and handles the completed event
        /// the subscribing and unsubscribing process internally.
        /// 
        /// Example usage:
        /// _compositor.CreateScopedBatch(CompositionBatchTypes.Animation,
        ///        (batch) => // Action
        ///        {
        ///            transitionVisual.StartAnimation("Scale.XY", _scaleUpAnimation);
        ///        },
        ///        (batch) => // Post Action
        ///        {
        ///            BackBtn.IsEnabled = true;
        ///        });
        /// 
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="batchType">Composition Batch Type</param>
        /// <param name="action">Action to perform within the scoped batch</param>
        /// <param name="postAction">Action to perform once the batch completes</param>
        public static void CreateScopedBatch(this Compositor compositor, CompositionBatchTypes batchType, 
            Action<CompositionScopedBatch> action, Action<CompositionScopedBatch> postAction = null)
        {
            if (action == null)
                throw new ArgumentException("Cannot create a scoped batch on an action with null value!", nameof(action));

            // Create ScopedBatch
            var batch = compositor.CreateScopedBatch(batchType);

            // Handler for the Completed Event
            TypedEventHandler<object, CompositionBatchCompletedEventArgs> handler = null;
            handler = (s, ea) =>
            {
                var scopedBatch = s as CompositionScopedBatch;

                // Unsubscribe the handler from the Completed event
                if (scopedBatch != null)
                {
                    scopedBatch.Completed -= handler;
                }

                try
                {
                    // Invoke the post action
                    postAction?.Invoke(scopedBatch);
                }
                finally
                {
                    scopedBatch?.Dispose();
                }
            };

            // Subscribe to the Completed event
            batch.Completed += handler;

            // Invoke the action
            action(batch);

            // End Batch
            batch.End();
        }
    }
}
