using System;
using Fusion;
using UnityEngine;
using static CardDrag;

public class CardPresenter : NetworkBehaviour, CardViewData.IProvider
{
    private CardView _view;

    [SerializeField] public CardViewData cardData;
    public event Action<CardViewData> onCardUpdate;

    [Networked] public NetworkBool IsFaceDown { get; set; }

    // 1. ATRIBUTOS DE RED DETERMINISTAS: 
    // Obligan a Fusion a ejecutar la función visual en cuanto la variable cambia en internet
    [Networked, OnChangedRender(nameof(OnVisualStateChanged))]
    public CardLocation CurrentLocation { get; set; }

    [Networked, OnChangedRender(nameof(OnVisualStateChanged))]
    public PlayerRef OwnerRef { get; set; }

    public override void Spawned()
    {
        _view = GetComponent<CardView>();

        // Inicialización forzada
        OnVisualStateChanged();
    }

    // Este método es llamado automáticamente por Fusion en cada frame de renderizado si hay cambios
    private void OnVisualStateChanged()
    {
        UpdateHierarchyAndPosition();
        UpdateLocalMVP();

        bool isMyHand =
    CurrentLocation == CardLocation.InHand &&
    OwnerRef == Runner.LocalPlayer;

        bool isDiscard =
            CurrentLocation == CardLocation.InDiscard;

        _view.SetFaceDown(!(isMyHand || isDiscard));
    }
    public void Initialize(CardViewData data)
    {
        cardData = data;
        IsFaceDown = true;
        CurrentLocation = CardLocation.InDeck;
        OwnerRef = PlayerRef.None;

        OnVisualStateChanged();
        RPC_SyncCardByID(data.cardID);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPC_SyncCardByID(int receivedID)
    {
        CardViewData originalData = DeckHandler.instance.GetCardDataByID(receivedID);
        if (originalData != null)
        {
            cardData = originalData;
            OnVisualStateChanged();
        }
    }

    public void SetOwnerAndLocation(
     PlayerRef newOwner,
     CardLocation newLocation,
     bool faceDown)
    {
        if (!Object.HasStateAuthority)
            return;

        OwnerRef = newOwner;
        CurrentLocation = newLocation;
        IsFaceDown = faceDown;

        OnVisualStateChanged();
    }

    public void UpdateHierarchyAndPosition()
    {
        if (GameManager.instance == null) return;

        Transform targetParent = GameManager.instance.deckPoint;
        HandView targetHandView = null;

        if (CurrentLocation == CardLocation.InHand)
        {
            if (OwnerRef == Runner.LocalPlayer)
            {
                targetHandView = GameManager.instance.localPoint.GetComponent<HandView>();
                if (targetHandView != null) targetParent = targetHandView.transform;
            }
            else
            {
                targetHandView = GameManager.instance.enemyPoint.GetComponent<HandView>();
                if (targetHandView != null) targetParent = targetHandView.transform;
            }
        }
        else if (CurrentLocation == CardLocation.InDiscard)
        {
            targetParent = GameManager.instance.discardPoint.transform;

            HandView localHand = GameManager.instance.localPoint.GetComponent<HandView>();
            HandView enemyHand = GameManager.instance.enemyPoint.GetComponent<HandView>();

            if (localHand != null) localHand.RemoveCardFromFan(_view);
            if (enemyHand != null) enemyHand.RemoveCardFromFan(_view);
        }

        // Si la jerarquía local de Unity necesita actualizarse
        if (targetParent != null && transform.parent != targetParent)
        {
            GetComponent<RectTransform>().SetParent(targetParent, false);
            transform.localScale = Vector3.one;

            if (targetHandView != null && _view != null)
            {
                targetHandView.AddCardToFan(_view);
            }
        }

        bool isInPile =
     CurrentLocation == CardLocation.InDeck ||
     CurrentLocation == CardLocation.InDiscard;

        if (isInPile &&
            targetParent != null &&
            transform.parent == targetParent)
        {
            // Cancela cualquier animación anterior del abanico.
            DG.Tweening.DOTween.Kill(transform);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }

    private void UpdateLocalMVP()
    {
        if (cardData == null) return;
        onCardUpdate?.Invoke(cardData);
    }
}
