using Microsoft.EntityFrameworkCore;

namespace EF.NestedSetModelSharp.Tests
{
    public class NodeInsertTests : IDisposable
    {
        NestedSetModelManager<AppDbContext, Node, int, int?> _ns;
        private AppDbContext _db;
        public Node? Clothing { get; set; }
        public Node? Men { get; set; }
        public Node? Women { get; set; }
        public Node? Suits { get; set; }
        public Node? Slacks { get; set; }
        public Node? Jackets { get; set; }
        public Node? Dresses { get; set; }
        public Node? Skirts { get; set; }
        public Node? Blouses { get; set; }
        public Node? EveningGowns { get; set; }
        public Node? SunDresses { get; set; }
        private List<Node> _catsTree;


        public NodeInsertTests()
        {
            new AppDbContext().Database.Migrate();
            SetUp();
        }

        protected void SetUp()
        {
            Clothing = null;
            Men = null;
            Women = null;
            Suits = null;
            Slacks = null;
            Jackets = null;
            Dresses = null;
            Skirts = null;
            Blouses = null;
            EveningGowns = null;
            SunDresses = null;
         
            // Clean up from the last test, but do this on set-up not
            // tear-down so it is possible to inspect the database with
            // the results of the last test
            _db = new AppDbContext();

            _db.Database.ExecuteSqlRaw("DELETE FROM \"Nodes\" where \"Id\" != 0");
            _ns = new NestedSetModelManager<AppDbContext, Node, int, int?>(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        private static Node NewNode(string name)
        {
            return new Node { Name = name };
        }

        [Fact]
        public void TestInsertRoot()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            AssertDb(Clothing.RootId, new Node(Clothing.Name, null, 0, 1, 2));
        }

        [Fact]
        public void TestInsertFirstLevelChild()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            
            AssertDb(
                Clothing.RootId,
                new Node(Clothing.Name, null, 0, 1, 4),
                new Node(Men.Name, Clothing.Id, 1, 2, 3)
            );
        }

        [Fact]
        public void TestInsertSecondLevelChild()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            
            AssertDb(
                Clothing.RootId,
                new Node(Clothing.Name, null, 0, 1, 6),
                new Node(Men.Name, Clothing.Id, 1, 2, 5),
                new Node(Suits.Name, Men.Id, 2, 3, 4)
            );
        }

        [Fact]
        public void TestInsertSiblingToRight()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            
            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Right);

            AssertDb(
                Clothing.RootId,
                new Node(Clothing.Name, null, 0, 1, 8),
                new Node(Men.Name, Clothing.Id, 1, 2, 5),
                new Node(Suits.Name, Men.Id, 2, 3, 4),
                new Node(Women.Name, Clothing.Id, 1, 6, 7)
            );
        }

        [Fact]
        public void TestInsertSiblingToLeft()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);

            AssertDb(
               Clothing.RootId,
               new Node(Clothing.Name, null, 0, 1, 8),
               new Node(Men.Name, Clothing.Id, 1, 4, 7),
               new Node(Suits.Name, Men.Id, 2, 5, 6),
               new Node(Women.Name, Clothing.Id, 1, 2, 3)
           );
        }

        [Fact]
        public void TestInsertDeepChildToLeft()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);

            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            AssertDb(
                Clothing.RootId,
                new Node(Clothing.Name, null, 0, 1, 10),
                new Node(Women.Name, Clothing.Id, 1, 2, 5),
                new Node(Blouses.Name, Women.Id, 2, 3, 4),
                new Node(Men.Name, Clothing.Id, 1, 6, 9),
                new Node(Suits.Name, Men.Id, 2, 7, 8)
            );
        }

        [Fact]
        public void TestInsertSiblingToRight2()
        {
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);

            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            AssertDb(
                Clothing.RootId,
                new Node(Clothing.Name, null, 0, 1, 12),
                new Node(Women.Name, Clothing.Id, 1, 2, 7),
                new Node(Blouses.Name, Women.Id, 2, 3, 4),
                new Node(Skirts.Name, Women.Id, 2, 5, 6),
                new Node(Men.Name, Clothing.Id, 1, 8, 11),
                new Node(Suits.Name, Men.Id, 2, 9, 10)
            );
        }

        private static void AssertDb(int? rootId, params Node[] expectedNodes)
        {
            using (var db = new AppDbContext())
            {
                var nodes = db.Nodes.Where(n => n.RootId == rootId);
                Assert.Equal(expectedNodes.Length, nodes.Count());
                for (var i = 0; i < expectedNodes.Length; i++)
                {
                    var node = nodes.SingleOrDefault(n => n.Name == expectedNodes[i].Name);
                    Assert.Equal(rootId, node.RootId);
                    Assert.Equal(expectedNodes[i].Left, node.Left);
                    Assert.Equal(expectedNodes[i].Right, node.Right);
                    Assert.Equal(expectedNodes[i].ParentId, node.ParentId);
                    Assert.Equal(expectedNodes[i].Level, node.Level);
                }
            }
        }
    }
}