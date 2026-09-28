using UnityEngine;

public abstract class CardPlaySO : ScriptableObject
{
    public abstract bool Play(CardPresenter card,CardEffectResolver resolver);
}