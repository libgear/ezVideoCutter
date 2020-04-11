using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Diagnostics;


namespace CutVideo
{
    public partial class MainWindow : Window
    {
        private string pathToFile = "";
        private bool isOpen = false;
        private bool mDonSlider = false;
        private bool isPlay = true;
        private double cutTime1 = 0.0;
        private double cutTime2 = 0.0;

        private static Brush BackGround_ON    = (Brush) (new BrushConverter().ConvertFrom("#FFD8FFD6"));
        private static Brush BackGround_ALPHA = (Brush)(new BrushConverter().ConvertFrom("#00636363"));
        private static Brush BackGround_ERROR = (Brush)(new BrushConverter().ConvertFrom("#FFFF5E5E"));

        private void DropEventD(DragEventArgs e) {
            string[] dropContent = (string[])e.Data.GetData(DataFormats.FileDrop);
            pathToFile = dropContent[0].ToString();
            if (pathToFile.EndsWith(".mp4"))
            {
                isOpen = false;
                MediaElement1.Source = new Uri(pathToFile);
                MediaElement1.Play();
                textBlock_dnd.Text = "";
                textBlock_dnd.Background = BackGround_ALPHA;
            }
            else
            {
                textBlock_dnd.Text = "\n\n\n\n\n\nFile not mp4";
                textBlock_dnd.Background = BackGround_ERROR;
            }
        }


        private void AddToContext() {

            Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4", true).CreateSubKey("shell");
            Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell", true).CreateSubKey("CutVideo").SetValue("", "Cut Video");
            Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell\\CutVideo", true).CreateSubKey("command").SetValue("", Environment.GetCommandLineArgs()[0] + " \"%1\"");
            Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell\\CutVideo", true).SetValue("Icon", Environment.GetCommandLineArgs()[0]);
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                if (Environment.GetCommandLineArgs()[1].EndsWith(".mp4")) {
                    textBlock_dnd.Text = Environment.GetCommandLineArgs()[1].ToString();
                    MediaElement1.Source = new Uri(Environment.GetCommandLineArgs()[1]);
                    MediaElement1.Play();
                    textBlock_dnd.Text = "";
                    textBlock_dnd.Background = BackGround_ALPHA;
                }
                else
                {
                    textBlock_dnd.Background = BackGround_ERROR;
                    textBlock_dnd.Text = "\n\n\n\n\nФайл неверного формата, нужон mp4";
                }

            }

        }



        // for set cut marker
        private void markerTimeline(int pos)
        {
            if (isOpen)
            {
                var ts = MediaElement1.NaturalDuration.TimeSpan;
                var ns = MediaElement1.Position;

                string h = "" + ns.Hours;
                string m = "" + ns.Minutes;
                string s = "" + ns.Seconds;
                if (ns.Hours < 10) { h = "0" + h; }
                if (ns.Minutes < 10) { m = "0" + m; }
                if (ns.Seconds < 10) { s = "0" + s; }
                switch (pos)
                {
                    case 0:
                        LineClip.X1 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        LeftTime.X1 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        LeftTime.X2 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        cutTime1 = ns.TotalSeconds;
                        time1.Content = $"{h}:{m}:{s}";
                        break;
                    case 1:
                        LineClip.X2 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        RightTime.X1 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        RightTime.X2 = 5 + (708 / ts.TotalSeconds * ns.TotalSeconds);
                        cutTime2 = ns.TotalSeconds - cutTime1 + 1;
                        time2.Content = $"{h}:{m}:{s}";
                        break;
                }
                clipTime.Text = $" > {Math.Floor(cutTime2)}s duration clip";
            }
        }

        public MainWindow()
        {
           
            InitializeComponent();
            AddToContext();
        }
        private void openfile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mp4)|*.mp4|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                textBlock_dnd.Background = BackGround_ALPHA;
                textBlock_dnd.Text = "";
                textBlock_dnd.IsEnabled = false;
                
                 var pathToFile = openFileDialog.FileName;
                MediaElement1.Source = new Uri(openFileDialog.FileName);
                MediaElement1.Play();
                isOpen = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openfile();
        }


        private void TextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            openfile();
        }


        //dnd content

        private void TextBlock_dnd_DragEnter(object sender, DragEventArgs e)
        {
            textBlock_dnd.Background = BackGround_ON;
            textBlock_dnd.Text = "\n\n\n\n\n\nDrop file";//i dont know how it
        }

        private void TextBlock_dnd_DragLeave(object sender, DragEventArgs e)
        {
            textBlock_dnd.Background = BackGround_ALPHA;
            textBlock_dnd.Text = "\n\n\n\n\n\nClick here or Open File to open the video file or drag and drop";
        }


        private void TextBlock_Drop(object sender, DragEventArgs e)
        {
            DropEventD(e);
        }
        private void MediaElement1_Drop(object sender, DragEventArgs e)
        {
            DropEventD(e);
        }

        // Executed when the file is loaded
        private void MediaElement1_MediaOpened(object sender, RoutedEventArgs e)
        {
            isOpen = true;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1); // here you can configure the execution period of the timer_Tick function
            timer.Start();
            timelineSlider.Maximum = MediaElement1.NaturalDuration.TimeSpan.TotalSeconds;
            var ns = MediaElement1.Position;
            timelineSlider.Value = 0;
            cutTime1 = 0.0;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (isOpen)
            {
                // edit time format
                var ts = MediaElement1.NaturalDuration.TimeSpan;
                var ns = MediaElement1.Position;
                string h = "" + ns.Hours;
                string m = "" + ns.Minutes;
                string s = "" + ns.Seconds;
                string qh = "" + ts.Hours;
                string qm = "" + ts.Minutes;
                string qs = "" + ts.Seconds;
                if (ns.Hours < 10) { h = "0" + h; }
                if (ns.Minutes < 10) { m = "0" + m; }
                if (ns.Seconds < 10) { s = "0" + s; }
                if (ts.Hours < 10) { qh = "0" + qh; }
                if (ts.Minutes < 10) { qm = "0" + qm; }
                if (ts.Seconds < 10) { qs = "0" + qs; }

                vTime.Text = $"{h}:{m}:{s}/{qh}:{qm}:{qs}"; //displays the time

                if (!mDonSlider)  //Synchronize timeline with video only when we do not interact with it
                {
                    timelineSlider.Value = ns.TotalSeconds; //Synchronize timeline
                }
            }
        }
        
        private void Time1_Click(object sender, RoutedEventArgs e) //left edge of the clip
        {
            markerTimeline(0);
        }

        private void Time2_Click(object sender, RoutedEventArgs e) //right edge of the clip
        {
            markerTimeline(1);
        }

        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e) //stops the video if you left mouse button has down on the timeline
        {
            MediaElement1.Pause();
            mDonSlider = true;
        }
        private void TimelineSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e) //Resumes video when you release left mouse button
        {
            MediaElement1.Play();
            mDonSlider = false;
            TimeSpan ts = TimeSpan.FromSeconds(timelineSlider.Value);
            MediaElement1.Position = ts;
        }
        

        private void Button_Click_1(object sender, RoutedEventArgs e) // Cut button event
        {
            try
            {
                pathToFile = MediaElement1.Source.ToString().Replace("file:///", "");

                string pathToSave = pathToFile.Replace(pathToFile.Split('/')[pathToFile.Split('/').Length - 1], "");
                string fileName = pathToFile.Split('/')[pathToFile.Split('/').Length - 1];

                if (Registry.CurrentUser.OpenSubKey(@"Software\\libgear\\ezVideoCutter").GetValue("Savepath").ToString() != "default")
                    pathToSave = Registry.CurrentUser.OpenSubKey(@"Software\\libgear\\ezVideoCutter").GetValue("Savepath").ToString();

                if (cutTime2 > 0)
                {
                    cut_processing.Visibility = Visibility.Visible;
                    MediaElement1.Pause();
                    isPlay = false;
                    Process process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/C {Environment.CurrentDirectory}\\ffmpeg.exe -ss {time1.Content}.0 -i \"{pathToFile}\" -c copy -t {Math.Floor(cutTime2)} \"{pathToSave + "/" + PrefixSave.Text + fileName}\" -y",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                    });
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, a) =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            cut_processing.Visibility = Visibility.Hidden;
                        });

                    };
                    //Process.Start("ffmpeg.exe", $"-ss {time1.Content}.0 -i \"{pathToFile}\" -c copy -t {Math.Floor(cutTime2)} \"{pathToSave+"/"+PrefixSave.Text+fileName}\"");
                }
                else
                {
                    MessageBox.Show("clip duration cannot be less than or equal to zero", "Cut error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("No video file. Please open video file.", "Cut error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void MediaElement1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) // play/pause on click vidio frame
        {
            
            if (isPlay)
            {
                MediaElement1.Pause();
                isPlay = false;
            }
            else
            {
                MediaElement1.Play();
                isPlay = true;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // set voluume
        {
            if (isOpen)
            {
                MediaElement1.Volume = (double)VolumeSlider.Value;
                volumeVal.Text = Math.Floor(VolumeSlider.Value * 100) + "%";
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) // set speed ratio
        {
            if (isOpen)
            {
                MediaElement1.SpeedRatio = (double)SpeedSlider.Value;
                SpeedMultiple.Text = Math.Round(SpeedSlider.Value,1) + "x";
            }
        }

        private void SettingsButto_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }
    }
}