using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Path = Windows.UI.Xaml.Shapes.Path;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SampleGallery.Controls
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
            this.InitializeComponent();
        }

        private static Geometry PathMarkupToGeometry(string pathMarkup)
        {
            try
            {
                var xaml = "<Path " + "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>" +
                "<Path.Data>" + pathMarkup + "</Path.Data></Path>";
                var path = XamlReader.Load(xaml) as Path;
                // Detach the PathGeometry from the Path
                if (path != null)
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
