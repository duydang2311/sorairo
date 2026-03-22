using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using R3;
using Sorairo.Common.Helpers;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Playlist;

public sealed class PlaylistView(PlaylistViewModel vm, PlaylistState playlistState)
    : ActivatableView
{
    private readonly ConditionalWeakTable<DataGridRow, IDisposable> rowDisposables = [];

    protected override void Init()
    {
        Content = CreateDataGrid();
    }

    protected override void OnActivated(ref DisposableBag disposables)
    {
        vm.Activate(ref disposables);
        disposables.Add(
            Disposable.Create(
                this,
                static state =>
                {
                    foreach (var (_, disposable) in state.rowDisposables)
                    {
                        disposable.Dispose();
                    }
                    state.rowDisposables.Clear();
                }
            )
        );
    }

    private DataGrid CreateDataGrid()
    {
        var dataGrid = new DataGrid
        {
            IsReadOnly = true,
            CanUserSortColumns = true,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "Artist",
                    Binding = CompiledBinding.Create<Track, string?>(
                        track => track.Artist,
                        null,
                        mode: BindingMode.OneWay
                    ),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "Title",
                    Binding = CompiledBinding.Create<Track, string?>(
                        track => track.Title,
                        null,
                        mode: BindingMode.OneWay
                    ),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "File",
                    Binding = CompiledBinding.Create<Track, Uri>(
                        track => track.Path,
                        null,
                        mode: BindingMode.OneWay,
                        converter: new FuncValueConverter<Uri, string>(a =>
                            Path.GetFileName(Guard.Against.Null(a).LocalPath)
                        )
                    ),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        }
            .Bind(FluentBinding.OneWay(vm, vm => vm.Tracks, DataGrid.ItemsSourceProperty))
            .BindResource(DataGrid.VerticalGridLinesBrushProperty, "SurfaceBorderBrush")
            .BindResource(BackgroundProperty, "SurfaceBrush");
        dataGrid.LoadingRow += (_, e) =>
        {
            var item = (Track)e.Row.DataContext!;
            e.Row.DoubleTapped += OnRowDoubleTapped;
            rowDisposables.Add(
                e.Row,
                e.Row.BindClass(
                    "active",
                    new Binding(nameof(PlaylistState.CurrentTrack))
                    {
                        Source = playlistState,
                        Mode = BindingMode.OneWay,
                        Converter = new FuncValueConverter<Track?, bool>(current =>
                            current is not null && item.Id == current.Id
                        ),
                    },
                    e.Row
                )
            );
        };
        dataGrid.UnloadingRow += (_, e) =>
        {
            if (rowDisposables.Remove(e.Row, out var disposable))
            {
                disposable.Dispose();
            }
        };

        return dataGrid;
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        var row = (DataGridRow)sender!;
        var item = (Track)row.DataContext!;
        if (vm.PlayCommand.CanExecute(item))
        {
            vm.PlayCommand.Execute(item);
        }
    }
}
