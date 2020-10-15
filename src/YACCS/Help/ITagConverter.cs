namespace YACCS.Help
{
	public interface ITagConverter
	{
		string Separator { get; }

		string Convert(TaggedString tagged);
	}
}