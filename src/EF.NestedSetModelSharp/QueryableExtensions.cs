namespace EF.NestedSetModelSharp
{
    public static class QueryableExtensions
    {
        public static IQueryable<INestedSetModel<T, TKey, TNullableKey>> GetDescendants<T, TKey, TNullableKey>(this IQueryable<INestedSetModel<T, TKey, TNullableKey>> queryable, INestedSetModel<T, TKey, TNullableKey> node,  int? depth = null) where T : class, INestedSetModel<T, TKey, TNullableKey>
        {
            var query = queryable.Where(n => n.Left > node.Left && n.Right < node.Right);
            if (depth.HasValue)
            {
                query = query.Where(n => n.Level == node.Level + depth.Value);
            }

            return query;
        }

        public static IQueryable<INestedSetModel<T, TKey, TNullableKey>> GetImmediateChildren<T, TKey, TNullableKey>(this IQueryable<INestedSetModel<T, TKey, TNullableKey>> queryable, INestedSetModel<T, TKey, TNullableKey> node) where T : class, INestedSetModel<T, TKey, TNullableKey>
            => queryable.Where(n => n.ParentId!.Equals(node.Id));

        public static IQueryable<INestedSetModel<T, TKey, TNullableKey>> GetAncestors<T, TKey, TNullableKey>(this IQueryable<INestedSetModel<T, TKey, TNullableKey>> queryable, INestedSetModel<T, TKey, TNullableKey> node) where T : class, INestedSetModel<T, TKey, TNullableKey>
            => queryable
                    .Where(n => n.Left < node.Left && n.Right > node.Right)
                    .OrderBy(n => n.Left);

    }
}
