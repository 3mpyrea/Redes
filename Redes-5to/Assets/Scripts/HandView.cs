using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private List<CardView> hand = new List<CardView>();

    [Header("Forma del abanico")]
    [SerializeField] private float separationX ;
    [SerializeField] private float archHeight ;
    [SerializeField] private float maxRotation;
    [SerializeField] private Vector2 center = Vector2.zero;

    [Header("Orientación")]
    [SerializeField] private bool opponentHand;

    private bool _layoutDirty;

    private void OnEnable()
    {
        _layoutDirty = true;
    }

    private void OnTransformChildrenChanged()
    {
        _layoutDirty = true;
    }

    // Conservamos estos métodos para que CardPresenter
    // pueda seguir utilizándolos sin cambiar sus llamadas.
    public void AddCardToFan(CardView card)
    {
        _layoutDirty = true;
    }

    public void RemoveCardFromFan(CardView card)
    {
        _layoutDirty = true;
    }

    private void LateUpdate()
    {
        if (!_layoutDirty)
            return;

        _layoutDirty = false;
        RebuildFan();
    }

    private void RebuildFan()
    {
        hand.Clear();

        // La mano se reconstruye desde sus hijos reales.
        foreach (Transform child in transform)
        {
            CardView view = child.GetComponent<CardView>();

            if (view != null)
                hand.Add(view);
        }

        int count = hand.Count;
        float direction = opponentHand ? -1f : 1f;

        for (int i = 0; i < count; i++)
        {
            RectTransform cardRect =
                hand[i].GetComponent<RectTransform>();

            if (cardRect == null)
                continue;

            // Recorre el abanico desde -1 hasta +1.
            float t = count > 1
                ? (float)i / (count - 1) * 2f - 1f
                : 0f;

            // Centra el conjunto alrededor del punto de la mano.
            float x = (i - (count - 1) * 0.5f) * separationX;

            // Los extremos quedan en la base y el centro se eleva.
            float y = count > 1
                ? (1f - t * t) * archHeight
                : 0f;

            Vector3 targetPosition = new Vector3(
                center.x + x,
                center.y + direction * y,
                0f
            );

            // La mano rival mira hacia el jugador de arriba.
            float baseRotation = opponentHand ? 180f : 0f;
            float angle = baseRotation - direction * t * maxRotation;

            cardRect.DOKill();

            cardRect
                .DOLocalMove(targetPosition, 0.4f)
                .SetEase(Ease.OutQuad);

            cardRect
                .DOLocalRotate(new Vector3(0f, 0f, angle), 0.4f)
                .SetEase(Ease.OutQuad);
        }
    }
}