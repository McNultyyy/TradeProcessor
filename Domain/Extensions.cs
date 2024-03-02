namespace TradeProcessor.Domain
{
	public static class Extensions
	{
		public static (T1, T2) GetTuple<T, T1, T2>(this T instance,
			Func<T, T1> func1,
			Func<T, T2> func2)
		{
			return (
					func1(instance),
					func2(instance)
					);
		}

		public static (T1, T2, T3) GetTuple<T, T1, T2, T3>(this T instance,
			Func<T, T1> func1,
			Func<T, T2> func2,
			Func<T, T3> func3)
		{
			return (
				func1(instance),
				func2(instance),
				func3(instance)
			);
		}

		public static void Apply<T>(this T instance, params Action<T>[] actions)
		{
			foreach (var action in actions)
			{
				action(instance);
			}
		}

		public static bool None<TSource>(this IEnumerable<TSource> source,
			Func<TSource, bool> predicate)
		{
			return !source.Any(predicate);
		}


		public static decimal RoundDownToMultiple(this decimal x, decimal y)
		{
			return Math.Round(x / y, MidpointRounding.ToZero) * y;
		}

		public static IEnumerable<IImbalance> OrderByGapType2(this IEnumerable<IImbalance> imbalances)
		{
			return imbalances.OrderBy(x => x.GapType, new GapTypeComparer());
		}

		public static IEnumerable<IImbalance> OrderByGapType(this IEnumerable<IImbalance> imbalances)
		{

			foreach (var imbalance in imbalances)
			{
				if (imbalance.GapType == GapType.Price)
					yield return imbalance;
			}


			foreach (var imbalance in imbalances)
			{
				if (imbalance.GapType == GapType.Volume)
					yield return imbalance;
			}


			foreach (var imbalance in imbalances)
			{
				if (imbalance.GapType == GapType.Liquidity)
					yield return imbalance;
			}


			foreach (var imbalance in imbalances)
			{
				if (imbalance.GapType == GapType.Opening)
					yield return imbalance;
			}
		}
	}

	public class GapTypeComparer : IComparer<GapType>
	{
		private static readonly IList<GapType> DefaultOrder = new List<GapType>()
		{
			GapType.Price, GapType.Volume, GapType.Liquidity, GapType.Opening
		};

		private readonly IList<GapType> _order;

		public GapTypeComparer(IList<GapType>? order = null)
		{
			_order = order ?? DefaultOrder;
		}

		public int Compare(GapType x, GapType y)
		{
			return _order.IndexOf(y) - _order.IndexOf(x);
		}
	}
}
