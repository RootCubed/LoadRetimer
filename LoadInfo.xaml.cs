using System;
using System.Windows.Controls;

namespace LoadRetimer {
    /// <summary>
    /// Interaction logic for LoadInfo.xaml
    /// </summary>
    public partial class LoadInfo : UserControl {

        public long frameStart = -1;
        public long frameEnd = -1;

        public LoadInfo() {
            InitializeComponent();
            TryCalculate();
        }

        public LoadInfo(string name) {
            InitializeComponent();
            SetName(name);
            TryCalculate();
        }

        public void SetName(string s) {
            LoadName.Content = s;
        }

        public void SetBegin(TimeSpan begin) {
            frameStart = (long) Math.Round(begin.TotalSeconds * MainWindow.frameRate);
            TryCalculate();
        }

        public void SetEnd(TimeSpan begin) {
            frameEnd = (long)Math.Round(begin.TotalSeconds * MainWindow.frameRate);
            TryCalculate();
        }

        private void TryCalculate() {
            LoadFrameDurationF.Content = "---";
            LoadFrameDurationS.Content = "---";
            if (frameStart > -1) {
                LoadFrameBeginS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", new TimeSpan((long)(frameStart / MainWindow.frameRate * 10_000_000)));
            }
            if (frameEnd > -1) {
                LoadFrameEndS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", new TimeSpan((long)(frameEnd / MainWindow.frameRate * 10_000_000)));
            }
            if (frameStart > -1 && frameEnd > -1) {
                int frameDuration = FrameDuration();
                LoadFrameDurationF.Content = String.Format("{0} frames", frameDuration);
                LoadFrameDurationS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", new TimeSpan((long)(frameDuration / MainWindow.frameRate * 10_000_000)));
            }
        }

        public int FrameDuration() {
            if (frameStart > -1 && frameEnd > -1) {
                return (int)(frameEnd - frameStart);
            } else {
                return 0;
            }
        }
    }
}
