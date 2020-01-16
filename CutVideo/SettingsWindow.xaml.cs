using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CutVideo
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool isEnableContextMenu = true;
        public SettingsWindow()
        {
            InitializeComponent();
            if (Registry.ClassesRoot.OpenSubKey(@"SystemFileAssociations\\.mp4\\shell\\CutVideo") != null)
            {
                setContextMenu.Content = "Remove \"Cut Video\" in context menu";
            }
            else
            {
                setContextMenu.Content = "Add \"Cut Video\" to context menu";
            }
                
            
                


        }

        private void setContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (Registry.ClassesRoot.OpenSubKey(@"SystemFileAssociations\\.mp4\\shell\\CutVideo") != null)
            {
                setContextMenu.Content = "Remove \"Cut Video\" in context menu";
                Registry.ClassesRoot.DeleteSubKeyTree(@"SystemFileAssociations\\.mp4\\shell\\CutVideo");
                isEnableContextMenu = false;
            }
            else
            {
                setContextMenu.Content = "Add \"Cut Video\" in context menu";
                Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4", true).CreateSubKey("shell");
                Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell", true).CreateSubKey("CutVideo").SetValue("", "Cut Video");
                Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell\\CutVideo", true).CreateSubKey("command").SetValue("", Environment.GetCommandLineArgs()[0] + " \"%1\"");
                Registry.ClassesRoot.OpenSubKey("SystemFileAssociations\\.mp4\\shell\\CutVideo", true).SetValue("Icon", Environment.GetCommandLineArgs()[0]);
                isEnableContextMenu = true;
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initwindow)
            {
                switch (pathSelectBox.SelectedIndex)
                {
                    case 0:
                        textPoxPathSave.IsEnabled = false;
                        Review_B.IsEnabled = false;
                        break;

                    case 1:
                        textPoxPathSave.IsEnabled = true;
                        Review_B.IsEnabled = true;
                        break;
                }
            }
        }
        public bool initwindow = false;
        private void Window_Initialized(object sender, EventArgs e)
        {
            initwindow = true;
        }

        private void Review_B_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = @"Select folder to set save path";
                var result = fbd.ShowDialog();
                if (fbd.SelectedPath!=textPoxPathSave.Text)
                {
                    textPoxPathSave.Text = fbd.SelectedPath;
                }
            }
        }
    }
}
