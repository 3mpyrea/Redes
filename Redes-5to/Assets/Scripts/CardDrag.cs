using UnityEngine;

public class CardDrag : MonoBehaviour
{
    public enum CardLocation
    {
        InDeck,
        InHand,
        InDiscard
    }

    private CardPresenter _presenter;

    private void Awake()
    {
        _presenter = GetComponent<CardPresenter>();
    }

    public void OnCardClicked()
    {
        // Primero verificamos que la carta esté lista en Fusion.
        if (_presenter == null ||
            _presenter.Object == null ||
            !_presenter.Object.IsValid)
        {
            Debug.LogWarning(
                "La carta aún no está inicializada en Fusion.",
                this
            );
            return;
        }

        // Después comprobamos el turno del jugador local.
        GameManager game = GameManager.instance;

        if (game == null ||
            !game.IsPlayersTurn(_presenter.Runner.LocalPlayer))
        {
            Debug.Log("Espera tu turno.");
            return;
        }

        // Solo llegamos aquí si podemos intentar una acción.
        switch (_presenter.CurrentLocation)
        {
            case CardLocation.InDeck:
                HandleDeckClick();
                break;

            case CardLocation.InHand:
                HandleHandClick();
                break;

            case CardLocation.InDiscard:
                break;
        }
    }
    private bool TryGetReadyDeck(out DeckHandler deck)
    {
        deck = DeckHandler.instance;

        if (deck == null)
        {
            Debug.LogWarning(
                "No existe una instancia de DeckHandler.",
                this
            );
            return false;
        }

        bool hasObject = deck.Object != null;
        bool isValid = hasObject && deck.Object.IsValid;

        Debug.Log(
            $"DeckHandler usado al hacer clic: {deck.GetInstanceID()} | " +
            $"Tiene Object: {hasObject} | " +
            $"Es válido: {isValid}",
            deck
        );

        if (!isValid)
        {
            Debug.LogWarning(
                "El mazo aún no está listo en Fusion.",
                deck
            );
            return false;
        }

        // Detecta también una referencia a otro runner.
        if (deck.Runner != _presenter.Runner)
        {
            Debug.LogWarning(
                "La carta y DeckHandler pertenecen a runners diferentes.",
                deck
            );
            return false;
        }

        return true;
    }

    private void HandleDeckClick()
    {
        if (!TryGetReadyDeck(out DeckHandler deck))
            return;

        // Las cartas del mazo no tienen propietario.
        deck.RPC_RequestDraw();
    }

    private void HandleHandClick()
    {
        if (_presenter.OwnerRef != _presenter.Runner.LocalPlayer)
            return;

        if (!TryGetReadyDeck(out DeckHandler deck))
            return;

        deck.RPC_RequestPlay(_presenter.Object.Id);
    }
}
