using UnityEngine;

[CreateAssetMenu(fileName = "SkipEffect",menuName = "Cards/Effects/Skip")]
public class SkipCardSO : CardPlaySO
{
    public override bool Play(
        CardPresenter card,
        CardEffectResolver resolver)
    {
        return resolver.TryPlaySkip(card);
    }
}