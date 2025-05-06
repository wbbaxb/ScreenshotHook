using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ScreenshotHook.Presentation.ObservableObjects;
using ScreenshotHook.Presentation.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ScreenshotHook.Presentation.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private ProcessInfoObservableObject _processInfo;
        private string _filterText;
        private WatermarkObservableObject _watermark;

        public ProcessInfoObservableObject ProcessInfo
        {
            get { return _processInfo; }
            set { _processInfo = value; OnPropertyChanged(); }
        }

        public string FilterText
        {
            get { return _filterText; }
            set { _filterText = value; OnPropertyChanged(); }
        }

        public WatermarkObservableObject Watermark
        {
            get { return _watermark; }
            set { _watermark = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProcessInfoObservableObject> ProcessInfos { get; set; }

        public ObservableCollection<int> FontSizes { get; set; }

        public ObservableCollection<string> FontFamilies { get; set; }

        public ICollectionView FilteredProcessInfos { get; }

        public MainWindowViewModel()
        {
            FilterText = string.Empty;
            ProcessInfos = new ObservableCollection<ProcessInfoObservableObject>();

            FilteredProcessInfos = CollectionViewSource.GetDefaultView(ProcessInfos);

            FilteredProcessInfos.Filter = obj =>
            {
                var item = obj as ProcessInfoObservableObject;

                var text = FilterText.Trim();

                if (string.IsNullOrEmpty(text)) return true;

                if (item.ProcessName.Equals(text, StringComparison.OrdinalIgnoreCase) || item.ProcessId.ToString().Equals(text))
                {
                    ProcessInfo = item;
                    return true;
                }

                return false;
            };

            Watermark = new WatermarkObservableObject()
            {
                Text = "Test",
                ColorR = 128,
                ColorG = 128,
                ColorB = 128,
                ColorA = 255,
                FontSize = 20,
                FontFamily = "Microsoft YaHei"
            };

            InitializeFontSizes();

            InitializeFontFamilies();

            GetProcessesAsync().ContinueWith(processes =>
            {
                Application.Current.Dispatcher.Invoke(() => BindingProcesses(processes.Result));
            });
        }

        private void InitializeFontFamilies()
        {
            FontFamilies = new ObservableCollection<string>();
            foreach (var fontFamily in System.Windows.Media.Fonts.SystemFontFamilies)
            {
                FontFamilies.Add(fontFamily.Source);
            }
        }

        private void InitializeFontSizes()
        {
            FontSizes = new ObservableCollection<int>();

            for (int i = 6; i <= 50; i++)
            {
                FontSizes.Add(i);
            }
        }

        public ICommand RefreshCommand => new RelayCommand(async () =>
        {
            var processes = await GetProcessesAsync();
            BindingProcesses(processes);
        });

        public ICommand FilterCommand => new RelayCommand(FilteredProcessInfos.Refresh);

        public ICommand HookCommand => new RelayCommand(Hook);

        public ICommand UnHookCommand => new RelayCommand(UnHook);

        public ICommand MinsizeCommand => new RelayCommand(() => Application.Current.MainWindow.WindowState = WindowState.Minimized);

        public ICommand CloseCommand => new RelayCommand(Application.Current.Shutdown);

        private async Task<Process[]> GetProcessesAsync()
        {
            return await Task.Run(Process.GetProcesses);
        }

        private void BindingProcesses(Process[] processes)
        {
            ProcessInfos.Clear();

            foreach (var item in processes)
            {
                ProcessInfos.Add(new ProcessInfoObservableObject()
                {
                    ProcessId = item.Id,
                    ProcessName = item.ProcessName
                });
            }

            ProcessInfo = FilteredProcessInfos.Cast<ProcessInfoObservableObject>().FirstOrDefault();
        }

        private void Hook()
        {
            if (ProcessInfo == null)
            {
                MessageBox.Show("Please select a process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var watermarkData = new
            {
                Text = Watermark.Text,
                FontName = Watermark.FontFamily,
                FontSize = Watermark.FontSize,
                ColorR = Watermark.ColorR,
                ColorG = Watermark.ColorG,
                ColorB = Watermark.ColorB,
                ColorA = Watermark.ColorA,
            };

            string watermarkJson = System.Text.Json.JsonSerializer.Serialize(watermarkData);

            HookApi.Hook(ProcessInfo.ProcessId, watermarkJson);
        }

        private void UnHook()
        {
            //TODO: 取消注入逻辑
        }
    }
}