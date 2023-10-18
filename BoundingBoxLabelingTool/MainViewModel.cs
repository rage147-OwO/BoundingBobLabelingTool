using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace BoundingBoxLabelingTool
{
    internal class MainViewModel : INotifyPropertyChanged
    {


        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }


        //바인딩, ObservableCollection은 UI에 바로 반영됨(양방향)
        private ObservableCollection<string> _imageList = new ObservableCollection<string>();
        public ObservableCollection<string> ImageList 
        {
            get { return _imageList; }
            set
            {
                _imageList = value;
                OnPropertyChanged(nameof(ImageList));
            }
        }



        private string _selectedImagePath;
        public string SelectedImagePath
        {
            get { return _selectedImagePath; }
            set
            {
                _selectedImagePath = value;
                OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

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






        public MainViewModel()
        {

        }


        public void LoadDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                FilePath = dialog.SelectedPath;
                string[] imageFiles = System.IO.Directory.GetFiles(FilePath, "*.png");
                ImageList.Clear();
                foreach (var imageFile in imageFiles)
                {
                    ImageList.Add(imageFile);


                }

            }


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


        public void HandleSelectionChanged(string selectedImagePath)
        {
            SelectedImagePath = selectedImagePath;

        }











        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void
            OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Debug.WriteLine(propertyName + " was set");
        }
    }

    internal class ImageManager
    {
        private string _directoryPath;
        private List<ImageData> _imageDatas;
        public ImageManager(string _directoryPath) {
            this._directoryPath = _directoryPath;
            _imageDatas = new List<ImageData>();

            


        }


    }

    public class ImageData
    {

    }


}
