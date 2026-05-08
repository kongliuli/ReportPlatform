using ReportDataMaker.Infrastructure;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;
using Xinglin.ReportEditor.Contracts.Enums;

namespace ReportDataMaker.ViewModels
{
    public class ElementViewModel : ViewModelBase
    {
        public readonly ReportExternalElementBase _element;

        // 毫米转像素的转换系数 (96 DPI)
        private const double MM_TO_PX = 3.7795275591;

        public string Id => _element.Id;
        public double X => _element.X * MM_TO_PX;
        public double Y => _element.Y * MM_TO_PX;
        public double Width => _element.Width * MM_TO_PX;
        public double Height => _element.Height * MM_TO_PX;
        public bool IsVisible => _element.IsVisible;
        public ElementGroup Group => _element.Group;
        public string FontFamily => _element.FontFamily;
        public double FontSize => _element.FontSize;
        public string ForegroundColor => _element.ForegroundColor;
        public string BackgroundColor => _element.BackgroundColor;

        public ElementViewModel(ReportExternalElementBase element)
        {
            _element = element;
        }
    }
}
