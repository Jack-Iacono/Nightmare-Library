using BehaviorTree;
using System.Collections;
using System.Collections.Generic;

public class pa_Idols : PassiveAttack
{
    public float avgIdolPlaceTime = 20;
    private int idolsPlaced;

    public pa_Idols(Enemy owner) : base(owner)
    {

    }

    protected override Node SetupTree()
    {
        // Establises the Behavior Tree and its logic
        Node root = new Selector(new List<Node>()
        {
            new Sequence(new List<Node>
            {
                new TaskWait("idolPlace", avgIdolPlaceTime, 4)
            }),
            new TaskWait("idolPlace", avgIdolPlaceTime, 4)
        });

        root.SetData("speed", owner.moveSpeed);

        return root;
    }

    public void RemoveIdol()
    {
        idolsPlaced--;
    }
}
