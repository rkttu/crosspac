using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crosspac.App.Services;
using Crosspac.Core.Execution;
using Crosspac.Core.Models;
using Crosspac.Core.Services;

namespace Crosspac.App.ViewModels;

public sealed partial class SolutionsViewModel : ViewModelBase
{
    private readonly ISolutionService _solutions;
    private readonly IContextService _context;
    private readonly IDialogService _dialogs;
    private readonly IStoragePickerService _picker;
    private CancellationTokenSource? _cts;

    public SolutionsViewModel(
        ISolutionService solutions,
        IContextService context,
        IDialogService dialogs,
        IStoragePickerService picker)
    {
        _solutions = solutions;
        _context = context;
        _dialogs = dialogs;
        _picker = picker;
    }

    public ObservableCollection<Solution> Solutions { get; } = new();

    [ObservableProperty] private Solution? _selectedSolution;
    [ObservableProperty] private string _exportPath = "./out";
    [ObservableProperty] private bool _exportManaged = true;
    [ObservableProperty] private string _importPath = "";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _status;

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status = "Loading solutions…";
        try
        {
            Solutions.Clear();
            foreach (var solution in await _solutions.ListAsync(_cts.Token))
                Solutions.Add(solution);
            Status = $"{Solutions.Count} solution(s).";
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (SelectedSolution is null)
        {
            Status = "Select a solution to export first.";
            return;
        }

        await RunAsync($"Exporting {SelectedSolution.UniqueName}…", token =>
            _solutions.ExportAsync(SelectedSolution.UniqueName, ExportPath, ExportManaged, token));
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportPath))
        {
            Status = "Enter the path to a solution .zip to import.";
            return;
        }

        var context = await _context.GetActiveAsync();
        var confirmed = await _dialogs.ConfirmAsync(
            "Confirm solution import",
            $"Import this solution into the live environment?\n\n" +
            $"File:         {ImportPath}\n" +
            $"Environment:  {context.EnvironmentName}\n" +
            $"              {context.EnvironmentUrl}\n" +
            $"Profile:      {context.ProfileUser}\n\n" +
            $"This can overwrite existing components and cannot be undone.");

        if (!confirmed)
        {
            Status = "Import cancelled.";
            return;
        }

        await RunAsync("Importing…", token => _solutions.ImportAsync(ImportPath, token), refreshAfter: true);
    }

    [RelayCommand]
    private async Task PublishAsync()
    {
        var context = await _context.GetActiveAsync();
        var confirmed = await _dialogs.ConfirmAsync(
            "Confirm publish",
            $"Publish all customizations to the live environment?\n\n" +
            $"Environment:  {context.EnvironmentName}\n" +
            $"              {context.EnvironmentUrl}\n" +
            $"Profile:      {context.ProfileUser}");

        if (!confirmed)
        {
            Status = "Publish cancelled.";
            return;
        }

        await RunAsync("Publishing…", token => _solutions.PublishAsync(token));
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedSolution is null)
        {
            Status = "Select a solution to delete first.";
            return;
        }

        var context = await _context.GetActiveAsync();
        var confirmed = await _dialogs.ConfirmAsync(
            "Confirm solution delete",
            $"Delete this solution from the live environment?\n\n" +
            $"Solution:     {SelectedSolution.UniqueName}\n" +
            $"Environment:  {context.EnvironmentName}\n" +
            $"              {context.EnvironmentUrl}\n\n" +
            $"This permanently removes the solution and cannot be undone.");

        if (!confirmed)
        {
            Status = "Delete cancelled.";
            return;
        }

        await RunAsync($"Deleting {SelectedSolution.UniqueName}…",
            token => _solutions.DeleteAsync(SelectedSolution.UniqueName, token), refreshAfter: true);
    }

    [RelayCommand]
    private async Task BrowseExportAsync()
    {
        var folder = await _picker.PickFolderAsync();
        if (folder is not null)
            ExportPath = folder;
    }

    [RelayCommand]
    private async Task BrowseImportAsync()
    {
        var file = await _picker.PickZipFileAsync();
        if (file is not null)
            ImportPath = file;
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    /// <summary>Shared execution wrapper: busy state, cancellation, status, optional refresh.</summary>
    private async Task RunAsync(string startStatus, Func<CancellationToken, Task<PacCommandResult>> operation, bool refreshAfter = false)
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status = startStatus;
        try
        {
            var result = await operation(_cts.Token);
            Status = result.Succeeded
                ? $"Done ({result.Duration.TotalSeconds:0.0}s)."
                : $"Failed (exit {result.ExitCode}). See log.";
            if (result.Succeeded && refreshAfter)
                await RefreshAsync();
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
