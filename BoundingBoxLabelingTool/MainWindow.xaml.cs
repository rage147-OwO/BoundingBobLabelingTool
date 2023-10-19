using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BoundingBoxLabelingTool
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isDragging = false;
        private Point _startPoint;
        private Rectangle _boundingBox;
        private List<Rectangle> _boundingBoxRectangles = new List<Rectangle>();
        private double _scaleRate = 1.0;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }



        #region Column 0
        private void LoadDir_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadDirectory();
        }
        private void RightArrow_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectUp();
            DrawBoundingBoxes(_scaleRate);
        }

        private void LeftArrow_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectDown();
            DrawBoundingBoxes(_scaleRate);
        }
        private void ImageListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel.ChangeSelectedIndex(ImageListBox.SelectedIndex);
            DrawBoundingBoxes(_scaleRate);
        }



        #endregion





        private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double scaleAmount = 0.1;
                double newScaleRate = _scaleRate + (e.Delta > 0 ? scaleAmount : -scaleAmount);

                // 스케일링 범위 제한
                const double MinimumScale = 0.1;
                const double MaximumScale = 10.0;
                newScaleRate = Math.Max(MinimumScale, Math.Min(MaximumScale, newScaleRate));

                _scaleRate = newScaleRate;
                Debug.WriteLine(_scaleRate.ToString());
            }


            DrawBoundingBoxes(_scaleRate);

            //change Image ScaleX , ScaleY
            selectedImage.LayoutTransform = new ScaleTransform(_scaleRate, _scaleRate);

        }



        #region Column 1

        private void ImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _startPoint = e.GetPosition(drawCanvas);
                _startPoint = new Point(_startPoint.X / _scaleRate, _startPoint.Y / _scaleRate);
                _isDragging = true;
                ((UIElement)sender).CaptureMouse();

                _boundingBox = new Rectangle
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                drawCanvas.Children.Add(_boundingBox);
                Canvas.SetLeft(_boundingBox, _startPoint.X);
                Canvas.SetTop(_boundingBox, _startPoint.Y);
            }
        }

        private void ImageControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(drawCanvas);
                mousePos = new Point(mousePos.X / _scaleRate, mousePos.Y / _scaleRate);
                double width = mousePos.X - _startPoint.X;
                double height = mousePos.Y - _startPoint.Y;

                _boundingBox.Width = Math.Abs(width) * _scaleRate;
                _boundingBox.Height = Math.Abs(height) * _scaleRate;

                double left = Math.Min(_startPoint.X, mousePos.X) * _scaleRate;
                double top = Math.Min(_startPoint.Y, mousePos.Y) * _scaleRate;

                Canvas.SetLeft(_boundingBox, left);
                Canvas.SetTop(_boundingBox, top);
            }
        }

        private void ImageControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDragging = false;
                ((UIElement)sender).ReleaseMouseCapture();

                double width = _boundingBox.Width / _scaleRate;
                double height = _boundingBox.Height / _scaleRate;
                double x = Canvas.GetLeft(_boundingBox) / _scaleRate;
                double y = Canvas.GetTop(_boundingBox) / _scaleRate;

                if(width > 0 && height > 0)
                {
                    _viewModel.AddBoundingBox(x, y, width, height);
                }

                drawCanvas.Children.Remove(_boundingBox);
                DrawBoundingBoxes(_scaleRate);
            }
        }

        private void DrawBoundingBoxes(double scale)
        {
            foreach (var rectangle in _boundingBoxRectangles)
            {
                drawCanvas.Children.Remove(rectangle);
            }

            _boundingBoxRectangles.Clear();

            foreach (var boundingBox in _viewModel.ImageDataManager.SelectedImageData.BoundingBoxes)
            {
                Rectangle boundingBoxRect = new Rectangle
                {
                    Width = boundingBox.Width * scale,
                    Height = boundingBox.Height * scale,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                drawCanvas.Children.Add(boundingBoxRect);
                Canvas.SetLeft(boundingBoxRect, boundingBox.X * scale);
                Canvas.SetTop(boundingBoxRect, boundingBox.Y * scale);

                _boundingBoxRectangles.Add(boundingBoxRect);
            }
        }

        #endregion
    }
}
