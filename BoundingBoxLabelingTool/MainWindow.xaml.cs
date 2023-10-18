using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace BoundingBoxLabelingTool
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void LoadDir_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadDirectory();
        }

        private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _viewModel.HandleMouseWheelScroll(e);
        }


        private void ImageDirListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedImagePath = imageDirListBox.SelectedItem.ToString();
            _viewModel.HandleSelectionChanged(selectedImagePath);
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            if (imageDirListBox.SelectedIndex > 0)
            {
                imageDirListBox.SelectedIndex--;
                _viewModel.HandleSelectionChanged(imageDirListBox.SelectedItem.ToString());
            }
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            if (imageDirListBox.SelectedIndex < imageDirListBox.Items.Count - 1)
            {
                imageDirListBox.SelectedIndex++;
                _viewModel.HandleSelectionChanged(imageDirListBox.SelectedItem.ToString());
            }

        }


        #region Bounding Box Drawing Logic

        private bool isDrawing;
        private Point startPoint;
        private Rect boundingBox;

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }



        private void Image_MouseMove(object sender, MouseEventArgs e)
        {

        }

        #endregion
    }
}
