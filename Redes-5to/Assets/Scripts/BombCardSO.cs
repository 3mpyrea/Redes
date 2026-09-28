using UnityEngine;

[CreateAssetMenu(fileName = "BombEffect", menuName = "Cards/Effects/Bomb")]
public class BombCardSO : CardPlaySO
{
    public override bool Play(
      CardPresenter card,
      CardEffectResolver resolver)
    {
        return resolver != null &&
               resolver.ResolveExplosive(card);
    }
}
