using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    Image _cardBackground;
    [SerializeField] public CardViewData _cardData;

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
}
