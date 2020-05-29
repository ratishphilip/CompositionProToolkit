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
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FluidBannerPage : Page
    {
        public FluidBannerPage()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
            Banner.ItemsLoading += Banner_ItemsLoading;
            Banner.ItemsLoaded += Banner_ItemsLoaded;
        }

        private void Banner_ItemsLoading(object sender, EventArgs e)
        {
            LoadingIndicator.Visibility = Visibility.Visible;
        }

        private void Banner_ItemsLoaded(object sender, EventArgs e)
        {
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var itemCount = 10;

            var items = new List<Uri>();
            var urls = (await GetUrls()).Take(itemCount).ToList();
            for (var i = 0; i < itemCount; i++)
            {
                //items.Add(new Uri($"ms-appx:///Assets/Images/Image{i + 1}.jpg"));
                items.Add(new Uri(urls.ElementAt(i)));
            }

            Banner.ItemsSource = items;
        }

        private static async Task<IEnumerable<string>> GetUrls()
        {
            string xml;

            using (var client = new HttpClient())
            {
                xml = await client.GetStringAsync("http://trailers.apple.com/trailers/home/xml/current.xml");
            }

            xml = xml.Replace("\u001f", "");

            var result = new List<string>();
            try
            {
                var xDoc = XDocument.Parse(xml);
                if (xDoc == null)
                    return result;
                var movieInfos = xDoc.Root.Elements("movieinfo");
                result.AddRange(from info in movieInfos
                                select info.Element("poster")
                                        into poster
                                where poster != null
                                select poster.Element("xlarge").Value);

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
