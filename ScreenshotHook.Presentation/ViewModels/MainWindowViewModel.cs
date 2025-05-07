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
                Text = Properties.Settings.Default.WatermarkText,
                ColorR = Properties.Settings.Default.WatermarkColorR,
                ColorG = Properties.Settings.Default.WatermarkColorG,
                ColorB = Properties.Settings.Default.WatermarkColorB,
                ColorA = Properties.Settings.Default.WatermarkColorA,
                FontSize = Properties.Settings.Default.WatermarkFontSize,
                FontFamily = Properties.Settings.Default.WatermarkFontFamily,
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

            SaveSettingsIfChanged();

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

        private void SaveSettingsIfChanged()
        {
            bool needSave = false;

            needSave |= UpdateSettingIfChanged(Watermark.Text, Properties.Settings.Default.WatermarkText,
                v => Properties.Settings.Default.WatermarkText = v);

            needSave |= UpdateSettingIfChanged(Watermark.ColorR, Properties.Settings.Default.WatermarkColorR,
                v => Properties.Settings.Default.WatermarkColorR = v);

            needSave |= UpdateSettingIfChanged(Watermark.ColorG, Properties.Settings.Default.WatermarkColorG,
                v => Properties.Settings.Default.WatermarkColorG = v);

            needSave |= UpdateSettingIfChanged(Watermark.ColorB, Properties.Settings.Default.WatermarkColorB,
                v => Properties.Settings.Default.WatermarkColorB = v);

            needSave |= UpdateSettingIfChanged(Watermark.ColorA, Properties.Settings.Default.WatermarkColorA,
                v => Properties.Settings.Default.WatermarkColorA = v);

            needSave |= UpdateSettingIfChanged(Watermark.FontSize, Properties.Settings.Default.WatermarkFontSize,
                v => Properties.Settings.Default.WatermarkFontSize = v);

            needSave |= UpdateSettingIfChanged(Watermark.FontFamily, Properties.Settings.Default.WatermarkFontFamily,
                v => Properties.Settings.Default.WatermarkFontFamily = v);

            if (needSave)
            {
                Properties.Settings.Default.Save();
            }
        }

        private bool UpdateSettingIfChanged<T>(T newValue, T currentValue, Action<T> updateAction)
        {
            if (!object.Equals(newValue, currentValue))
            {
                updateAction(newValue);
                return true;
            }
            return false;
        }

        private void UnHook()
        {
            if (ProcessInfo == null)
            {
                MessageBox.Show("Please select a process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            HookApi.UnHook(ProcessInfo.ProcessId);
        }
    }
}