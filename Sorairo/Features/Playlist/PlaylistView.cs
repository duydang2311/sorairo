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
                    Binding = new Binding(nameof(Track.Artist)) { Mode = BindingMode.OneWay },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "Title",
                    Binding = new Binding(nameof(Track.Title), BindingMode.OneWay),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "File",
                    Binding = new Binding(nameof(Track.Path), BindingMode.OneWay)
                    {
                        Converter = new FuncValueConverter<Uri, string>(a =>
                            Path.GetFileName(Guard.Against.Null(a).LocalPath)
                        ),
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        }
            .Bind(FluentBinding.OneWay(vm, vm => vm.Tracks, DataGrid.ItemsSourceProperty))
            .BindResource(DataGrid.VerticalGridLinesBrushProperty, "SurfaceBorderBrush");
        dataGrid.LoadingRow += (_, e) =>
        {
            var item = (Track)e.Row.DataContext!;
            e.Row.DoubleTapped += OnRowDoubleTapped;
            rowDisposables.Add(
                e.Row,
                e.Row.BindClass(
                    "active",
                    new Binding(nameof(PlaylistState.CurrentTrack), BindingMode.OneWay)
                    {
                        Source = playlistState,
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
                Console.WriteLine("Dispose " + e.Row.Index);
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
