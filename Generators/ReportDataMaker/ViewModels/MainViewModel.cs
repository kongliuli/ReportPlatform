using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly TemplateLoaderService _templateLoader;
        private readonly DataExportService _dataExportService;
        private readonly DataBindingService _dataBindingService;
        private ExternalTemplateDefinition _currentTemplate;
        private TemplateData _templateData;

        private string _statusText = "就绪";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _templateInfo = "未加载模板";
        public string TemplateInfo
        {
            get => _templateInfo;
            set => SetProperty(ref _templateInfo, value);
        }

        private TemplatePreviewViewModel _previewViewModel;
        public TemplatePreviewViewModel PreviewViewModel
        {
            get => _previewViewModel;
            set => SetProperty(ref _previewViewModel, value);
        }

        private bool _isTemplateLoaded;
        public bool IsTemplateLoaded
        {
            get => _isTemplateLoaded;
            set
            {
                if (SetProperty(ref _isTemplateLoaded, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand LoadTemplateCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ShowPreviewCommand { get; }
        public ICommand ShowDataEntryCommand { get; }

        public MainViewModel()
        {
            _templateLoader = new TemplateLoaderService();
            _dataExportService = new DataExportService();
            _dataBindingService = new DataBindingService();
            _templateData = new TemplateData();

            LoadTemplateCommand = new RelayCommand(ExecuteLoadTemplate);
            ExportDataCommand = new RelayCommand(ExecuteExportData, CanExportData);
            ShowPreviewCommand = new RelayCommand(ExecuteShowPreview, CanShowPreview);
            ShowDataEntryCommand = new RelayCommand(ExecuteShowDataEntry, CanShowDataEntry);
        }

        private async void ExecuteLoadTemplate(object parameter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "选择模板文件"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                StatusText = "正在加载模板...";

                var jsonContent = await Task.Run(() => File.ReadAllText(dialog.FileName));

                if (_templateLoader.IsExternalTemplateFormat(jsonContent))
                {
                    _currentTemplate = await Task.Run(() => _templateLoader.LoadExternalTemplateFromContent(jsonContent));
                    _templateLoader.ClassifyElements(_currentTemplate, new HashSet<string>());

                    var fixedCount = _currentTemplate.Elements?.Count(el => el.Group == ElementGroup.Fixed) ?? 0;
                    var editableCount = _currentTemplate.Elements?.Count(el => el.Group == ElementGroup.Editable) ?? 0;
                    var adapterCount = _currentTemplate.Elements?.Count(el => el.Group == ElementGroup.DataAdapter) ?? 0;

                    PreviewViewModel = new TemplatePreviewViewModel(_currentTemplate);
                    TemplateInfo = $"模板: {_currentTemplate.Name} (Fixed:{fixedCount} Editable:{editableCount} Adapter:{adapterCount})";
                    StatusText = "模板加载成功";
                    IsTemplateLoaded = true;
                }
                else
                {
                    MessageBox.Show("当前仅支持外部模板格式！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusText = "模板格式不支持";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"加载失败: {ex.Message}";
                MessageBox.Show($"加载模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExportData(object parameter)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                Title = "导出数据文件"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                _dataExportService.ExportData(_templateData, dialog.FileName);
                StatusText = "数据导出成功";
                MessageBox.Show("数据导出成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteShowPreview(object parameter)
        {
            if (PreviewViewModel != null)
            {
                PreviewViewModel.RefreshElements();
                StatusText = "预览已刷新";
            }
        }

        private void ExecuteShowDataEntry(object parameter)
        {
            var dataEntryViewModel = new DataEntryViewModel(_currentTemplate, _dataBindingService, _templateData);
            dataEntryViewModel.CloseRequested += OnDataEntryCloseRequested;

            var dataEntryWindow = new Views.DataEntryWindow
            {
                DataContext = dataEntryViewModel,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            dataEntryWindow.ShowDialog();
        }

        private void OnDataEntryCloseRequested(object sender, bool success)
        {
            if (success)
            {
                StatusText = "数据录入成功";
                PreviewViewModel?.RefreshElements();
            }
        }

        private bool CanExportData(object parameter) => IsTemplateLoaded;
        private bool CanShowPreview(object parameter) => IsTemplateLoaded;
        private bool CanShowDataEntry(object parameter) => IsTemplateLoaded;
    }
}
