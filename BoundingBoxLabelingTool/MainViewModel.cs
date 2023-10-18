using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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



        private double _scaleRate = 1.0;
        public double ScaleRate //바인딩
        {
            get { return _scaleRate; }
            set
            {
                if (_scaleRate != value)
                {
                    _scaleRate = value;
                    OnPropertyChanged(nameof(ScaleRate));
                    Debug.WriteLine(_scaleRate);
                }
            }
        }




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

        public void HandleMouseWheelScroll(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double scaleMultiplier = 0.1;
                ScaleRate = e.Delta > 0 ? ScaleRate * (1 + scaleMultiplier) : ScaleRate * (1 - scaleMultiplier);
                Debug.WriteLine(ScaleRate.ToString());
            }
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
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }








}
