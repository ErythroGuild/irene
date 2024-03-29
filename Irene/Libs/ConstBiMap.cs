﻿namespace Irene;

class ConstBiMap<T, U>
	where T: notnull
	where U: notnull
{
	private readonly ReadOnlyDictionary<T, U> _tableForward;
	private readonly ReadOnlyDictionary<U, T> _tableReverse;

	public ConstBiMap(IReadOnlyDictionary<T, U> table) {
		ConcurrentDictionary<T, U> tableForward = new (table);
		ConcurrentDictionary<U, T> tableReverse = new ();

		foreach (T key in tableForward.Keys) {
			U value = tableForward[key];
			if (tableReverse.ContainsKey(value))
				throw new ArgumentException("Input map is not one-to-one.", nameof(table));
			else
				tableReverse.TryAdd(value, key);
		}

		_tableForward = new (tableForward);
		_tableReverse = new (tableReverse);
	}

	public U this[T key] => _tableForward[key];
	public T this[U key] => _tableReverse[key];
	// Explicit accessors in case the indexer overloads are ambiguous
	// (T and U are the same type).
	public U GetForward(T key) => _tableForward[key];
	public T GetReverse(U key) => _tableReverse[key];

	// Overloads for checking if a key exists.
	public bool Contains(T key) => _tableForward.ContainsKey(key);
	public bool Contains(U key) => _tableReverse.ContainsKey(key);
	// Explicit accessors in case the argument overloads are ambiguous
	// (T and U are the same type).
	public bool ContainsFirst(T key) => _tableForward.ContainsKey(key);
	public bool ContainsLast (U key) => _tableReverse.ContainsKey(key);

	// Fetch a set of all keys.
	public IReadOnlySet<T> KeysForward() => new HashSet<T>(_tableForward.Keys);
	public IReadOnlySet<U> KeysReverse() => new HashSet<U>(_tableReverse.Keys);
}
