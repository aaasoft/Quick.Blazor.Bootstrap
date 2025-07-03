using System.Collections;
using System.Linq.Expressions;

namespace Quick.Blazor.Bootstrap;

public class DialogParameters : IEnumerable<KeyValuePair<string, object>>
{
    internal static readonly DialogParameters Default = new();

    internal Dictionary<string, object> _parameters = new();

    public void Add(string parameterName, object value)
    {
        _parameters[parameterName] = value;
    }

    public T Get<T>(string parameterName)
    {
        if (_parameters.TryGetValue(parameterName, out var value))
        {
            return (T)value;
        }

        throw new KeyNotFoundException($"{parameterName} does not exist in Dialog parameters");
    }

    public T TryGet<T>(string parameterName)
    {
        if (_parameters.TryGetValue(parameterName, out var value))
        {
            return (T)value;
        }

        return default;
    }

    public int Count => _parameters.Count;

    public object this[string parameterName]
    {
        get => Get<object>(parameterName);
        set => _parameters[parameterName] = value;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _parameters.GetEnumerator();
    }
}


public class DialogParameters<T> : DialogParameters
{
    public void Add<TParam>(Expression<Func<T, TParam>> propertyExpression, TParam value)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"Argument '{nameof(propertyExpression)}' must be a '{nameof(MemberExpression)}'");
        }

        Add(memberExpression.Member.Name, value);
    }

    public TParam Get<TParam>(Expression<Func<T, TParam>> propertyExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"Argument '{nameof(propertyExpression)}' must be a '{nameof(MemberExpression)}'");
        }

        return Get<TParam>(memberExpression.Member.Name);
    }

    public TParam TryGet<TParam>(Expression<Func<T, TParam>> propertyExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException($"Argument '{nameof(propertyExpression)}' must be a '{nameof(MemberExpression)}'");
        }

        return TryGet<TParam>(memberExpression.Member.Name);
    }
}