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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ToolkitGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LaunchPage : Page
    {
        public LaunchPage()
        {
            InitializeComponent();
        }

        private void OnNavigationViewLoaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in TopLevelNav.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Home_Page")
                {
                    TopLevelNav.SelectedItem = item;
                    break;
                }
            }

            ContentFrame.Navigate(typeof(HomePage));
        }

        private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                //ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                if (args.InvokedItem is TextBlock itemContent)
                {
                    switch (itemContent.Tag)
                    {
                        case "Home_Page":
                            ContentFrame.Navigate(typeof(HomePage));
                            break;

                        case "GeometricSurface_Page":
                            ContentFrame.Navigate(typeof(GeometrySurfacePage));
                            break;

                        case "MaskSurface_Page":
                            ContentFrame.Navigate(typeof(MaskSurfacePage));
                            break;

                        case "GaussianMaskSurface_Page":
                            ContentFrame.Navigate(typeof(GaussianMaskSurfacePage));
                            break;

                        case "ImageSurface_Page":
                            ContentFrame.Navigate(typeof(ImageSurfacePage));
                            break;

                        case "ImageMaskSurface_Page":
                            ContentFrame.Navigate(typeof(ImageMaskSurfacePage));
                            break;

                        case "Squircle_Page":
                            ContentFrame.Navigate(typeof(SquirclePage));
                            break;

                        case "CompositionGeometricClip_Page":
                            ContentFrame.Navigate(typeof(CompositionGeometricClipPage));
                            break;

                        case "VisualReflection_Page":
                            ContentFrame.Navigate(typeof(VisualReflectionPage));
                            break;

                        case "Win2dML_Page":
                            ContentFrame.Navigate(typeof(CanvasGeometryParserPage));
                            break;

                        case "FluidWrapPanel_Page":
                            ContentFrame.Navigate(typeof(FluidWrapPanelPage));
                            break;

                        case "FluidToggleSwitch_Page":
                            ContentFrame.Navigate(typeof(FluidToggleSwitchPage));
                            break;

                        case "ColorShadow_Page":
                            ContentFrame.Navigate(typeof(ColorShadowPage));
                            break;

                        case "FluidBanner_Page":
                            ContentFrame.Navigate(typeof(FluidBannerPage));
                            break;

                        case "ImageFrame_Page":
                            ContentFrame.Navigate(typeof(ImageFramePage));
                            break;

                        case "FrostedGlass_Page":
                            ContentFrame.Navigate(typeof(FrostedGlassPage));
                            break;

                        case "ProfileControl_Page":
                            ContentFrame.Navigate(typeof(ProfileControlPage));
                            break;

                        case "CanvasElement_Page":
                            ContentFrame.Navigate(typeof(CanvasElementPage));
                            break;

                        case "FluidProgressRing_Page":
                            ContentFrame.Navigate(typeof(FluidProgressRingPage));
                            break;

                        case "ProgressRing3d_Page":
                            ContentFrame.Navigate(typeof(ProgressRing3dPage));
                            break;
                    }
                }
            }
        }
    }
}
