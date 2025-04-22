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

        public ObservableCollection<ProcessInfoObservableObject> ProcessInfos { get; set; }

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

            GetProcessesAsync().ContinueWith(processes =>
            {
                Application.Current.Dispatcher.Invoke(() => BindingProcesses(processes.Result));
            });
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
            if(ProcessInfo == null)
            {
                MessageBox.Show("Please select a process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var watermarkData = new
            {
                Text = "Test",
                FontName = "SimSun",
                FontSize = 18,
                ColorR = 128,
                ColorG = 128,
                ColorB = 128,
                ColorA = 255,
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