using FinEdgeAnalytics.DTOs;

namespace FinEdgeAnalytics.Transformers
{
	public interface ITransformer
	{
		IAsyncEnumerable<List<LoaderDto>> TransformData();
	}
}
