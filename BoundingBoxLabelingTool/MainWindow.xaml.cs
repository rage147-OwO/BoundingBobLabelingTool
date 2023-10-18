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


        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ChangeSelectedIndex(ImageListBox.SelectedIndex);
        }









        private void Right_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectUp();
        }
        private void Left_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectDown();
        }






        private bool isDragging = false;
        private Point startPoint;
        private Rectangle boundingBox; // 바운딩 박스를 저장할 변수

        private void ImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                startPoint = e.GetPosition(drawCanvas);
                isDragging = true;

                // 새로운 바운딩 박스 생성
                boundingBox = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                // 바운딩 박스를 Canvas에 추가
                drawCanvas.Children.Add(boundingBox);
                Canvas.SetLeft(boundingBox, startPoint.X);
                Canvas.SetTop(boundingBox, startPoint.Y);
            }
        }

        private void ImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(drawCanvas);
                double width = mousePos.X - startPoint.X;
                double height = mousePos.Y - startPoint.Y;

                // 바운딩 박스의 크기 조정
                boundingBox.Width = Math.Abs(width);
                boundingBox.Height = Math.Abs(height);

                // 바운딩 박스 위치 조정
                Canvas.SetLeft(boundingBox, Math.Min(startPoint.X, mousePos.X));
                Canvas.SetTop(boundingBox, Math.Min(startPoint.Y, mousePos.Y));
            }
        }

        private void ImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
            }
        }
    }
}
