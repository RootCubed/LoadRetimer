using System;
using System.Windows;

namespace LoadRetimer {
    /// <summary>
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class FinalRetimeWindow : Window {
        public FinalRetimeWindow() {
            InitializeComponent();
        }

        public FinalRetimeWindow(TimeSpan wloads, TimeSpan loads) {
            InitializeComponent();
            WLoads.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", wloads);
            Loads.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", loads);
            WOLoads.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", wloads - loads);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
