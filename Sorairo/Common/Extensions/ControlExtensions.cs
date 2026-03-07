using Avalonia.Data;

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

    public static T BindDynamicResource<T>(this T control, AvaloniaProperty property, object key)
        where T : Control
    {
        control[!property] = control.BindDynamicResource(key);
        return control;
    }

    public static IBinding BindDynamicResource<T>(this T control, object key)
        where T : IResourceHost
    {
        return control.GetResourceObservable(key).ToBinding();
    }
}
