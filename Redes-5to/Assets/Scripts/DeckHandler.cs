using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using static CardDrag;

public class DeckHandler : NetworkBehaviour
{
    public static DeckHandler instance;

    public const int ExplosiveId = 0;
    public const int DeactivateId = 1;

    [Header("Efectos")]
    [SerializeField] private CardEffectResolver effectResolver;

    [Header("Mazo")]
    [SerializeField] private CardViewData[] cardsAvailabe;
    [SerializeField] public CardPresenter cardPrefab;

    public List<CardPresenter> cardDeck =
        new List<CardPresenter>();

    private bool _deckStarted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public override void Spawned()
    {
        Debug.Log(
            $"DeckHandler registrado. " +
            $"Autoridad: {Object.HasStateAuthority}",
            this
        );
    }

    public void InitiateDeck(NetworkRunner runner)
    {
        if (runner == null ||
            !runner.IsSharedModeMasterClient ||
            _deckStarted)
        {
            return;
        }

        if (cardsAvailabe == null ||
            cardPrefab == null ||
            GameManager.instance == null ||
            GameManager.instance.deckPoint == null)
        {
            Debug.LogError("Faltan referencias para crear el mazo.");
            return;
        }

        _deckStarted = true;
        Shuffle(cardsAvailabe);

        foreach (CardViewData data in cardsAvailabe)
        {
            if (data == null)
                continue;

            CardPresenter card = runner.Spawn(
                cardPrefab,
                GameManager.instance.deckPoint.position,
                Quaternion.identity
            );

            card.Initialize(data);
            cardDeck.Add(card);
        }

        StartCoroutine(DelayedCardDistribution(runner));
    }

    private IEnumerator DelayedCardDistribution(
        NetworkRunner runner)
    {
        yield return new WaitForSeconds(0.2f);

        GameManager game = GameManager.instance;

        // Esperar el registro de ambos componentes antes del reparto.
        while (game != null &&
               (Object == null ||
                !Object.IsValid ||
                game.Object == null ||
                !game.Object.IsValid))
        {
            yield return null;
            game = GameManager.instance;
        }

        if (game == null)
            yield break;

        if (!Object.HasStateAuthority ||
            !game.Object.HasStateAuthority ||
            game.Runner != runner)
        {
            Debug.LogError(
                "El mazo y GameManager necesitan la misma autoridad."
            );
            yield break;
        }

        game.DealInitialCards(runner);
    }

    public CardViewData GetCardDataByID(int idToFind)
    {
        if (cardsAvailabe == null)
            return null;

        return cardsAvailabe.FirstOrDefault(data =>
            data != null &&
            data.cardID == idToFind
        );
    }

    public void Shuffle(CardViewData[] source)
    {
        if (source == null)
            return;

        for (int i = source.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            CardViewData temporary = source[i];
            source[i] = source[randomIndex];
            source[randomIndex] = temporary;
        }
    }

    public bool CanAct(PlayerRef player)
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority)
        {
            return false;
        }

        GameManager game = GameManager.instance;

        if (game == null ||
            game.Object == null ||
            !game.Object.IsValid ||
            !game.Object.HasStateAuthority ||
            game.Runner != Runner)
        {
            return false;
        }

        if (effectResolver == null ||
            !effectResolver.IsConfiguredFor(this, game))
        {
            return false;
        }

        return game._initialDealComplete &&
               Runner.ActivePlayers.Contains(player) &&
               game.IsPlayersTurn(player);
    }

    public bool IsManagedCard(CardPresenter card)
    {
        return Object != null &&
               Object.IsValid &&
               Object.HasStateAuthority &&
               card != null &&
               card.Object != null &&
               card.Object.IsValid &&
               card.Object.HasStateAuthority &&
               card.Runner == Runner &&
               card.cardData != null &&
               cardDeck.Contains(card);
    }

    public void GiveCard(
        CardPresenter card,
        PlayerRef player)
    {
        if (!IsManagedCard(card))
            return;

        card.SetOwnerAndLocation(
            player,
            CardLocation.InHand,
            true
        );
    }

    public void DiscardCard(CardPresenter card)
    {
        if (!IsManagedCard(card))
            return;

        card.SetOwnerAndLocation(
            PlayerRef.None,
            CardLocation.InDiscard,
            false
        );
    }

    public void ReturnToDeckRandomly(CardPresenter card)
    {
        if (!IsManagedCard(card))
            return;

        // Quitar su posición anterior en la lista.
        cardDeck.Remove(card);

        var remainingDeck = cardDeck.Where(c =>
            c != null &&
            c.Object != null &&
            c.Object.IsValid &&
            c.CurrentLocation == CardLocation.InDeck
        ).ToList();

        // Elegir un hueco entre las cartas restantes,
        // incluyendo el principio y el final.
        int position = Random.Range(
            0,
            remainingDeck.Count + 1
        );

        if (position == remainingDeck.Count)
        {
            cardDeck.Add(card);
        }
        else
        {
            int index = cardDeck.IndexOf(
                remainingDeck[position]
            );

            cardDeck.Insert(index, card);
        }

        card.SetOwnerAndLocation(
            PlayerRef.None,
            CardLocation.InDeck,
            true
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestDraw(RpcInfo info = default)
    {
        PlayerRef player = info.Source;

        if (!CanAct(player))
            return;

        CardPresenter card = cardDeck.FirstOrDefault(c =>
            c != null &&
            c.Object != null &&
            c.Object.IsValid &&
            c.CurrentLocation == CardLocation.InDeck
        );

        if (!IsManagedCard(card))
        {
            Debug.LogWarning(
                "No hay una carta disponible bajo esta autoridad."
            );
            return;
        }

        bool isExplosive =
            card.cardData.cardID == ExplosiveId;

        CardPlaySO effect = card.cardData._playRef;

        // Verificar antes de sacar la carta del mazo.
        if (isExplosive && !(effect is BombCardSO))
        {
            Debug.LogError(
                "La explosiva necesita un BombCardSO en Play Ref."
            );
            return;
        }

        GiveCard(card, player);

        // Solo la explosiva se activa automáticamente al robar.
        if (isExplosive)
        {
            bool resolved = effect.Play(card, effectResolver);

            if (!resolved)
            {
                Debug.LogError(
                    "No se pudo resolver la explosiva."
                );
                return;
            }
        }

        GameManager game = GameManager.instance;

        if (!game.GameOver)
            game.EndTurn(player);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestPlay(NetworkId cardId,RpcInfo info = default)
    {
        PlayerRef player = info.Source;

        if (!CanAct(player))return;

        CardPresenter card = cardDeck.FirstOrDefault(c =>
            IsManagedCard(c) &&
            c.Object.Id == cardId
        );

        if (card == null ||card.CurrentLocation != CardLocation.InHand ||card.OwnerRef != player)
        {
            return;
        }

        int typeId = card.cardData.cardID;

        // No se juegan directamente desde la mano.
        if (typeId == ExplosiveId || typeId == DeactivateId)
        {
            return;
        }

        CardPlaySO effect = card.cardData._playRef;

        if (effect == null)
        {
            Debug.LogWarning(
                "La carta no tiene un efecto asignado."
            );
            return;
        }

        bool resolved = effect.Play(card, effectResolver);

        if (!resolved)Debug.Log("La jugada fue rechazada.");

        // No descartar aquí.
        // Cada efecto mueve las cartas que corresponden.
    }
}