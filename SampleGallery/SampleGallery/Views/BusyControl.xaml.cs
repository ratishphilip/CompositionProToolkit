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
using Template10.Common;
using Template10.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SampleGallery.Views
{
    public sealed partial class BusyControl : UserControl
    {
        public BusyControl()
        {
            this.InitializeComponent();
        }

        public string BusyText
        {
            get { return (string)GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }
        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.Register(nameof(BusyText), typeof(string), typeof(BusyControl), new PropertyMetadata("Please wait..."));

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(BusyControl), new PropertyMetadata(false));

        // hide and show busy dialog
        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null)
                    return;
                var view = modal.ModalContent as BusyControl;
                if (view == null)
                    modal.ModalContent = view = new BusyControl();
                modal.IsModal = view.IsBusy = busy;
                view.BusyText = text;
            });
        }
    }
}
