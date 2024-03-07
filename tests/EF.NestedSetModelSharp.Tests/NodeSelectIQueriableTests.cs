using Microsoft.EntityFrameworkCore;

namespace EF.NestedSetModelSharp.Tests
{
    public class NodeSelectIQueriableTests : IDisposable
    {
        NestedSetModelManager<Node, int, int?> _ns;
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
        private List<Node> _suitsTree;


        public NodeSelectIQueriableTests()
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
            _ns = new NestedSetModelManager<Node, int, int?>(_db);
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
        public void TestSelectImmediateChildren()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            //Action
            var results = _db.Nodes.GetImmediateChildren(Clothing)
                .OrderBy(a => a.Left)
                .ToList();

            //Assert
            Assert.Equal(2, results?.Count);
            Assert.Equal(Women.Id, results?[0].Id);
            Assert.Equal(Men.Id, results?[1].Id);
        }

        [Fact]
        public void TestGetImmediateChildren2EFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var immediateChildren = _db.Nodes.GetImmediateChildren(Women)
                .OrderBy(a => a.Left)
                .ToList();

            // Assert
            Assert.Equal(3, immediateChildren?.Count);
            Assert.Equal(Blouses.Id, immediateChildren?[0].Id);
            Assert.Equal(Skirts.Id, immediateChildren?[1].Id);
            Assert.Equal(Dresses.Id, immediateChildren?[2].Id);
        }

        [Fact]
        public void TestGetDescendantsEFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var immediateChildren = _db.Nodes.GetDescendants(Clothing).ToList();

            // Assert
            Assert.Equal(10, immediateChildren?.Count);
        }

        [Fact]
        public void TestGetDescendants1EFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);


            // Action
            var results = _db.Nodes.GetDescendants(Men).ToList();

            // Assert
            Assert.Equal(3, results?.Count);

            var nodeSuit = results?.Where(c => c.Id == Suits.Id).SingleOrDefault();
            Assert.NotNull(nodeSuit);

            var nodeJackets = results?.Where(c => c.Id == Jackets.Id).SingleOrDefault();
            Assert.NotNull(nodeJackets);

            var nodeSlacks = results?.Where(c => c.Id == Slacks.Id).SingleOrDefault();
            Assert.NotNull(nodeSlacks);

        }

        [Fact]
        public void TestGetDescendantsWithMultipleThreesEFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var results = _db.Nodes.GetDescendants(Women).ToList();

            // Assert
            Assert.Equal(5, results?.Count);

            var nodeBlouses = results?.Where(c => c.Id == Blouses.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeBlouses);

            var nodeSkirts = results?.Where(c => c.Id == Skirts.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeSkirts);

            var nodeDresses = results?.Where(c => c.Id == Dresses.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeDresses);

            var nodeEveningGowns = results?.Where(c => c.Id == EveningGowns.Id && c.Level == Dresses.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeEveningGowns);

            var nodeSunDresses = results?.Where(c => c.Id == SunDresses.Id && c.Level == Dresses.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeSunDresses);

        }

        [Fact]
        public void TestSelectSpecificDepthOfDescendantsEFQueryableExtension()
        {

            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var results = _db.Nodes.GetDescendants(Women, 2).ToList();

            // Assert
            // Assert
            Assert.Equal(5, results?.Count);

            var nodeBlouses = results?.Where(c => c.Id == Blouses.Id).SingleOrDefault();
            Assert.NotNull(nodeBlouses);

            var nodeSkirts = results?.Where(c => c.Id == Skirts.Id).SingleOrDefault();
            Assert.NotNull(nodeSkirts);

            var nodeDresses = results?.Where(c => c.Id == Dresses.Id).SingleOrDefault();
            Assert.NotNull(nodeDresses);

            var nodeEveningGowns = results?.Where(c => c.Id == EveningGowns.Id).SingleOrDefault();
            Assert.NotNull(nodeEveningGowns);

            var nodeSunDresses = results?.Where(c => c.Id == SunDresses.Id).SingleOrDefault();
            Assert.NotNull(nodeSunDresses);
        }

        [Fact]
        public void TestSelectSpecificDepthOfDescendants1EFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var results = _db.Nodes.GetDescendants(Women, 1).ToList();

            // Assert
            Assert.Equal(3, results?.Count);

            var nodeBlouses = results?.Where(c => c.Id == Blouses.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeBlouses);

            var nodeSkirts = results?.Where(c => c.Id == Skirts.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeSkirts);

            var nodeDresses = results?.Where(c => c.Id == Dresses.Id && c.Level == Women.Level + 1).SingleOrDefault();
            Assert.NotNull(nodeDresses);
        }

        [Fact]
        public void TestSelectAncestorsEFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var results = _db.Nodes.GetAncestors(Dresses).ToList();

            // Assert
            Assert.Equal(2, results?.Count);

            Assert.Equal(Clothing.Id, results?[0].Id);
            Assert.Equal(Women.Id, results?[1].Id);
        }

        [Fact]
        public void TestSelectAncestors1EFQueryableExtension()
        {
            // Insert data
            Clothing = _ns.InsertRoot(NewNode("Clothing"), NestedSetModelInsertMode.Right);
            Men = _ns.InsertBelow(Clothing.Id, NewNode("Men"), NestedSetModelInsertMode.Left);
            Suits = _ns.InsertBelow(Men.Id, NewNode("Suits"), NestedSetModelInsertMode.Left);
            Jackets = _ns.InsertBelow(Suits.Id, NewNode("Jackets"), NestedSetModelInsertMode.Right);
            Slacks = _ns.InsertBelow(Suits.Id, NewNode("Slacks"), NestedSetModelInsertMode.Left);

            Women = _ns.InsertNextTo(Men.Id, NewNode("Women"), NestedSetModelInsertMode.Left);
            Blouses = _ns.InsertBelow(Women.Id, NewNode("Blouses"), NestedSetModelInsertMode.Left);
            Skirts = _ns.InsertNextTo(Blouses.Id, NewNode("Skirts"), NestedSetModelInsertMode.Right);
            Dresses = _ns.InsertBelow(Women.Id, NewNode("Dresses"), NestedSetModelInsertMode.Right);
            EveningGowns = _ns.InsertBelow(Dresses.Id, NewNode("Evening Gowns"), NestedSetModelInsertMode.Left);
            SunDresses = _ns.InsertBelow(Dresses.Id, NewNode("Sun Dresses"), NestedSetModelInsertMode.Left);

            // Action
            var results = _ns.GetAncestors(SunDresses.Id).ToList();

            // Assert
            Assert.Equal(3, results?.Count);

            Assert.Equal(Clothing.Id, results?[0].Id);
            Assert.Equal(Women.Id, results?[1].Id);
            Assert.Equal(Dresses.Id, results?[2].Id);
        }

    }
}