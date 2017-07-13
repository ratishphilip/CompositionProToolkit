using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfileControlPage : Page
    {
        private readonly List<Uri> _profiles;
        private int currentIndex;
        private int totalProfiles;
        public ProfileControlPage()
        {
            InitializeComponent();
            _profiles = new List<Uri>
            {
                new Uri("ms-appx:///Assets/Images/p1.jpg"),
                new Uri("ms-appx:///Assets/Images/p2.jpg"),
                new Uri("ms-appx:///Assets/Images/p3.jpg"),
                new Uri("ms-appx:///Assets/Images/p4.jpg"),
                new Uri("ms-appx:///Assets/Images/p5.jpg"),
                new Uri("ms-appx:///Assets/Images/p6.jpg")
            };
            currentIndex = 0;
            totalProfiles = _profiles.Count;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            profile.Source = _profiles[currentIndex];
        }

        private void OnPreviousProfile(object sender, RoutedEventArgs e)
        {
            currentIndex = (--currentIndex + totalProfiles) % totalProfiles;
            profile.Source = _profiles[currentIndex];
        }

        private void OnNextProfile(object sender, RoutedEventArgs e)
        {
            currentIndex = (++currentIndex) % totalProfiles;
            profile.Source = _profiles[currentIndex];
        }
    }
}
