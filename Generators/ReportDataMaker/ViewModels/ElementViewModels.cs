using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.ViewModels
{
    public class TextElementViewModel : ElementViewModel
    {
        private readonly ExternalTextElement _textElement;

        public string Text => _textElement.Text;
        public string TextAlignment => _textElement.TextAlignment;

        public TextElementViewModel(ExternalTextElement element) : base(element)
        {
            _textElement = element;
        }
    }

    public class LineElementViewModel : ElementViewModel
    {
        private readonly ExternalLineElement _lineElement;

        public string LineColor => _lineElement.LineColor;
        public double LineWidth => _lineElement.LineWidth;
        public double StartX => _lineElement.StartX;
        public double StartY => _lineElement.StartY;
        public double EndX => _lineElement.EndX;
        public double EndY => _lineElement.EndY;

        public LineElementViewModel(ExternalLineElement element) : base(element)
        {
            _lineElement = element;
        }
    }

    public class DropdownElementViewModel : ElementViewModel
    {
        private readonly ExternalDropdownElement _dropdownElement;

        public string Value => _dropdownElement.Value;
        public string Placeholder => _dropdownElement.Placeholder;

        public DropdownElementViewModel(ExternalDropdownElement element) : base(element)
        {
            _dropdownElement = element;
        }
    }

    public class NumberElementViewModel : ElementViewModel
    {
        private readonly ExternalNumberElement _numberElement;

        public double Value => _numberElement.Value;
        public string Format => _numberElement.Format;
        public string Unit => _numberElement.Unit;

        public NumberElementViewModel(ExternalNumberElement element) : base(element)
        {
            _numberElement = element;
        }
    }

    public class DateElementViewModel : ElementViewModel
    {
        private readonly ExternalDateElement _dateElement;

        public string Value => _dateElement.Value;
        public string Format => _dateElement.Format;

        public DateElementViewModel(ExternalDateElement element) : base(element)
        {
            _dateElement = element;
        }
    }

    public class TableElementViewModel : ElementViewModel
    {
        private readonly ExternalTableElement _tableElement;

        public int Rows => _tableElement.Rows;
        public int Columns => _tableElement.Columns;

        public TableElementViewModel(ExternalTableElement element) : base(element)
        {
            _tableElement = element;
        }
    }

    public class AdapterElementViewModel : ElementViewModel
    {
        public string Label => _element.Label ?? _element.Id ?? _element.DataPath ?? "";
        public string DisplayValue
        {
            get
            {
                if (_element is ExternalTextElement txtEl)
                    return txtEl.Text ?? "";
                return _element.DefaultValue ?? "";
            }
        }

        public AdapterElementViewModel(ReportExternalElementBase element) : base(element)
        {
        }
    }
}
