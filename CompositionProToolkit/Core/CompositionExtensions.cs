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

using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using CompositionProToolkit.Expressions;

namespace CompositionProToolkit
{
    /// <summary>
    /// Extension Methods for Compositor
    /// </summary>
    public static class CompositionExtensions
    {
        /// <summary>
        /// Creates a custom shaped Effect Brush using BackdropBrush and a Mask
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="mask">IMaskSurface</param>
        /// <param name="blendColor">Color to blend in the BackdropBrush</param>
        /// <param name="blurAmount">Blur Amount of the Backdrop Brush</param>
        /// <param name="backdropBrush">Backdrop Brush (optional). If not provided, then compositor creates it.</param>
        /// <returns>CompositionEffectBrush</returns>
        public static CompositionEffectBrush CreateMaskedBackdropBrush(this Compositor compositor, IMaskSurface mask,
            Color blendColor, float blurAmount, CompositionBackdropBrush backdropBrush = null)
        {
            // Blur Effect
            var blurEffect = new GaussianBlurEffect()
            {
                Name = "Blur",
                BlurAmount = blurAmount,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("backdrop"),
            };

            // Blend Effect
            var blendEffect = new BlendEffect
            {
                Foreground = new ColorSourceEffect
                {
                    Name = "Color",
                    Color = blendColor
                },
                Background = blurEffect,
                Mode = BlendEffectMode.Multiply
            };

            // Composite Effect
            var effect = new CompositeEffect
            {
                Mode = CanvasComposite.DestinationIn,
                Sources =
                {
                    blendEffect,
                    new CompositionEffectSourceParameter("mask")
                }

            };

            // Create Effect Factory
            var factory = compositor.CreateEffectFactory(effect, new[] { "Blur.BlurAmount", "Color.Color" });
            // Create Effect Brush
            var brush = factory.CreateBrush();

            // Set the BackDropBrush
            // If no backdrop brush is provided, create one
            brush.SetSourceParameter("backdrop", backdropBrush ?? compositor.CreateBackdropBrush());

            // Set the Mask
            // Create SurfaceBrush from CompositionMask
            var maskBrush = compositor.CreateSurfaceBrush(mask.Surface);
            brush.SetSourceParameter("mask", maskBrush);

            return brush;
        }

        /// <summary>
        /// Updates the CompositionSurfaceBrush's Stretch and Alignment options
        /// </summary>
        /// <param name="surfaceBrush">CompositionSurfaceBrush</param>
        /// <param name="stretch">Stretch mode</param>
        /// <param name="alignX">Horizontal Alignment</param>
        /// <param name="alignY">Vertical Alignment</param>
        /// <param name="alignXAnimation">The animation to use to update the horizontal alignment of the surface brush</param>
        /// <param name="alignYAnimation">The animation to use to update the vertical alignment of the surface brush</param>
        public static void UpdateSurfaceBrushOptions(this CompositionSurfaceBrush surfaceBrush, Stretch stretch, 
            AlignmentX alignX, AlignmentY alignY, ScalarKeyFrameAnimation alignXAnimation = null,
            ScalarKeyFrameAnimation alignYAnimation = null)
        {
            // Stretch Mode
            switch (stretch)
            {
                case Stretch.None:
                    surfaceBrush.Stretch = CompositionStretch.None;
                    break;
                case Stretch.Fill:
                    surfaceBrush.Stretch = CompositionStretch.Fill;
                    break;
                case Stretch.Uniform:
                    surfaceBrush.Stretch = CompositionStretch.Uniform;
                    break;
                case Stretch.UniformToFill:
                    surfaceBrush.Stretch = CompositionStretch.UniformToFill;
                    break;
            }

            // Horizontal Alignment
            var finalAlignX = surfaceBrush.HorizontalAlignmentRatio;
            switch (alignX)
            {
                case AlignmentX.Left:
                    finalAlignX = 0;
                    break;
                case AlignmentX.Center:
                    finalAlignX = 0.5f;
                    break;
                case AlignmentX.Right:
                    finalAlignX = 1;
                    break;
            }

            // If animation is available, animate to the new value
            // otherwise set it explicitly
            if (alignXAnimation == null)
            {
                surfaceBrush.HorizontalAlignmentRatio = finalAlignX;
            }
            else
            {
                alignXAnimation.InsertKeyFrame(1f, finalAlignX);
                surfaceBrush.StartAnimation(() => surfaceBrush.HorizontalAlignmentRatio, alignXAnimation);
            }

            // Vertical Alignment
            var finalAlignY = surfaceBrush.VerticalAlignmentRatio;
            switch (alignY)
            {
                case AlignmentY.Top:
                    finalAlignY = 0;
                    break;
                case AlignmentY.Center:
                    finalAlignY = 0.5f;
                    break;
                case AlignmentY.Bottom:
                    finalAlignY = 1;
                    break;
            }

            // If animation is available, animate to the new value
            // otherwise set it explicitly
            if (alignYAnimation == null)
            {
                surfaceBrush.VerticalAlignmentRatio = finalAlignY;
            }
            else
            {
                alignYAnimation.InsertKeyFrame(1f, finalAlignY);
                surfaceBrush.StartAnimation(() => surfaceBrush.VerticalAlignmentRatio, alignYAnimation);
            }
        }
    }
}
