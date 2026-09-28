using System;
using System.Linq;
using Fusion;
using UnityEngine;
using static CardDrag;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    [Header("Referencias de la mesa")]
    public Transform localPoint;
    public Transform enemyPoint;
    public GameObject canvas;
    public GameObject discardPoint;
    public Transform deckPoint;

    public bool _initialDealComplete;
    public bool IsNetworkReady { get; private set; }

    [Networked]
    public PlayerRef CurrentTurn { get; set; }

    [Networked]
    public NetworkBool MatchStarted { get; set; }

    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool GameOver { get; set; }

    [Networked]
    public PlayerRef Winner { get; set; }

    // Evento local: cada cliente avisa a su propia interfaz.
    public event Action ResultChanged;

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
        if (Object.HasStateAuthority)
        {
            CurrentTurn = PlayerRef.None;
            MatchStarted = false;
            GameOver = false;
            Winner = PlayerRef.None;

            _initialDealComplete = false;
        }

        IsNetworkReady = true;

        OnGameOverChanged();
    }

    private void OnGameOverChanged()
    {
        ResultChanged?.Invoke();
    }

    public void DealInitialCards(NetworkRunner runner)
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority ||
            runner != Runner ||
            !runner.IsSharedModeMasterClient ||
            _initialDealComplete)
        {
            return;
        }

        DeckHandler deck = DeckHandler.instance;

        if (deck == null || deck.cardDeck.Count == 0)
            return;

        const int explosiveId = 0;
        const int deactivateId = 1;
        const int cardsPerPlayer = 4;

        var players = runner.ActivePlayers
            .OrderBy(player => player.PlayerId)
            .ToList();

        // Esta partida está diseñada para dos jugadores.
        if (players.Count != 2)
            return;

        var availableCards = deck.cardDeck.Where(card =>
            card != null &&
            card.Object != null &&
            card.Object.IsValid &&
            card.Object.HasStateAuthority &&
            card.cardData != null &&
            card.CurrentLocation == CardLocation.InDeck
        ).ToList();

        var deactivateCards = availableCards.Where(card =>
            card.cardData.cardID == deactivateId
        ).ToList();

        var normalCards = availableCards.Where(card =>
            card.cardData.cardID != explosiveId &&
            card.cardData.cardID != deactivateId
        ).ToList();

        int requiredNormalCards =
            players.Count * (cardsPerPlayer - 1);

        if (deactivateCards.Count < players.Count ||
            normalCards.Count < requiredNormalCards)
        {
            Debug.LogWarning(
                "No hay suficientes cartas disponibles " +
                "para el reparto inicial.",
                this
            );
            return;
        }

        // Primera carta: una desactivadora para cada jugador.
        for (int i = 0; i < players.Count; i++)
        {
            deactivateCards[i].SetOwnerAndLocation(
                players[i],
                CardLocation.InHand,
                true
            );
        }

        // Completar las manos sin explosivas ni desactivadoras.
        int normalIndex = 0;

        foreach (PlayerRef player in players)
        {
            for (int i = 1; i < cardsPerPlayer; i++)
            {
                normalCards[normalIndex].SetOwnerAndLocation(
                    player,
                    CardLocation.InHand,
                    true
                );

                normalIndex++;
            }
        }

        _initialDealComplete = true;
        BeginTurns();
    }

    public bool IsPlayersTurn(PlayerRef player)
    {
        if (!IsNetworkReady ||
            Object == null ||
            !Object.IsValid)
        {
            return false;
        }

        return MatchStarted &&
               !GameOver &&
               player != PlayerRef.None &&
               CurrentTurn == player;
    }

    public void BeginTurns()
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority ||
            !_initialDealComplete ||
            MatchStarted ||
            GameOver)
        {
            return;
        }

        var players = Runner.ActivePlayers
            .OrderBy(player => player.PlayerId)
            .ToList();

        if (players.Count != 2)
            return;

        CurrentTurn = players[0];
        MatchStarted = true;
    }

    public void EndTurn(PlayerRef player)
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority ||
            !IsPlayersTurn(player))
        {
            return;
        }

        var players = Runner.ActivePlayers
            .OrderBy(activePlayer => activePlayer.PlayerId)
            .ToList();

        if (players.Count != 2)
        {
            CurrentTurn = PlayerRef.None;
            MatchStarted = false;
            return;
        }

        int currentIndex = players.IndexOf(player);

        if (currentIndex < 0)
            return;

        int nextIndex = (currentIndex + 1) % players.Count;
        CurrentTurn = players[nextIndex];
    }

    public void FinishMatch(PlayerRef loser)
    {
        if (Object == null ||
            !Object.IsValid ||
            !Object.HasStateAuthority ||
            !MatchStarted ||
            GameOver)
        {
            return;
        }

        var players = Runner.ActivePlayers.ToList();

        if (players.Count != 2 || !players.Contains(loser))
            return;

        PlayerRef winner = players.First(player => player != loser);

        // Guardar todos los datos del resultado.
        Winner = winner;
        CurrentTurn = PlayerRef.None;
        MatchStarted = false;
        GameOver = true;

        // Respuesta inmediata para la interfaz de la autoridad.
        // Los otros clientes recibirán OnChangedRender.
        OnGameOverChanged();
    }

    public override void Despawned(
    NetworkRunner runner,
    bool hasState)
    {
        IsNetworkReady = false;
        ResultChanged?.Invoke();
    }
}