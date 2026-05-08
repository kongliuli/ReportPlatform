using System;
using System.Collections.Generic;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    public class DataPathResolver
    {
        public object Resolve(string dataPath, ReportDataContext context)
        {
            if (context == null || string.IsNullOrEmpty(dataPath))
                return null;

            string[] segments = dataPath.Split('.');
            if (segments.Length == 0)
                return null;

            object current = null;

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];

                if (i == 0)
                {
                    current = ResolveRoot(segment, context);
                    if (current == null)
                        return null;
                }
                else if (current is Dictionary<string, object> dict)
                {
                    if (TryParseArrayAccess(segment, out string arrayKey, out int arrayIndex))
                    {
                        if (dict.TryGetValue(arrayKey, out var arrValue))
                        {
                            current = AccessArrayElement(arrValue, arrayIndex);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        dict.TryGetValue(segment, out current);
                    }
                }
                else if (current is List<Dictionary<string, object>> itemList)
                {
                    if (TryParseArrayAccess(segment, out string listKey, out int listIndex))
                    {
                        if (listKey == "*")
                        {
                            return CollectAllValues(itemList, segment);
                        }
                        else if (listIndex >= 0 && listIndex < itemList.Count)
                        {
                            var item = itemList[listIndex];
                            string remaining = ExtractRemainingPath(segment);
                            if (!string.IsNullOrEmpty(remaining))
                            {
                                current = ResolveFromDictionary(item, remaining);
                            }
                            else
                            {
                                current = item;
                            }
                            return current;
                        }
                        return null;
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        private object ResolveRoot(string segment, ReportDataContext context)
        {
            return segment.ToLower() switch
            {
                "patient" => context.Patient,
                "report" => context.Report,
                "doctor" => context.Doctor,
                "hospital" => context.Hospital,
                "items" => context.Items,
                _ => null
            };
        }

        private bool TryParseArrayAccess(string segment, out string key, out int index)
        {
            key = segment;
            index = -1;

            int bracketStart = segment.IndexOf('[');
            if (bracketStart < 0)
                return false;

            int bracketEnd = segment.IndexOf(']');
            if (bracketEnd < 0)
                return false;

            key = segment.Substring(0, bracketStart);
            string indexStr = segment.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);

            if (indexStr == "*")
            {
                index = -2;
                return true;
            }

            if (int.TryParse(indexStr, out int parsedIndex))
            {
                index = parsedIndex;
                return true;
            }

            return false;
        }

        private object AccessArrayElement(object arrayValue, int index)
        {
            if (arrayValue is List<Dictionary<string, object>> dictList)
            {
                if (index >= 0 && index < dictList.Count)
                    return dictList[index];
            }
            else if (arrayValue is System.Collections.IList objList)
            {
                if (index >= 0 && index < objList.Count)
                    return objList[index];
            }
            return null;
        }

        private List<object> CollectAllValues(List<Dictionary<string, object>> itemList, string segment)
        {
            var allValues = new List<object>();
            string remaining = ExtractRemainingPath(segment);

            foreach (var item in itemList)
            {
                if (!string.IsNullOrEmpty(remaining))
                {
                    object val;
                    if (item.TryGetValue(remaining, out val))
                        allValues.Add(val);
                }
                else
                {
                    allValues.Add(item);
                }
            }

            return allValues;
        }

        private string ExtractRemainingPath(string segment)
        {
            int bracketEnd = segment.IndexOf(']');
            if (bracketEnd < 0)
                return string.Empty;

            return segment.Substring(bracketEnd + 1).TrimStart('.');
        }

        private object ResolveFromDictionary(Dictionary<string, object> item, string remainingPath)
        {
            if (item == null || string.IsNullOrEmpty(remainingPath))
                return item;

            string[] remainingSegments = remainingPath.Split('.');
            object current = item;

            foreach (string seg in remainingSegments)
            {
                if (current is Dictionary<string, object> dict)
                {
                    if (!dict.TryGetValue(seg, out current))
                        return null;
                }
                else
                {
                    return null;
                }
            }

            return current;
        }
    }
}
