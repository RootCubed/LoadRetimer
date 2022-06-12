﻿using System;
using System.Windows.Controls;

namespace LoadRetimer {
    /// <summary>
    /// Interaction logic for LoadInfo.xaml
    /// </summary>
    public partial class LoadInfo : UserControl {

        public long frameStart = -1;
        public long frameEnd = -1;

        private bool isInclusive = false;

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
            LoadName.Text = s;
        }

        public void MakeUnchangable() {
            LoadName.IsReadOnly = true;
        }

        public void MakeNonInclusive() {
            isInclusive = true;
        }

        public void Rename(String newName) {
            SetName(newName);
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
                TimeSpan tmp = new TimeSpan((long)(frameStart / MainWindow.frameRate * 10_000_000));
                LoadFrameBeginS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", Helper.RoundTimeSpanMillis(tmp));
            }
            if (frameEnd > -1) {
                TimeSpan tmp = new TimeSpan((long)(frameEnd / MainWindow.frameRate * 10_000_000));
                LoadFrameEndS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", Helper.RoundTimeSpanMillis(tmp));
            }
            if (frameStart > -1 && frameEnd > -1) {
                int frameDuration = FrameDuration();
                LoadFrameDurationF.Content = String.Format("{0} frames", frameDuration);
                TimeSpan tmp = new TimeSpan((long)(frameDuration / MainWindow.frameRate * 10_000_000));
                LoadFrameDurationS.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", Helper.RoundTimeSpanMillis(tmp));
            }
        }

        public int FrameDuration() {
            if (frameStart > -1 && frameEnd > -1) {
                long addEnd = (isInclusive) ? 0 : 1;
                return (int)(frameEnd - frameStart + addEnd); // get inclusive length
            } else {
                return 0;
            }
        }

        private void LoadFrameBeginS_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            MainWindow parentWindow = (MainWindow)MainWindow.GetWindow(this);
            if (parentWindow == null) return;
            TimeSpan tmp = new TimeSpan((long)(frameStart / MainWindow.frameRate * 10_000_000));
            parentWindow.Video.Position = Helper.CeilTimeSpanMillis(tmp + parentWindow.Video.PlaybackStartTime.GetValueOrDefault());
        }

        private void LoadFrameEndS_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            MainWindow parentWindow = (MainWindow)MainWindow.GetWindow(this);
            if (parentWindow == null) return;
            TimeSpan tmp = new TimeSpan((long)(frameEnd / MainWindow.frameRate * 10_000_000));

            // not exactly sure why I can't just round here...
            parentWindow.Video.Position = Helper.CeilTimeSpanMillis(tmp + parentWindow.Video.PlaybackStartTime.GetValueOrDefault());
        }
    }
}
