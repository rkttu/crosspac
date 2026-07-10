using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crosspac.App.Services;
using Crosspac.Core.Execution;

namespace Crosspac.App.ViewModels;

/// <summary>
/// Shared sink for all pac output. Subscribes to <see cref="IPacRunner.OutputReceived"/>
/// (raised off the UI thread) and marshals every line back onto the UI thread — this is
/// the single place in the app that crosses that boundary.
/// </summary>
public sealed partial class CommandLogViewModel : ViewModelBase
{
    private readonly IClipboardService _clipboard;
    private readonly IStoragePickerService _picker;

    public CommandLogViewModel(IClipboardService clipboard, IStoragePickerService picker)
    {
        _clipboard = clipboard;
        _picker = picker;
    }

    public ObservableCollection<LogEntry> Entries { get; } = new();

    /// <summary>When true, the view keeps the newest line in view as output streams in.</summary>
    [ObservableProperty] private bool _autoScroll = true;

    /// <summary>Transient feedback (copied / saved / errors) shown in the log header.</summary>
    [ObservableProperty] private string? _status;

    public void Attach(IPacRunner runner) =>
        runner.OutputReceived += (_, line) => Append(line);

    private void Append(PacOutputLine line) =>
        Dispatcher.UIThread.Post(() => Entries.Add(new LogEntry(
            line.Timestamp.ToString("HH:mm:ss"),
            line.Text,
            line.IsError)));

    [RelayCommand]
    private void Clear()
    {
        Entries.Clear();
        Status = null;
    }

    [RelayCommand]
    private async Task CopyAsync()
    {
        if (Entries.Count == 0)
        {
            Status = "Nothing to copy.";
            return;
        }

        await _clipboard.SetTextAsync(BuildLogText());
        Status = $"Copied {Entries.Count} line(s) to clipboard.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Entries.Count == 0)
        {
            Status = "Nothing to save.";
            return;
        }

        var path = await _picker.PickSaveFilePathAsync(
            "Save command log", $"crosspac-log-{DateTime.Now:yyyyMMdd-HHmmss}.log", "log");
        if (path is null)
            return;

        try
        {
            await File.WriteAllTextAsync(path, BuildLogText());
            Status = $"Saved to {path}";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private string BuildLogText()
    {
        var sb = new StringBuilder();
        foreach (var entry in Entries)
            sb.Append(entry.Time).Append("  ").AppendLine(entry.Text);
        return sb.ToString();
    }
}

public sealed record LogEntry(string Time, string Text, bool IsError);
