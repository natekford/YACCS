using System;

namespace YACCS.SwappedArguments;

/// <summary>
/// Specifies that the parameter can be in different positions.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class SwappableAttribute : Attribute;