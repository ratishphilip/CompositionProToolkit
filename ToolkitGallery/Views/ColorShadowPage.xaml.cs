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

using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ColorShadowPage : Page
    {
        private Dictionary<string, Uri> _images;
        private Dictionary<string, Color> _shadowColors;

        public ColorShadowPage()
        {
            this.InitializeComponent();
            _images = new Dictionary<string, Uri>
            {
                {"Image1", new Uri("ms-appx:///Assets/Images/RainbowRose.png")},
                {"Image2", new Uri("ms-appx:///Assets/Images/Flower.png")},
                {"Image3", new Uri("ms-appx:///Assets/Images/Autumn.jpg")},
                {"Image4", new Uri("ms-appx:///Assets/Images/ColorWheel.png")},
                {"Image5", new Uri("ms-appx:///Assets/Images/HaltAndCatchFire.png")},
                {"Image6", new Uri("ms-appx:///Assets/Images/Flowers_Wide.jpg")},
                {"Image7", new Uri("ms-appx:///Assets/Images/Alienist.jpg")},
            };

            ImageList.ItemsSource = _images.Keys.ToList();

            _shadowColors = new Dictionary<string, Color>
            {
                {"Black", Colors.Black},
                {"Gray", Colors.Gray},
                {"Yellow", Colors.Yellow},
                {"Blue", Colors.SlateBlue},
                {"Magenta", Colors.Magenta},
                {"Red", Colors.Red},
            };

            ShadowColorList.ItemsSource = _shadowColors.Keys.ToList();
        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            var selValue = ImageList.SelectedValue as string;
            if (!String.IsNullOrWhiteSpace(selValue) && _images.ContainsKey(selValue))
            {
                ColorShadowCtrl.ImageUri = _images[selValue];
            }
        }

        private void ColorShadowPaddingValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var padding = CSPaddingSlider?.Value ?? 0;
            ColorShadowCtrl.ColorShadowPadding = new Thickness(padding);
        }

        private void ColorMaskPaddingValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ColorShadowCtrl.ColorMaskPadding = new Thickness(MaskPaddingSlider?.Value ?? 0);
        }

        private void OnShadowToggled(object sender, RoutedEventArgs e)
        {
            ShadowStack.Visibility = ShadowSwitch.IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnShadowColorChanged(object sender, SelectionChangedEventArgs e)
        {
            var selColor = ShadowColorList.SelectedValue as string;

            if (!String.IsNullOrWhiteSpace(selColor) && _shadowColors.ContainsKey(selColor))
            {
                ColorShadowCtrl.ShadowColor = _shadowColors[selColor];
            }
        }
    }
}
