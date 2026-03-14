using System.Linq.Expressions;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Sorairo.Common.Helpers;

public sealed class FluentBinding<TSource, TValue>(
    TSource source,
    string path,
    AvaloniaProperty property,
    Expression<Func<TSource, TValue>> expression
)
{
    private readonly TSource source = source;
    private readonly string path = path;
    private readonly AvaloniaProperty property = property;
    private readonly Expression<Func<TSource, TValue>> expression = expression;
    private IValueConverter? converter;
    private BindingMode mode;

    public TSource GetSource() => source;

    public string GetPath() => path;

    public AvaloniaProperty GetProperty() => property;

    public Expression<Func<TSource, TValue>> GetExpression() => expression;

    public BindingMode GetMode() => mode;

    public IValueConverter? GetConverter() => converter;

    public FluentBinding<TSource, TValue> Mode(BindingMode mode)
    {
        this.mode = mode;
        return this;
    }

    public FluentBinding<TSource, TValue> Convert<TConverted>(Func<TValue?, TConverted> converter)
    {
        this.converter = new FuncValueConverter<TValue, TConverted>(converter);
        return this;
    }
}

public static class FluentBinding
{
    public static FluentBinding<TSource, TValue> Bind<TSource, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> expression,
        AvaloniaProperty property
    )
    {
        var body = expression.Body;
        if (body is not MemberExpression memberExpression)
        {
            throw new NotSupportedException();
        }
        return new FluentBinding<TSource, TValue>(
            source,
            memberExpression.Member.Name,
            property,
            expression
        );
    }

    public static FluentBinding<TSource, TValue> OneWay<TSource, TValue>(
        TSource source,
        Expression<Func<TSource, TValue>> func,
        AvaloniaProperty property
    )
    {
        return Bind(source, func, property).Mode(BindingMode.OneWay);
    }
}
