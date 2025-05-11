using Microsoft.Toolkit.Mvvm.ComponentModel;
using ScreenshotHook.Presentation.Enums;

namespace ScreenshotHook.Presentation.ObservableObjects
{
    public class ProcessInfoObservableObject : ObservableObject
    {
        private int _processId;
        private string _processName;
        private bool _isHooked;
        private WatermarkObservableObject _watermarkObservableObject;
        private Bit _bit;

        public int ProcessId
        {
            get { return _processId; }
            set { _processId = value; OnPropertyChanged(); }
        }

        public string ProcessName
        {
            get { return _processName; }
            set { _processName = value; OnPropertyChanged(); }
        }

        public bool IsHooked
        {
            get { return _isHooked; }
            set { _isHooked = value; OnPropertyChanged(); }
        }

        public WatermarkObservableObject WatermarkObservableObject
        {
            get { return _watermarkObservableObject; }
            set { _watermarkObservableObject = value; OnPropertyChanged(); }
        }

        public Bit Bit
        {
            get { return _bit; }
            set { _bit = value; OnPropertyChanged(); }
        }
    }
}