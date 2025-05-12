using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ScreenshotHook.Presentation.Enums;
using ScreenshotHook.Presentation.ObservableObjects;
using ScreenshotHook.Presentation.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

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
            set
            {
                _processInfo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanHook));
                OnPropertyChanged(nameof(CanUnhook));
            }
        }

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                _filterText = value;
                OnPropertyChanged();
                FilteredProcessInfos?.Refresh();
            }
        }

        public WatermarkObservableObject Watermark
        {
            get { return _watermark; }
            set { _watermark = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ProcessInfoObservableObject> ProcessInfos { get; set; }

        public ObservableCollection<ProcessInfoObservableObject> HookedProcesses { get; set; }

        public ObservableCollection<int> FontSizes { get; set; }

        public ObservableCollection<string> FontFamilies { get; set; }

        public ICollectionView FilteredProcessInfos { get; }

        public bool CanHook => ProcessInfo != null && !ProcessInfo.IsHooked;

        public bool CanUnhook => ProcessInfo != null && ProcessInfo.IsHooked;

        public MainWindowViewModel()
        {
            FilterText = Properties.Settings.Default.ProcessName;
            ProcessInfos = new ObservableCollection<ProcessInfoObservableObject>();
            HookedProcesses = new ObservableCollection<ProcessInfoObservableObject>();
            FilteredProcessInfos = CollectionViewSource.GetDefaultView(ProcessInfos);

            FilteredProcessInfos.Filter = obj =>
            {
                var item = obj as ProcessInfoObservableObject;

                var text = FilterText.Trim();

                if (string.IsNullOrEmpty(text)) return true;

                if (item.ProcessName.Contains(text, StringComparison.OrdinalIgnoreCase) || item.ProcessId.ToString().Equals(text))
                {
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
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentXmlLanguage = XmlLanguage.GetLanguage(currentCulture.IetfLanguageTag);

            foreach (var fontFamily in Fonts.SystemFontFamilies)
            {
                if (fontFamily.FamilyNames.TryGetValue(currentXmlLanguage, out string displayName))
                {
                    FontFamilies.Add(displayName);
                }
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

        public ICommand HookCommand => new RelayCommand(Hook);

        public ICommand UnHookCommand => new RelayCommand(() => UnHook(ProcessInfo));

        public ICommand ReHookCommand => new RelayCommand(() =>
        {
            UnHook(ProcessInfo);
            Hook();
        });

        public ICommand SelectCommand => new RelayCommand<ProcessInfoObservableObject>(p =>
        {
            if (p != null)
            {
                ProcessInfo = p;
            }
        });

        public ICommand RemoveCommand => new RelayCommand<ProcessInfoObservableObject>(UnHook);

        public ICommand MinsizeCommand => new RelayCommand(() => Application.Current.MainWindow.WindowState = WindowState.Minimized);

        public ICommand CloseCommand => new RelayCommand(Application.Current.MainWindow.Close);

        public ICommand ShowCommand => new RelayCommand(() =>
        {
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.Activate();
        });

        public ICommand ShutdownCommand => new RelayCommand(() =>
        {
            var errorList = new List<string>();
            try
            {
                foreach (var processInfo in HookedProcesses)
                {
                    if (processInfo != null && processInfo.IsHooked)
                    {
                        HookApi.UnHook(processInfo.ProcessId, processInfo.Bit == Bit.Bit64);
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(ex.Message);
            }
            finally
            {
                if (errorList.Count > 0)
                {
                    MessageBox.Show("Failed to unhook processes: " + string.Join("\n", errorList));
                }

                Application.Current.Shutdown();
            }
        });

        private async Task<Process[]> GetProcessesAsync()
        {
            return await Task.Run(() =>
            {
                var allProcesses = Process.GetProcesses();
                var currentProcessId = Process.GetCurrentProcess().Id;
                return allProcesses.Where(p =>
                {
                    try
                    {
                        // 排除系统进程
                        if (p.SessionId <= 0)
                        {
                            return false;
                        }

                        // 排除没有主模块的进程
                        if (p.MainModule == null)
                        {
                            return false;
                        }

                        // 排除已退出进程
                        if (p.HasExited)
                        {
                            return false;
                        }

                        // 排除自身
                        if (p.Id == currentProcessId)
                        {
                            return false;
                        }

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }).ToArray();
            });
        }
   
        private void BindingProcesses(Process[] processes)
        {
            var currentProcessesById = ProcessInfos.ToDictionary(p => p.ProcessId);
            var newProcessIds = processes.Select(p => p.Id).ToHashSet();

            // 添加新进程
            foreach (var process in processes)
            {
                if (!currentProcessesById.ContainsKey(process.Id))
                {
                    ProcessInfos.Add(new ProcessInfoObservableObject()
                    {
                        ProcessId = process.Id,
                        ProcessName = process.ProcessName,
                        Bit = Win32.GetProcessBit(process),
                    });
                }
            }

            // 移除已不存在的进程
            var processesToRemove = ProcessInfos.Where(p => !newProcessIds.Contains(p.ProcessId)).ToList();
            foreach (var processToRemove in processesToRemove)
            {
                ProcessInfos.Remove(processToRemove);
            }
        }

        private void Hook()
        {
            if (!CheckProcessIsRunning(ProcessInfo))
            {
                return;
            }

            var watermarkData = new WatermarkObservableObject()
            {
                Text = Watermark.Text,
                FontFamily = Watermark.FontFamily,
                FontSize = Watermark.FontSize,
                ColorR = Watermark.ColorR,
                ColorG = Watermark.ColorG,
                ColorB = Watermark.ColorB,
                ColorA = Watermark.ColorA,
            };

            string watermarkJson = System.Text.Json.JsonSerializer.Serialize(watermarkData);

            if (!HookApi.Hook(ProcessInfo.ProcessId, ProcessInfo.Bit == Bit.Bit64, watermarkJson))
            {
                return;
            }

            ProcessInfo.IsHooked = true;

            OnPropertyChanged(nameof(CanHook));
            OnPropertyChanged(nameof(CanUnhook));

            ProcessInfo.WatermarkObservableObject = watermarkData;

            SaveSettingsIfChanged();

            if (!HookedProcesses.Contains(ProcessInfo))
            {
                HookedProcesses.Add(ProcessInfo);
            }
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

            needSave |= UpdateSettingIfChanged(ProcessInfo.ProcessName, Properties.Settings.Default.ProcessName,
                v => Properties.Settings.Default.ProcessName = v);

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

        private void UnHook(ProcessInfoObservableObject process)
        {
            if (!CheckProcessIsRunning(process))
            {
                return;
            }

            if (!HookApi.UnHook(process.ProcessId, process.Bit == Bit.Bit64))
            {
                return;
            }

            process.IsHooked = false;

            OnPropertyChanged(nameof(CanHook));
            OnPropertyChanged(nameof(CanUnhook));

            if (HookedProcesses.Contains(process))
            {
                HookedProcesses.Remove(process);
            }
        }

        private bool CheckProcessIsRunning(ProcessInfoObservableObject processInfo)
        {
            if (processInfo == null)
            {
                MessageBox.Show("Please select a process first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                Process.GetProcessById(processInfo.ProcessId);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The process is not running. Click OK to refresh the list.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshCommand.Execute(null);
                HookedProcesses.Remove(processInfo);
                return false;
            }

            return true;
        }
    }
}