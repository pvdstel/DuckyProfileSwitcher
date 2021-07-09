using System.Collections.Generic;

namespace DuckyProfileSwitcher.Utilities
{
    internal static class ListUtilities
    {
        public static void ReplaceWith<T>(this IList<T> list, T existingRule, T newRule)
        {
            int index = list.IndexOf(existingRule);
            if (index < 0)
            {
                return;
            }

            list.RemoveAt(index);
            list.Insert(index, newRule);
        }
    }
}
