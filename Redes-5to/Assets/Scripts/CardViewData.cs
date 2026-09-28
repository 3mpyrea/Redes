using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Xml;

[CreateAssetMenu(fileName = "CARD BASE", menuName = "SO/Card MVP")]

public class CardViewData : ScriptableObject
{
    [field: SerializeField] public string _description { get; private set; }
    [field: SerializeField] public Sprite _background { get; private set; }
    [field: SerializeField] public Sprite _backFace { get; private set; }
    [field: SerializeField] public int cardID;
    [field: SerializeField] public CardPlaySO _playRef { get; private set; }



    public interface IProvider
    {
        event Action<CardViewData> onCardUpdate;
    }

    
}
