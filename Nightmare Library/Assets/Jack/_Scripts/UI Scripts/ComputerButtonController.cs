using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ComputerButtonController : EventTrigger
{
    private Image image;
    private bool onImage = false;
    private int spriteSetIndex = 0;

    [Tooltip("Sprite Sets will be referenced by index, default is index 0")]
    public List<ButtonSpriteSet> spriteSets;

    [Header("Sound Override")]
    public bool enableSounds = true;

    [Header("Text Overrides")]
    public RectTransform textRect;
    public Vector2 textOffset = new Vector2(0, -10);

    private void Start()
    {
        image = GetComponent<Image>();
        image.sprite = spriteSets[spriteSetIndex].normalSprite;
    }

    private void OnDisable()
    {
        if(image)
            image.sprite = spriteSets[spriteSetIndex].normalSprite;
    }

    public override void OnPointerEnter(PointerEventData data)
    {
        onImage = true;

        image.sprite = spriteSets[spriteSetIndex].hoverSprite;

        base.OnPointerEnter(data);
    }
    public override void OnPointerExit(PointerEventData data)
    {
        onImage = false;

        image.sprite = spriteSets[spriteSetIndex].normalSprite;

        if (textRect)
            textRect.anchoredPosition = Vector2.zero;

        base.OnPointerExit(data);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        image.sprite = spriteSets[spriteSetIndex].pressedSprite;

        

        if (textRect)
            textRect.anchoredPosition = textOffset;

        if (onImage)
            base.OnPointerDown(eventData);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (onImage)
        {
            base.OnPointerUp(eventData);
            image.sprite = spriteSets[spriteSetIndex].hoverSprite;

            if (textRect)
                textRect.anchoredPosition = Vector2.zero;
        }
        else
            image.sprite = spriteSets[spriteSetIndex].normalSprite;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (onImage)
            base.OnPointerClick(eventData);
    }

    public void SetSpriteSetIndex(int i)
    {
        if(spriteSets.Count > i)
            spriteSetIndex = i;
        else
        {
            Debug.Log("Index too large, default to last sprite set");
            spriteSetIndex = spriteSets.Count - 1;
        }

        if(!image)
            image = GetComponent<Image>();

        image.sprite = spriteSets[spriteSetIndex].normalSprite;
    }
}
