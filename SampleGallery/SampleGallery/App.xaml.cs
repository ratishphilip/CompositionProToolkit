using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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

namespace SampleGallery
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : BootStrapper
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            //SplashFactory = (e) => new Views.SplashControl(e);

            #region App settings

            //var _settings = SettingsService.Instance;
            //RequestedTheme = _settings.AppTheme;
            //CacheMaxDuration = _settings.CacheMaxDuration;
            //ShowShellBackButton = _settings.UseShellBackButton;

            RequestedTheme = ApplicationTheme.Light;
            CacheMaxDuration = TimeSpan.FromDays(2);
            ShowShellBackButton = true;

            #endregion
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // content may already be shell when resuming
            if ((Window.Current.Content as ModalDialog) == null)
            {
                // setup hamburger shell inside a modal dialog
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(nav),
                    ModalContent = new Views.BusyControl(),
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here

            NavigationService.Navigate(typeof(Views.HomePage));
            await Task.CompletedTask;
        }

        public override Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        {
            var launchActivatedEventArgs = args as LaunchActivatedEventArgs;
            if ((launchActivatedEventArgs != null) && (launchActivatedEventArgs.PrelaunchActivated))
            {
                runOnStartAsync = false;
                return Task.CompletedTask;
            }

            return base.OnPrelaunchAsync(args, out runOnStartAsync);
        }
    }
}
