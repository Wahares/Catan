using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerInventoryView : MonoBehaviour
{
    public List<CardView> visibleCards;

    [SerializeField]
    private Material blankNormal, blankTrade, blankPolitics, blankScience;
    [SerializeField]
    private GameObject handPrefab, specialPrefab, persistentPrefab;

    [SerializeField]
    private Transform handPivot, persistentPivot, specialPivot;

    private bool isMine = false;


    public List<HandCardView> selectedCards = new();



    public void initialize(bool isMine)
    {
        this.isMine = isMine;
    }

    private void revalidateViews()
    {
        distributeInHand(visibleCards.FindAll((card) => card is HandCardView).OrderBy((card) => card.ID).ToList());
        distributeSpecials(visibleCards.FindAll((card) => card is SpecialCardView).OrderBy((card) => card.ID).ToList());
        distributePersistent(visibleCards.FindAll((card) => card is PersistentPointCard).OrderBy((card) => card.ID).ToList());
    }

    public void AddCard(int ID)
    {
        GameObject card = null;
        CardSO item = PlayerInventoriesManager.instance.availableCards[ID];
        switch (item.CardViewType)
        {
            case cardViewType.Normal:
                card = Instantiate(handPrefab, handPivot);
                break;
            case cardViewType.Special:
                card = Instantiate(specialPrefab, specialPivot);
                break;
            case cardViewType.Persistent:
                card = Instantiate(persistentPrefab, persistentPivot);
                break;
        }
        card.transform.localEulerAngles = Vector3.zero;
        card.GetComponent<CardView>().Initialize(ID);
        if (!isMine)
        {
            Material newTexture = null;
            if (item is SpecialCard)
            {
                switch ((item as SpecialCard).sourceType)
                {
                    case growthType.Blank:
                        newTexture = blankNormal;
                        break;
                    case growthType.Trade:
                        newTexture = blankTrade;
                        break;
                    case growthType.Politics:
                        newTexture = blankPolitics;
                        break;
                    case growthType.Science:
                        newTexture = blankScience;
                        break;
                }
            }
            else
                card.GetComponent<CardView>();
            newTexture = blankNormal;
            card.GetComponent<CardView>().OverrideTexture(newTexture);
            if (card.GetComponent<HandCardEffect>() != null)
                card.GetComponent<HandCardEffect>().enabled = false;
        }
        visibleCards.Add(card.GetComponent<CardView>());
        revalidateViews();
    }
    public void RemoveCard(int ID)
    {
        CardView[] tmp = visibleCards.Where(card => card.ID == ID).ToArray();
        if (tmp.Length == 0)
        {
            Debug.LogError("Tried to remove null card!");
            return;
        }
        CardView view = tmp[0];
        visibleCards.Remove(view);
        view.DestroyCard();
        revalidateViews();
    }

    private void distributeInHand(List<CardView> cards)
    {
        Vector2 offset = -Vector2.right / cards.Count / 2;
        for (int i = 0; i < cards.Count; i++)
        {
            Vector2 pos = offset + Vector2.right * i / cards.Count;
            pos.y = (float)i / cards.Count * 0.01f;
            cards[i].transform.DOComplete();
            cards[i].transform.DOLocalMove(new Vector3(pos.x, 0, -pos.y), 0.1f);
        }
    }
    private void distributeSpecials(List<CardView> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = specialPivot.right * i;
        }
    }
    private void distributePersistent(List<CardView> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.localPosition = persistentPivot.right * i;
        }
    }
}
