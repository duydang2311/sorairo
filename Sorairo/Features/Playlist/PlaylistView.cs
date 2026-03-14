using Ardalis.GuardClauses;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using R3;
using Sorairo.Common.Models;
using Sorairo.Common.UI;

namespace Sorairo.Features.Playlist;

public sealed class PlaylistView(PlaylistViewModel vm, PlaylistState playlistState) : ViewBase
{
    protected override void Init()
    {
        Content = CreateDataGrid();
    }

    protected override void OnActivated(ref DisposableBag disposables) { }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    private DataGrid CreateDataGrid()
    {
        var dataGrid = new DataGrid
        {
            ItemsSource = playlistState.Items,
            IsReadOnly = true,
            CanUserResizeColumns = true,
            CanUserSortColumns = true,
            Columns =
            {
                new DataGridTextColumn
                {
                    Header = "Artist",
                    Binding = new Binding(nameof(PlaylistItem.Artist), BindingMode.OneWay),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "Title",
                    Binding = new Binding(nameof(PlaylistItem.Title), BindingMode.OneWay),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
                new DataGridTextColumn
                {
                    Header = "File",
                    Binding = new Binding(nameof(PlaylistItem.Path), BindingMode.OneWay)
                    {
                        Converter = new FuncValueConverter<Uri, string>(a =>
                            Path.GetFileName(Guard.Against.Null(a).LocalPath)
                        ),
                    },
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                },
            },
        }.BindResource(DataGrid.VerticalGridLinesBrushProperty, "SurfaceBorderBrush");
        dataGrid.LoadingRow += (_, e) =>
        {
            var item = (PlaylistItem)e.Row.DataContext!;
            e.Row.DoubleTapped += OnRowDoubleTapped;
            e.Row.BindClass(
                "active",
                new Binding(nameof(PlaylistState.CurrentItem), BindingMode.OneWay)
                {
                    Source = playlistState,
                    Converter = new FuncValueConverter<PlaylistItem?, bool>(current =>
                        current is not null && item.Id == current.Id
                    ),
                },
                e.Row
            );
        };
        return dataGrid;
    }

    private void OnRowDoubleTapped(object? sender, TappedEventArgs e)
    {
        var row = (DataGridRow)sender!;
        var item = (PlaylistItem)row.DataContext!;
        if (vm.PlayCommand.CanExecute(item))
        {
            vm.PlayCommand.Execute(item);
        }
    }
}
