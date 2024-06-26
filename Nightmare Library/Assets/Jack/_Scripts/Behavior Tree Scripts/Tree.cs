using UnityEngine;

namespace BehaviorTree
{
    public abstract class Tree
    {
        public Node root = null;

        public void Initialize()
        {
            root = SetupTree();
        }

        public virtual void UpdateTree(float dt)
        {
            // Evaluate the nodes
            if(root != null)
            {
                root.Check(dt);
            }
        }

        protected abstract Node SetupTree();
    }
}
