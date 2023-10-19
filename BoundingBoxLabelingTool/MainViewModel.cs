using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;


namespace BoundingBoxLabelingTool
{
    internal class MainViewModel : INotifyPropertyChanged
    {












        public ImageDataManager ImageDataManager { get; set; }

        public MainViewModel()
        {

        }


        public void LoadDirectory()
        {
            ImageDataManager = new ImageDataManager();
            ImageDataManager.LoadDirectory();
            OnPropertyChanged(nameof(ImageDataManager));
        }
        public void ChangeSelectedIndex(int selectedIndex)
        {
            ImageDataManager.SelectChange(selectedIndex);
            OnPropertyChanged(nameof(ImageDataManager));
        }



        public void SelectUp()
        {
            ImageDataManager.SelectUp();
            OnPropertyChanged(nameof(ImageDataManager));
        }

        public void SelectDown()
        {
            ImageDataManager.SelectDown();
            OnPropertyChanged(nameof(ImageDataManager));
        }


        public void AddBoundingBox(double x, double y, double width, double height)
        {
            ImageDataManager.AddBoundingBox(x, y, width, height);
            OnPropertyChanged(nameof(ImageDataManager));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void
            OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Debug.WriteLine(propertyName + " was set");
        }
    }



    public class ImageDataManager
    {
        public ObservableCollection<ImageData> ImageDatas { get; set; }
        public ImageData SelectedImageData { get; set; }

        public int SelectedIndex
        {
            get
            {
                return ImageDatas.IndexOf(SelectedImageData);
            }
        }

        public void AddBoundingBox(double x, double y, double width, double height)
        {
            SelectedImageData.BoundingBoxes.Add(new BoundingBox() { X = x, Y = y, Width = width, Height = height });
            SaveLabelingData();
        }


        public int ImageCount
        {
            get
            {
                return ImageDatas.Count;
            }
        }

        public ImageDataManager()
        {

        }
        public void LoadDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string FilePath = dialog.SelectedPath;
                string[] files = System.IO.Directory.GetFiles(FilePath, "*.png");
                ImageDatas = new ObservableCollection<ImageData>();
                foreach (string file in files)
                {
                    ImageDatas.Add(new ImageData(file));
                }
                SelectedImageData = ImageDatas[0];
            }




        }




        public void SelectChange(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < ImageDatas.Count)
            {
                SelectedImageData = ImageDatas[selectedIndex];
            }
        }

        public void SelectUp()
        {
            if (ImageDatas.Count > 0)
            {
                int selectedIndex = ImageDatas.IndexOf(SelectedImageData);
                if (selectedIndex < ImageDatas.Count - 1)
                {
                    SelectedImageData = ImageDatas[selectedIndex + 1];
                }
            }

            Debug.WriteLine("SelectUp");
        }
        public void SelectDown()
        {
            if (ImageDatas.Count > 0)
            {
                int selectedIndex = ImageDatas.IndexOf(SelectedImageData);
                if (selectedIndex > 0)
                {
                    SelectedImageData = ImageDatas[selectedIndex - 1];
                }
            }

            Debug.WriteLine("SelectDown");
        }

        public void SaveLabelingData()
        {
            // Save SelectedImageData Labeling Data as Yolo Format
            if (SelectedImageData != null)
            {
                string ImagePath = SelectedImageData.ImagePath;
                string LabelingDataPath = ImagePath.Replace(".png", ".txt");
                string LabelingData = "";

                // Get image dimensions
                int imageWidth = 0;
                int imageHeight = 0;
                try
                {
                    using (Bitmap bitmap = new Bitmap(ImagePath))
                    {
                        imageWidth = bitmap.Width;
                        imageHeight = bitmap.Height;
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions, such as invalid image format or file not found
                    Debug.WriteLine("Error: " + ex.Message);
                    return;
                }

                foreach (var boundingBox in SelectedImageData.BoundingBoxes)
                {
                    // Normalize the coordinates and dimensions
                    double centerX = (boundingBox.X + boundingBox.Width / 2) / imageWidth;
                    double centerY = (boundingBox.Y + boundingBox.Height / 2) / imageHeight;
                    double width = boundingBox.Width / imageWidth;
                    double height = boundingBox.Height / imageHeight;

                    LabelingData += "0 " + centerX.ToString() + " " + centerY.ToString() + " " + width.ToString() + " " + height.ToString() + "\n";
                }

                System.IO.File.WriteAllText(LabelingDataPath, LabelingData);
            }
        }



        public class ImageData
        {

            public string ImagePath { get; set; }
            public string ImageName { get; set; }


            public ImageData(string _ImagePath)
            {
                ImagePath = _ImagePath;
                ImageName = System.IO.Path.GetFileName(ImagePath);
            }

            public ObservableCollection<BoundingBox> BoundingBoxes { get; set; } = new ObservableCollection<BoundingBox>();



        }
        public class BoundingBox
        {
            private double _x;
            private double _y;
            private double _width;
            private double _height;
            public double X
            {
                get { return _x; }
                set
                {
                    if (_x != value)
                    {
                        _x = value;
                        Debug.WriteLine("BoundingBox" + _x);
                    }
                }
            }
            public double Y
            {
                get { return _y; }
                set
                {
                    if (_y != value)
                    {
                        _y = value;
                        Debug.WriteLine("BoundingBox" + _y);
                    }
                }
            }
            public double Width;
            public double Height;

            public BoundingBox()
            {
                //Draw bounding box in canvas

            }


        }








    }
}
