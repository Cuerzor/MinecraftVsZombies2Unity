using System;
using System.Collections.Generic;
using System.Drawing;
using Tools;
using Tools.Mathematics;
using UnityEngine;

namespace PVZEngine.Level.Collisions
{
    public class QuadTreeNode<T> : IPoolable where T : IQuadTreeNodeObject
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuadTreeNode()
        {
        }
        public void Init(QuadTree<T> tree, QuadTreeNode<T> parent, Rect size, int depth = 0)
        {
            Tree = tree;
            Parent = parent;
            bounds = size;
            looseBounds = bounds;
            looseBounds.position -= looseBounds.size;
            looseBounds.size *= 2;
            this.depth = depth;
        }
        public Rect GetRect()
        {
            return bounds;
        }

        #region 获取子节点
        public int GetChildCount()
        {
            return children.Count;
        }
        public QuadTreeNode<T> GetChild(int i)
        {
            return children[i];
        }
        #endregion

        #region 插入
        /// <summary>
        /// 插入一个新的Rect
        /// </summary>
        public void Insert(QuadTreeItem<T> item)
        {
            items.Add(item);
            item.node = this;
            GainObject();
        }
        private void GainObject()
        {
            totalTargetCount++;
            var parent = Parent;
            while (parent != null)
            {
                parent.totalTargetCount++;
                parent = parent.Parent;
            }

            //判断是否创建子节点
            if (children.Count <= 0 && (totalTargetCount > Tree.MaxObjects && depth < Tree.MaxDepth))
            {
                // 分裂，并将完全位于子节点中的物体分给子节点。
                CreateChildren();
                //填充对象到新创建的子节点中
                SpreadObjectsToChildren();
            }
        }
        #endregion

        #region 移除
        public bool Remove(QuadTreeItem<T> item)
        {
            //父节点坍缩
            //遍历移除
            if (items.Remove(item))
            {
                LoseObject();
                return true;
            }
            return false;
        }
        private void LoseObject()
        {
            totalTargetCount--;
            if (totalTargetCount <= Tree.MaxObjects)
            {
                RecycleObjectsFromChildren();
                RemoveChildren();
            }
            if (Parent != null)
            {
                Parent.LoseObject();
            }
        }
        #endregion

        #region 移动
        public void MoveItemTo(QuadTreeItem<T> item, QuadTreeNode<T> targetNode)
        {
            if (this == targetNode)
                return;
            if (!items.Remove(item))
                return;
            targetNode.items.Add(item);
            item.node = targetNode;
            targetNode.GainObject();
            LoseObject();
        }
        #endregion

        #region 评估目标节点
        public QuadTreeNode<T> EvaluateNode(Rect rect)
        {
            if (children.Count <= 0)
            {
                return this;
            }
            if (!CanChildContain(rect))
                return this;
            var child = GetContainerChild(rect);
            return child.EvaluateNode(rect);
        }
        public bool CanContain(Rect rect)
        {
            var thisSize = bounds.size;
            var rectSize = rect.size;
            return rectSize.x < thisSize.y && rectSize.y < thisSize.y;
        }
        public bool CanChildContain(Rect rect)
        {
            var thisSize = bounds.size;
            var childSize = thisSize * 0.5f;
            var rectSize = rect.size;
            return rectSize.x < childSize.y && rectSize.y < childSize.y;
        }
        public QuadTreeNode<T> GetContainerChild(Rect rect)
        {
            var thisCenter = bounds.center;
            var center = rect.center;
            int index = 0;
            if (center.x >= thisCenter.x)
            {
                index |= 1;
            }
            if (center.y >= thisCenter.y)
            {
                index |= 2;
            }
            return children[index];
        }
        #endregion

        #region 查找目标
        public void FindTargetsInRect(Rect rect, List<T> results, float rewind = 0)
        {
            if (!rect.Intersects(looseBounds))
                return;

            //查找匹配对象
            foreach (var item in items)
            {
                var target = item.target;
                if (rect.Intersects(target.GetCollisionRect(rewind)))
                {
                    results.Add(target);
                }
            }

            //遍历子节点
            foreach (var child in children)
            {
                child.FindTargetsInRect(rect, results, rewind);
            }
        }
        #endregion

        #region 创建子节点
        /// <summary>
        /// 分割四叉树，为这个结点计算出四个子结点
        /// </summary>
        private void CreateChildren()
        {
            //这部分是计算子结点的x,y,w,h
            float halfWidth = bounds.width / 2;
            float halfHeight = bounds.height / 2;
            float x = bounds.x;
            float y = bounds.y;
            children.Add(Tree.CreateNode(Tree, this, new Rect(x, y, halfWidth, halfHeight), depth + 1));
            children.Add(Tree.CreateNode(Tree, this, new Rect(x + halfWidth, y, halfWidth, halfHeight), depth + 1));
            children.Add(Tree.CreateNode(Tree, this, new Rect(x, y + halfHeight, halfWidth, halfHeight), depth + 1));
            children.Add(Tree.CreateNode(Tree, this, new Rect(x + halfWidth, y + halfHeight, halfWidth, halfHeight), depth + 1));
        }
        private void SpreadObjectsToChildren()
        {
            var thisSize = bounds.size;
            var childSize = thisSize * 0.5f;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var movingItem = items[i];
                var target = movingItem.target;
                var movingRect = target.GetCollisionRect();
                if (!CanChildContain(movingRect))
                    continue;
                var child = GetContainerChild(movingRect);
                child.items.Add(movingItem);
                child.totalTargetCount++;
                movingItem.node = child;
                items.RemoveAt(i);
            }
        }
        #endregion 

        #region 回收子节点
        private void RemoveChildren()
        {
            foreach (var child in children)
            {
                Tree.ReleaseNode(child);
            }
            children.Clear();
        }
        private void RecycleObjectsFromChildren()
        {
            //遍历子节点
            foreach (var child in children)
            {
                for (var i = child.items.Count - 1; i >= 0; i--)
                {
                    var recycling = child.items[i];
                    items.Add(recycling);
                    recycling.node = this;
                }
            }
        }
        #endregion

        void IPoolable.Reset()
        {
            Tree = null;
            Parent = null;
            bounds = default;
            looseBounds = default;
            items.Clear();
            totalTargetCount = 0;
            children.Clear();
            depth = 0;
        }

        public QuadTree<T> Tree { get; private set; }
        public QuadTreeNode<T> Parent { get; private set; }
        //rect范围
        private Rect bounds;
        private Rect looseBounds;
        //当前层级内的对象
        private List<QuadTreeItem<T>> items = new List<QuadTreeItem<T>>();
        private int totalTargetCount;
        //子节点
        private List<QuadTreeNode<T>> children = new List<QuadTreeNode<T>>();
        //当前层级
        private int depth;
    }
    public interface IQuadTreeNodeObject : IEquatable<IQuadTreeNodeObject>
    {
        Rect GetCollisionRect(float rewind = 0);
    }
}
