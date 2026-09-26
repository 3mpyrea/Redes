using System;
using UnityEngine;
using UnityEngine.UI;

public class CardPresenter : MonoBehaviour, CardViewData.IProvider
{
    CardView _view;
    [SerializeField] public CardViewData _data;


    public event Action<CardViewData> onCardUpdate;

    public void Initialize(CardViewData data)
    {

        //onCardUpdate += Duplicate;
        onCardUpdate?.Invoke(data);
    }


}
