using System;
using System.Collections.Generic;
using System.Reflection;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services
{
    public class DataBindingService
    {
        private readonly DataPathResolver _pathResolver;

        public DataBindingService()
        {
            _pathResolver = new DataPathResolver();
        }

        public void ApplyBindings(ExternalTemplateDefinition template, ReportDataContext context)
        {
            if (template?.DataBindings == null || context == null)
                return;

            foreach (var binding in template.DataBindings)
            {
                if (string.IsNullOrEmpty(binding.ElementId) || string.IsNullOrEmpty(binding.DataPath))
                    continue;

                var element = FindElementById(template.Elements, binding.ElementId);
                if (element == null)
                    continue;

                var value = _pathResolver.Resolve(binding.DataPath, context);
                ApplyBindingToElement(element, binding, value);
            }
        }

        public void ApplyInlineBinding(ExternalElementBase element, ReportDataContext context)
        {
            if (element == null || !element.IsDataBound || string.IsNullOrEmpty(element.DataPath) || context == null)
                return;

            var value = _pathResolver.Resolve(element.DataPath, context);
            ApplyValueToElement(element, value);
        }

        private ExternalElementBase FindElementById(List<ExternalElementBase> elements, string elementId)
        {
            if (elements == null)
                return null;

            foreach (var element in elements)
            {
                if (element.Id == elementId)
                    return element;

                if (element is ExternalContainerElement containerEl)
                {
                    var found = FindElementById(containerEl.Children, elementId);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private void ApplyBindingToElement(ExternalElementBase element, DataBindingDefinition binding, object value)
        {
            if (value == null && !string.IsNullOrEmpty(binding.DefaultValue))
            {
                ApplyValueToElement(element, binding.DefaultValue);
                return;
            }

            if (value == null)
                return;

            var stringValue = FormatValue(value, binding.FormatString);
            ApplyValueToElement(element, stringValue, binding.BindingType);
        }

        private void ApplyValueToElement(ExternalElementBase element, object value, string bindingType = "text")
        {
            if (element == null || value == null)
                return;

            var stringValue = value?.ToString();

            switch (bindingType?.ToLower())
            {
                case "text":
                    if (element is ExternalTextElement textEl)
                        textEl.Text = stringValue;
                    else if (element is ExternalNumberElement numEl && double.TryParse(stringValue, out var num))
                        numEl.Value = num;
                    else if (element is ExternalDateElement dateEl)
                        dateEl.Value = stringValue;
                    else if (element is ExternalDropdownElement dropEl)
                        dropEl.Value = stringValue;
                    else
                        element.DefaultValue = stringValue;
                    break;

                case "image":
                    if (element is ExternalImageElement imgEl)
                        imgEl.Src = stringValue;
                    break;

                case "visibility":
                    if (stringValue != null)
                        element.IsVisible = !string.Equals(stringValue, "false", StringComparison.OrdinalIgnoreCase)
                                        && !string.Equals(stringValue, "0");
                    break;

                case "repeat":
                    break;

                default:
                    if (element is ExternalTextElement tEl)
                        tEl.Text = stringValue;
                    else if (element is ExternalNumberElement nEl && double.TryParse(stringValue, out var n))
                        nEl.Value = n;
                    else
                        element.DefaultValue = stringValue;
                    break;
            }
        }

        private string FormatValue(object value, string formatString)
        {
            if (string.IsNullOrEmpty(formatString))
                return value?.ToString();

            if (value is DateTime dt)
                return dt.ToString(formatString);
            else if (value is IFormattable formattable)
                return formattable.ToString(formatString, null);

            return string.Format(formatString, value);
        }

        public object GetValue(object data, string path)
        {
            if (data == null || string.IsNullOrEmpty(path))
                return null;

            string[] parts = path.Split('.');
            object current = data;

            foreach (string part in parts)
            {
                if (current == null)
                    return null;

                if (part.Contains('['))
                {
                    int bracketIndex = part.IndexOf('[');
                    string propertyName = part.Substring(0, bracketIndex);
                    int index = int.Parse(part.Substring(bracketIndex + 1, part.Length - bracketIndex - 2));

                    current = GetPropertyValue(current, propertyName);
                    if (current is IList<object> list)
                    {
                        if (index >= 0 && index < list.Count)
                            current = list[index];
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    current = GetPropertyValue(current, part);
                }
            }

            return current;
        }

        public void SetValue(object data, string path, object value)
        {
            if (data == null || string.IsNullOrEmpty(path))
                return;

            string[] parts = path.Split('.');
            object current = data;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];

                if (part.Contains('['))
                {
                    int bracketIndex = part.IndexOf('[');
                    string propertyName = part.Substring(0, bracketIndex);
                    int index = int.Parse(part.Substring(bracketIndex + 1, part.Length - bracketIndex - 2));

                    current = GetPropertyValue(current, propertyName);
                    if (current is IList<object> list)
                    {
                        if (index >= 0 && index < list.Count)
                            current = list[index];
                        else
                            return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    current = GetPropertyValue(current, part);
                }

                if (current == null)
                    return;
            }

            string lastPart = parts[parts.Length - 1];

            if (lastPart.Contains('['))
            {
                int bracketIndex = lastPart.IndexOf('[');
                string propertyName = lastPart.Substring(0, bracketIndex);
                int index = int.Parse(lastPart.Substring(bracketIndex + 1, lastPart.Length - bracketIndex - 2));

                object collection = GetPropertyValue(current, propertyName);
                if (collection is IList<object> list)
                {
                    if (index >= 0 && index < list.Count)
                    {
                    }
                }
            }
            else
            {
                SetPropertyValue(current, lastPart, value);
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty(propertyName);

            if (property != null)
                return property.GetValue(obj);

            return null;
        }

        private void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj == null)
                return;

            Type type = obj.GetType();
            PropertyInfo property = type.GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                Type propertyType = property.PropertyType;
                object convertedValue = Convert.ChangeType(value, propertyType);
                property.SetValue(obj, convertedValue);
            }
        }
    }
}
