// Copyright (c) Ratish Philip 
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
// CompositionProToolkit v1.0.1
// 

using CompositionProToolkit;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Numerics;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VisualReflectionPage : Page
    {
        private Compositor _compositor;
        private ICompositionGenerator _generator;

        public VisualReflectionPage()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _generator = _compositor.CreateCompositionGenerator();

            var distance = 4f;
            var visualSize = new Vector2(225, 150);
            var vRefHeight = visualSize.Y + (visualSize.Y * 0.7f) + distance;
            var hRefWidth = visualSize.X + (visualSize.X * 0.7f) + distance;

            var vRefLeft = (float)(CompositionGrid1.ActualWidth - visualSize.X) / 2f;
            var vRefTop = (float)(CompositionGrid1.ActualHeight - vRefHeight) / 2f;
            var hRefLeft = (float)(CompositionGrid1.ActualWidth - hRefWidth) / 2f;
            var hRefTop = (float)(CompositionGrid1.ActualHeight - visualSize.Y) / 2f;

            var surfaceImage = await _generator.CreateImageSurfaceAsync(new Uri("ms-appx:///Assets/Images/Car.jpg"),
                visualSize.ToSize(), ImageSurfaceOptions.Default);
            var surfaceBrush = _compositor.CreateSurfaceBrush(surfaceImage.Surface);

            var visual1 = _compositor.CreateSpriteVisual();
            visual1.Size = visualSize;
            visual1.Offset = new Vector3(vRefLeft, vRefTop, 0);
            visual1.Brush = surfaceBrush;

            _generator.CreateReflection(visual1, distance);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid1, visual1);

            var invertEffect = new InvertEffect()
            {
                Source = new CompositionEffectSourceParameter("source")
            };

            var invertEffectFactory = _compositor.CreateEffectFactory((IGraphicsEffect) invertEffect);
            var invertBrush = invertEffectFactory.CreateBrush();
            invertBrush.SetSourceParameter("source", surfaceBrush);

            var visual2 = _compositor.CreateSpriteVisual();
            visual2.Size = visualSize;
            visual2.Offset = new Vector3(hRefLeft, hRefTop, 0);
            visual2.Brush = invertBrush;

            _generator.CreateReflection(visual2, distance, 0.7f, ReflectionLocation.Right);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid2, visual2);

            var visual3 = _compositor.CreateSpriteVisual();
            visual3.Size = visualSize;
            visual3.Offset = new Vector3(vRefLeft, vRefTop, 0);
            visual3.Brush = surfaceBrush;

            var inVisual = _compositor.CreateSpriteVisual();
            inVisual.Size = new Vector2(80, 80);
            inVisual.Offset = new Vector3(20, 10, 0);
            inVisual.Brush = _compositor.CreateColorBrush(Colors.Yellow);

            var inVisual2 = _compositor.CreateSpriteVisual();
            inVisual2.Size = new Vector2(80, 80);
            inVisual2.Offset = new Vector3(60, 60, 0);
            inVisual2.Brush = _compositor.CreateColorBrush(Colors.Blue);
            visual3.Children.InsertAtTop(inVisual2);
            visual3.Children.InsertAtTop(inVisual);

            _generator.CreateReflection(visual3, distance, 0.7f, ReflectionLocation.Bottom);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid3, visual3);

            var visual4 = _compositor.CreateSpriteVisual();
            visual4.Size = visualSize;
            visual4.Offset = new Vector3(hRefLeft, hRefTop, 0);
            visual4.Brush = surfaceBrush;

            var inVisual3 = _compositor.CreateSpriteVisual();
            inVisual3.Size = new Vector2(80, 80);
            inVisual3.Offset = new Vector3(20, 10, 0);
            inVisual3.Brush = _compositor.CreateColorBrush(Colors.Yellow);

            var inVisual4 = _compositor.CreateSpriteVisual();
            inVisual4.Size = new Vector2(80, 80);
            inVisual4.Offset = new Vector3(60, 40, 0);
            inVisual4.Brush = _compositor.CreateColorBrush(Colors.Blue);
            visual4.Children.InsertAtTop(inVisual3);
            visual4.Children.InsertAtTop(inVisual4);


            _generator.CreateReflection(visual4, distance, 0.7f, ReflectionLocation.Right);

            ElementCompositionPreview.SetElementChildVisual(CompositionGrid4, visual4);

        }
    }
}
