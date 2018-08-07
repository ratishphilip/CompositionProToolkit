using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CompositionProToolkit.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SampleGallery.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgressRing3dPage : Page
    {
        private readonly string[] _options = { "Square Nodes", "Round Nodes" };

        private int _currentOption = 0;

        public ProgressRing3dPage()
        {
            this.InitializeComponent();
            btn.Content = _options[_currentOption];
        }

        private void OnSwitchNodes(object sender, RoutedEventArgs e)
        {
            progress3d.NodeShape = _currentOption == 1 ? ProgressRing3d.NodeShapeType.Circle : ProgressRing3d.NodeShapeType.Square;
            _currentOption = (_currentOption + 1) % 2;
            btn.Content = _options[_currentOption];
        }
    }
}
