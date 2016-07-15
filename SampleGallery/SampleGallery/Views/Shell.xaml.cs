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
using SampleGallery.Controls;
using Template10.Controls;
using Template10.Services.NavigationService;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.MyHamburgerMenu;

        public Shell()
        {
            Instance = this;
            InitializeComponent();
            Instance.MyHamburgerMenu.SelectedChanged += MyHamburgerMenu_SelectedChanged;
        }

        private void MyHamburgerMenu_SelectedChanged(object sender, Template10.Common.ChangedEventArgs<HamburgerButtonInfo> e)
        {
            SetGlyphForeground(e.OldValue, MyHamburgerMenu.NavButtonForeground);
            SetGlyphForeground(e.NewValue, MyHamburgerMenu.NavButtonCheckedForeground);
        }

        private static void SetGlyphForeground(HamburgerButtonInfo info, Brush brush)
        {
            var stack = info?.Content as StackPanel;

            if (stack == null)
                return;

            var icon = stack.Children.OfType<GlyphIcon>().FirstOrDefault();
            if (icon != null)
            {
                icon.Foreground = brush;
            }
            var text = stack.Children.OfType<TextBlock>().FirstOrDefault();
            if (text != null)
            {
                text.Foreground = brush;
            }
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            MyHamburgerMenu.NavigationService = navigationService;
        }

        private void MyHamburgerMenu_OnPaneClosed(object sender, EventArgs e)
        {
        }
    }
}
