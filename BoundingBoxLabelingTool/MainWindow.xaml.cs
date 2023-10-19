using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BoundingBoxLabelingTool
{
    public class TextBlockTraceListener : TraceListener
    {
        private TextBlock _textBlock;
        private readonly Queue<string> _debugLines = new Queue<string>();
        private const int MaxDebugLines = 5;

        public TextBlockTraceListener(TextBlock textBlock)
        {
            _textBlock = textBlock;
        }

        public override void Write(string message)
        {
            _debugLines.Enqueue(message.Trim());
            if (_debugLines.Count > MaxDebugLines)
            {
                _debugLines.Dequeue();
            }

            _textBlock.Dispatcher.Invoke(() =>
            {
                _textBlock.Text = string.Join(Environment.NewLine, _debugLines);
            });
        }

        public override void WriteLine(string message)
        {
            Write(message.Trim() + Environment.NewLine);
        }
    }


    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private bool _isDrawing = false;
        private Point _startPoint;
        private Rectangle _boundingBox;
        private List<Rectangle> _boundingBoxRectangles = new List<Rectangle>();
        private double _scaleRate = 1.0;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Debug.Listeners.Add(new TextBlockTraceListener(debugTextBlock));
        }



        #region Column 0
        private void LoadDir_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadDirectory();
            DrawBoundingBoxes(_scaleRate);
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
            if (e.ChangedButton == MouseButton.Left && !Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _startPoint = e.GetPosition(drawCanvas);
                _startPoint = new Point(_startPoint.X / _scaleRate, _startPoint.Y / _scaleRate);
                _isDrawing = true;
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
            if (_isDrawing && e.LeftButton == MouseButtonState.Pressed)
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
                if (_isDrawing)
                {
                    _isDrawing = false;
                    ((UIElement)sender).ReleaseMouseCapture();

                    double width = _boundingBox.Width / _scaleRate;
                    double height = _boundingBox.Height / _scaleRate;
                    double x = Canvas.GetLeft(_boundingBox) / _scaleRate;
                    double y = Canvas.GetTop(_boundingBox) / _scaleRate;

                    if (width > 0 && height > 0)
                    {
                        // 클래스 정보를 얻어올 수 있는 방법에 따라 클래스 ID를 가져오세요.
                        // 예: int classId = GetClassIdFromUserInput();
                        if(int.TryParse(ClassTextBox.Text, out int classId))
                        {
                            _viewModel.AddBoundingBox(x, y, width, height, int.Parse(ClassTextBox.Text));
                        }
                    }     
                    drawCanvas.Children.Remove(_boundingBox);
                    DrawBoundingBoxes(_scaleRate);
                }
            }
        }


        private void DrawBoundingBoxes(double scale)
        {                                              
            Debug.WriteLine("DrawBoundingBoxes");
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
                    StrokeThickness = 4
                };
                boundingBoxRect.MouseLeftButtonDown += BoundingBoxRectangle_MouseLeftButtonDown;
                boundingBoxRect.MouseMove += BoundingBoxRectangle_MouseMove;
                boundingBoxRect.MouseLeftButtonUp += BoundingBoxRectangle_MouseLeftButtonUp;
                drawCanvas.Children.Add(boundingBoxRect);
                Canvas.SetLeft(boundingBoxRect, boundingBox.X * scale);
                Canvas.SetTop(boundingBoxRect, boundingBox.Y * scale);

                _boundingBoxRectangles.Add(boundingBoxRect);
            }
        }


        private bool isDragging = false;
        private Point startPoint;
        private Rectangle selectedRectangle;
        private void BoundingBoxRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {                                                                
            //Ctrl + Click
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Rectangle clickedRectangle = (Rectangle)sender;
                int selectedBoundingBoxIndex = _boundingBoxRectangles.IndexOf(clickedRectangle);
                _viewModel.DeleteBoundingBox(selectedBoundingBoxIndex);
                DrawBoundingBoxes(_scaleRate);
            }

            //Click
            selectedRectangle = sender as Rectangle;
            isDragging = true;
            startPoint = e.GetPosition(drawCanvas);
            selectedRectangle.CaptureMouse();
        }


        private void BoundingBoxRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var newPoint = e.GetPosition(drawCanvas);
                double deltaX = newPoint.X - startPoint.X;
                double deltaY = newPoint.Y - startPoint.Y;

                Canvas.SetLeft(selectedRectangle, Canvas.GetLeft(selectedRectangle) + deltaX);
                Canvas.SetTop(selectedRectangle, Canvas.GetTop(selectedRectangle) + deltaY);

                startPoint = newPoint;
            }
        }

        private void BoundingBoxRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                selectedRectangle.ReleaseMouseCapture();
                isDragging = false;
                
                // Update bounding box data by index
                int selectedBoundingBoxIndex = _boundingBoxRectangles.IndexOf(selectedRectangle);
                double x = Canvas.GetLeft(selectedRectangle) / _scaleRate;
                double y = Canvas.GetTop(selectedRectangle) / _scaleRate;
                double width = selectedRectangle.Width / _scaleRate;
                double height = selectedRectangle.Height / _scaleRate;
                _viewModel.MoveBoundingBox(selectedBoundingBoxIndex, x, y);


            }
        }




            #endregion
        }
}
