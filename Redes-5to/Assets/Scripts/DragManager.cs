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

        //_raycastResults.Clear();

        ////Debug.Log("start drag");
        //_graphicRaycaster.Raycast(eventData, _raycastResults);
        //if (_raycastResults.Count > 0)
        //{
        //    dragObject = _raycastResults[0].gameObject.GetComponent<CardDrag>();
        //    if (dragObject != null) { dragObject.StartDrag(eventData); } 
        //}

    }

    public void OnDrag(PointerEventData eventData)
    {
        //// Debug.Log("dragging");
        //if (dragObject != null)
        //{
        //    dragObject.transform.position += new Vector3(eventData.delta.x, eventData.delta.y, 0);
           
        //}
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (dragObject != null)
        //{
        //    //dragObject.EndDrag();
        //    dragObject = null;
        //}
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click");
        _raycastResults.Clear();
        _graphicRaycaster.Raycast(eventData, _raycastResults);
        if (_raycastResults.Count > 0)
        {
            dragObject = _raycastResults[0].gameObject.GetComponentInParent<CardDrag>();
            if (dragObject != null)
            {
                dragObject.OnCardClicked();
                Debug.Log("drag object found");
            }
        }
    }
}