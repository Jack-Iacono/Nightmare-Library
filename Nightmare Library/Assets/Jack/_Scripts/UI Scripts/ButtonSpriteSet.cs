using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ButtonSpriteSet", order = 1)]
public class ButtonSpriteSet : ScriptableObject
{

    public Sprite normalSprite;
    public Sprite hoverSprite;
    public Sprite pressedSprite;

}
