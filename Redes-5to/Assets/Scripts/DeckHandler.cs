using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeckHandler : MonoBehaviour
{
    public static DeckHandler instance;


    [SerializeField] CardViewData[] cardsAvailabe;
    public List<CardView> cardDeck = new List<CardView>();

    [SerializeField] public CardPresenter cardPrefab;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void InitiateDeck()
    {
        foreach (var card in cardsAvailabe)
        {
            CardPresenter newCard = Instantiate(cardPrefab, transform.position, transform.rotation);
            newCard.Initialize(card);
            cardDeck.Add(newCard.GetComponent<CardView>());
            newCard.GetComponent<RectTransform>().SetParent(GameManager.instance.canvas.transform, false);
            newCard.gameObject.SetActive(false);
            
        }
    }

    public void Shuffle(List<CardView> source)
    {
        if (source == null || source.Count <= 1) return;

        int n = source.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);

            CardView value = source[k];
            source[k] = source[n];
            source[n] = value;
        }
    }
}
