using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Crosspac.Core.Execution;

namespace Crosspac.App.ViewModels;

/// <summary>
/// Shared sink for all pac output. Subscribes to <see cref="IPacRunner.OutputReceived"/>
/// (raised off the UI thread) and marshals every line back onto the UI thread — this is
/// the single place in the app that crosses that boundary.
/// </summary>
public sealed partial class CommandLogViewModel : ViewModelBase
{
    public ObservableCollection<LogEntry> Entries { get; } = new();

    public void Attach(IPacRunner runner) =>
        runner.OutputReceived += (_, line) => Append(line);

    private void Append(PacOutputLine line) =>
        Dispatcher.UIThread.Post(() => Entries.Add(new LogEntry(
            line.Timestamp.ToString("HH:mm:ss"),
            line.Text,
            line.IsError)));

    [RelayCommand]
    private void Clear() => Entries.Clear();
}

public sealed record LogEntry(string Time, string Text, bool IsError);
