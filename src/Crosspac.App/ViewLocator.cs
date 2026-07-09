using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Crosspac.App.ViewModels;

namespace Crosspac.App;

/// <summary>
/// Maps a <c>*ViewModel</c> to its matching <c>*View</c> by naming convention, so a
/// ContentControl bound to a view model materializes the right view automatically.
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        return type is not null
            ? (Control)Activator.CreateInstance(type)!
            : new TextBlock { Text = "View not found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
