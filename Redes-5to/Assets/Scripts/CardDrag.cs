using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using DG.Tweening;
using TMPro;

public class CardDrag : MonoBehaviour
{
    public RectTransform rectTransform;

    public Vector2 originalPos;
    public Vector3 originalSize;

    bool zoomed;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalSize = rectTransform.localScale;

    }
    public void StartDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
           (RectTransform)GameManager.instance.canvas.GetComponent<Canvas>().transform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        originalPos = rectTransform.anchoredPosition; ;
    }

    public void EndDrag()
    {
        Debug.Log("ending drag");
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("hit: " + hit.transform.gameObject.name);
            if (hit.collider.gameObject.GetComponent<DiscardPlace>())
            {
               // var animalito = hit.collider.gameObject.GetComponent<Animalito>();
                //if (animalito != null && animalito.IsPlayerAlly)
                //{
                //    rectTransform.anchoredPosition = originalPos;   // vuelve a la mano
                //    return;
                //}
                GetComponent<CardView>()._cardData._playRef.Play(gameObject);
            }
            else { rectTransform.anchoredPosition = originalPos; }
        }
    }


    public void Click(RectTransform cardPos)
    {
        //RectTransform cardTxtRect = GameManager.instance.descriptTxt.GetComponent<RectTransform>();

        Debug.Log("enter click");
        if (!zoomed)
        {
            rectTransform.DOScale(originalSize * 2, 0.2f)
                    .SetEase(Ease.OutQuad);



            //cardTxtRect.localPosition = new Vector3(cardPos.localPosition.x, cardPos.localPosition.y + 400, cardPos.localPosition.z);
           // GameManager.instance.descriptTxt.SetActive(true);
           // cardTxtRect.localScale = Vector3.zero;
           // cardTxtRect.DOScale(GameManager.instance.descriptTxtSize, 0.3f).SetEase(Ease.OutQuad);
           // GameManager.instance.descriptTxt.GetComponentInChildren<TextMeshProUGUI>().text = GetComponent<CardView>()._cardData._description;

            zoomed = true;
        }
        else if (zoomed)
        {
            rectTransform.DOScale(originalSize, 0.2f)
                   .SetEase(Ease.OutQuad);

           // cardTxtRect.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            //GameManager.instance.descriptTxt.SetActive(false);

            zoomed = false;
        }
    }
}
