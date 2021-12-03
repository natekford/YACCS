using System.Collections.Immutable;
using System.Globalization;

namespace YACCS.CommandAssemblies;

/// <summary>
/// Specifies what cultures this plugin supports.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public sealed class SupportedCulturesAttribute : Attribute
{
	/// <summary>
	/// The cultures which are supported.
	/// </summary>
	public ImmutableArray<CultureInfo> SupportedCultures { get; }

	/// <summary>
	/// Creates a new <see cref="SupportedCulturesAttribute"/>.
	/// </summary>
	/// <param name="supportedCultures">
	/// <inheritdoc cref="SupportedCultures" path="/summary"/>
	/// </param>
	public SupportedCulturesAttribute(params string[] supportedCultures)
	{
		SupportedCultures = supportedCultures
			.Select(CultureInfo.GetCultureInfo)
			.ToImmutableArray();
	}
}