using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RD.Controls;

public enum MessageBoxType
{
    Information,
    Warning,
    Error,
    Success,
    Question
}

public enum MessageBoxButtons
{
    OK,
    OKCancel,
    YesNo,
    YesNoCancel,
    Custom
}

public partial class CustomMessageBox : Window
{
    #region Dependency Properties
    
    public static readonly DependencyProperty MessageTitleProperty =
        DependencyProperty.Register("MessageTitle", typeof(string), typeof(CustomMessageBox), 
            new PropertyMetadata("Message"));

    public string MessageTitle
    {
        get { return (string)GetValue(MessageTitleProperty); }
        set { SetValue(MessageTitleProperty, value); }
    }

    #endregion

    #region Properties

    public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
    public string MessageContent { get; set; } = string.Empty;
    public MessageBoxType MessageType { get; set; } = MessageBoxType.Information;
    public MessageBoxButtons ButtonType { get; set; } = MessageBoxButtons.OK;
    public List<(string Text, MessageBoxResult Result, bool IsDefault, bool IsCancel)> CustomButtons { get; set; } 
        = new List<(string, MessageBoxResult, bool, bool)>();

    #endregion

    #region Constructor

    public CustomMessageBox()
    {
        InitializeComponent();
        Loaded += CustomMessageBox_Loaded;
    }

    #endregion

    #region Event Handlers

    private void CustomMessageBox_Loaded(object sender, RoutedEventArgs e)
    {
        SetMessageIcon();
        SetMessageText();
        CreateButtons();
        
        // Focus the default button or first button
        var defaultButton = ButtonsPanel.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault(b => b.IsDefault);
        (defaultButton ?? ButtonsPanel.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault())?.Focus();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Result = MessageBoxResult.Cancel;
        DialogResult = false;
        Close();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is MessageBoxResult result)
        {
            Result = result;
            DialogResult = result != MessageBoxResult.Cancel && result != MessageBoxResult.No;
            Close();
        }
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        switch (e.Key)
        {
            case Key.Escape:
                var cancelButton = ButtonsPanel.Children.OfType<System.Windows.Controls.Button>()
                    .FirstOrDefault(b => (MessageBoxResult)b.Tag == MessageBoxResult.Cancel);
                if (cancelButton != null)
                {
                    Button_Click(cancelButton, new RoutedEventArgs());
                }
                break;
                
            case Key.Enter:
                var defaultButton = ButtonsPanel.Children.OfType<System.Windows.Controls.Button>().FirstOrDefault(b => b.IsDefault);
                if (defaultButton != null)
                {
                    Button_Click(defaultButton, new RoutedEventArgs());
                }
                break;
        }
    }

    #endregion

    #region Private Methods

    private void SetMessageIcon()
    {
        var (icon, color) = MessageType switch
        {
            MessageBoxType.Information => ("\uE946", "#0078D4"), // Info icon
            MessageBoxType.Warning => ("\uE7BA", "#FF8C00"),     // Warning icon
            MessageBoxType.Error => ("\uE783", "#E74C3C"),       // Error icon
            MessageBoxType.Success => ("\uE73E", "#27AE60"),     // Checkmark icon
            MessageBoxType.Question => ("\uE9CE", "#8E44AD"),    // Question icon
            _ => ("\uE946", "#0078D4")
        };

        MessageIcon.Text = icon;
        MessageIcon.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
    }

    private void SetMessageText()
    {
        MessageText.Text = MessageContent;
    }

    private void CreateButtons()
    {
        ButtonsPanel.Children.Clear();

        if (ButtonType == MessageBoxButtons.Custom && CustomButtons.Any())
        {
            CreateCustomButtons();
        }
        else
        {
            CreateStandardButtons();
        }
    }

    private void CreateStandardButtons()
    {
        var buttons = ButtonType switch
        {
            MessageBoxButtons.OK => new[] { ("OK", MessageBoxResult.OK, true, true) },
            MessageBoxButtons.OKCancel => new[] 
            { 
                ("OK", MessageBoxResult.OK, true, false),
                ("Cancel", MessageBoxResult.Cancel, false, true)
            },
            MessageBoxButtons.YesNo => new[] 
            { 
                ("Yes", MessageBoxResult.Yes, true, false),
                ("No", MessageBoxResult.No, false, true)
            },
            MessageBoxButtons.YesNoCancel => new[] 
            { 
                ("Yes", MessageBoxResult.Yes, true, false),
                ("No", MessageBoxResult.No, false, false),
                ("Cancel", MessageBoxResult.Cancel, false, true)
            },
            _ => new[] { ("OK", MessageBoxResult.OK, true, true) }
        };

        foreach (var (text, result, isDefault, isCancel) in buttons)
        {
            CreateButton(text, result, isDefault, isCancel);
        }
    }

    private void CreateCustomButtons()
    {
        foreach (var (text, result, isDefault, isCancel) in CustomButtons)
        {
            CreateButton(text, result, isDefault, isCancel);
        }
    }

    private void CreateButton(string text, MessageBoxResult result, bool isDefault, bool isCancel)
    {
        var button = new System.Windows.Controls.Button
        {
            Content = text,
            Tag = result,
            IsDefault = isDefault,
            IsCancel = isCancel,
            MinWidth = 80,
            Margin = new Thickness(8, 0, 0, 0),
            Style = result == MessageBoxResult.Cancel || result == MessageBoxResult.No 
                ? FindResource("FluentSecondaryButtonStyle") as Style 
                : FindResource("FluentButtonStyle") as Style
        };

        button.Click += Button_Click;
        ButtonsPanel.Children.Add(button);
    }

    #endregion

    #region Static Show Methods

    public static MessageBoxResult Show(string message)
    {
        return Show(message, "Message", MessageBoxType.Information, MessageBoxButtons.OK);
    }

    public static MessageBoxResult Show(string message, string title)
    {
        return Show(message, title, MessageBoxType.Information, MessageBoxButtons.OK);
    }

    public static MessageBoxResult Show(string message, string title, MessageBoxType type)
    {
        return Show(message, title, type, MessageBoxButtons.OK);
    }

    public static MessageBoxResult Show(string message, string title, MessageBoxType type, MessageBoxButtons buttons)
    {
        return Show(message, title, type, buttons, System.Windows.Application.Current.MainWindow);
    }

    public static MessageBoxResult Show(string message, string title, MessageBoxType type, MessageBoxButtons buttons, Window owner)
    {
        var messageBox = new CustomMessageBox
        {
            MessageTitle = title,
            MessageContent = message,
            MessageType = type,
            ButtonType = buttons,
            Owner = owner
        };

        messageBox.ShowDialog();
        return messageBox.Result;
    }

    public static MessageBoxResult ShowCustom(string message, string title, MessageBoxType type, 
        params (string Text, MessageBoxResult Result, bool IsDefault, bool IsCancel)[] customButtons)
    {
        return ShowCustom(message, title, type, System.Windows.Application.Current.MainWindow, customButtons);
    }

    public static MessageBoxResult ShowCustom(string message, string title, MessageBoxType type, Window owner,
        params (string Text, MessageBoxResult Result, bool IsDefault, bool IsCancel)[] customButtons)
    {
        var messageBox = new CustomMessageBox
        {
            MessageTitle = title,
            MessageContent = message,
            MessageType = type,
            ButtonType = MessageBoxButtons.Custom,
            CustomButtons = customButtons.ToList(),
            Owner = owner
        };

        messageBox.ShowDialog();
        return messageBox.Result;
    }

    #endregion
}
