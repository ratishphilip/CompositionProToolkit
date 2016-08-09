using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionExpressionToolkit;
using CompositionProToolkit;
using Microsoft.Graphics.Canvas;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompositionSurfaceImagePage : Page
    {
        private ICompositionSurfaceImage _surfaceImage;
        private SpriteVisual _visual;
        private Compositor compositor;
        private ICompositionGenerator generator;
        private event Action ImageLoaded;

        public CompositionSurfaceImagePage()
        {
            this.InitializeComponent();
            ImageLoaded += OnImageLoaded;

            Loaded += OnLoaded;
            SizeChanged += CompositionSurfaceImagePage_SizeChanged;
        }

        private void RunTask(Uri uri, Size size)
        {
            Task.Run(async () =>
            {
                var options = new CompositionSurfaceImageOptions(Stretch.Uniform, AlignmentX.Center, AlignmentY.Center, 1f,
                CanvasImageInterpolation.HighQualityCubic)
                {
                    SurfaceBackgroundColor = Colors.Green
                };

                _surfaceImage =
                    await generator.CreateSurfaceImageAsync(uri, size, options);

                ImageLoaded();
            });
        }

        private void OnImageLoaded()
        {
            _visual.Brush = compositor.CreateSurfaceBrush(_surfaceImage.Surface);
        }

        private async void CompositionSurfaceImagePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_surfaceImage == null)
                return;

            _visual.Size = new Vector2(this.ActualWidth.Single(), this.ActualHeight.Single());
            await _surfaceImage.ResizeAsync(new Size(this.ActualWidth, this.ActualHeight));
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            generator = CompositionGeneratorFactory.GetCompositionGenerator(compositor);

            _visual = compositor.CreateSpriteVisual();
            _visual.Size = new Vector2(this.ActualWidth.Single(), this.ActualHeight.Single());
            _visual.Offset = new Vector3();

            _visual.Brush = compositor.CreateColorBrush(Colors.Maroon);

            ElementCompositionPreview.SetElementChildVisual(this, _visual);

            RunTask(new Uri("ms-appx:///Assets/Images/Image3.jpg"), _visual.Size.ToSize());
        }
    }
}
