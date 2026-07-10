using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Crosspac.App.ViewModels;

namespace Crosspac.App.Views;

public partial class CommandLogView : UserControl
{
    private CommandLogViewModel? _viewModel;

    // Set when new content arrives (or auto-scroll is re-enabled); cleared once we have
    // actually reached the bottom. Layout for a newly-added row can take several passes,
    // so we keep re-pinning on each LayoutUpdated until the offset is truly at the end —
    // a single deferred ScrollToEnd races the final layout pass and can stop a line short.
    private bool _pendingScroll;

    public CommandLogView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        LogScroller.LayoutUpdated += OnScrollerLayoutUpdated;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.Entries.CollectionChanged -= OnEntriesChanged;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as CommandLogViewModel;

        if (_viewModel is not null)
        {
            _viewModel.Entries.CollectionChanged += OnEntriesChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Content added before the view attached (or a re-attach) still needs pinning.
            if (_viewModel.AutoScroll && _viewModel.Entries.Count > 0)
                _pendingScroll = true;
        }
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && (_viewModel?.AutoScroll ?? false))
            _pendingScroll = true;
    }

    // Re-enabling auto-scroll should snap straight to the newest line.
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CommandLogViewModel.AutoScroll) && _viewModel!.AutoScroll)
        {
            _pendingScroll = true;
            // Idle re-enable won't invalidate layout on its own, so kick off the first
            // scroll here; the resulting offset change drives the LayoutUpdated retries.
            LogScroller.ScrollToEnd();
        }
    }

    private void OnScrollerLayoutUpdated(object? sender, EventArgs e)
    {
        if (!_pendingScroll || _viewModel?.AutoScroll != true)
            return;

        LogScroller.ScrollToEnd();

        var maxOffset = LogScroller.Extent.Height - LogScroller.Viewport.Height;
        if (LogScroller.Offset.Y >= maxOffset - 0.5)
            _pendingScroll = false;
    }
}
