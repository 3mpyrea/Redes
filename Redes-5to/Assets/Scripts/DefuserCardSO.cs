using UnityEngine;

[CreateAssetMenu(fileName = "DefuseEffect", menuName = "Cards/Effects/Defuse")]
public class DefuserCardSO : CardPlaySO
{
    public override bool Play(
        CardPresenter card,
        CardEffectResolver resolver)
    {
        return resolver.TryPlaySkip(card);
    }

}
