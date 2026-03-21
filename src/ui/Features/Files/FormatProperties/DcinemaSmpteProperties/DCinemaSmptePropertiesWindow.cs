using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaSmpteProperties;

public class DCinemaSmptePropertiesWindow : Window
{
    public DCinemaSmptePropertiesWindow(DCinemaSmptePropertiesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(Window.TitleProperty, new Binding(nameof(vm.WindowTitle))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
        });
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 550;
        MinHeight = 500;
        vm.Window = this;
        DataContext = vm;

        var labelWidth = 150;
        var lang = Se.Language.File.PropertiesDCinema;

        // General section
        var checkBoxGenerateIdAuto = UiUtil.MakeCheckBox(lang.GenerateIdAuto, vm, nameof(vm.GenerateIdAuto));

        var labelSubtitleId = UiUtil.MakeLabel(lang.SubtitleId).WithMinWidth(labelWidth);
        var textBoxSubtitleId = UiUtil.MakeTextBox(200, vm, nameof(vm.SubtitleId));
        var buttonGenerateId = UiUtil.MakeButton(lang.GenerateId, vm.GenerateSubtitleIdCommand);
        var panelSubtitleId = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelSubtitleId, textBoxSubtitleId, buttonGenerateId }
        };

        var labelMovieTitle = UiUtil.MakeLabel(lang.MovieTitle).WithMinWidth(labelWidth);
        var textBoxMovieTitle = UiUtil.MakeTextBox(300, vm, nameof(vm.MovieTitle));
        var panelMovieTitle = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelMovieTitle, textBoxMovieTitle }
        };

        var labelReelNumber = UiUtil.MakeLabel(lang.ReelNumber).WithMinWidth(labelWidth);
        var numericUpDownReelNumber = UiUtil.MakeNumericUpDownInt(1, 250, 1, 120, vm, nameof(vm.ReelNumber));
        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language).WithMinWidth(80);
        var comboBoxLanguage = UiUtil.MakeComboBox<string>(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithMinWidth(100);
        var panelReelNumber = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelReelNumber, numericUpDownReelNumber, labelLanguage, comboBoxLanguage }
        };

        var labelIssueDate = UiUtil.MakeLabel(lang.IssueDate).WithMinWidth(labelWidth);
        var textBoxIssueDate = UiUtil.MakeTextBox(200, vm, nameof(vm.IssueDate));
        var buttonToday = UiUtil.MakeButton(lang.Now, vm.SetTodayIssueDateCommand);
        var panelIssueDate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelIssueDate, textBoxIssueDate, buttonToday }
        };

        var labelEditRate = UiUtil.MakeLabel(lang.EditRate).WithMinWidth(labelWidth);
        var textBoxEditRate = UiUtil.MakeTextBox(80, vm, nameof(vm.EditRate));
        var labelTimeCodeRate = UiUtil.MakeLabel(lang.TimeCodeRate).WithMinWidth(100);
        var comboBoxTimeCodeRate = UiUtil.MakeComboBox<string>(vm.TimeCodeRates, vm, nameof(vm.SelectedTimeCodeRate)).WithMinWidth(80);
        var panelEditRate = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelEditRate, textBoxEditRate, labelTimeCodeRate, comboBoxTimeCodeRate }
        };

        var labelStartTime = UiUtil.MakeLabel(lang.StartTime).WithMinWidth(labelWidth);
        var textBoxStartTime = UiUtil.MakeTextBox(100, vm, nameof(vm.StartTime));
        var panelStartTime = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelStartTime, textBoxStartTime }
        };

        // Font section
        var labelFontId = UiUtil.MakeLabel(lang.FontId).WithMinWidth(labelWidth);
        var textBoxFontId = UiUtil.MakeTextBox(200, vm, nameof(vm.FontId));
        var panelFontId = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontId, textBoxFontId }
        };

        var labelFontUri = UiUtil.MakeLabel(lang.FontUri).WithMinWidth(labelWidth);
        var textBoxFontUri = UiUtil.MakeTextBox(200, vm, nameof(vm.FontUri));
        var buttonGenerateFontUri = UiUtil.MakeButton(lang.Generate, vm.GenerateFontUriCommand);
        var panelFontUri = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontUri, textBoxFontUri, buttonGenerateFontUri }
        };

        var labelFontColor = UiUtil.MakeLabel(lang.FontColor).WithMinWidth(labelWidth);
        var buttonFontColor = UiUtil.MakeButton(lang.ChooseColor, vm.ChooseFontColorCommand).WithMinWidth(100);
        var panelFontColorPreview = new Border
        {
            Width = 30,
            Height = 20,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black)
        };
        panelFontColorPreview.Bind(Border.BackgroundProperty, new Binding(nameof(vm.FontColor))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter()
        });

        var panelFontColor = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontColor, buttonFontColor, panelFontColorPreview }
        };

        var labelFontEffect = UiUtil.MakeLabel(lang.FontEffect).WithMinWidth(labelWidth);
        var comboBoxFontEffect = UiUtil.MakeComboBox<string>(vm.FontEffects, vm, nameof(vm.SelectedFontEffect)).WithMinWidth(100);
        var panelFontEffect = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontEffect, comboBoxFontEffect }
        };

        var labelFontEffectColor = UiUtil.MakeLabel(lang.EffectColor).WithMinWidth(labelWidth);
        var buttonFontEffectColor = UiUtil.MakeButton(lang.ChooseColor, vm.ChooseFontEffectColorCommand).WithMinWidth(100);
        var panelFontEffectColorPreview = new Border
        {
            Width = 30,
            Height = 20,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black)
        };
        panelFontEffectColorPreview.Bind(Border.BackgroundProperty, new Binding(nameof(vm.FontEffectColor))
        {
            Source = vm,
            Mode = BindingMode.OneWay,
            Converter = new ColorToBrushConverter()
        });

        var panelFontEffectColor = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontEffectColor, buttonFontEffectColor, panelFontEffectColorPreview }
        };

        var labelFontSize = UiUtil.MakeLabel(lang.FontSize).WithMinWidth(labelWidth);
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownInt(0, 250, 42, 120, vm, nameof(vm.FontSize));
        var labelTopBottomMargin = UiUtil.MakeLabel(lang.TopBottomMargin).WithMinWidth(100);
        var numericUpDownTopBottomMargin = UiUtil.MakeNumericUpDownInt(1, 50, 8, 120, vm, nameof(vm.TopBottomMargin));
        var panelFontSize = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFontSize, numericUpDownFontSize, labelTopBottomMargin, numericUpDownTopBottomMargin }
        };

        var labelFadeUpTime = UiUtil.MakeLabel(lang.FadeUpTime).WithMinWidth(labelWidth);
        var numericUpDownFadeUpTime = UiUtil.MakeNumericUpDownInt(0, 50, 0, 120, vm, nameof(vm.FadeUpTime));
        var labelFadeUpFrames = UiUtil.MakeLabel(lang.Frames);
        var labelFadeDownTime = UiUtil.MakeLabel(lang.FadeDownTime).WithMinWidth(80);
        var numericUpDownFadeDownTime = UiUtil.MakeNumericUpDownInt(0, 50, 0, 120, vm, nameof(vm.FadeDownTime));
        var labelFadeDownFrames = UiUtil.MakeLabel(lang.Frames);
        var panelFadeUpTime = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 10,
            Children = { labelFadeUpTime, numericUpDownFadeUpTime, labelFadeUpFrames, labelFadeDownTime, numericUpDownFadeDownTime, labelFadeDownFrames }
        };

        // Font GroupBox
        var fontGroupBox = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = lang.Font, FontWeight = FontWeight.Bold },
                    panelFontId,
                    panelFontUri,
                    panelFontColor,
                    panelFontEffect,
                    panelFontEffectColor,
                    panelFontSize,
                    panelFadeUpTime
                }
            }
        };

        var scrollViewer = new ScrollViewer
        {
            Content = new StackPanel
            {
                Spacing = 10,
                Margin = UiUtil.MakeWindowMargin(),
                Children =
                {
                    checkBoxGenerateIdAuto,
                    panelSubtitleId,
                    panelMovieTitle,
                    panelReelNumber,
                    panelIssueDate,
                    panelEditRate,
                    panelStartTime,
                    fontGroupBox
                }
            }
        };

        var buttonImport = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        var buttonExport = UiUtil.MakeButton(lang.Export, vm.ExportCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonImport, buttonExport, buttonOk, buttonCancel);

        var mainPanel = new DockPanel
        {
            LastChildFill = true,
            Children = { buttonPanel, scrollViewer }
        };
        DockPanel.SetDock(buttonPanel, Dock.Bottom);

        Content = mainPanel;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private class ColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.White;
        }
    }
}
