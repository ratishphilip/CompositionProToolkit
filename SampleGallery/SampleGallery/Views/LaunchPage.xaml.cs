using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LaunchPage : Page
    {
        public LaunchPage()
        {
            this.InitializeComponent();
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

                        case "CustomShapes_Page":
                            ContentFrame.Navigate(typeof(MaskSurfacePage));
                            break;

                        case "MaskedBackdropBrush_Page":
                            ContentFrame.Navigate(typeof(MaskedBackdropPage));
                            break;

                        case "FluidProgressRing_Page":
                            ContentFrame.Navigate(typeof(FluidProgressRingPage));
                            break;

                        case "FluidWrapPanel_Page":
                            ContentFrame.Navigate(typeof(FluidWrapPanelPage));
                            break;

                        case "ImageFrame_Page":
                            ContentFrame.Navigate(typeof(ImageFramePage));
                            break;

                        case "VisualReflection_Page":
                            ContentFrame.Navigate(typeof(VisualReflectionPage));
                            break;

                        case "FluidBanner_Page":
                            ContentFrame.Navigate(typeof(FluidBannerPage));
                            break;

                        case "Win2dML_Page":
                            ContentFrame.Navigate(typeof(CanvasGeometryParserPage));
                            break;

                        case "CanvasElement_Page":
                            ContentFrame.Navigate(typeof(CanvasElementPage));
                            break;

                        case "FluidToggleSwitch_Page":
                            ContentFrame.Navigate(typeof(FluidToggleSwitchPage));
                            break;

                        case "ProfileControl_Page":
                            ContentFrame.Navigate(typeof(ProfileControlPage));
                            break;

                        case "ProgressRing3d_Page":
                            ContentFrame.Navigate(typeof(ProgressRing3dPage));
                            break;

                        case "FrostedGlass_Page":
                            ContentFrame.Navigate(typeof(FrostedGlassPage));
                            break;

                        case "CompositionGeometricClip_Page":
                            ContentFrame.Navigate(typeof(CompositionGeometricClipPage));
                            break;
                    }
                }
            }
        }
    }
}
