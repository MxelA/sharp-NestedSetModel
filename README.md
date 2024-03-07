This is a .NET8 package for working with trees in relational databases.

__Contents:__
- [Theory](#what-are-nested-sets)
- [Documentation](#documentation)


  
What are nested sets?
---------------------

Nested sets or [Nested Set Model](http://en.wikipedia.org/wiki/Nested_set_model) is
a way to effectively store hierarchical data in a relational table. From wikipedia:

> The nested set model is to number the nodes according to a tree traversal,
> which visits each node twice, assigning numbers in the order of visiting, and
> at both visits. This leaves two numbers for each node, which are stored as two
> attributes. Querying becomes inexpensive: hierarchy membership can be tested by
> comparing these numbers. Updating requires renumbering and is therefore expensive.

### Applications

Nested Set Model shows good performance. It is tuned to be fast for
getting related nodes. It'is ideally suited for building multi-depth menu or
categories for shop or similar things.

Documentation
-------------

Suppose that we have a model `ClothingCategory`, a `$node` variable is an instance of that model
and the node that we are manipulating. It can be a fresh model or one from database.

#### Setup DB Entity (DB Model)

You create a class, and you inherit INestedSetModel interface:

```c#
public class ClothingCategory : INestedSetModel<Clothing, int, int?>
{
     public int Id { get; set; }
     public Node Parent { get; set; }
     public List<Node> Children { get; set; }
     public List<Node> Descendants { get; set; }
     public int? ParentId { get; set; }
     public int Level { get; set; }
     public int Left { get; set; }
     public int Right { get; set; }
     public string Name { get; set; }
     public bool Moving { get; set; }
     public Node Root { get; set; }
     public int? RootId { get; set; }
}
```

In DbContext register ClothingCategory entity
```c#
public class AppDbContext : DbContext
{
    public DbSet<ClothingCategory> Nodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureNested<ClothingCategory, int, int?>();
    }
}
```

#### Creating Root node

You can simply create a root node:

```c#
 _db = new AppDbContext();
 _ns = new NestedSetModelManager<ClothingCategory, int, int?>(_db);


ClothingCategory clothingCateogry = new ClothingCategory {
  Name = "Clothing"
}
 
_ns.InsertRoot(clothingCateogry, NestedSetModelInsertMode.Right);
```

#### Insert first level child
```c#
ClothingCategory men = new ClothingCategory {
  Name = "Men"
};

_ns.InsertBelow(clothingCateogry.Id, men, NestedSetModelInsertMode.Left);

```
