using UnityEngine;

[CreateAssetMenu(fileName = "NormalEffect", menuName = "Cards/Effects/Normal")]
public class NormalCard : CardPlaySO
{
    public override bool Play(
        CardPresenter card,
        CardEffectResolver resolver)
    {
        return resolver != null &&
               resolver.TryPlayNormal(card);
    }


}
