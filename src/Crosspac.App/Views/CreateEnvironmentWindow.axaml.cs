using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Crosspac.Core.Models;

namespace Crosspac.App.Views;

public partial class CreateEnvironmentWindow : Window
{
    public CreateEnvironmentWindow()
    {
        InitializeComponent();
        TypeCombo.ItemsSource = Enum.GetValues<EnvironmentType>();
        TypeCombo.SelectedItem = EnvironmentType.Sandbox;
    }

    private void OnCreate(object? sender, RoutedEventArgs e)
    {
        var type = TypeCombo.SelectedItem is EnvironmentType t ? t : EnvironmentType.Sandbox;

        // Blank fields are passed through as-is; the service trims and omits empty ones so pac
        // applies its own defaults.
        Close(new EnvironmentCreateOptions(
            type,
            Name: NameBox.Text,
            Domain: DomainBox.Text,
            Currency: CurrencyBox.Text,
            Region: RegionBox.Text,
            Language: LanguageBox.Text));
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);
}
