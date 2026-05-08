namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public abstract class ElementBase
{
    private string? _id;
    
    public string Id
    {
        get => _id ??= GenerateShortId();
        set => _id = value;
    }
    
    private static string GenerateShortId() => Guid.NewGuid().ToString("N");
    
    public double X { get; set; }
    
    public double Y { get; set; }
    
    public double Width { get; set; }
    
    public double Height { get; set; }
    
    public double Rotation { get; set; }
    
    public int ZIndex { get; set; }
    
    public string? Tooltip { get; set; }
    
    public bool IsLocked { get; set; }
}
