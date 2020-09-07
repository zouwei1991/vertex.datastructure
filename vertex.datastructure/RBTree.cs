using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vertex.datastructure
{

    public class RBTree<T1,T2> where T1:IComparable 
    {
        class RBTreeNode
        {
            public T2 Value { get; set; }

            public T1 Key { get; set; }

            public RBTreeNode LeftNode { get; set; }

            public RBTreeNode RightNode { get; set; }

            public RBTreeNode ParentNode { get; set; }

            public List<RBTreeNode> EqualNodes { get; } = new List<RBTreeNode>();

            public RBTreeColor Color { get; set; }

            public RBTreeNode()
            {

            }

            public RBTreeNode(T1 key,T2 value)
            {
                this.Key = key;
                this.Value = value;
            }

            public RBTreeNode(T1 key,T2 value,RBTreeNode leftnode,RBTreeNode rightnode)
            {
                this.Key = key;
                this.Value = value;
                this.LeftNode = leftnode;
                this.RightNode = rightnode;
            }

            public RBTreeNode(T1 key,T2 value,RBTreeNode leftnode,RBTreeNode rightnode,RBTreeColor color)
            {
                this.Key = key;
                this.Value = value;
                LeftNode = leftnode;
                RightNode = rightnode;
                Color = color;
            }

            public void SetParent(RBTreeNode parent,RBTreeDirection direction)
            {
                this.ParentNode = parent;
                if (parent != null)
                {
                    if (direction == RBTreeDirection.left)
                    {
                        parent.LeftNode = this;
                    }
                    else
                    {
                        parent.RightNode = this;
                    }
                }                
            }

            public void SetChild(RBTreeNode childNode,RBTreeDirection direction)
            {
                if (direction == RBTreeDirection.left)
                {
                    this.LeftNode = childNode;                   
                }
                else
                {
                    this.RightNode = childNode;
                }
                if (childNode != null)
                {
                    childNode.ParentNode = this;
                }
            }

            public void SetColor(RBTreeColor color)
            {
                this.Color = color;
            }
           
        }

        enum RBTreeDirection:byte
        {
            left=0,
            right=1
        }

        enum RBTreeColor:byte
        {
            red=0,
            black=1
        }

        private RBTreeNode rootTreeNode = null;

         //*  1) A node is either red or black
         //*  2) The root is black
         //*  3) All leaves(NULL) are black
         //*  4) Both children of every red node are black
         //*  5) Every simple path from root to leaves contains the same number
         //* of black nodes.
        public void Insert(T1 key,T2 value)
        {
            if (rootTreeNode == null)
            {
                rootTreeNode = new RBTreeNode(key,value, null, null, RBTreeColor.black);
                return;
            }
            RBTreeNode newNode = new RBTreeNode(key,value);
            RBTreeNode parentNode = GetParentNodeToInsert(newNode, rootTreeNode);
            if (parentNode.Key.CompareTo(newNode.Key) == 0)
            {
                newNode.ParentNode = parentNode.ParentNode;
                newNode.LeftNode = parentNode.LeftNode;
                newNode.RightNode = parentNode.RightNode;
                parentNode.EqualNodes.Add(newNode);
                return;
            }
            RBTreeDirection direction = newNode.Key.CompareTo(parentNode.Key)>0?RBTreeDirection.right:RBTreeDirection.left;
            newNode.SetParent(parentNode, direction);
            byte mask = 1;
            RBTreeColor color = (RBTreeColor)((byte)parentNode.Color ^ mask);
            newNode.SetColor(color);
            if (color == RBTreeColor.red)
                return;
            //如果插入的节点是黑色节点则需要旋转平衡
            RBTreeNode gparentNode = parentNode.ParentNode;
            RBTreeDirection pDirection = parentNode.Equals(gparentNode.LeftNode) ? RBTreeDirection.left : RBTreeDirection.right;
            RBTreeNode uncleNode = pDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
            //左侧则右旋
            if (pDirection == RBTreeDirection.left)
            {
                #region condition1
                //          b                      r
                //         / \                    / \
                //        r   r   --->           b   b
                //       /                            \
                //      b                              r               
                if (direction==RBTreeDirection.left && IsRedNode(uncleNode))
                {
                    Rotate(newNode, parentNode, uncleNode, gparentNode, RBTreeDirection.right);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(parentNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,违反红色节点的子节点必须是黑色节点原则,通过旋转消除
                    EraseColor(parentNode);
                }
                #endregion
                #region condition2
                //         b                          b                       r
                //        / \                        / \                     / \
                //       r   r         ---->        r   r     ---->         b   b
                //        \                        /                             \
                //         b                      b                               r                
                if (direction==RBTreeDirection.right && IsRedNode(uncleNode))
                {
                    parentNode.SetParent(newNode, RBTreeDirection.left);
                    parentNode.SetColor(RBTreeColor.black);
                    parentNode.SetChild(null, RBTreeDirection.right);
                    newNode.SetParent(gparentNode, RBTreeDirection.left);
                    newNode.SetColor(RBTreeColor.red);
                    Rotate(parentNode, newNode, uncleNode, gparentNode, RBTreeDirection.right);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(newNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,违反红色节点的子节点必须是黑色节点原则,通过旋转消除
                    EraseColor(parentNode);
                }
                #endregion
                #region condition3
                //         b                           r                         b
                //        / \                         / \                       / \
                //       r  (b)        ----->        b   b       ----->        r   r
                //      /                                 \                         \
                //     b                                  (b)                       (b)                
                if (direction==RBTreeDirection.left && !IsRedNode(uncleNode))
                {
                    Rotate(newNode, parentNode, uncleNode, gparentNode, RBTreeDirection.right);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(parentNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,此时直接对祖父级和父级颜色取反
                    parentNode.SetColor(RBTreeColor.black);
                    parentNode.LeftNode.SetColor(RBTreeColor.red);
                    parentNode.RightNode.SetColor(RBTreeColor.red);
                    return;
                }
                #endregion
                #region condition4
                //         b                   b                        r
                //        / \                 / \                      / \
                //       r  (b)   ---->      r  (b)   ----->          b   b
                //        \                 /                              \
                //         b               b                               (b)               
                if (direction==RBTreeDirection.right && !IsRedNode(uncleNode))
                {
                    parentNode.SetParent(newNode, RBTreeDirection.left);
                    parentNode.SetChild(null, RBTreeDirection.right);
                    parentNode.SetColor(RBTreeColor.black);
                    newNode.SetParent(gparentNode, RBTreeDirection.left);
                    newNode.SetColor(RBTreeColor.red);
                    Rotate(parentNode, newNode, uncleNode, gparentNode, RBTreeDirection.right);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(newNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,此时直接对祖父级和父级颜色取反
                    newNode.SetColor(RBTreeColor.black);
                    newNode.LeftNode.SetColor(RBTreeColor.red);
                    newNode.RightNode.SetColor(RBTreeColor.red);
                    return;
                }
                #endregion
            }
            //右侧则左旋
            else
            {
                #region condition1
                //        b                       r
                //       / \                     / \
                //      r   r       ---->       b   b
                //           \                 /
                //            b               r               
                if (direction==RBTreeDirection.right && IsRedNode(uncleNode))
                {
                    Rotate(newNode, parentNode, uncleNode, gparentNode, RBTreeDirection.left);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(parentNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,违反红色节点的子节点必须是黑色节点原则,通过旋转消除
                    EraseColor(parentNode);
                }
                #endregion
                #region condition2
                //        b                  b                     r
                //       / \                / \                   / \
                //      r   r   ---->      r   r        ---->    b   b
                //         /                    \               /
                //        b                      b             r               
                if (direction==RBTreeDirection.left && IsRedNode(uncleNode))
                {
                    parentNode.SetParent(newNode, RBTreeDirection.right);
                    parentNode.SetColor(RBTreeColor.black);
                    parentNode.SetChild(null, RBTreeDirection.left);
                    newNode.SetParent(gparentNode, RBTreeDirection.right);
                    newNode.SetColor(RBTreeColor.red);
                    Rotate(parentNode, newNode, uncleNode, gparentNode, RBTreeDirection.left);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(newNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,违反红色节点的子节点必须是黑色节点原则,通过旋转消除
                    EraseColor(parentNode);
                }
                #endregion
                #region condition3
                //       b                      r
                //      / \                    / \
                //    (b)  r       ---->      b   b
                //          \                /
                //           b             (b)                
                if (direction==RBTreeDirection.right && !IsRedNode(uncleNode))
                {
                    Rotate(newNode, parentNode, uncleNode, gparentNode, RBTreeDirection.left);
                    //旋转后如果祖父节点的父节点是根节点或者黑色节点,旋转结束
                    if (!IsRedNode(parentNode.ParentNode))
                        return;
                    //旋转后如果祖父节点的父节点是红色节点,此时直接对祖父级和父级颜色取反
                    parentNode.SetColor(RBTreeColor.black);
                    parentNode.LeftNode.SetColor(RBTreeColor.red);
                    parentNode.RightNode.SetColor(RBTreeColor.red);
                    return;
                }
                #endregion
                #region condition4
                //       b                     b                          r
                //      / \                   / \                        / \
                //    (b)  r     ---->      (b)  r        ---->         b   b
                //        /                       \                    /
                //       b                         b                 (b)                
                if (direction==RBTreeDirection.left && !IsRedNode(uncleNode))
                {
                    parentNode.SetParent(newNode, RBTreeDirection.right);
                    parentNode.SetChild(null, RBTreeDirection.left);
                    parentNode.SetColor(RBTreeColor.black);
                    newNode.SetParent(gparentNode, RBTreeDirection.right);
                    newNode.SetColor(RBTreeColor.red);
                    Rotate(parentNode, newNode, uncleNode, parentNode, RBTreeDirection.left);
                    if (!IsRedNode(newNode.ParentNode))
                        return;
                    newNode.SetColor(RBTreeColor.black);
                    newNode.LeftNode.SetColor(RBTreeColor.red);
                    newNode.LeftNode.SetColor(RBTreeColor.red);
                    return;
                }
                #endregion
            }
        }

        public T2 Find(T1 key)
        {
            if (key == null)
                throw new ArgumentNullException("key is null");
            if (rootTreeNode == null)
                throw new ArgumentNullException("the tree has no root");
            RBTreeNode matchNode = FindNode(key, rootTreeNode);
            return matchNode.Value;
        }

        public void Update(T1 key,T2 value)
        {
            if (key == null)
                throw new ArgumentNullException("key is null");
            if (rootTreeNode == null)
                throw new ArgumentNullException("the tree has no root");
            RBTreeNode matchNode = FindNode(key, rootTreeNode);
            matchNode.Value = value;
        }

        public void Delete(T1 key)
        {
            if (key == null)
                throw new ArgumentNullException("key is null");
            if (rootTreeNode == null)
                throw new ArgumentNullException("the tree has no root");
            RBTreeNode matchNode = FindNode(key, rootTreeNode);
            RBTreeNode leaveNode= RotateValue(matchNode);
            if (leaveNode == null)
                throw new Exception("unexcepted rotate value exception");
            //旋转设值至叶结点,如果叶节点是红色,直接删除,否则进行旋转消除颜色
            if (IsRedNode(leaveNode))
            {
                RBTreeDirection direction = GetDirection(leaveNode, leaveNode.ParentNode);
                if (direction == RBTreeDirection.left)
                {
                    leaveNode.ParentNode.LeftNode = null;
                }
                else
                {
                    leaveNode.ParentNode.RightNode = null;
                }
                leaveNode = null;
                return;
            }
            else
            {
                bool isrootnode = IsRootNode(leaveNode);
                if (isrootnode)
                {
                    rootTreeNode=leaveNode = null;
                    return;
                }
                RBTreeDirection dDirection = GetDirection(leaveNode, leaveNode.ParentNode);
                RBTreeNode parentNode = leaveNode.ParentNode;
                RBTreeNode gparentNode = parentNode.ParentNode;
                RBTreeDirection gDirection = GetDirection(parentNode, gparentNode);
                if (dDirection == RBTreeDirection.left)
                {
                   
                    RBTreeNode brotherNode = parentNode.RightNode;
                    if (brotherNode != null)
                    {
                        //           r                   b                     b
                        //          / \                 / \                   / \
                        //         d   b     --->      r  (r)     --->       r   r
                        //            / \             / \                     \
                        //          (r) (r)          d  (r)                   (r)
                        RBTreeNode childNode = brotherNode.LeftNode;
                        RBTreeNode childBrotherNode = brotherNode.RightNode;
                        if(childNode==null && childBrotherNode == null)
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.left);
                            parentNode.SetChild(null,RBTreeDirection.right);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.LeftNode = null;
                            leaveNode = null;
                        }
                        else if (childNode == null)
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.left);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.LeftNode = null;
                            leaveNode = null;
                        }
                        else if (childBrotherNode == null)
                        {
                            brotherNode.SetParent(childNode, RBTreeDirection.right);
                            brotherNode.SetChild(null, RBTreeDirection.left);
                            brotherNode.SetColor(RBTreeColor.red);
                            childNode.SetParent(parentNode, RBTreeDirection.right);
                            childNode.SetColor(RBTreeColor.black);
                            parentNode.SetParent(childNode, RBTreeDirection.left);
                            parentNode.SetChild(null, RBTreeDirection.right);
                            childNode.SetParent(gparentNode, RBTreeDirection.left);
                            parentNode.LeftNode = null;
                            leaveNode = null;
                        }
                        else
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.left);
                            parentNode.SetChild(childNode, RBTreeDirection.right);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.LeftNode = null;
                            EraseColor(childNode);
                            leaveNode = null;
                        }
                    }                    
                    else
                    {
                        //          b                    b                     b
                        //         / \                  / \                   / \
                        //        r  (b)     --->      b  (b)      --->      b  (b)
                        //       /                    /
                        //      d                    d
                        parentNode.SetColor(RBTreeColor.black);
                        parentNode.LeftNode = null;
                        leaveNode = null;
                    }
                }
                else
                {
                    RBTreeNode brotherNode = parentNode.LeftNode;
                    if (brotherNode != null)
                    {
                        //            r                    b                     b
                        //           / \                  / \                   / \
                        //          b   d      ---->    (r)  r      ---->     (r)  r
                        //         / \                      / \                   / 
                        //       (r) (r)                  (r)  d                (r)  
                        RBTreeNode childNode = parentNode.LeftNode;
                        RBTreeNode childBrotherNode = parentNode.RightNode;
                        if(childNode==null && childBrotherNode == null)
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.right);
                            parentNode.SetChild(null, RBTreeDirection.left);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.RightNode = null;
                            leaveNode = null;
                        }
                        else if (childNode == null)
                        {
                            brotherNode.SetParent(childBrotherNode, RBTreeDirection.left);
                            brotherNode.SetChild(null, RBTreeDirection.right);
                            brotherNode.SetColor(RBTreeColor.red);
                            childBrotherNode.SetParent(parentNode, RBTreeDirection.left);
                            childBrotherNode.SetColor(RBTreeColor.black);
                            parentNode.SetParent(brotherNode, RBTreeDirection.right);
                            parentNode.SetChild(null, RBTreeDirection.left);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.RightNode = null;
                            leaveNode = null;
                        }
                        else if (childBrotherNode == null)
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.right);
                            parentNode.SetChild(null, RBTreeDirection.left);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.RightNode = null;
                            leaveNode = null;
                        }
                        else
                        {
                            parentNode.SetParent(brotherNode, RBTreeDirection.right);
                            parentNode.SetChild(childBrotherNode, RBTreeDirection.left);
                            brotherNode.SetParent(gparentNode, gDirection);
                            parentNode.RightNode = null;
                            EraseColor(childBrotherNode);
                            leaveNode = null;
                        }
                    }
                    else
                    {
                        //            b                      b                    b
                        //           / \                    / \                  / \
                        //         (b)  r      ---->      (b)  b      ---->    (b)  b
                        //               \                      \
                        //                d                      d
                        parentNode.SetColor(RBTreeColor.black);
                        parentNode.RightNode = null;
                        leaveNode = null;
                    }
                }




            }
        }

        private RBTreeNode RotateValue(RBTreeNode matchNode)
        {
            //          d                       n                   d                    n                       d                      n
            //         / \                     / \                 / \                  / \                     / \                    / \
            //        n   n        --->       n   n               n   n     --->       n   n                   n   n     --->         n   n   
            //       / \                     / \                 / \                  / \                     / \                    / \
            //      n   n                   n   d               n  (n)               d  (n)                 (n)  n                 (n)  d
            
            //         d                        n                     d                         n                   d                  n
            //        / \        --->          / \                   / \          --->         / \                 / \     --->       / \
            //      (n)  n                   (n)  d                 n  (n)                    d  (n)              n   n              d   n            
            RBTreeNode finalNode = null;
            RBTreeNode parentNode = matchNode.LeftNode;
            RBTreeNode uncleNode = matchNode.RightNode;
            RBTreeNode childNode = parentNode?.LeftNode;
            RBTreeNode brotherNode = parentNode?.RightNode;
            if(parentNode==null && uncleNode==null)
            {
                finalNode = matchNode;
                return finalNode;
            }
            if (parentNode == null)
            {
                ExchangeNodeValue(matchNode, uncleNode);
                finalNode = uncleNode;
                return finalNode;
            }
            if (uncleNode == null)
            {
                ExchangeNodeValue(matchNode, parentNode);
                finalNode = parentNode;
                return finalNode;
            }
            if(parentNode!=null && uncleNode != null)
            {
                if(childNode==null && brotherNode == null)
                {
                    ExchangeNodeValue(matchNode, parentNode);
                    finalNode = parentNode;
                    return finalNode;
                }
                if (brotherNode == null)
                {
                    ExchangeNodeValue(parentNode, matchNode);
                    ExchangeNodeValue(parentNode, childNode);
                    finalNode = childNode;
                    return finalNode;
                }
                if (childNode == null)
                {
                    ExchangeNodeValue(matchNode, brotherNode);
                    finalNode = brotherNode;
                    return finalNode;
                }
                if(childNode!=null && brotherNode != null)
                {
                    ExchangeNodeValue(matchNode, brotherNode);
                    finalNode = RotateValue(brotherNode);
                    return finalNode;
                }
            }
            return null;
        }

        private RBTreeNode FindNode(T1 key,RBTreeNode node)
        {
            if (node == null)
                throw new Exception("key is not found");
            int compare = key.CompareTo(node.Key);
            if (compare == 0)
                return node;
            if (compare > 0)
                return FindNode(key, node.RightNode);
            else
                return FindNode(key, node.LeftNode);
        }

        private void EraseColor(RBTreeNode newNode)
        {
            RBTreeDirection direction = GetDirection(newNode, newNode.ParentNode);
            RBTreeNode parentNode = newNode.ParentNode;
            RBTreeNode brotherNode = direction == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
            RBTreeDirection pDirection = GetDirection(parentNode, parentNode.ParentNode);
            RBTreeNode gparentNode = parentNode.ParentNode;
            RBTreeNode uncleNode = pDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
            EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, direction);
        }

        private void EraseColor(RBTreeNode newNode,RBTreeNode brotherNode,RBTreeNode parentNode,RBTreeNode uncleNode,RBTreeNode gparentNode,RBTreeDirection direction)
        {
            RBTreeDirection pDirection = GetDirection(parentNode, parentNode.ParentNode);
            bool isRootNode = IsRootNode(gparentNode);
            RBTreeDirection gDirection = GetDirection(gparentNode, gparentNode.ParentNode);
            RBTreeNode temp = gparentNode.ParentNode;
            if (direction == RBTreeDirection.right)
            {
                
                if (pDirection == RBTreeDirection.right)
                {
                    //        b                         r                        r                   
                    //       / \                       / \                      / \                  
                    //      r   r      ---->          r   b       --->         b   b      
                    //     / \                           / \                      / \                  
                    //    r  (b)                       (b)  r                   (b)  r                      
                    if (IsRedNode(uncleNode))
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.left);
                        gparentNode.SetChild(brotherNode, RBTreeDirection.left);
                        newNode.SetColor(RBTreeColor.black);
                        //如果是根节点,将当前祖父节点颜色设置为黑色,更新为根节点,颜色消除结束
                        if (isRootNode)
                        {
                            parentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = parentNode;
                            return;
                        }
                        else
                        {
                            //如果当前祖父节点的父节点是黑色节点,颜色消除结束
                            if (!IsRedNode(parentNode.ParentNode))
                            {
                                return;
                            }
                            //否则重复此过程
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(parentNode, parentNode.ParentNode);
                                newNode = parentNode;
                                parentNode = parentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newgDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newgDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }
                    //        b                         r                         b
                    //       / \                       / \                       / \
                    //      r   b     ---->           r   b        ---->        r   r
                    //     / \                           / \                       / \
                    //    r  (b)                       (b)  b                    (b)  b
                    else
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.left);
                        gparentNode.SetChild(brotherNode, RBTreeDirection.left);
                        parentNode.SetColor(RBTreeColor.black);
                        gparentNode.SetColor(RBTreeColor.red);
                        if (isRootNode)
                        {
                            rootTreeNode = parentNode;
                        }
                        return;
                    }
                }
                else
                {
                    //    b                       r
                    //   / \                     / \
                    //  r   r       ----->      b  (b)
                    //     / \                 / \
                    //    r  (b)              r   r
                    if (IsRedNode(uncleNode))
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.right);
                        gparentNode.SetChild(newNode, RBTreeDirection.right);
                        if (isRootNode)
                        {
                            parentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = parentNode;
                            return;
                        }
                        else
                        {
                            if (!IsRedNode(parentNode.ParentNode))
                            {
                                return;
                            }
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(parentNode, parentNode.ParentNode);
                                newNode = parentNode;
                                parentNode = parentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newpDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newpDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }
                    //        b                        r
                    //       / \                      / \
                    //      b   r       ---->        b   b        变换颜色无需旋转
                    //         / \                      / \
                    //        r  (b)                   r  (b)
                    else
                    {
                        parentNode.SetColor(RBTreeColor.black);
                        gparentNode.SetColor(RBTreeColor.red);
                        if (isRootNode)
                        {
                            gparentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = gparentNode;
                            return;
                        }
                        else
                        {
                            if (!IsRedNode(gparentNode.ParentNode))
                            {
                                return;
                            }
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(gparentNode, gparentNode.ParentNode);
                                newNode = gparentNode;
                                parentNode = gparentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newpDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newpDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }
                }
            }
            else
            {
                if (pDirection == RBTreeDirection.left)
                {
                    //       b                           r                              r 
                    //      / \                         / \                            / \
                    //     r   r             ---->     b   r         ---->            b   b
                    //        / \                     / \                            / \
                    //       b   r                   r   b                          r   b
                    if (IsRedNode(uncleNode))
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.left);
                        gparentNode.SetChild(parentNode, RBTreeDirection.right);
                        newNode.SetColor(RBTreeColor.black);
                        if (isRootNode)
                        {
                            parentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = parentNode;
                            return;
                        }
                        else
                        {
                            if (!IsRedNode(parentNode.ParentNode))
                            {
                                return;
                            }
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(parentNode, parentNode.ParentNode);
                                newNode = parentNode;
                                parentNode = parentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newpDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newpDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }
                    //       b                          r                       b
                    //      / \                        / \                     / \
                    //    (b)  r          ---->       b   r       ---->       r   r
                    //        / \                    / \                     / \
                    //       b   r                 (b)  b                  (b)  b
                    else
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.left);
                        gparentNode.SetChild(brotherNode, RBTreeDirection.right);
                        gparentNode.SetColor(RBTreeColor.red);
                        parentNode.SetColor(RBTreeColor.black);
                        if (isRootNode)
                        {
                            rootTreeNode = parentNode;
                        }
                    }                    
                }
                else
                {
                    //          b                       r
                    //         / \                     / \
                    //        r   r      ---->        b   b
                    //       / \                         / \
                    //      b   r                       r   r
                    if (IsRedNode(uncleNode))
                    {
                        parentNode.SetParent(temp, gDirection);
                        gparentNode.SetParent(parentNode, RBTreeDirection.right);
                        gparentNode.SetChild(newNode, RBTreeDirection.left);
                        if (isRootNode)
                        {
                            parentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = parentNode;
                            return;
                        }
                        else
                        {
                            if (!IsRedNode(parentNode.ParentNode))
                            {
                                return;
                            }
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(parentNode, parentNode.ParentNode);
                                newNode = parentNode;
                                parentNode = parentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newpDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newpDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }
                    //         b                       r  
                    //        / \                     / \
                    //       r  (b)     ---->        b  (b)  变换颜色无需旋转
                    //      / \                     / \
                    //     b   r                   b   r
                    else
                    {
                        parentNode.SetColor(RBTreeColor.black);
                        gparentNode.SetColor(RBTreeColor.red);
                        if (isRootNode)
                        {
                            gparentNode.SetColor(RBTreeColor.black);
                            rootTreeNode = gparentNode;
                            return;
                        }
                        else
                        {
                            if (!IsRedNode(gparentNode.ParentNode))
                            {
                                return;
                            }
                            else
                            {
                                RBTreeDirection newDirection = GetDirection(gparentNode, gparentNode.ParentNode);
                                newNode = gparentNode;
                                parentNode = gparentNode.ParentNode;
                                brotherNode = newDirection == RBTreeDirection.left ? parentNode.RightNode : parentNode.LeftNode;
                                RBTreeDirection newpDirection = GetDirection(parentNode, parentNode.ParentNode);
                                gparentNode = parentNode.ParentNode;
                                uncleNode = newpDirection == RBTreeDirection.left ? gparentNode.RightNode : gparentNode.LeftNode;
                                EraseColor(newNode, brotherNode, parentNode, uncleNode, gparentNode, newDirection);
                            }
                        }
                    }                    
                }
            }
        }

        private void Rotate(RBTreeNode newNode,RBTreeNode parentNode,RBTreeNode uncleNode,RBTreeNode gparentNode,RBTreeDirection direction)
        {
            //          b                      r
            //         / \                    / \
            //        r   r   --->           b   b
            //       /                            \
            //      b                              r
            bool isroot = IsRootNode(gparentNode);
            RBTreeNode temp = gparentNode.ParentNode;
            RBTreeDirection dir = GetDirection(gparentNode, gparentNode.ParentNode);
            gparentNode.SetParent(parentNode, direction);
            RBTreeDirection antiDirection = (RBTreeDirection)((byte)direction ^ 1);
            gparentNode.SetChild(null, antiDirection);
            //如果祖父节点是根节点,旋转后直接设置为黑色,旋转结束
            if (isroot)
            {
                parentNode.SetParent(null, RBTreeDirection.left);
                parentNode.SetColor(RBTreeColor.black);
                rootTreeNode = parentNode;
            }
            //如果不是根节点,则需要根据祖父节点的颜色判断是否需要继续旋转来消除红-红情景
            else
            {
                parentNode.SetParent(temp, dir);               
            }
            #region 
            //if (direction == RBTreeDirection.right)
            //{               
            //    gparentNode.SetParent(parentNode, RBTreeDirection.right);
            //    //如果祖父节点是根节点,旋转后直接设置为黑色,旋转结束
            //    if (isroot)
            //    {
            //        parentNode.SetParent(null, RBTreeDirection.left);
            //        parentNode.SetColor(RBTreeColor.black);
            //    }
            //    //如果不是根节点,则需要根据祖父节点的颜色判断是否需要继续旋转来消除红-红情景
            //    else
            //    {
            //        parentNode.SetParent(temp, dir);
            //    }
            //}
            //else
            //{
            //    gparentNode.SetParent(parentNode, RBTreeDirection.left);
            //    if (isroot)
            //    {
            //        parentNode.SetParent(null, RBTreeDirection.left);
            //        parentNode.SetColor(RBTreeColor.black);
            //    }
            //    else
            //    {
            //        parentNode.SetParent(temp, dir);
            //    }
            //}
            #endregion
        }

        private RBTreeNode GetParentNodeToInsert(RBTreeNode newNode,RBTreeNode currentNode)
        {
            RBTreeNode tempNode = currentNode;
            if (newNode.Key.CompareTo(tempNode.Key) > 0)
            {
                if (tempNode.RightNode == null)
                    return tempNode;
                else
                    return GetParentNodeToInsert(newNode, tempNode.RightNode);
            }
            else if (newNode.Key.CompareTo(tempNode.Key) == 0)
            {               
                return tempNode;
            }
            else
            {
                if (tempNode.LeftNode == null)
                    return tempNode;
                else
                    return GetParentNodeToInsert(newNode, tempNode.LeftNode);
            }
        }

        private bool IsRedNode(RBTreeNode node)
        {
            if (node == null)
                return false;
            return node.Color == RBTreeColor.red;
        }

        private bool IsRootNode(RBTreeNode node)
        {
            return rootTreeNode.Equals(node);
        }

        private bool IsLeaveNode(RBTreeNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node is null");
            if (node.LeftNode == null && node.RightNode == null)
                return true;
            return false;
        }

        private void ExchangeNodeValue(RBTreeNode node1,RBTreeNode node2)
        {
            if (node1 == null || node2 == null)
                return;
            T2 temp = node1.Value;
            T1 key = node1.Key;
            node1.Value = node2.Value;
            node1.Key = node2.Key;
            node2.Value = temp;
            node2.Key = key;
        }

        private RBTreeDirection GetDirection(RBTreeNode childNode,RBTreeNode parentNode)
        {
            if (parentNode == null)
                return RBTreeDirection.left;
            if (childNode.Equals(parentNode.LeftNode))
                return RBTreeDirection.left;
            else
                return RBTreeDirection.right;
        }
    }
}
