using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class ListExtensions 
{
	// O(1)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool RemoveBySwap<T>(this IList<T> list, int index)
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return true;
	}
}
