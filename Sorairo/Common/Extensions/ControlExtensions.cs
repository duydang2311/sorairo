using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Styling;
using Sorairo.Common.Helpers;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Avalonia.Controls;

#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ControlExtensions
{
    public static T Dock<T>(this T control, Dock dock)
        where T : Control
    {
        control.SetValue(DockPanel.DockProperty, dock);
        return control;
    }

    // public static T WithValue<T>(this T control, AvaloniaProperty property, object? value)
    //     where T : Control
    // {
    //     control.SetValue(property, value);
    //     return control;
    // }

    public static T GridColumn<T>(this T control, int value)
        where T : Control
    {
        control.SetValue(Grid.ColumnProperty, value);
        return control;
    }

    public static T SpanColumn<T>(this T control, int value)
        where T : Control
    {
        control.SetValue(Grid.ColumnSpanProperty, value);
        return control;
    }

    public static T GridRow<T>(this T control, int value)
        where T : Control
    {
        control.SetValue(Grid.RowProperty, value);
        return control;
    }

    public static T Style<T>(this T control, IStyle style)
        where T : Control
    {
        control.Styles.Add(style);
        return control;
    }

    public static T BindResource<T>(this T control, AvaloniaProperty property, object key)
        where T : Control
    {
        control.Bind(property, control.GetResourceObservable(key));
        return control;
    }

    public static T WithBind<T>(this T control, AvaloniaProperty property, Binding binding)
        where T : Control
    {
        control.Bind(property, binding);
        return control;
    }

    public static TControl Bind<TControl, TSource, TValue>(
        this TControl control,
        FluentBinding<TSource, TValue> binding
    )
        where TControl : Control
    {
        control.Bind(
            binding.GetProperty(),
            new Binding(binding.GetPath(), binding.GetMode())
            {
                Source = binding.GetSource(),
                Converter = binding.GetConverter(),
            }
        );
        return control;
    }

    public static T Class<T>(this T control, params IEnumerable<string> classes)
        where T : Control
    {
        control.Classes.AddRange(classes);
        return control;
    }
}
