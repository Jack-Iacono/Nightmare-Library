using System.Collections.Generic;
using System.Diagnostics;

namespace BehaviorTree
{
    /// <summary>
    /// Will check nodes until one returns either Failure or Running, it will then exit
    /// </summary>
    public class Sequence : Node
    {
        public Sequence() : base() { }
        public Sequence(List<Node> children) : base(children) { }

        public override Status Check(float dt)
        {
            // Will continue running if the node returns success, stops at this node otherwise
            foreach(Node node in children)
            {
                switch (node.Check(dt))
                {
                    case Status.FAILURE:
                        InvokeReset();
                        status = Status.FAILURE;
                        return status;
                    case Status.SUCCESS:
                        continue;
                    case Status.RUNNING:
                        status = Status.RUNNING;
                        return status;
                    default:
                        status = Status.SUCCESS;
                        return status;
                }
            }

            InvokeReset();
            status = Status.SUCCESS;
            return status;
        }
    }
}
