using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Localization;

namespace YACCS.Commands.Models
{
	/// <summary>
	/// An immutable list of strings which supports localization
	/// through <see cref="Localize.This(string, string?)"/>.
	/// </summary>
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public sealed class ImmutablePath : IReadOnlyList<string>
	{
		private readonly ImmutableArray<string> _Keys;
		/// <inheritdoc />
		public int Count => _Keys.Length;
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {_Keys.Length}";

		/// <inheritdoc />
		public string this[int index] => Localize.This(_Keys[index]);

		/// <summary>
		/// Creates a new <see cref="ImmutablePath"/>.
		/// </summary>
		/// <param name="keys">The values to use as parts of the name.</param>
		public ImmutablePath(IEnumerable<string> keys)
		{
			if (keys is ImmutablePath name)
			{
				_Keys = name._Keys;
			}
			else
			{
				_Keys = keys.ToImmutableArray();
			}
		}

		/// <inheritdoc />
		public IEnumerator<string> GetEnumerator()
		{
			foreach (var key in _Keys)
			{
				yield return Localize.This(key);
			}
		}

		/// <inheritdoc />
		public override string ToString()
			=> string.Join(CommandServiceUtils.SPACE, this);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}