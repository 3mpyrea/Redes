using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
public class DragManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CardDrag dragObject;
    [SerializeField] private GraphicRaycaster _graphicRaycaster;
    private List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private void Start()
    {
        //Debug.Log("drag start func");
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

        _raycastResults.Clear();

        //Debug.Log("start drag");
        _graphicRaycaster.Raycast(eventData, _raycastResults);
        if (_raycastResults.Count > 0)
        {
            dragObject = _raycastResults[0].gameObject.GetComponent<CardDrag>();
            dragObject.StartDrag(eventData);
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        // Debug.Log("dragging");
        if (dragObject != null)
        {
            dragObject.transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
            foreach (Image item in dragObject.transform)
            { item.color = new Color32(65, 65, 65, 255); }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragObject.EndDrag();
        dragObject = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click");
        _raycastResults.Clear();
        _graphicRaycaster.Raycast(eventData, _raycastResults);
        if (_raycastResults.Count > 0)
        {
            dragObject = _raycastResults[0].gameObject.GetComponent<CardDrag>();
            dragObject.Click(dragObject.rectTransform);
            Debug.Log("drag object found");
        }
    }
}