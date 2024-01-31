using System.Collections.Immutable;
using System.Globalization;

namespace YACCS.CommandAssemblies;

/// <summary>
/// Specifies what cultures this plugin supports.
/// </summary>
/// <remarks>
/// Creates a new <see cref="SupportedCulturesAttribute"/>.
/// </remarks>
/// <param name="supportedCultures">
/// <inheritdoc cref="SupportedCultures" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public sealed class SupportedCulturesAttribute(params string[] supportedCultures)
	: Attribute
{
	/// <summary>
	/// The cultures which are supported.
	/// </summary>
	public ImmutableArray<CultureInfo> SupportedCultures { get; }
		= supportedCultures.Select(CultureInfo.GetCultureInfo).ToImmutableArray();
}