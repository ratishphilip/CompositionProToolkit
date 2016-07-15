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
// CompositionProToolkit v0.3
// 

using Windows.UI;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace CompositionProToolkit
{
    /// <summary>
    /// Extension Methods for Compositor
    /// </summary>
    public static class CompositorExtensions
    {
        /// <summary>
        /// Creates a custom shaped Effect Brush using BackdropBrush and a Mask
        /// </summary>
        /// <param name="compositor">Compositor</param>
        /// <param name="mask">ICompositionMask</param>
        /// <param name="blendColor">Color to blend in the BackdropBrush</param>
        /// <param name="blurAmount">Blur Amount of the Backdrop Brush</param>
        /// <param name="backdropBrush">Backdrop Brush (optional). If not provided, then compositor creates it.</param>
        /// <returns>CompositionEffectBrush</returns>
        public static CompositionEffectBrush CreateMaskedBackdropBrush(this Compositor compositor, ICompositionMask mask,
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
    }
}
