using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CompositionProToolkit.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageFramePage : Page
    {
        private int count = 0;
        private List<String> _images;
        private Dictionary<int, Stretch> _stretchModes;
        private Dictionary<int, AlignmentX> _alignXModes;
        private Dictionary<int, AlignmentY> _alignYModes;
        private List<Uri> _uris;
        private int _currentIndex;

        public ImageFramePage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
            ImageFrame.ImageOpened += OnImageOpened;

            _images = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                _images.Add($"Image{i + 1}");
            }
            ImageCB.ItemsSource = _images;


            _stretchModes = new Dictionary<int, Stretch>()
            {
                [0] = Stretch.Uniform,
                [1] = Stretch.Fill,
                [2] = Stretch.UniformToFill,
                [3] = Stretch.None
            };
            StretchCB.ItemsSource = new List<String>()
            {
                "Uniform",
                "Fill",
                "UniformToFill",
                "None"
            };

            _alignXModes = new Dictionary<int, AlignmentX>()
            {
                [0] = AlignmentX.Left,
                [1] = AlignmentX.Center,
                [2] = AlignmentX.Right
            };
            AlignXCB.ItemsSource = new List<String>()
            {
                "Left",
                "Center",
                "Right"
            };

            _alignYModes = new Dictionary<int, AlignmentY>()
            {
                [0] = AlignmentY.Top,
                [1] = AlignmentY.Center,
                [2] = AlignmentY.Bottom
            };
            AlignYCB.ItemsSource = new List<String>()
            {
                "Top",
                "Center",
                "Bottom"
            };

            TransitionCB.ItemsSource = new List<string>()
            {
                "FadeIn",
                "SlideLeft",
                "SlideRight",
                "SlideUp",
                "SlideDown",
                "ZoomIn"
            };

            ImageCB.SelectedIndex = -1;
            StretchCB.SelectedIndex = -1;
            AlignXCB.SelectedIndex = -1;
            AlignYCB.SelectedIndex = -1;

            _uris = new List<Uri>()
            {
                new Uri($"ms-appx:///Assets/Images/Image1.jpg"),
                new Uri($"ms-appx:///Assets/Images/Image2.jpg"),
                new Uri($"ms-appx:///Assets/Images/Image3.jpg"),
                new Uri("http://umad.com/img/2015/6/black-and-white-cat-wallpaper-5494-5772-hd-wallpapers.jpg"),
                new Uri("http://66.media.tumblr.com/eb82497094c835e10f0a5e1b31263b01/tumblr_mtumx7AxJh1rwsyzqo1_1280.png"),
                new Uri("http://www.repairerdrivennews.com/wp-content/uploads/2016/04/tesla-model-3-provided-2016-2-1.png")
            };
        }

        private async void OnImageOpened(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //ImageTB.Text = $"IO - {++count}";
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ImageCB.SelectedIndex = 0;
            StretchCB.SelectedIndex = 0;
            AlignXCB.SelectedIndex = 1;
            AlignYCB.SelectedIndex = 1;
            TransitionCB.SelectedIndex = 0;
        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            _currentIndex = (sender as ComboBox).SelectedIndex;

            ImageFrame.Source = _uris[_currentIndex];
        }

        private void OnStretchChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StretchCB.SelectedIndex >= 0)
                ImageFrame.Stretch = _stretchModes[StretchCB.SelectedIndex];
        }

        private void OnAlignXChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlignXCB.SelectedIndex >= 0)
                ImageFrame.AlignX = _alignXModes[AlignXCB.SelectedIndex];
        }

        private void OnAlignYChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlignYCB.SelectedIndex >= 0)
                ImageFrame.AlignY = _alignYModes[AlignYCB.SelectedIndex];
        }

        private async void OnCornerRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var radius = (sender as Slider)?.Value;

            if (!radius.HasValue)
                return;

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ImageFrame.CornerRadius = new CornerRadius(radius.Value);
            });
        }

        private async void OnTransitionModeChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = (sender as ComboBox).SelectedIndex;

            if (index >= 0)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ImageFrame.TransitionMode = (TransitionModeType)index;
                });
            }
        }
    }
}
