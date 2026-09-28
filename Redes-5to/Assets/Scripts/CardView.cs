using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    Image _cardBackground;
    [SerializeField] public CardViewData _cardData;
    [SerializeField] private GameObject back;


    private void Awake()
    {
        _cardBackground = GetComponent<Image>();

        var presenter = GetComponentInParent<CardViewData.IProvider>();
        presenter.onCardUpdate += CreateIcon;
    }

    void CreateIcon(CardViewData data)
    {
        _cardBackground.sprite = data._background;
        _cardData = data;
    }

    public void SetFaceDown(bool faceDown)
    {
        back.SetActive(faceDown);
    }
}
