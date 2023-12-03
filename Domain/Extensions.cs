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
	}
}
