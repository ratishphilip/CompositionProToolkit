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
