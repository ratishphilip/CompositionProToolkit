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
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Path = Windows.UI.Xaml.Shapes.Path;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ToolkitGallery.Controls
{
    public sealed partial class GlyphIcon : UserControl
    {
        #region Glyph

        /// <summary>
        /// Glyph Dependency Property
        /// </summary>
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.Register("Glyph", typeof(string), typeof(GlyphIcon),
                new PropertyMetadata(string.Empty, OnGlyphChanged));

        /// <summary>
        /// Gets or sets the Glyph property. This dependency property 
        /// indicates the name of the Glyph to be set for the GlyphIcon.
        /// </summary>
        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Glyph property.
        /// </summary>
        /// <param name="d">GlyphIcon</param>
		/// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = (GlyphIcon)d;
            var oldGlyph = (string)e.OldValue;
            var newGlyph = icon.Glyph;
            icon.OnGlyphChanged(oldGlyph, newGlyph);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Glyph property.
        /// </summary>
		/// <param name="oldGlyph">Old Value</param>
		/// <param name="newGlyph">New Value</param>
        void OnGlyphChanged(string oldGlyph, string newGlyph)
        {
            ApplyGlyph(newGlyph);
        }

        private void ApplyGlyph(string glyph)
        {
            if (String.IsNullOrWhiteSpace(glyph))
                return;

            try
            {
                //int count = Application.Current.Resources.Count;
                //foreach (var resource in Application.Current.Resources)
                //{
                //    var k = resource.Key;
                //}

                //if (Application.Current.Resources.ContainsKey(glyph))
                //{
                //    // Check if the Glyph is defined in the Application Resources
                //    var data = Application.Current.Resources[glyph] as string;
                //    // Try to set the path data with the path data string obtained
                //    GlyphPath.SetValue(Path.DataProperty, data);
                //}

                //if (Application.Current.Resources.MergedDictionaries.Any())
                //{
                //    var rd = Application.Current.Resources.MergedDictionaries[1];
                //    if (rd == null)
                //        return;

                //    foreach (var key in rd)
                //    {
                //        var val = rd[key];
                //    }

                //    if (rd.ContainsKey(glyph))
                //    {
                //        // Check if the Glyph is defined in the Application Resources
                //        var data = rd[glyph] as string;
                //        // Try to set the path data with the path data string obtained
                //        GlyphPath.SetValue(Path.DataProperty, data);
                //    }
                //}
                //GlyphPath.SetValue(Path.DataProperty, glyph);
                GlyphPath.Data = PathMarkupToGeometry(glyph);

            }
            catch (Exception e)
            {
                var msg = e.Data;
            }
        }

        #endregion

        public GlyphIcon()
        {
            InitializeComponent();
        }

        private static Geometry PathMarkupToGeometry(string pathMarkup)
        {
            try
            {
                var xaml = "<Path " + "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Path.Data>" + pathMarkup + "</Path.Data></Path>";
                // Detach the PathGeometry from the Path
                if (XamlReader.Load(xaml) is Path path)
                {
                    Geometry geometry = path.Data;
                    path.Data = null;
                    return geometry;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }
    }
}
