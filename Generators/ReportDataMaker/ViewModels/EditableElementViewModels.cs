using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReportDataMaker.Models;
using ReportDataMaker.Services;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.ViewModels
{
    public class EditableTextViewModel : EditableElementViewModel
    {
        public EditableTextViewModel(
            ExternalTextElement element,
            DataBindingService dataBindingService,
            TemplateData templateData) : base(element, dataBindingService, templateData)
        {
        }

        protected override void LoadInitialValue()
        {
            var textElement = _element as ExternalTextElement;
            if (!string.IsNullOrEmpty(DataPath))
            {
                var existingValue = _dataBindingService.GetValue(_templateData, DataPath);
                _value = existingValue?.ToString() ?? textElement?.Text ?? _element.DefaultValue ?? "";
            }
            else
            {
                _value = textElement?.Text ?? _element.DefaultValue ?? "";
            }
        }
    }

    public class EditableDropdownViewModel : EditableElementViewModel
    {
        private readonly ExternalDropdownElement _dropdownElement;

        public ObservableCollection<string> Options { get; }

        public EditableDropdownViewModel(
            ExternalDropdownElement element,
            DataBindingService dataBindingService,
            TemplateData templateData) : base(element, dataBindingService, templateData)
        {
            _dropdownElement = element;
            Options = new ObservableCollection<string>(element.Options ?? new System.Collections.Generic.List<string>());

            if (!string.IsNullOrEmpty(element.Value))
            {
                Value = element.Value;
            }
            else if (!string.IsNullOrEmpty(element.Placeholder) && Options.Count == 0)
            {
                Options.Add(element.Placeholder);
            }
        }

        public override void CommitValue()
        {
            base.CommitValue();
            _dropdownElement.Value = Value;
        }
    }

    public class EditableNumberViewModel : EditableElementViewModel
    {
        private readonly ExternalNumberElement _numberElement;

        public double? MinValue => _numberElement.MinValue;
        public double? MaxValue => _numberElement.MaxValue;
        public string Unit => _numberElement.Unit;

        public EditableNumberViewModel(
            ExternalNumberElement element,
            DataBindingService dataBindingService,
            TemplateData templateData) : base(element, dataBindingService, templateData)
        {
            _numberElement = element;
        }

        protected override void LoadInitialValue()
        {
            if (!string.IsNullOrEmpty(DataPath))
            {
                var existingValue = _dataBindingService.GetValue(_templateData, DataPath);
                _value = existingValue?.ToString() ?? (_numberElement.Value > 0 ? _numberElement.Value.ToString() : "");
            }
            else
            {
                _value = _numberElement.Value > 0 ? _numberElement.Value.ToString() : "";
            }
        }

        public override void ValidateValue()
        {
            base.ValidateValue();

            if (string.IsNullOrWhiteSpace(Value))
                return;

            if (!double.TryParse(Value, out var numValue))
            {
                ValidationError = "请输入有效的数字";
                return;
            }

            if (MinValue.HasValue && numValue < MinValue.Value)
            {
                ValidationError = $"值不能小于 {MinValue.Value}";
            }
            else if (MaxValue.HasValue && numValue > MaxValue.Value)
            {
                ValidationError = $"值不能大于 {MaxValue.Value}";
            }
        }

        public override void CommitValue()
        {
            base.CommitValue();
            if (double.TryParse(Value, out var numValue))
            {
                _numberElement.Value = numValue;
            }
        }
    }

    public class EditableDateViewModel : EditableElementViewModel
    {
        private readonly ExternalDateElement _dateElement;

        private DateTime? _dateValue;
        public DateTime? DateValue
        {
            get => _dateValue;
            set
            {
                if (SetProperty(ref _dateValue, value))
                {
                    Value = value?.ToString("yyyy-MM-dd");
                }
            }
        }

        public EditableDateViewModel(
            ExternalDateElement element,
            DataBindingService dataBindingService,
            TemplateData templateData) : base(element, dataBindingService, templateData)
        {
            _dateElement = element;
        }

        protected override void LoadInitialValue()
        {
            string dateString = null;

            if (!string.IsNullOrEmpty(DataPath))
            {
                var existingValue = _dataBindingService.GetValue(_templateData, DataPath);
                dateString = existingValue?.ToString() ?? _dateElement.Value;
            }
            else
            {
                dateString = _dateElement.Value;
            }

            if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var dt))
            {
                _dateValue = dt;
                _value = dt.ToString("yyyy-MM-dd");
            }
        }

        public override void CommitValue()
        {
            base.CommitValue();
            _dateElement.Value = Value;
        }
    }
}
