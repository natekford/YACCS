using System.Threading.Tasks;

namespace YACCS.Commands.Interactivity
{
	public interface ICriterion<in TContext, in TInput>
	{
		Task<bool> JudgeAsync(TContext context, TInput input);
	}
}