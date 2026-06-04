namespace Xinglin.ReportEditor.Core.SharedInterfaces;

public interface IJsonTemplateSerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
    object Deserialize(string json, Type type);
}
