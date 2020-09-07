using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vertex.datastructure;


namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {           
            RBTree<int, int> rbtree = new RBTree<int, int>();
            rbtree.Insert(30, 30);
            rbtree.Insert(25, 25);
            rbtree.Insert(36, 36);
            rbtree.Insert(48, 48);
            rbtree.Insert(49, 49);
            rbtree.Insert(50, 50);
            rbtree.Insert(26, 26);
            rbtree.Insert(22, 22);
            rbtree.Insert(23, 23);

            int value = rbtree.Find(48);

            rbtree.Update(50, 500);

            int v1 = rbtree.Find(50);

            rbtree.Delete(26);
        }
    }
}
