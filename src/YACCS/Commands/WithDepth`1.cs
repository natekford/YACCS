namespace YACCS.Commands
{
	/// <summary>
	/// Contains a command and the depth of the path used to retrieve it.
	/// </summary>
	public readonly struct WithDepth<T>
	{
		/// <summary>
		/// The command being iterated.
		/// </summary>
		public T Command { get; }
		/// <summary>
		/// The depth of the path used to retrieve it.
		/// </summary>
		public int Depth { get; }

		/// <summary>
		/// Creates a new <see cref="WithDepth{T}"/>.
		/// </summary>
		/// <param name="depth">
		/// <inheritdoc cref="Depth" path="/summary"/>
		/// </param>
		/// <param name="command">
		/// <inheritdoc cref="Command" path="/summary"/>
		/// </param>
		public WithDepth(int depth, T command)
		{
			Command = command;
			Depth = depth;
		}

		/// <summary>
		/// Deconstructs this struct.
		/// </summary>
		/// <param name="depth"><see cref="Depth"/></param>
		/// <param name="command"><see cref="Command"/></param>
		public void Deconstruct(out int depth, out T command)
		{
			depth = Depth;
			command = Command;
		}
	}
}