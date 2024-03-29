﻿namespace Irene.Utils;

static partial class Util {
	// Returns the functional inverse of a given Dictionary.
	public static IDictionary<T2, T1> Invert<T1, T2>(IReadOnlyDictionary<T1, T2> dict)
		where T1 : notnull
		where T2 : notnull
	{
		Dictionary<T2, T1> dictInverse = new ();
		foreach (T1 key in dict.Keys)
			dictInverse.Add(dict[key], key);
		return dictInverse;
	}

	// Returns the first member of an ICollection.
	public static T GetFirst<T>(this ICollection<T> collection) =>
		collection.GetEnumerator().Current;

	// Converts a Collection to a List.
	public static IList<T> AsList<T>(this IReadOnlyCollection<T> collection) {
		List<T> list = new ();
		foreach (T @object in collection)
			list.Add(@object);
		return list;
	}

	// Pick and interpolate a random string from a list of templates.
	public static string ChooseRandom(IList<string> templates, params object[] data) {
		int i = Random.Shared.Next(0, templates.Count);
		return string.Format(templates[i], data);
	}
}
