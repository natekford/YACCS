namespace YACCS.Results;

/// <summary>
/// A non-specific result indicating failure.
/// </summary>
public class Failure : Result
{
	/// <summary>
	/// A singleton instance with no message.
	/// </summary>
	public static Failure Instance { get; } = new();

	/// <inheritdoc cref="Failure(string)"/>
	public Failure() : this(string.Empty)
	{
	}

	/// <summary>
	/// Creates a new <see cref="Failure"/>.
	/// </summary>
	/// <param name="message">
	/// <inheritdoc cref="Result.Response" path="/summary"/>
	/// </param>
	public Failure(string message) : base(false, message)
	{
	}
}

/// <summary>
/// A non-specific result indicating success.
/// </summary>
public class Success : Result
{
	/// <summary>
	/// A singleton instance with no message.
	/// </summary>
	public static Success Instance { get; } = new();

	/// <inheritdoc cref="Success(string)"/>
	public Success() : this(string.Empty)
	{
	}

	/// <summary>
	/// Creates a new <see cref="Success"/>.
	/// </summary>
	/// <param name="message">
	/// <inheritdoc cref="Result.Response" path="/summary"/>
	/// </param>
	public Success(string message) : base(true, message)
	{
	}
}

/// <summary>
/// Boxes a value so it can be returned as a result.
/// </summary>
public class ValueResult : Result
{
	/// <summary>
	/// The value of this result.
	/// </summary>
	public object? Value { get; }

	/// <summary>
	/// Creates a new <see cref="ValueResult"/>.
	/// </summary>
	/// <param name="value">
	/// <inheritdoc cref="Value" path="/summary"/>
	/// </param>
	public ValueResult(object? value) : base(true, value?.ToString() ?? string.Empty)
	{
		Value = value;
	}
}