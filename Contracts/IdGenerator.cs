using NanoidDotNet;

namespace Xinglin.ReportEditor.Contracts;

public static class IdGenerator
{
    public static string NewId() => Nanoid.Generate(size: 21);
}
