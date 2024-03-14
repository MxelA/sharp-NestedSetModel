using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EF.NestedSetModelSharp
{
   
    public class NestedSetModelService<T, TKey, TNullableKey>
        where T : class, INestedSetModel<T, TKey, TNullableKey>
    {
        private readonly DbSet<T> _nodesSet;
        private readonly DbContext _db;

        public NestedSetModelService(DbContext dbContext)
        {
            _db = dbContext;
            _nodesSet = _db.Set<T>();
        }

        private static IQueryable<T> QueryById(IQueryable<T> nodes, TKey id)
        {
            return nodes.Where(_PropertyEqualsExpression(nameof(INestedSetModel<T, TKey, TNullableKey>.Id), id));
        }

        private IQueryable<T> GetNodes(TNullableKey rootId)
        {
            return _nodesSet.Where(PropertyEqualsExpression(nameof(INestedSetModel<T, TKey, TNullableKey>.RootId), rootId));
        }


        private Expression<Func<T, bool>> PropertyEqualsExpression(string propertyName, TKey key)
        {
            return _PropertyEqualsExpression(propertyName, key);
        }

        private Expression<Func<T, bool>> PropertyEqualsExpression(string propertyName, TNullableKey key)
        {
            return _PropertyEqualsExpression(propertyName, key);
        }

        private static Expression<Func<T, bool>> _PropertyEqualsExpression<TField>(string propertyName, TField key)
        {
            var parameterExpression = Expression.Parameter(typeof(T), "entity");
            if (string.IsNullOrEmpty(propertyName))
                throw new NotSupportedException();
            return Expression.Lambda<Func<T, bool>>(
                Expression.Equal(Expression.Property(parameterExpression, typeof(T), propertyName), Expression.Convert(Expression.Constant(key), typeof(TField))),
                parameterExpression);
        }

        public List<T> Delete(TKey nodeId, bool soft = false)
        {
            var nodeToDelete = GetNode(nodeId);
            var nodeToDeleteLeft = nodeToDelete.Left;
            var difference = nodeToDelete.Right - nodeToDelete.Left + 1;
            var rootId = nodeToDelete.RootId;
            var deleted = GetNodes(rootId).Where(s => s.Left >= nodeToDelete.Left && s.Right <= nodeToDelete.Right).ToList();
            if (soft)
                foreach (var node in deleted)
                    node.Moving = true;
            else
                foreach (var node in deleted)
                    _nodesSet.Remove(node);
            var nodesToUpdate = GetNodes(rootId).Where(s => s.Left > nodeToDelete.Left || s.Right >= nodeToDelete.Left).ToList();
            foreach (var nodeToUpdate in nodesToUpdate)
            {
                if (nodeToUpdate.Moving)
                    continue;
                if (nodeToUpdate.Left >= nodeToDeleteLeft)
                    nodeToUpdate.Left -= difference;
                nodeToUpdate.Right -= difference;
            }
            var minLeft = deleted.Min(s => s.Left) - 1;
            // Reset to 1
            foreach (var deletedNode in deleted)
            {
                deletedNode.Left -= minLeft;
                deletedNode.Right -= minLeft;
                deletedNode.ParentId = default(TNullableKey);
            }
            if (!soft)
            {
                _db.SaveChanges();
                foreach (var deletedSite in deleted)
                    deletedSite.Id = default(TKey);
            }
            return deleted;
        }

        public void MoveToParent(TKey nodeId, TNullableKey parentId,
            NestedSetModelInsertMode insertMode)
        {
            Move(nodeId, parentId, default(TNullableKey), insertMode);
        }

        public void MoveToSibling(TKey nodeId, TNullableKey siblingId,
            NestedSetModelInsertMode insertMode)
        {
            Move(nodeId, default(TNullableKey), siblingId, insertMode);
        }

        private void Move(TKey nodeId, TNullableKey toParentId, TNullableKey toSiblingId,
            NestedSetModelInsertMode insertMode)
        {
            //var node = _nodes.Single(KeyEqualsExpression(nodeId));
            var deletedNodes = Delete(nodeId);
            Insert(toParentId, toSiblingId, deletedNodes, insertMode);
        }

        public T InsertRoot(T node,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(default(TNullableKey), default(TNullableKey), new[] { node }, insertMode).First();
        }

        public List<T> InsertRoot(IEnumerable<T> nodeTree,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(default(TNullableKey), default(TNullableKey), nodeTree, insertMode);
        }

        public T InsertBelow(TNullableKey parentId, T node,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(parentId, default(TNullableKey), new[] { node }, insertMode).First();
        }

        public List<T> InsertBelow(TNullableKey parentId, IEnumerable<T> nodeTree,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(parentId, default(TNullableKey), nodeTree, insertMode);
        }

        public T InsertNextTo(TNullableKey siblingId, T node,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(default(TNullableKey), siblingId, new[] { node }, insertMode).First();
        }

        public T InsertNextTo(TNullableKey siblingId, List<T> nodeTree,
            NestedSetModelInsertMode insertMode)
        {
            return Insert(default(TNullableKey), siblingId, nodeTree, insertMode).First();
        }


        private List<T> Insert(TNullableKey parentId, TNullableKey siblingId, IEnumerable<T> nodeTree,
            NestedSetModelInsertMode insertMode)
        {
            var nodeArray = nodeTree as T[] ?? nodeTree.ToArray();
            var lowestLeft = nodeArray.Min(n => n.Left);
            var highestRight = nodeArray.Max(n => n.Right);
            
            if (lowestLeft == 0 && highestRight == 0)
            {
                if (nodeArray.Length == 1)
                {
                    var node = nodeArray.Single();
                    node.Left = 1;
                    node.Right = 2;
                    lowestLeft = 1;
                    highestRight = 2;
                }
                else
                {
                    throw new ArgumentException("Node tree must have left right values", nameof(nodeTree));
                }
            }

            var difference = highestRight - lowestLeft;
            
            var nodeTreeRoot = nodeArray.Single(n => n.Left == lowestLeft);

            T? parent = null;
            T? sibling = null;

            var isRoot = Equals(parentId, default(TNullableKey)) && Equals(siblingId, default(TNullableKey));

            (parent, sibling, siblingId)  = GetMostRightImmediateChild(insertMode, parentId, siblingId);
            
            int? siblingLeft = null;
            int? siblingRight = null;
            var rootId = default(TNullableKey);
            if (!Equals(siblingId, default(TNullableKey)))
            {
                if (sibling == null)
                {
                    sibling = GetNode(siblingId);
                }
                siblingLeft = sibling.Left;
                siblingRight = sibling.Right;
                parentId = sibling.ParentId;
                rootId = sibling.RootId;
            }
            int? parentLeft = null;
            if (!Equals(parentId, default(TNullableKey)))
            {
                if (parent == null)
                {
                    parent = GetNode(parentId);
                }
                parentLeft = parent.Left;
                rootId = parent.RootId;
            }

            //Set level of nodes
            nodeArray = SetNodesLevel(parent, nodeArray);
            
            var left = 0;
            var right = 0;
            CalculateLeftAndRightValues(difference, insertMode, rootId, sibling, siblingLeft, siblingRight, parent, parentLeft, ref left, ref right);
            
            var leftChange = left - nodeTreeRoot.Left;
            var rightChange = right - nodeTreeRoot.Right;
            foreach (var node in nodeArray)
            {
                node.Left += leftChange;
                node.Right += rightChange;
            }
            nodeTreeRoot.ParentId = parentId;
            var newNodes = nodeArray.Where(n => !n.Moving).ToList();
            if (newNodes.Any())
            {
                _nodesSet.AddRange(newNodes);
            }
            var movingNodes = nodeArray.Where(n => n.Moving).ToList();
            foreach (var node in movingNodes)
            {
                node.Moving = false;
            }
            _db.SaveChanges();
            // Update the root ID
            if (isRoot)
            {
                nodeTreeRoot.RootId = ToNullableKey(nodeTreeRoot.Id);
                nodeTreeRoot.Root = nodeTreeRoot;
                _db.SaveChanges();
            }
            else if (Equals(rootId, default(TNullableKey)))
            {
                var rootIds = newNodes.Select(n => n.RootId).Distinct().ToArray();
                if (rootIds.Length > 1)
                {
                    throw new ArgumentException("Unable to identify root node ID of node tree as multiple have been supplied.");
                }
                if (Equals(rootId, default(TNullableKey)) &&
                    rootIds.Length == 0 || (rootIds.Length == 1 && Equals(rootIds[0], default(TNullableKey))))
                {
                    rootId = rootIds[0];
                    //nodeTreeRoot.RootId = rootId;//ToNullableKey(GetNodes(rootId).Single(n => n.Left == 1).Id);
                }
            }
            if (!Equals(rootId, default(TNullableKey)))
            {
                foreach (var newNode in newNodes)
                {
                    newNode.RootId = rootId;
                }
                _db.SaveChanges();
            }
            else if (!isRoot)
            {
                throw new Exception("Unable to determine root ID of non-root node");
            }
            // Update the parent IDs now we have them
            foreach (var newNode in newNodes)
            {
                if (newNode != nodeTreeRoot)
                {
                    var path = GetPathToNode(newNode, newNodes).Reverse();
                    var current = newNode;
                    foreach (var ancestor in path)
                    {
                        current.ParentId = (TNullableKey)(object)ancestor.Id;
                        current = ancestor;
                    }
                }
            }
            _db.SaveChanges();
            return newNodes;
        }

        private (T?, T?, TNullableKey?) GetMostRightImmediateChild(NestedSetModelInsertMode insertMode, TNullableKey? parentId, TNullableKey? siblingId)
        {
            T? parent = null;
            T? sibling = null;

            if (!Equals(parentId, default(TNullableKey)) &&
                insertMode == NestedSetModelInsertMode.Right)
            {
                parent = GetNode(parentId);
                if (parent == null)
                {
                    throw new ArgumentException(string.Format("Unable to find node parent with ID of {0}", parentId));
                }
                var temp = parent;
                var rightMostImmediateChild = GetNodes(parent.RootId)
                    .Where(s => s.Left >= temp.Left && s.Right <= temp.Right && s.Level == temp.Level + 1)
                    .OrderByDescending(s => s.Right)
                    .ToList()
                    .FirstOrDefault(n => !n.Moving)
                    ;
                sibling = rightMostImmediateChild;
                if (sibling != null)
                {
                    siblingId = (TNullableKey?)(object)sibling.Id;
                }
            }

            return (parent, sibling, siblingId);
        }

        private T[] SetNodesLevel(T? parent, T[] nodeArray)
        {
            var minLevel = nodeArray.Min(n => n.Level);
            foreach (var node in nodeArray)
            {
                node.Level -= minLevel;
                if (parent != null)
                {
                    node.Level += parent.Level + 1;
                }
            }

            return nodeArray;
        }

        private void CalculateLeftAndRightValues(int difference, NestedSetModelInsertMode insertMode, TNullableKey? rootId, T? sibling, int? siblingLeft, int? siblingRight, T? parent, int? parentLeft, ref int left, ref int right)
        {
            switch (insertMode)
            {
                case NestedSetModelInsertMode.Left:
                    {
                        IEnumerable<T> nodes;
                        if (sibling != null)
                        {
                            nodes = GetNodes(rootId)
                                .Where(s => s.Left >= siblingLeft || s.Right >= siblingRight).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = sibling.Left;
                            right = sibling.Left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left >= siblingLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else if (parent != null)
                        {
                            nodes = GetNodes(rootId).Where(s => s.Right >= parentLeft).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = parent.Left + 1;
                            right = left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > parentLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else
                        {
                            left = 1;
                            right = 1 + difference;
                        }
                    }
                    break;
                case NestedSetModelInsertMode.Right:
                    {
                        List<T> nodes;
                        if (sibling != null)
                        {
                            nodes = GetNodes(rootId)
                                .Where(s => s.Left > siblingRight || s.Right > siblingRight)
                                .ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = sibling.Right + 1;
                            right = sibling.Right + 1 + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > siblingLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else if (parent != null)
                        {
                            nodes = GetNodes(rootId)
                                .Where(s => s.Right >= parentLeft).ToList()
                                .Where(n => !n.Moving)
                                .ToList();
                            left = parent.Left + 1;
                            right = left + difference;
                            foreach (var nodeToUpdate in nodes)
                            {
                                if (nodeToUpdate.Left > parentLeft)
                                    nodeToUpdate.Left += difference + 1;
                                nodeToUpdate.Right += difference + 1;
                            }
                        }
                        else
                        {
                            left = 1;
                            right = 1 + difference;
                        }
                    }
                    break;
            }
        }

        private static TNullableKey ToNullableKey(TKey id)
        {
            return (TNullableKey)(object)id;
        }

        /// <summary>
        /// Returns all descendants of a node
        /// </summary>
        /// <param name="nodeId">The node for which to find the path to</param>
        /// <returns></returns>
        public IQueryable<T> GetDescendants(TKey nodeId, int? depth = null)
        {
            var node = GetNodeData(nodeId);
            var query = _nodesSet.Where(n => n.Left > node.Left && n.Right < node.Right);
            if (depth.HasValue)
            {
                query = query.Where(n => n.Level <= node.Level + depth.Value);
            }
            return query;
        }

        private NodeData<TNullableKey> GetNodeData(TKey nodeId)
        {
            var node = QueryById(_nodesSet, nodeId)
                .Select(n => new NodeData<TNullableKey> { Level = n.Level, Left = n.Left, Right = n.Right, RootId = n.RootId }).Single();
            return node;
        }

        private class NodeData<TNullableKey>
        {
            public int Level { get; set; }
            public int Left { get; set; }
            public int Right { get; set; }
            public TNullableKey RootId { get; set; }
        }

        /// <summary>
        /// Returns the immediate children of a given node, i.e. its ancestors
        /// </summary>
        /// <param name="nodeId">The node for which to find the path to</param>
        /// <returns></returns>
        public IQueryable<T> GetImmediateChildren(TKey nodeId)
        {
            return _nodesSet.Where(PropertyEqualsExpression(nameof(INestedSetModel<T, TKey, TNullableKey>.ParentId), (TNullableKey)(object)nodeId));
        }

        /// <summary>
        /// Returns the path to a given node, i.e. its ancestors
        /// </summary>
        /// <param name="nodeId">The node for which to find the path to</param>
        /// <returns></returns>
        public IOrderedEnumerable<T> GetAncestors(TKey nodeId)
        {
            var node = GetNodeData(nodeId);
            return GetAncestors(node, GetNodes(node.RootId));
        }

        /// <summary>
        /// Returns the path to a given node, i.e. its ancestors
        /// </summary>
        /// <param name="node">The node for which to find the path to</param>
        /// <returns></returns>
        public IOrderedEnumerable<T> GetPathToNode(T node)
        {
            return GetPathToNode(node, GetNodes(node.RootId));
        }

        /// <summary>
        /// Returns the path to a given node within a set of nodes, i.e. its ancestors
        /// </summary>
        /// <param name="node">The node for which to find the path to</param>
        /// <param name="nodeSet">The set of nodes to limit the search to</param>
        /// <returns></returns>
        public static IOrderedEnumerable<T> GetPathToNode(T node, IEnumerable<T> nodeSet)
        {
            return GetAncestors(AsNodeData(node), nodeSet);
        }

        private static NodeData<TNullableKey> AsNodeData(T node)
        {
            return new NodeData<TNullableKey> { Left = node.Left, Right = node.Right, RootId = node.RootId };
        }

        /// <summary>
        /// Returns the path to a given node within a set of nodes, i.e. its ancestors
        /// </summary>
        /// <param name="node">The node for which to find the path to</param>
        /// <param name="nodeSet">The set of nodes to limit the search to</param>
        /// <returns></returns>
        private static IOrderedEnumerable<T> GetAncestors(NodeData<TNullableKey> node, IEnumerable<T> nodeSet)
        {
            return nodeSet
                    .Where(n => n.Left < node.Left && n.Right > node.Right)
                    .OrderBy(n => n.Left)
                ;
        }

        private T GetNode(TNullableKey id)
        {
            return GetNode((TKey)(object)id);
        }

        private T GetNode(TKey id)
        {
            return _nodesSet.Single(PropertyEqualsExpression(nameof(INestedSetModel<T, TKey, TNullableKey>.Id), id));
        }
    }
}