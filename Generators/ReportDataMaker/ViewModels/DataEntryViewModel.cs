using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels
{
    public class DataEntryViewModel : ViewModelBase
    {
        private readonly ExternalTemplateDefinition _template;
        private readonly DataBindingService _dataBindingService;
        private readonly TemplateData _templateData;

        public string Title => $"数据录入 - {_template.Name}";

        private ObservableCollection<EditableElementViewModel> _editableElements;
        public ObservableCollection<EditableElementViewModel> EditableElements
        {
            get => _editableElements;
            set => SetProperty(ref _editableElements, value);
        }

        private ObservableCollection<ElementViewModel> _fixedElements;
        public ObservableCollection<ElementViewModel> FixedElements
        {
            get => _fixedElements;
            set => SetProperty(ref _fixedElements, value);
        }

        private ObservableCollection<AdapterElementViewModel> _adapterElements;
        public ObservableCollection<AdapterElementViewModel> AdapterElements
        {
            get => _adapterElements;
            set => SetProperty(ref _adapterElements, value);
        }

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> CloseRequested;

        public DataEntryViewModel(
            ExternalTemplateDefinition template,
            DataBindingService dataBindingService,
            TemplateData templateData)
        {
            _template = template;
            _dataBindingService = dataBindingService;
            _templateData = templateData;

            ConfirmCommand = new RelayCommand(ExecuteConfirm);
            CancelCommand = new RelayCommand(ExecuteCancel);

            LoadElements();
        }

        private void LoadElements()
        {
            var sorted = _template.Elements?
                .OrderBy(el => el.Y)
                .ThenBy(el => el.X)
                .ToList() ?? new System.Collections.Generic.List<ExternalElementBase>();

            FixedElements = new ObservableCollection<ElementViewModel>(
                sorted.Where(el => el.Group == ElementGroup.Fixed)
                      .Select(el => new ElementViewModel(el))
            );

            EditableElements = new ObservableCollection<EditableElementViewModel>(
                sorted.Where(el => el.Group == ElementGroup.Editable)
                      .Select(CreateEditableViewModel)
            );

            AdapterElements = new ObservableCollection<AdapterElementViewModel>(
                sorted.Where(el => el.Group == ElementGroup.DataAdapter)
                      .Select(el => new AdapterElementViewModel(el))
            );
        }

        private EditableElementViewModel CreateEditableViewModel(ExternalElementBase element)
        {
            return element switch
            {
                ExternalDropdownElement dropdown => new EditableDropdownViewModel(dropdown, _dataBindingService, _templateData),
                ExternalNumberElement number => new EditableNumberViewModel(number, _dataBindingService, _templateData),
                ExternalDateElement date => new EditableDateViewModel(date, _dataBindingService, _templateData),
                ExternalTextElement text => new EditableTextViewModel(text, _dataBindingService, _templateData),
                _ => new EditableElementViewModel(element, _dataBindingService, _templateData)
            };
        }

        private void ExecuteConfirm(object parameter)
        {
            // 验证所有字段
            bool hasErrors = false;
            foreach (var element in EditableElements)
            {
                element.ValidateValue();
                if (element.HasError)
                {
                    hasErrors = true;
                }
            }

            if (hasErrors)
            {
                MessageBox.Show("请修正输入错误后再提交！", "验证失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 提交所有值
            foreach (var element in EditableElements)
            {
                element.CommitValue();
            }

            MessageBox.Show("数据录入成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseRequested?.Invoke(this, true);
        }

        private void ExecuteCancel(object parameter)
        {
            CloseRequested?.Invoke(this, false);
        }
    }
}
