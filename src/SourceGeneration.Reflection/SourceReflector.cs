using Microsoft.CodeAnalysis;
using System.ComponentModel;
using System.Reflection;

namespace SourceGeneration.Reflection;

#pragma warning disable IDE0305

public static class SourceReflector
{
    private static readonly Dictionary<Type, SourceTypeInfo> _types = [];
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, SourceTypeInfo> _reflectionTypes = [];

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Add(SourceTypeInfo typeInfo)
    {
#if NET5_0_OR_GREATER
        _types.TryAdd(typeInfo.Type, typeInfo);
#else
        if (!_types.ContainsKey(typeInfo.Type))
            _types.Add(typeInfo.Type, typeInfo);
#endif
    }

    public static T CreateInstance<
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    T>(params object?[] args)
    {
        if (typeof(T).IsValueType && (args == null || args.Length == 0))
        {
            return default!;
        }

        return (T)CreateInstance(GetRequiredType<T>(), args);
    }

    public static object CreateInstance(
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        Type type,
        params object?[] args)
    {
        return CreateInstance(GetRequiredType(type), args);
    }

    public static object CreateInstance(
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        SourceTypeInfo typeInfo,
        params object?[] args)
    {
        args ??= [];

        if (typeInfo.Type.IsValueType)
        {
            if (typeInfo.IsReflected)
            {
                return Activator.CreateInstance(typeInfo.Type, args)!;
            }

            if (args.Length == 0)
            {
                var ctor = typeInfo.DeclaredConstructors.FirstOrDefault(x => x.Parameters.Length == 0 && x.Accessibility == SourceAccessibility.Public);
                if (ctor != null)
                {
                    return ctor.Invoke(args)!;
                }
            }
        }



        var methods = typeInfo.DeclaredConstructors.Where(x => x.Parameters.Count(x => !x.HasDefaultValue) <= args.Length).ToList();
        var method = Type.DefaultBinder.BindToMethod(ReflectionExtensions.DeclaredOnlyLookup, methods.Select(x => x.ConstructorInfo).ToArray(), ref args, null, null, null, out _);
        if (typeInfo.IsReflected)
        {
            return method.Invoke(null, args)!;
        }
        else
        {
            return methods.First(x => x.ConstructorInfo == method).Invoke(args);
        }
    }


    public static SourceTypeInfo GetRequiredType<
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    T>(bool allowRuntimeReflection = false)
    {
        return GetRequiredType(typeof(T), allowRuntimeReflection);
    }

    public static SourceTypeInfo GetRequiredType(
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    Type type, bool allowRuntimeReflection = false)
    {
        if (allowRuntimeReflection)
        {
            return GetType(type, true)!;
        }
        else
        {
            return GetType(type, false) ?? throw new KeyNotFoundException($"No type info found for type '{type}', please ensure that the type '{type}' has the [SourceReflectionAttribute]");
        }
    }

    public static SourceTypeInfo? GetType<
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
    T>(bool allowRuntimeReflection = false) => GetType(typeof(T), allowRuntimeReflection);

    public static SourceTypeInfo[] GetTypes() => [.. _types.Values];

    //#if NET5_0_OR_GREATER
    //    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(allowReflect))]
    //#endif
    public static SourceTypeInfo? GetType(
#if NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)]
#endif
        Type type, bool allowRuntimeReflection = false)
    {
        if (_types.TryGetValue(type, out SourceTypeInfo? value))
        {
            return value;
        }

        if (allowRuntimeReflection)
        {
            return _reflectionTypes.GetOrAdd(type, CreateSourceTypeInfo);
        }
        return null;
    }

    private static SourceTypeInfo CreateSourceTypeInfo(
#if NET5_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(ReflectionExtensions.DefaultAccessMembers)] 
#endif
        Type type)
    {
        var isEnum = type.IsEnum;
        var isStruct = type.IsValueType && !type.IsPrimitive && !isEnum;
        return new SourceTypeInfo
        {
            Type = type,
            Accessibility = type.IsPublic ? SourceAccessibility.Public : SourceAccessibility.Internal,
            BaseType = type.BaseType,
            Name = type.Name,
            IsStruct = isStruct,
            IsEnum = isEnum,
            EnumUnderlyingType = isEnum ? type.GetEnumUnderlyingType() : null,
#if NET5_0_OR_GREATER
            IsReadOnly = isStruct && type.GetCustomAttribute<System.Runtime.CompilerServices.IsReadOnlyAttribute>() != null,
#endif
            IsStatic = type.IsAbstract && type.IsSealed,

            IsReflected = true,
            DeclaredConstructorsInitializer = () => type.GetConstructors(ReflectionExtensions.DeclaredOnlyLookup).Select(static x => new SourceConstructorInfo(() => x)
            {
                Name = ".ctor",
                Accessibility = x.GetAccessibility(),
                IsStatic = x.IsStatic,
                Parameters = CreateParameterInfos(x.GetParameters()),
            }).ToArray(),
            DeclaredMethodsInitializer = () => type.GetMethods(ReflectionExtensions.DeclaredOnlyLookup).Select(static x => new SourceMethodInfo(() => x)
            {
                Name = x.Name,
                Accessibility = x.GetAccessibility(),
                IsStatic = x.IsStatic,
                ReturnType = x.ReturnType,
                ReturnNullableAnnotation = x.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>() == null ? SourceNullableAnnotation.None : SourceNullableAnnotation.Annotated,
                Parameters = CreateParameterInfos(x.GetParameters()),
            }).ToArray(),
            DeclaredFieldsInitializer = () => type.GetFields(ReflectionExtensions.DeclaredOnlyLookup).Select(static x => new SourceFieldInfo(() => x)
            {
                Name = x.Name,
                Accessibility = x.GetAccessibility(),
                FieldType = x.FieldType,
                IsStatic = x.IsStatic,
                IsReadOnly = x.IsInitOnly,
                IsRequired = x.GetCustomAttribute<System.Runtime.CompilerServices.RequiredMemberAttribute>() != null,
                NullableAnnotation = x.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>() == null ? SourceNullableAnnotation.None : SourceNullableAnnotation.Annotated,
            }).ToArray(),
            DeclaredPropertiesInitializer = () => type.GetProperties(ReflectionExtensions.DeclaredOnlyLookup).Select(static x =>
            {
                var parameters = x.GetIndexParameters();
                return new SourcePropertyInfo(() => x)
                {
                    Name = x.Name,
                    Accessibility = x.GetAccessibility(),
                    PropertyType = x.PropertyType,
                    IsStatic = x.GetMethod?.IsStatic == true || x.SetMethod?.IsStatic == true,
                    CanRead = x.CanRead,
                    CanWrite = x.CanWrite,
                    IsRequired = x.GetCustomAttribute<System.Runtime.CompilerServices.RequiredMemberAttribute>() != null,
                    IsAbstract = x.CanRead ? x.GetMethod!.IsAbstract : x.SetMethod!.IsAbstract,
                    IsVirtual = x.CanRead ? x.GetMethod!.IsVirtual : x.SetMethod!.IsVirtual,
                    IsIndexer = parameters != null && parameters.Length > 0,
                    IndexerParameters = parameters == null ? [] : CreateParameterInfos(parameters),
                    IsInitOnly = x.IsInitOnly(),
                    NullableAnnotation = x.PropertyType.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>() == null ? SourceNullableAnnotation.None : SourceNullableAnnotation.Annotated,
                };
            }).ToArray(),
        };
    }

    private static SourceParameterInfo[] CreateParameterInfos(ParameterInfo[] parameterInfos)
    {
        SourceParameterInfo[] parameters = new SourceParameterInfo[parameterInfos.Length];
        for (int i = 0; i < parameterInfos.Length; i++)
        {
            var p = parameterInfos[i];
            parameters[i] = new SourceParameterInfo
            {
                Name = p.Name!,
                DefaultValue = p.DefaultValue,
                HasDefaultValue = p.HasDefaultValue,
                NullableAnnotation = SourceNullableAnnotation.None,
                ParameterType = p.ParameterType,
            };
        }
        return parameters;
    }

}
