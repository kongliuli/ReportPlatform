namespace Xinglin.WebReportEditor.Core.SharedInterfaces;

public interface IPdfSharpTemplateRenderer
{
    byte[] RenderToPdf(object templateDefinition);
}
