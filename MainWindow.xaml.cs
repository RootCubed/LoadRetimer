using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;

namespace LoadRetimer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static double frameRate = 30;

        private DispatcherTimer timerVideo;
        private DispatcherTimer timerAnalyzer;

        readonly string[] loadNames = new string[] {
            "1-1 Start",
            "1-1 End",
            "1-2 Start",
            "1-2 Pipe 1",
            "1-2 Pipe 2",
            "1-2 End",
            "1-3 Start",
            "1-3 Pipe",
            "1-3 End",
            "1-C Start",
            "1-C End",
            "5-1 Start",
            "5-1 Pipe",
            "5-1 End",
            "Piranha Start",
            "Piranha End",
            "5-3 Start",
            "5-3 Pipe",
            "5-3 End",
            "5-T Start",
            "5-T Door",
            "5-T End",
            "5-4 Start",
            "5-4 Pipe 1",
            "5-4 Pipe 2",
            "5-4 End",
            "5-GH Start",
            "5-GH Door 1",
            "5-GH Door 2",
            "5-GH End",
            "5-C Start",
            "5-C End",
            "8-1 Start",
            "8-1 Pipe",
            "8-1 End",
            "8-2 Start",
            "8-2 Pipe 1",
            "8-2 Pipe 2",
            "8-2 Pipe 3",
            "8-2 End",
            "8-7 Start",
            "8-7 Pipe",
            "8-7 End",
            "8-A Start",
            "8-A Pipe 1",
            "8-A Pipe 2",
            "8-A Pipe 3",
            "8-A Fade 1",
            "8-A End",
            "8-C Start",
            "8-C Pipe 1",
            "8-C Pipe 2",
            "8-C Door"
        };

        public MainWindow() {
            InitializeComponent();
            Unosquare.FFME.Library.FFmpegDirectory = @"./ffmpeg";
            TotalRunInfo.SetName("Entire Run");

            Video.ScrubbingEnabled = false;

            timerVideo = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(5)
            };
            timerVideo.Tick += TimerVideo_Tick;
            timerVideo.Start();

            timerAnalyzer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            timerAnalyzer.Tick += Analyzer_Tick;
            timerAnalyzer.Start();
        }

        private void ButtonPlayPause_Click(object sender, RoutedEventArgs e) {
            if (!Video.IsOpen) return;
            if (Video.MediaState == Unosquare.FFME.Common.MediaPlaybackState.Play) {
                Video.Pause();
                PlayPause.Content = "Play";
            } else if (Video.MediaState == Unosquare.FFME.Common.MediaPlaybackState.Pause) {
                Video.Play();
                PlayPause.Content = "Pause";
            } else if (Video.MediaState == Unosquare.FFME.Common.MediaPlaybackState.Stop) {
                Video.Position = new TimeSpan(0);
                Video.Play();
                PlayPause.Content = "Pause";
            }
        }

        private void FrameBack_Click(object sender, RoutedEventArgs e) {
            if (!Video.IsOpen) return;
            Video.StepBackward();
        }

        private void FrameForward_Click(object sender, RoutedEventArgs e) {
            if (!Video.IsOpen) return;
            Video.StepForward();
        }

        private void Info_Click(object sender, RoutedEventArgs e) {
            new Info().ShowDialog();
        }

        private LoadInfo CreateNewLoadInfo() {
            ListBoxItem lbi = new ListBoxItem();
            LoadInfo newLoad = new LoadInfo(loadNames[LoadBox.Items.Count]);
            lbi.Content = newLoad;
            LoadBox.Items.Add(lbi);
            LoadBox.ScrollIntoView(lbi);
            LoadBox.SelectedItem = lbi;
            LoadBox.Focus();
            return newLoad;
        }

        private void BeginLoad_Click(object sender, RoutedEventArgs e) {
            if (LoadBox.SelectedItem == null) {
                CreateNewLoadInfo();
                LoadInfo selLoad = (LoadInfo)((ListBoxItem)LoadBox.SelectedItem).Content;
                selLoad.SetBegin(Video.FramePosition);
            } else {
                LoadInfo selLoad = (LoadInfo)((ListBoxItem)LoadBox.SelectedItem).Content;
                selLoad.SetBegin(Video.FramePosition);
                LoadBox.SelectedItem = null;
            }
        }

        private void EndLoad_Click(object sender, RoutedEventArgs e) {
            if (LoadBox.SelectedItem == null) {
                CreateNewLoadInfo();
                LoadInfo selLoad = (LoadInfo)((ListBoxItem)LoadBox.SelectedItem).Content;
                selLoad.SetEnd(Video.FramePosition);
            } else {
                LoadInfo selLoad = (LoadInfo)((ListBoxItem)LoadBox.SelectedItem).Content;
                selLoad.SetEnd(Video.FramePosition);
                LoadBox.SelectedItem = null;
            }
        }

        private void Retime_Click(object sender, RoutedEventArgs e) {
            if (TotalRunInfo.FrameDuration() == 0) {
                System.Windows.MessageBox.Show("You must retime the runs without loads before calculating the final retime.");
                return;
            }
            long loadFrames = 0;
            for (int i = 0; i < LoadBox.Items.Count; i++) {
                LoadInfo li = (LoadInfo)((ListBoxItem)LoadBox.Items[i]).Content;
                loadFrames += li.FrameDuration();
            }
            var tsLoads = new TimeSpan((long)(loadFrames / frameRate * 10_000_000));
            var tsWLoads = new TimeSpan((long)(TotalRunInfo.FrameDuration() / frameRate * 10_000_000));
            new FinalRetimeWindow(tsWLoads, tsLoads).ShowDialog();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Title = "Select a video",
                Filter = "Video files (*.mp4, *.mkv, *.flv, *.wmv, *.avi)|*.mp4;*.mkv;*.flv;*.wmv;*.avi|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Video.Open(new Uri(ofd.FileName));
            }
        }

        private void OpenLoads_Click(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog {
                Title = "Select loads",
                Filter = "Load Retimer files (*.lds)|*.lds|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var f = ofd.OpenFile();
                var binReader = new BinaryReader(f);
                bool hasTotalRun = binReader.ReadByte() == 1;
                int loadCount = binReader.ReadByte();
                frameRate = binReader.ReadDouble();
                FPSLabel.Content = String.Format("FPS: {0:F2}", frameRate);
                if (hasTotalRun) {
                    long startFrames = binReader.ReadUInt32();
                    long endFrames = binReader.ReadUInt32();
                    TotalRunInfo.SetBegin(new TimeSpan((long)(startFrames / frameRate * 10_000_000)));
                    TotalRunInfo.SetEnd(new TimeSpan((long)(endFrames / frameRate * 10_000_000)));
                }

                LoadBox.Items.Clear();
                for (int i = 0; i < loadCount; i++) {
                    long startFrames = binReader.ReadUInt32();
                    long endFrames = binReader.ReadUInt32();
                    LoadInfo newLoad = CreateNewLoadInfo();
                    newLoad.SetBegin(new TimeSpan((long)(startFrames / frameRate * 10_000_000)));
                    newLoad.SetEnd(new TimeSpan((long)(endFrames / frameRate * 10_000_000)));
                }
            }
        }

        private void SaveLoads_Click(object sender, RoutedEventArgs e) {
            var ofd = new SaveFileDialog {
                Title = "Save loads",
                Filter = "Load Retimer files (*.lds)|*.lds|All files (*.*)|*.*"
            };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var f = ofd.OpenFile();
                var binWriter = new BinaryWriter(f);
                if (TotalRunInfo.FrameDuration() > 0) {
                    binWriter.Write((byte)1);
                } else {
                    binWriter.Write((byte)0);
                }
                binWriter.Write((byte)LoadBox.Items.Count);
                binWriter.Write(frameRate);
                if (TotalRunInfo.FrameDuration() > 0) {
                    binWriter.Write((UInt32)TotalRunInfo.frameStart);
                    binWriter.Write((UInt32)TotalRunInfo.frameEnd);
                }
                for (int i = 0; i < LoadBox.Items.Count; i++) {
                    LoadInfo li = (LoadInfo)((ListBoxItem)LoadBox.Items[i]).Content;
                    binWriter.Write((UInt32)li.frameStart);
                    binWriter.Write((UInt32)li.frameEnd);
                }
            }
        }

        private void Video_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e) {
            FPSLabel.Content = String.Format("FPS: {0:F2}", Video.VideoFrameRate);
            DurationLabel.Content = String.Format("Duration: {0:hh\\:mm\\:ss\\.fff}", Video.NaturalDuration);
            frameRate = Video.VideoFrameRate;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (!Video.IsOpen) return;
            if (e.Key == Key.A) {
                Video.StepBackward();
                e.Handled = true;
            }
            if (e.Key == Key.D) {
                Video.StepForward();
                e.Handled = true;
            }
            if (e.Key == Key.S) {
                BeginLoad_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F) {
                EndLoad_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.Q) {
                Video.Seek(Video.Position - new TimeSpan(0, 0, 5));
                e.Handled = true;
            }
            if (e.Key == Key.E) {
                Video.Seek(Video.Position + new TimeSpan(0, 0, 5));
                e.Handled = true;
            }
            if (e.Key == Key.Space) {
                ButtonPlayPause_Click(null, null);
                e.Handled = true;
            }
        }

        private void StartRun_Click(object sender, RoutedEventArgs e) {
            if (!Video.IsOpen) return;
            TotalRunInfo.SetBegin(Video.FramePosition);
        }

        private void EndRun_Click(object sender, RoutedEventArgs e) {
            if (!Video.IsOpen) return;
            TotalRunInfo.SetEnd(Video.FramePosition);
        }

        private bool userChanging = false;

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (userChanging) {
                if (!Video.IsOpen) return;
                var ts = new TimeSpan((long)(Video.NaturalDuration.GetValueOrDefault().TotalSeconds * Slider.Value * 10_000));
                Video.Position = ts;
            }
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            userChanging = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            userChanging = false;
        }

        private enum AnalyzeState {
            Idle,
            SelectTL, SelectBR,
            FindStartFast, FindEndFast,
            FindStart, FindEnd,
        };

        private AnalyzeState analyzeState = AnalyzeState.SelectTL;

        private void SeekNext_Click(object sender, RoutedEventArgs e) {
            if (analyzeState == AnalyzeState.SelectTL) {
                System.Windows.MessageBox.Show("Click on the top-left corner, then on the bottom-right corner of the game area.");
            } else {
                analyzeState = AnalyzeState.FindStart;
            }
        }

        private void SeekPrev_Click(object sender, RoutedEventArgs e) {
            if (analyzeState == AnalyzeState.SelectTL) {
                System.Windows.MessageBox.Show("Click on the top-left corner, then on the bottom-right corner of the game area.");
            } else {
                analyzeState = AnalyzeState.FindEnd;
            }
        }

        int rectX1 = 0, rectY1 = 0, rectX2 = 0, rectY2 = 0;

        private void Video_MouseDown(object sender, MouseButtonEventArgs e) {
            return; // turn this off for now
            /*var m = Mouse.GetPosition(Video);
            if (m.X < 0 || m.Y < 0 || m.X > Video.ActualWidth || m.Y > Video.ActualHeight) return;
            var p = TransformToNaturalVideo(m);
            if (analyzeState == AnalyzeState.SelectTL) {
                Rect.Margin = new Thickness(m.X, m.Y, 0, 0);
                rectX1 = (int) p.X;
                rectY1 = (int) p.Y;
                analyzeState = AnalyzeState.SelectBR;
            } else if (analyzeState == AnalyzeState.SelectBR) {
                double w = m.X - Rect.Margin.Left;
                double h = m.Y - Rect.Margin.Top;
                if (w <= 0 || h <= 0) return;
                rectX2 = (int) p.X;
                rectY2 = (int) p.Y;
                Rect.Width = w;
                Rect.Height = h;
            }*/
        }

        private Point TransformToNaturalVideo(Point m) {
            double videoRatio = (double)Video.NaturalVideoWidth / Video.NaturalVideoHeight;
            double actualRatio = Video.ActualWidth / Video.ActualHeight;
            double xOffset = 0;
            double yOffset = 0;
            if (videoRatio < actualRatio) {
                xOffset = (Video.ActualWidth - (Video.ActualHeight / Video.NaturalVideoHeight * Video.NaturalVideoWidth)) / 2;
            } else {
                yOffset = (Video.ActualHeight - (Video.ActualWidth / Video.NaturalVideoWidth * Video.NaturalVideoHeight)) / 2;
            }
            double w = Video.ActualWidth - xOffset * 2;
            double h = Video.ActualHeight - yOffset * 2;
            int x = (int)((m.X - xOffset) * Video.NaturalVideoWidth / w);
            int y = (int)((m.Y - yOffset) * Video.NaturalVideoHeight / h);
            return new Point(x, y);
        }

        private Point TransformFromNaturalVideo(Point m) {
            double videoRatio = (double)Video.NaturalVideoWidth / Video.NaturalVideoHeight;
            double actualRatio = Video.ActualWidth / Video.ActualHeight;
            double xOffset = 0;
            double yOffset = 0;
            if (videoRatio < actualRatio) {
                xOffset = (Video.ActualWidth - (Video.ActualHeight / Video.NaturalVideoHeight * Video.NaturalVideoWidth)) / 2;
            } else {
                yOffset = (Video.ActualHeight - (Video.ActualWidth / Video.NaturalVideoWidth * Video.NaturalVideoHeight)) / 2;
            }
            double w = Video.ActualWidth - xOffset * 2;
            double h = Video.ActualHeight - yOffset * 2;
            int x = (int)(m.X * w / Video.NaturalVideoWidth + xOffset);
            int y = (int)(m.Y * h / Video.NaturalVideoHeight + yOffset);
            return new Point(x, y);
        }

        private void TimerVideo_Tick(object sender, object e) {
            if (!Video.IsOpen) return;
            Slider.Value = Video.Position.TotalMilliseconds / Video.NaturalDuration.GetValueOrDefault().TotalMilliseconds * 1000;
            TimePosition.Content = String.Format("{0:hh\\:mm\\:ss\\.fff}", new TimeSpan((long)(Video.Position.TotalMilliseconds * 10_000)));
            var pos = Mouse.GetPosition(Video);
            Magnifier.Viewbox = new Rect(pos.X - 10, pos.Y + 10, 20, 20);
        }

        private async void Analyzer_Tick(object sender, object e) {
            if (analyzeState == AnalyzeState.Idle || analyzeState == AnalyzeState.SelectBR || analyzeState == AnalyzeState.SelectTL) return;
            if (Video.IsSeeking) return;
            var bmp = await Video.CaptureBitmapAsync();
            bool isBlack = IsBlack(bmp);
            if (analyzeState == AnalyzeState.FindStart) {
                if (isBlack) {
                    analyzeState = AnalyzeState.Idle;
                } else {
                    await Video.StepForward();
                }
            } else if (analyzeState == AnalyzeState.FindEnd) {
                if (isBlack) {
                    analyzeState = AnalyzeState.Idle;
                } else {
                    await Video.StepBackward();
                }
            }
        }

        private System.Drawing.Bitmap tempBmp;

        private bool IsBlack(System.Drawing.Bitmap bitmap) {
            tempBmp = bitmap.Clone(new System.Drawing.Rectangle(rectX1, rectY1, rectX2 - rectX1, rectY2 - rectY1), bitmap.PixelFormat);
            int w = tempBmp.Width;
            int h = tempBmp.Height;
            for (double x = 0; x < 1; x += 0.05) {
                for (double y = 0; y < 1; y += 0.05) {
                    if (tempBmp.GetPixel((int)(w * x), (int)(h * y)).GetBrightness() > 0.1) return false;
                }
            }
            return true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (rectX1 > 0 && rectY1 > 0 && rectX2 > 0 && rectY2 > 0) {
                var p1 = TransformFromNaturalVideo(new Point(rectX1, rectY1));
                var p2 = TransformFromNaturalVideo(new Point(rectX2, rectY2));
                Rect.Margin = new Thickness(p1.X, p1.Y, 0, 0);
                Rect.Width = p2.X - p1.X;
                Rect.Height = p2.Y - p1.Y;
            }
        }
    }

    class Helper {
        public static TimeSpan RoundTimeSpanMillis(TimeSpan ts) {
            long ticks = (long) Math.Round(ts.Ticks / 10_000.0) * 10_000;
            var res = new TimeSpan(ticks);
            return res;
        }
    }
}
