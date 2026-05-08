using System.Collections.ObjectModel;
using System.Linq;
using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;

namespace ReportDataMaker.ViewModels
{
    public class TemplatePreviewViewModel : ViewModelBase
    {
        public readonly ExternalTemplateDefinition _template;

        // 毫米转像素的转换系数 (96 DPI)
        private const double MM_TO_PX = 3.7795275591;

        public double CanvasWidth => _template.PageWidth * MM_TO_PX;
        public double CanvasHeight => _template.PageHeight * MM_TO_PX;

        private ObservableCollection<ElementViewModel> _elements;
        public ObservableCollection<ElementViewModel> Elements
        {
            get => _elements;
            set => SetProperty(ref _elements, value);
        }

        public TemplatePreviewViewModel(ExternalTemplateDefinition template)
        {
            _template = template;
            LoadElements();
        }

        private void LoadElements()
        {
            Elements = new ObservableCollection<ElementViewModel>(
                _template.Elements?.Select(CreateElementViewModel) ?? Enumerable.Empty<ElementViewModel>()
            );
        }

        private ElementViewModel CreateElementViewModel(ExternalElementBase element)
        {
            return element switch
            {
                ExternalTextElement textEl => new TextElementViewModel(textEl),
                ExternalDropdownElement dropdownEl => new DropdownElementViewModel(dropdownEl),
                ExternalNumberElement numberEl => new NumberElementViewModel(numberEl),
                ExternalDateElement dateEl => new DateElementViewModel(dateEl),
                ExternalLineElement lineEl => new LineElementViewModel(lineEl),
                ExternalTableElement tableEl => new TableElementViewModel(tableEl),
                _ => new ElementViewModel(element)
            };
        }

        public void RefreshElements()
        {
            LoadElements();
        }
    }
}
