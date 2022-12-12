using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ListExtensions 
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int IndexOf<T>(this IList<T> list, T item)
	{
		return list.IndexOf(item);
	}

	// O(1)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool RemoveBySwap<T>(this IList<T> list, int index)
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return true;
	}
}
