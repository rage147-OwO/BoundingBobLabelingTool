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




        private ObservableCollection<ImageData> _imageDatas;

        public ObservableCollection<ImageData> ImageDatas
        {
            get { return _imageDatas; }
            set
            {
                _imageDatas = value;
                OnPropertyChanged(nameof(ImageDatas));
            }
        }
                                        



        private ImageData _selectedImageData;

        public ImageData SelectedImageData
        {
            get { return _selectedImageData; }
            set
            {
                if (_selectedImageData != value)
                {
                    _selectedImageData = value;
                    OnPropertyChanged(nameof(SelectedImageData));
                    Debug.WriteLine(_selectedImageData.ImagePath);
                }
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



        
        //윈도우 브라우저를 열고 폴더를 선택, 이미지를 모두 읽어서 ImageData를 채움
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

        public void HandleMouseWheelScroll(MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double scaleMultiplier = 0.1;
                ScaleRate = e.Delta > 0 ? ScaleRate * (1 + scaleMultiplier) : ScaleRate * (1 - scaleMultiplier);
                Debug.WriteLine(ScaleRate.ToString());
            }
        }


        public void SelectChange(int selectedIndex)
        {
            if (selectedIndex >= 0 && selectedIndex < ImageDatas.Count)
            {
                SelectedImageData = ImageDatas[selectedIndex];
            }
        }
        
        public void SelectUp(){
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

















        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void
            OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Debug.WriteLine(propertyName + " was set");
        }
    }

    public class ImageData
    {
        public string ImagePath { get; set; }   
        public string ImageName
        {
            get
            {
                return System.IO.Path.GetFileName(ImagePath);
            }
        }

        public ImageData(string _ImagePath)
        {
            ImagePath = _ImagePath;
        }

    }




}
