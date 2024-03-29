﻿namespace YACCS.CommandAssemblies;

/// <summary>
/// Specifies the assembly contains plugins.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class PluginAttribute : Attribute;