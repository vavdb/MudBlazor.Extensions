﻿using System.Linq.Expressions;
using Nextended.Core.Extensions;
using Nextended.Core.Helper;

namespace MudBlazor.Extensions.Components.ObjectEdit.Options;

public partial class RenderData
{
    #region Static Factory Methods

    public static RenderData<TPropertyType, TFieldType> For<TComponent, TPropertyType, TFieldType>(
        Expression<Func<TComponent, TFieldType>> valueField, Action<TComponent> options,
        Func<TPropertyType, TFieldType> toFieldTypeConverter = null,
        Func<TFieldType, TPropertyType> toPropertyTypeConverter = null) where TComponent : new()
        => new(valueField.GetMemberName(), typeof(TComponent), DictionaryHelper.GetValuesDictionary(options, true))
        {
            ToFieldTypeConverterFn = toFieldTypeConverter,
            ToPropertyTypeConverterFn = toPropertyTypeConverter
        };

    public static RenderData<TPropertyType, TFieldType> For<TComponent, TPropertyType, TFieldType>(
        Expression<Func<TComponent, TFieldType>> valueField, TComponent instanceForAttributes,
        Func<TPropertyType, TFieldType> toFieldTypeConverter = null,
        Func<TFieldType, TPropertyType> toPropertyTypeConverter = null) where TComponent : new()
        => new(valueField.GetMemberName(), typeof(TComponent),
            DictionaryHelper.GetValuesDictionary(instanceForAttributes, true))
        {
            ToFieldTypeConverterFn = toFieldTypeConverter,
            ToPropertyTypeConverterFn = toPropertyTypeConverter
        };

    public static RenderData<TPropertyType, TFieldType> For<TComponent, TPropertyType, TFieldType>(
        Expression<Func<TComponent, TFieldType>> valueField,
        Func<TPropertyType, TFieldType> toFieldTypeConverter = null,
        Func<TFieldType, TPropertyType> toPropertyTypeConverter = null)
        => new(valueField.GetMemberName(), typeof(TComponent))
        {
            ToFieldTypeConverterFn = toFieldTypeConverter,
            ToPropertyTypeConverterFn = toPropertyTypeConverter
        };

    public static RenderData<TPropertyType, TPropertyType> For<TComponent, TPropertyType>(
        Expression<Func<TComponent, TPropertyType>> valueField, Action<TComponent> options) where TComponent : new()
        => new(valueField.GetMemberName(), typeof(TComponent), DictionaryHelper.GetValuesDictionary(options, true));

    public static RenderData<TPropertyType, TPropertyType> For<TComponent, TPropertyType>(
        Expression<Func<TComponent, TPropertyType>> valueField, TComponent instanceForAttributes)
        where TComponent : new()
        => new(valueField.GetMemberName(), typeof(TComponent),
            DictionaryHelper.GetValuesDictionary(instanceForAttributes, true));

    public static RenderData<TPropertyType, TPropertyType> For<TComponent, TPropertyType>(
        Expression<Func<TComponent, TPropertyType>> valueField)
        => new(valueField.GetMemberName(), typeof(TComponent));

    public static RenderData For<TComponent>(Action<TComponent> options) where TComponent : new()
        => For(typeof(TComponent), DictionaryHelper.GetValuesDictionary(options, true));

    public static RenderData For<TComponent>(IDictionary<string, object> attributes = null)
        => For(typeof(TComponent), attributes);

    public static RenderData For(Type componentType,
        IDictionary<string, object> attributes = null) => new(componentType, attributes);
    public static RenderData For(ICustomRenderer customRenderer) => new(null) { CustomRenderer = customRenderer };

    #endregion
}