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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
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
