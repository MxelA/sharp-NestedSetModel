namespace EF.NestedSetModelSharp.Tests
{
    public class ClothingCategory : INestedSetModel<ClothingCategory, int, int?>
    {
        public int Id { get; set; }
        public ClothingCategory Parent { get; set; }
        public List<ClothingCategory> Children { get; set; }
        public List<ClothingCategory> Descendants { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }
        public string Name { get; set; }
        public bool Moving { get; set; }
        public ClothingCategory Root { get; set; }
        public int? RootId { get; set; }

        public ClothingCategory() { }

        public ClothingCategory(string name, int? parentId, int level, int left, int right)
            : this(0, parentId, level, left, right, name)
        {
        }

        public ClothingCategory(int id, int? parentId, int level, int left, int right, string name)
        {
            Id = id;
            ParentId = parentId;
            Level = level;
            Left = left;
            Right = right;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}