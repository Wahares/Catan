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


    public List<CardView> selectedCardsViews = new();

    private PlayerCardsOptionsController pcoc;


    public void initialize(bool isMine)
    {
        this.isMine = isMine;
        if (isMine)
            pcoc = GetComponent<PlayerCardsOptionsController>();
    }
    private void Awake()
    {
        TurnManager.OnMyTurnEnded += resetSelected;
    }
    private void OnDestroy()
    {
        TurnManager.OnMyTurnEnded -= resetSelected;
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
            Material newTexture = blankNormal;
            if (item is SpecialCard)
            {
                switch ((item as SpecialCard).sourceType)
                {
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
        float spacing = Mathf.Clamp(3 / (cards.Count == 0 ? 1 : cards.Count), 0.25f, 0.5f);
        Vector2 offset = -Vector2.right * spacing * cards.Count / 2;
        for (int i = 0; i < cards.Count; i++)
        {
            Vector2 pos = offset + Vector2.right * i * spacing;
            pos.y = (float)i / cards.Count * 0.01f;
            cards[i].transform.DOComplete();
            cards[i].transform.DOLocalMove(new Vector3(pos.x, 0, -pos.y), 0.1f);
        }
    }
    private void distributeSpecials(List<CardView> cards)
    {
        for (int i = 0; i < cards.Count; i++)
            cards[i].transform.localPosition = Vector3.right * i * 0.7f;
    }
    private void distributePersistent(List<CardView> cards)
    {
        float offset = cards.Count / 2 * 0.7f;
        for (int i = 0; i < cards.Count; i++)
            cards[i].transform.localPosition = Vector3.right * (i*0.7f-offset);
    }

    public void OnSelectedCardsChanged() => pcoc?.OnSelectedChanged();

    public void resetSelected()
    {
        foreach (var card in selectedCardsViews)
            (card as HandCardView)?.SimpleDeselect();
        selectedCardsViews.Clear();
        OnSelectedCardsChanged();
    }
}
