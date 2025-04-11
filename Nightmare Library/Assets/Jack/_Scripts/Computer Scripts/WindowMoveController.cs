using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowMoveController : EventTrigger
{
    private bool movingWindow = false;
    private RectTransform windowRect;
    [SerializeField]
    private ComputerWindow connectedWindow;

    private Vector2 windowSize;

    private Canvas canvas;
    private RectTransform canvasRectTransform;
    private Vector2 canvasLocalPosition;
    private Vector2 cursorOffset = Vector2.zero;

    private Vector2 canvasSize;
    private Vector2 canvasBoundsX;
    private Vector2 canvasBoundsY;

    private void Start()
    {
        windowRect = connectedWindow.GetComponent<RectTransform>();
        windowSize = windowRect.rect.size;

        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        canvasSize = canvasRectTransform.rect.size;

        canvasBoundsX = new Vector2((-canvasSize.x / 2) + (windowSize.x / 2), (canvasSize.x / 2) - (windowSize.x / 2));
        canvasBoundsY = new Vector2((-canvasSize.y / 2) + (windowSize.y / 2), (canvasSize.y / 2) - (windowSize.y / 2));

        canvasLocalPosition = windowRect.anchoredPosition;
    }

    private void Update()
    {
        if (movingWindow)
        {
            canvasLocalPosition = new Vector2(Input.mousePosition.x - canvasSize.x / 2, Input.mousePosition.y - canvasSize.y / 2);

            canvasLocalPosition += cursorOffset;

            canvasLocalPosition.x = Mathf.Clamp(canvasLocalPosition.x, canvasBoundsX.x, canvasBoundsX.y);
            canvasLocalPosition.y = Mathf.Clamp(canvasLocalPosition.y, canvasBoundsY.x, canvasBoundsY.y);

            windowRect.anchoredPosition = canvasLocalPosition;
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        EndMovingWindow();
        base.OnPointerUp(eventData);
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        BeginMovingWindow();

        Vector2 mousePosition = new Vector2(Input.mousePosition.x - canvasSize.x / 2, Input.mousePosition.y - canvasSize.y / 2);
        Vector2 offset = Vector2.zero;

        cursorOffset = canvasLocalPosition - mousePosition;

        base.OnPointerDown(eventData);
    }

    public void BeginMovingWindow()
    {
        movingWindow = true;
        connectedWindow.SetActiveWindow();
    }
    public void EndMovingWindow()
    {
        movingWindow = false;
    }
}
