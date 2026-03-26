using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ql_nhanSW.Form
{
    public partial class Window1 : Window
    {


        public Window1()

        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SwitchRegister_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            SignUpPanel.Visibility = Visibility.Visible;
        }

        private void SwitchLogin_Click(object sender, RoutedEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Visible;
            SignUpPanel.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            // Fade out SignInPanel
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));
            SignInPanel.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(400);

            SignInPanel.Visibility = Visibility.Collapsed;
            LoadingOverlay.Visibility = Visibility.Visible;

            // Fade in LoadingOverlay
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
            LoadingOverlay.BeginAnimation(OpacityProperty, fadeIn);
        }

        private void LogoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Fade out LoadingOverlay
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.4));
            LoadingOverlay.BeginAnimation(OpacityProperty, fadeOut);

            LoadingOverlay.Visibility = Visibility.Collapsed;

            // Fade in SignInPanel
            SignInPanel.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
            SignInPanel.BeginAnimation(OpacityProperty, fadeIn);
        }
    }
}