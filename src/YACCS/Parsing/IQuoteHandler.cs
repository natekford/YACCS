namespace YACCS.Parsing
{
	public interface IQuoteHandler
	{
		bool ValidEndQuote(char? p, char c, char? n);

		bool ValidSplit(char? p, char c, char? n);

		bool ValidStartQuote(char? p, char c, char? n);
	}
}