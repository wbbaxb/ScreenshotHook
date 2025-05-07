using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ScreenshotHook.Presentation.ObservableObjects
{
    public class ProcessInfoObservableObject : ObservableObject
    {
        private int _processId;
        private string _processName;
        private bool _isHooked;

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
    }
}