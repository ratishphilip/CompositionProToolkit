using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
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
