using System;
using System.Collections.Generic;
using System.Linq;

namespace MasterClassSagaPattern.Common
{
    public static class CollectionsExtensions
    {
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            return collection.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> collection) => collection == null || collection.IsEmpty();

        public static bool IsEmpty<T>(this IEnumerable<T> collection) => !collection.Any();

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || collection.IsEmpty();
    }
}
