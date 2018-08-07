using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CompositionProToolkit.Win2d;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using CompositionProToolkit;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CanvasElementPage : Page
    {
        private CanvasRenderLayer _layer1;
        private CanvasRenderLayer _layer2;
        private CanvasRenderLayer _layer3;
        private CanvasRenderLayer _layer4;
        private CanvasRenderLayer _layer5;

        private CanvasElement _element;

        private bool _isInitialized;

        public CanvasElementPage()
        {
            this.InitializeComponent();
        }

        private void OnCanvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            var geom1 = CanvasObject.CreateGeometry(sender, "O 116 116 128 128");
            var fill1 = CanvasObject.CreateBrush(sender, "SC #00adef");
            var stroke1 = CanvasObject.CreateStroke(sender, "ST 8 SC #2a388f");
            _layer1 = new CanvasRenderLayer(geom1, fill1, stroke1);

            var geom2 = CanvasObject.CreateGeometry(sender, "U 56 56 64 64 8 8");
            var fill2 = CanvasObject.CreateBrush(sender, "SC #ed1c24");
            var stroke2 = CanvasObject.CreateStroke(sender, "ST 2 SC #404041");
            _layer2 = new CanvasRenderLayer(geom2, fill2, stroke2);

            var geom3 = CanvasObject.CreateGeometry(sender, "U 136 56 64 64 8 8");
            var fill3 = CanvasObject.CreateBrush(sender, "SC #38b449");
            _layer3 = new CanvasRenderLayer(geom3, fill3, stroke2);

            var geom4 = CanvasObject.CreateGeometry(sender, "U 56 136 64 64 8 8");
            var fill4 = CanvasObject.CreateBrush(sender, "SC #fff100");
            _layer4 = new CanvasRenderLayer(geom4, fill4, stroke2);

            var geom5 = CanvasObject.CreateGeometry(sender, "R 96 96 64 64");
            var fill5 = CanvasObject.CreateBrush(sender, "SC #f7931d");
            _layer5 = new CanvasRenderLayer(geom5, fill5, stroke2);

            var layers = new List<CanvasRenderLayer> { _layer1, _layer2, _layer3, _layer4, _layer5 };
            _element = new CanvasElement(256f, 256f, layers);
            _isInitialized = true;
        }

        private void OnCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            _element?.Render(args.DrawingSession, CanvasCtrl.Width.ToSingle(), CanvasCtrl.Height.ToSingle(), new Vector2(0, 0), new Vector4(10), 0f);
        }

        private void OnLayerToggled(Object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            var layers = new List<ICanvasRenderLayer>();

            if (Layer1TB.IsOn)
                layers.Add(_layer1);
            if (Layer2TB.IsOn)
                layers.Add(_layer2);
            if (Layer3TB.IsOn)
                layers.Add(_layer3);
            if (Layer4TB.IsOn)
                layers.Add(_layer4);
            if (Layer5TB.IsOn)
                layers.Add(_layer5);

            _element.Layers = layers;
            CanvasCtrl.Invalidate();
        }

        private void CanvasElementPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            CanvasCtrl.Width = Math.Max(40, e.NewSize.Width.ToSingle());
            CanvasCtrl.Height = Math.Max(40, e.NewSize.Height.ToSingle() - 240);

            CanvasCtrl.Invalidate();
        }
    }
}
