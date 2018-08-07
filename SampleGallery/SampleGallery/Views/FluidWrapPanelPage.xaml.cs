using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SampleGallery.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FluidWrapPanelPage : Page
    {
        private Random _random = new Random();
        private Brush[] _brushes;
        private int count;

        #region UseRandomChildSize

        /// <summary>
        /// UseRandomChildSize Dependency Property
        /// </summary>
        public static readonly DependencyProperty UseRandomChildSizeProperty =
            DependencyProperty.Register("UseRandomChildSize", typeof(bool), typeof(FluidWrapPanelPage),
                new PropertyMetadata(true, OnRandomChildSizeChanged));

        private static void OnRandomChildSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as FluidWrapPanelPage;
            window?.RefreshFluidWrapPanel();
        }

        /// <summary>
        /// Gets or sets the UseRandomChildSize property. This dependency property 
        /// indicates whether the children should be of different size or same size.
        /// </summary>
        public bool UseRandomChildSize
        {
            get { return (bool)GetValue(UseRandomChildSizeProperty); }
            set { SetValue(UseRandomChildSizeProperty, value); }
        }

        #endregion

        public FluidWrapPanelPage()
        {
            this.InitializeComponent();

            _brushes = new Brush[] {
                                        new SolidColorBrush(Color.FromArgb(255, 76, 217, 100)),
                                        new SolidColorBrush(Color.FromArgb(255, 0, 122, 255)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 150, 0)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 45, 85)),
                                        new SolidColorBrush(Color.FromArgb(255, 88, 86, 214)),
                                        new SolidColorBrush(Color.FromArgb(255, 255, 204, 0)),
                                        new SolidColorBrush(Color.FromArgb(255, 142, 142, 147)),
                                      };

            Loaded += MainWindow_Loaded;
            count = 0;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            OrientationCB.ItemsSource = new List<string> { "Horizontal", "Vertical" };
            OrientationCB.SelectedIndex = 0;
            RefreshFluidWrapPanel();
        }

        private void RefreshFluidWrapPanel()
        {
            count = 0;
            var items = new ObservableCollection<UIElement>();
            var maxCount = _random.Next(15, 20);
            for (var i = 0; i < maxCount; i++)
            {
                var brush = _brushes[_random.Next(_brushes.Length)];
                var factorWidth = UseRandomChildSize ? _random.Next(1, 3) : 1;
                var factorHeight = UseRandomChildSize ? _random.Next(1, 3) : 1;

                var ctrl = new FluidItemControl
                {
                    Width = factorWidth * panel.ItemWidth,
                    Height = factorHeight * panel.ItemHeight,
                    Fill = brush,
                    Data = (++count).ToString()
                };

                items.Add(ctrl);
            }

            panel.ItemsSource = items;
        }

        private void OnOrientationChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (OrientationCB.SelectedValue as string)
            {
                case "Horizontal":
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    panel.Orientation = Orientation.Horizontal;
                    break;
                case "Vertical":
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    panel.Orientation = Orientation.Vertical;
                    break;
            }
        }

        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            RefreshFluidWrapPanel();
        }

        private async void OnAdd(object sender, RoutedEventArgs e)
        {
            var brush = _brushes[_random.Next(_brushes.Length)];
            //var factor = 1;
            var factorWidth = UseRandomChildSize ? _random.Next(1, 3) : 1;
            var factorHeight = UseRandomChildSize ? _random.Next(1, 3) : 1;

            var ctrl = new FluidItemControl
            {
                Width = factorWidth * panel.ItemWidth,
                Height = factorHeight * panel.ItemHeight,
                Fill = brush,
                Data = (++count).ToString()
            };

            await panel.AddChildAsync(ctrl);
        }
    }
}
