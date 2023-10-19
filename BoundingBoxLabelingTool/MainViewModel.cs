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
        public void LoadDirectory()
        {
            Debug.WriteLine("LoadDirectory");
            ImageDataManager = new ImageDataManager();
            ImageDataManager.LoadDirectory();
            OnPropertyChanged(nameof(ImageDataManager));
        }
        public void ChangeSelectedIndex(int selectedIndex)
        {
            Debug.WriteLine("ChangeSelectedIndex"+selectedIndex);
            ImageDataManager.SelectChange(selectedIndex);
            OnPropertyChanged(nameof(ImageDataManager));
        }



        public void SelectUp()
        {
            Debug.WriteLine("SelectUp");
            ImageDataManager.SelectUp();
            OnPropertyChanged(nameof(ImageDataManager));
        }

        public void SelectDown()
        {
            Debug.Write("SelectDown");
            ImageDataManager.SelectDown();
            OnPropertyChanged(nameof(ImageDataManager));
        }


        public void AddBoundingBox(double x, double y, double width, double height, int ClassId)
        {
            Debug.WriteLine("AddBoundingBox");
            ImageDataManager.AddBoundingBox(x, y, width, height, ClassId);
            OnPropertyChanged(nameof(ImageDataManager));
        }
        public void MoveBoundingBox(int index, double x, double y)
        {
            Debug.WriteLine("MoveBoundingBox");
            ImageDataManager.MoveBoundingBox(index, x, y);
            OnPropertyChanged(nameof(ImageDataManager));
        }
        public void DeleteBoundingBox(int index)
        {
            Debug.WriteLine("DeleteBoundingBox");
            ImageDataManager.SelectedImageData.BoundingBoxes.RemoveAt(index);
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

        public void AddBoundingBox(double x, double y, double width, double height, int C)
        {
            SelectedImageData.BoundingBoxes.Add(new BoundingBox() { X = x, Y = y, Width = width, Height = height, ClassId=C});
            SaveLabelingData();
        }
        public void MoveBoundingBox(int index, double x, double y)
        {
            if(index>=0 && index < SelectedImageData.BoundingBoxes.Count)
            {
                SelectedImageData.BoundingBoxes[index].X = x;
                SelectedImageData.BoundingBoxes[index].Y = y;
                SaveLabelingData();
            }
        }

        public int ImageCount
        {
            get
            {
                return ImageDatas.Count;
            }
        }

        public void LoadDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string FilePath = dialog.SelectedPath;
                string[] jpgfiles = System.IO.Directory.GetFiles(FilePath, "*.jpg");
                string[] pngfiles = System.IO.Directory.GetFiles(FilePath, "*.png");
                string[] files = jpgfiles.Concat(pngfiles).ToArray();
                ImageDatas = new ObservableCollection<ImageData>();
                foreach (string file in files)
                {
                    ImageDatas.Add(new ImageData(file));
                }
                SelectedImageData = ImageDatas[0];


                // Load Labeling Data
               files = System.IO.Directory.GetFiles(FilePath, "*.txt");
                foreach (ImageData data in ImageDatas)
                {
                    // Create a new bounding box collection for the current ImageData object
                    data.BoundingBoxes = new ObservableCollection<BoundingBox>();

                    // Check if the corresponding txt file exists
                    string labelingDataPath = data.ImagePath.Replace(".jpg", ".txt");
                    labelingDataPath = labelingDataPath.Replace(".png", ".txt");
                    if (Array.Exists(files, file => file.Equals(labelingDataPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Load the image to get its width and height
                        using (Bitmap bitmap = new Bitmap(data.ImagePath))
                        {
                            data.ImageWidth = bitmap.Width;
                            data.ImageHeight = bitmap.Height;
                        }

                        string[] lines = File.ReadAllLines(labelingDataPath);
                        foreach (string line in lines)
                        {
                            string[] values = line.Split(' ');

                            if (values.Length == 5) // Assuming the format is "class_id centerX centerY width height"
                            {
                                int classId = int.Parse(values[0]);
                                double centerX = double.Parse(values[1]);
                                double centerY = double.Parse(values[2]);
                                double width = double.Parse(values[3]);
                                double height = double.Parse(values[4]);

                                // Convert centerX and centerY to top-left corner coordinates
                                double x = (centerX - width / 2) * data.ImageWidth;
                                double y = (centerY - height / 2) * data.ImageHeight;

                                // Create and add a new bounding box
                                BoundingBox boundingBox = new BoundingBox
                                {
                                    X = x,
                                    Y = y,
                                    Width = width * data.ImageWidth,
                                    Height = height * data.ImageHeight,
                                    ClassId = classId
                                };

                                data.BoundingBoxes.Add(boundingBox);
                            }
                        }
                    }
                }





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
        }

        public void SaveLabelingData()
        {
            // Save SelectedImageData Labeling Data as Yolo Format
            if (SelectedImageData != null)
            {
                string ImagePath = SelectedImageData.ImagePath;
                string LabelingDataPath = ImagePath.Replace(".jpg", ".txt");
                LabelingDataPath = LabelingDataPath.Replace(".png", ".txt");
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
                    int classId = boundingBox.ClassId;

                    LabelingData += classId+" " + centerX.ToString() + " " + centerY.ToString() + " " + width.ToString() + " " + height.ToString() + "\n";
                }

                System.IO.File.WriteAllText(LabelingDataPath, LabelingData);
            }
        }



        public class ImageData
        {

            public int ImageWidth { get; set; }
            public int ImageHeight { get; set; }

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

            public double X { get; set; }
            public double Y { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }

            public int ClassId { get; set; }

            public BoundingBox()
            {
                ClassId = 0;
            }
        }








    }
}
