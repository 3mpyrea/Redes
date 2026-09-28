using System.Linq;
using Fusion;
using UnityEngine;
using static CardDrag;

public class CardEffectResolver : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private DeckHandler deckHandler;

    public bool IsConfiguredFor(
        DeckHandler deck,
        GameManager game)
    {
        return deckHandler == deck &&
               gameManager == game;
    }

    private bool CanResolve(CardPresenter card)
    {
        if (deckHandler == null || gameManager == null)
            return false;

        if (!deckHandler.IsManagedCard(card))
            return false;

        return card.CurrentLocation == CardLocation.InHand &&
               deckHandler.CanAct(card.OwnerRef);
    }

    public bool TryPlaySkip(CardPresenter card)
    {
        if (!CanResolve(card))
            return false;

        PlayerRef player = card.OwnerRef;

        deckHandler.DiscardCard(card);
        gameManager.EndTurn(player);

        return true;
    }

    public bool TryPlayNormal(CardPresenter card)
    {
        if (!CanResolve(card))
            return false;

        deckHandler.DiscardCard(card);

        // No tiene efecto adicional ni termina el turno.
        return true;
    }

    public bool ResolveExplosive(CardPresenter bomb)
    {
        if (!CanResolve(bomb))
            return false;

        if (bomb.cardData.cardID != DeckHandler.ExplosiveId)
            return false;

        PlayerRef player = bomb.OwnerRef;

        CardPresenter defuser =
            deckHandler.cardDeck.FirstOrDefault(card =>
                deckHandler.IsManagedCard(card) &&
                card.CurrentLocation == CardLocation.InHand &&
                card.OwnerRef == player &&
                card.cardData.cardID == DeckHandler.DeactivateId
            );

        if (defuser == null)
        {
            // Revelar la explosiva y declarar al perdedor.
            deckHandler.DiscardCard(bomb);
            gameManager.FinishMatch(player);
            return true;
        }

        // Consumir la defensa y devolver la explosiva al mazo.
        deckHandler.DiscardCard(defuser);
        deckHandler.ReturnToDeckRandomly(bomb);

        // El método de robo finalizará el turno.
        return true;
    }
}