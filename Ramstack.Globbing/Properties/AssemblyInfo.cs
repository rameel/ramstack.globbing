[module: System.Runtime.CompilerServices.SkipLocalsInit]

// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global

namespace System.Runtime.CompilerServices;

/// <summary>
/// Specifies that a type has required members or that a member is required.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class
    | AttributeTargets.Struct
    | AttributeTargets.Field
    | AttributeTargets.Property,
    Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute;

/// <summary>
/// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
/// </summary>
/// <param name="featureName">The name of the required compiler feature.</param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
#pragma warning disable CS9113 // Parameter is unread.
internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute;
#pragma warning restore CS9113 // Parameter is unread.
