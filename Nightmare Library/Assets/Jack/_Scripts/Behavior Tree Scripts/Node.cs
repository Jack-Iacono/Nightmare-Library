using System.Collections.Generic;

namespace BehaviorTree
{

    public enum Status
    {
        RUNNING, SUCCESS, FAILURE
    }

    public class Node
    {
        protected Status status;

        public Node parent;
        protected List<Node> children = new List<Node>();

        private Dictionary<string, object> sharedData = new Dictionary<string, object>();

        public Node()
        {
            parent = null;
        }
        public Node(List<Node> children)
        {
            foreach(Node child in children)
            {
                AddChild(child);
            }
        }

        private void AddChild(Node child)
        {
            child.parent = this;
            children.Add(child);
        }

        public virtual Status Check(float dt) => Status.FAILURE;

        public void SetData(string key, object value)
        {
            sharedData[key] = value;
        }

        /// <summary>
        /// Sets data for the root of the Behavior Tree i.e. makes a global variable
        /// </summary>
        /// <param name="key">The key for the dictionary</param>
        /// <param name="value">The value to store</param>
        public void SetRootData(string key, object value)
        {
            if(parent != null)
                parent.SetRootData(key, value);
            else
                SetData(key, value);
        }

        public object GetData(string key)
        {
            object value = null;

            if(sharedData.TryGetValue(key, out value))
                return value;

            Node node = parent;
            while(node != null)
            {
                value = node.GetData(key);
                if(value != null) return value;
                node = node.parent;
            }

            // At the root of the tree
            return null;
        }
        public bool ClearData(string key)
        {
            if (sharedData.ContainsKey(key))
            {
                sharedData.Remove(key);
                return true;
            }

            Node node = parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared) return true;
                node = node.parent;
            }

            // At the root of the tree
            return false;
        }
    }

}
