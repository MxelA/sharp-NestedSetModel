namespace EF.NestedSetModelSharp
{
    public abstract class NestedSetModel<T, TKey, TNullableKey>
    {
        public TKey Id { get; set; }
        public T Parent { get; set; }
        public TNullableKey ParentId { get; set; }
        public int Level { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public bool Moving { get; set; }
        public T Root { get; set; }
        public TNullableKey RootId { get; set; }
        public List<T> Children { get; set; }
        public List<T> Descendants { get; set; }
    }
}