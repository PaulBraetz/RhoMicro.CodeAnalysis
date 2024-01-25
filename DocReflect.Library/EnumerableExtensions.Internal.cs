namespace RhoMicro.CodeAnalysis.Library;

static partial class EnumerableExtensions
{
	public static IReadOnlyDictionary<String, TValue> ToEquatableNameMap<TValue>(
		this IEnumerable<TValue> elements,
		Func<TValue, String> nameSelector,
		String elementsName)
	{
		_ = elements ?? throw new ArgumentNullException(elementsName);

		var map = new Dictionary<String, TValue>();

		foreach(var element in elements)
		{
			var name = nameSelector.Invoke(element);
			if(map.ContainsKey(name))
			{
				throw new ArgumentException(
					$"{elementsName} contains duplicate name: {name}",
					elementsName);
			}

			map.Add(name, element);
		}

		var result = map.AsEquatable();

		return result;
	}
}
