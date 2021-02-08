using System.Windows;

namespace LoadRetimer {
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : Window {
        public Info() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
