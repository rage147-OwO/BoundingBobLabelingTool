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

            double scale = _viewModel.ScaleRate;


            //change bounding box size
            boundingBox.Width = initialWidth * scale;
            boundingBox.Height = initialHeight * scale;

            //change bounding box position
            Canvas.SetLeft(boundingBox, initialLeft * scale);
            Canvas.SetTop(boundingBox, initialTop * scale);
            DrawBoundingBoxes(_viewModel.ScaleRate);


        }


        double initialWidth;
        double initialHeight;
        double initialLeft;
        double initialTop;



        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ChangeSelectedIndex(ImageListBox.SelectedIndex);
            DrawBoundingBoxes(_viewModel.ScaleRate);
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
        private Rectangle boundingBox;

        private void ImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                startPoint = e.GetPosition(drawCanvas);
                startPoint = new Point(startPoint.X / _viewModel.ScaleRate, startPoint.Y / _viewModel.ScaleRate); // Scale the start point
                isDragging = true;
                ((UIElement)sender).CaptureMouse();

                boundingBox = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

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
                mousePos = new Point(mousePos.X / _viewModel.ScaleRate, mousePos.Y / _viewModel.ScaleRate); // Scale the mouse position
                double width = mousePos.X - startPoint.X;
                double height = mousePos.Y - startPoint.Y;

                // Update bounding box dimensions and position after scaling the image
                boundingBox.Width = Math.Abs(width) * _viewModel.ScaleRate;
                boundingBox.Height = Math.Abs(height) * _viewModel.ScaleRate;

                double left = Math.Min(startPoint.X, mousePos.X) * _viewModel.ScaleRate;
                double top = Math.Min(startPoint.Y, mousePos.Y) * _viewModel.ScaleRate;

                Canvas.SetLeft(boundingBox, left);
                Canvas.SetTop(boundingBox, top);
            }
        }


        private void ImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
                ((UIElement)sender).ReleaseMouseCapture();

                // Save bounding box dimensions and position after scaling the image
                initialWidth = boundingBox.Width;
                initialHeight = boundingBox.Height;
                initialLeft = Canvas.GetLeft(boundingBox);
                initialTop = Canvas.GetTop(boundingBox);

                //Save bounding box in _viewModel.ImageDataManager.SelectedImageData.BoundingBoxes
                //Calculate bounding box position and size in original image
                double x = initialLeft / _viewModel.ScaleRate;
                double y = initialTop / _viewModel.ScaleRate;
                double width = initialWidth / _viewModel.ScaleRate;
                double height = initialHeight / _viewModel.ScaleRate;
                _viewModel.AddBoundingBox(x, y, width, height);


                //Clear bounding box
                drawCanvas.Children.Remove(boundingBox);

                DrawBoundingBoxes(_viewModel.ScaleRate);

            }
        }


        //Rectangle List
        private List<Rectangle> boundingBoxRectangles = new List<Rectangle>();

        private void DrawBoundingBoxes(double scale)
        {
            // 기존의 바운딩 박스를 지우기
            foreach (var rectangle in boundingBoxRectangles)
            {
                drawCanvas.Children.Remove(rectangle);
            }

            // 새로운 바운딩 박스 리스트 초기화
            boundingBoxRectangles.Clear();

            foreach (var boundingBox in _viewModel.ImageDataManager.SelectedImageData.BoundingBoxes)
            {
                // 바운딩 박스를 그리는 코드
                Rectangle boundingBoxRect = new Rectangle
                {
                    Width = boundingBox.Width * scale,
                    Height = boundingBox.Height * scale,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                // Canvas에 바운딩 박스를 추가
                drawCanvas.Children.Add(boundingBoxRect);

                // 바운딩 박스의 위치 설정
                Canvas.SetLeft(boundingBoxRect, boundingBox.X * scale);
                Canvas.SetTop(boundingBoxRect, boundingBox.Y * scale);

                // 리스트에 바운딩 박스 추가
                boundingBoxRectangles.Add(boundingBoxRect);
            }
        }


    }
}
                                                