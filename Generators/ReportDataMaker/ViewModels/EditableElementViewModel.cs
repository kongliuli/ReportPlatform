using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReportDataMaker.Models;
using ReportDataMaker.Services;

namespace ReportDataMaker.ViewModels
{
    public class EditableElementViewModel : ElementViewModel
    {
        protected readonly DataBindingService _dataBindingService;
        protected readonly TemplateData _templateData;

        public string Label => !string.IsNullOrEmpty(_element.Label) ? _element.Label : _element.Id ?? "";
        public string DataPath => _element.DataPath;
        public bool IsRequired => _element.IsRequired;

        protected string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    OnValueChanged();
                }
            }
        }

        private string _validationError;
        public string ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ValidationError);

        public EditableElementViewModel(
            ExternalElementBase element,
            DataBindingService dataBindingService,
            TemplateData templateData) : base(element)
        {
            _dataBindingService = dataBindingService;
            _templateData = templateData;
            LoadInitialValue();
        }

        protected virtual void LoadInitialValue()
        {
            if (!string.IsNullOrEmpty(DataPath))
            {
                var existingValue = _dataBindingService.GetValue(_templateData, DataPath);
                _value = existingValue?.ToString() ?? _element.DefaultValue ?? "";
            }
            else
            {
                _value = _element.DefaultValue ?? "";
            }
        }

        protected virtual void OnValueChanged()
        {
            ValidateValue();
        }

        public virtual void ValidateValue()
        {
            if (IsRequired && string.IsNullOrWhiteSpace(Value))
            {
                ValidationError = "此字段为必填项";
            }
            else
            {
                ValidationError = null;
            }
        }

        public virtual void CommitValue()
        {
            if (!string.IsNullOrEmpty(DataPath))
            {
                _dataBindingService.SetValue(_templateData, DataPath, Value);
            }

            if (_element is ExternalTextElement textEl)
            {
                textEl.Text = Value;
            }
        }
    }
}
