using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ReportDataMaker.Models;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.Services
{
    /// <summary>
    /// 输入控件生成服务 - 为可编辑元素生成输入控件
    /// </summary>
    public class InputControlGenerator
    {
        private readonly CanvasRenderer _canvasRenderer;

        // Modern color palette
        private static readonly Color LabelColor = Color.FromRgb(100, 116, 139);
        private static readonly Color BorderColor = Color.FromRgb(226, 232, 240);
        private static readonly Color FocusBorderColor = Color.FromRgb(37, 99, 235);
        private static readonly Color InputBgColor = Color.FromRgb(255, 255, 255);
        private static readonly Color CardBgColor = Color.FromRgb(255, 255, 255);

        public InputControlGenerator(CanvasRenderer canvasRenderer)
        {
            _canvasRenderer = canvasRenderer;
        }

        public FrameworkElement GenerateControl(ReportExternalElementBase element, Action<string> onValueChanged)
        {
            return element switch
            {
                ExternalTextElement textEl => GenerateTextControl(textEl, onValueChanged),
                ExternalNumberElement numEl => GenerateNumberControl(numEl, onValueChanged),
                ExternalDateElement dateEl => GenerateDateControl(dateEl, onValueChanged),
                ExternalDropdownElement dropEl => GenerateDropdownControl(dropEl, onValueChanged),
                ExternalCheckboxElement cbEl => GenerateCheckboxControl(cbEl, onValueChanged),
                ExternalRadioElement radioEl => GenerateRadioControl(radioEl, onValueChanged),
                ExternalImageElement imgEl => GenerateImageControl(imgEl, onValueChanged),
                ExternalBarcodeElement barcodeEl => GenerateBarcodeControl(barcodeEl, onValueChanged),
                ExternalQrCodeElement qrEl => GenerateQrCodeControl(qrEl, onValueChanged),
                ExternalSignatureElement sigEl => GenerateSignatureControl(sigEl, onValueChanged),
                _ => null
            };
        }

        private static Style CreateTextBoxStyle()
        {
            var style = new Style(typeof(TextBox));

            style.Setters.Add(new Setter(Control.BackgroundProperty, new SolidColorBrush(InputBgColor)));
            style.Setters.Add(new Setter(Control.BorderBrushProperty, new SolidColorBrush(BorderColor)));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
            style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
            style.Setters.Add(new Setter(Control.FontSizeProperty, 13.0));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));

            var template = new ControlTemplate(typeof(TextBox));
            var borderElement = new FrameworkElementFactory(typeof(Border));
            borderElement.Name = "border";
            borderElement.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderElement.SetBinding(Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderElement.SetBinding(Border.BorderThicknessProperty,
                new System.Windows.Data.Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderElement.SetBinding(Border.PaddingProperty,
                new System.Windows.Data.Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });
            borderElement.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

            var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
            scrollViewer.Name = "PART_ContentHost";
            borderElement.AppendChild(scrollViewer);

            template.VisualTree = borderElement;

            var mouseOverTrigger = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty,
                new SolidColorBrush(FocusBorderColor), "border"));
            template.Triggers.Add(mouseOverTrigger);

            var focusedTrigger = new Trigger { Property = UIElement.IsFocusedProperty, Value = true };
            focusedTrigger.Setters.Add(new Setter(Border.BorderBrushProperty,
                new SolidColorBrush(FocusBorderColor), "border"));
            focusedTrigger.Setters.Add(new Setter(Border.BorderThicknessProperty,
                new Thickness(1.5), "border"));
            template.Triggers.Add(focusedTrigger);

            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            return style;
        }

        private static TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = string.IsNullOrEmpty(text) ? "" : text + ":",
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(LabelColor),
                Margin = new Thickness(0, 0, 0, 4),
                TextWrapping = TextWrapping.Wrap
            };
        }

        private static Border CreateControlCard(UIElement child)
        {
            return new Border
            {
                Child = child,
                Background = new SolidColorBrush(CardBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8)
            };
        }

        private static Grid CreateControlContainer()
        {
            var container = new Grid
            {
                Margin = new Thickness(2, 2, 2, 2)
            };

            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            return container;
        }

        private FrameworkElement GenerateTextControl(ExternalTextElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var textBox = new TextBox
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = element.Text ?? element.DefaultValue ?? ""
            };

            textBox.TextChanged += (s, e) =>
            {
                element.Text = textBox.Text;
                onValueChanged?.Invoke(textBox.Text);
            };

            Grid.SetRow(textBox, 1);
            container.Children.Add(textBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateNumberControl(ExternalNumberElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var textBox = new TextBox
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = element.Value > 0 ? element.Value.ToString() : ""
            };

            textBox.PreviewTextInput += (s, e) =>
            {
                e.Handled = !IsNumericInput(e.Text);
            };

            textBox.TextChanged += (s, e) =>
            {
                if (double.TryParse(textBox.Text, out var value))
                {
                    element.Value = value;
                    onValueChanged?.Invoke(textBox.Text);
                }
            };

            Grid.SetRow(textBox, 1);
            container.Children.Add(textBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateDateControl(ExternalDateElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var datePicker = new DatePicker
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(element.Value) && DateTime.TryParse(element.Value, out var date))
            {
                datePicker.SelectedDate = date;
            }

            datePicker.SelectedDateChanged += (s, e) =>
            {
                element.Value = datePicker.SelectedDate?.ToString("yyyy-MM-dd");
                onValueChanged?.Invoke(element.Value);
            };

            Grid.SetRow(datePicker, 1);
            container.Children.Add(datePicker);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateDropdownControl(ExternalDropdownElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var comboBox = new ComboBox
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            if (element.Options != null)
            {
                foreach (var option in element.Options)
                    comboBox.Items.Add(option);
            }

            if (!string.IsNullOrEmpty(element.Value))
            {
                comboBox.SelectedItem = element.Value;
            }

            comboBox.SelectionChanged += (s, e) =>
            {
                element.Value = comboBox.SelectedItem?.ToString();
                onValueChanged?.Invoke(element.Value);
            };

            Grid.SetRow(comboBox, 1);
            container.Children.Add(comboBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateCheckboxControl(ExternalCheckboxElement element, Action<string> onValueChanged)
        {
            var container = new Grid { Margin = new Thickness(2, 2, 2, 2) };

            var checkBox = new CheckBox
            {
                Content = GetLabelText(element),
                IsChecked = element.Checked,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin = new Thickness(0, 4, 0, 4)
            };

            checkBox.Checked += (s, e) =>
            {
                element.Checked = true;
                onValueChanged?.Invoke("true");
            };
            checkBox.Unchecked += (s, e) =>
            {
                element.Checked = false;
                onValueChanged?.Invoke("false");
            };

            container.Children.Add(checkBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateRadioControl(ExternalRadioElement element, Action<string> onValueChanged)
        {
            var container = new Grid { Margin = new Thickness(2, 2, 2, 2) };

            var radioButton = new RadioButton
            {
                Content = GetLabelText(element),
                GroupName = element.GroupName ?? "default",
                IsChecked = element.Checked,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin = new Thickness(0, 4, 0, 4),
                Tag = element.Value
            };

            radioButton.Checked += (s, e) =>
            {
                element.Checked = true;
                onValueChanged?.Invoke(element.Value ?? "on");
            };

            container.Children.Add(radioButton);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateImageControl(ExternalImageElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var button = new Button
            {
                Content = "选择图片文件",
                Height = 30,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(14, 6, 14, 6),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            if (!string.IsNullOrEmpty(element.Src))
            {
                button.Content = System.IO.Path.GetFileName(element.Src);
            }

            button.Click += (s, e) =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "图片文件 (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "选择图片文件"
                };
                if (dialog.ShowDialog() == true)
                {
                    element.Src = dialog.FileName;
                    onValueChanged?.Invoke(dialog.FileName);
                    button.Content = System.IO.Path.GetFileName(dialog.FileName);
                }
            };

            Grid.SetRow(button, 1);
            container.Children.Add(button);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateBarcodeControl(ExternalBarcodeElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var textBox = new TextBox
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = element.Value ?? ""
            };

            textBox.TextChanged += (s, e) =>
            {
                element.Value = textBox.Text;
                onValueChanged?.Invoke(textBox.Text);
            };

            Grid.SetRow(textBox, 1);
            container.Children.Add(textBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateQrCodeControl(ExternalQrCodeElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var textBox = new TextBox
            {
                Background = new SolidColorBrush(InputBgColor),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8, 6, 8, 6),
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                Text = element.Value ?? ""
            };

            textBox.TextChanged += (s, e) =>
            {
                element.Value = textBox.Text;
                onValueChanged?.Invoke(textBox.Text);
            };

            Grid.SetRow(textBox, 1);
            container.Children.Add(textBox);

            return CreateControlCard(container);
        }

        private FrameworkElement GenerateSignatureControl(ExternalSignatureElement element, Action<string> onValueChanged)
        {
            var container = CreateControlContainer();

            container.Children.Add(CreateLabel(GetLabelText(element)));
            Grid.SetRow((UIElement)container.Children[0], 0);

            var signatureArea = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1, 1, 1, 2),
                CornerRadius = new CornerRadius(4),
                MinHeight = 56
            };

            signatureArea.Child = new TextBlock
            {
                Text = "点击此处签名（需手写板支持）",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                FontSize = 12
            };

            Grid.SetRow(signatureArea, 1);
            container.Children.Add(signatureArea);

            return CreateControlCard(container);
        }

        private string GetLabelText(ReportExternalElementBase element)
        {
            var label = !string.IsNullOrEmpty(element.Label)
                ? element.Label
                : !string.IsNullOrEmpty(element.DataPath)
                    ? element.DataPath
                    : GetElementDisplayText(element);

            if (element.IsRequired && !label.EndsWith(" *"))
                label += " *";

            return label;
        }

        private static string GetElementDisplayText(ReportExternalElementBase element)
        {
            if (element is ExternalTextElement t && !string.IsNullOrEmpty(t.Text))
                return t.Text;
            if (!string.IsNullOrEmpty(element.Id))
                return element.Id;
            return "未命名";
        }

        private bool IsNumericInput(string text)
        {
            return double.TryParse(text, out _) || text == "." || text == "-";
        }
    }
}
