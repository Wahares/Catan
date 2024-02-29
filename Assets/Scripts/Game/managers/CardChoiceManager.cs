using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardChoiceManager : MonoBehaviour
{
    public static CardChoiceManager instance;
    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private Transform cardsPivot;
    [SerializeField]
    private TMPro.TextMeshProUGUI titleText;
    [SerializeField]
    private UnityEngine.UI.Button acceptButton, cancelButton;
    [SerializeField]
    private CanvasGroup canvas;
    private void Awake()
    {
        instance = this;
        acceptButton.onClick.AddListener(() => { acceptCurrent(); });
        cancelButton.onClick.AddListener(() => { cancelCurrent(); });
        resetView();
        TurnManager.OnMyTurnEnded += ForceComplete;
    }
    private void OnDestroy()
    {
        instance = null;
        TurnManager.OnMyTurnEnded -= ForceComplete;
    }
    public SingleChoice currentChoice { get; private set; }
    private List<SingleCardChoiceController> generatedCards = new();
    private List<SingleCardChoiceController> currentSelected = new();

    public void CreateChoice(string title, List<CardSO> cards, int numberOfCards, Action<List<CardSO>> OnAccepted, Action OnCanceled, Action ForceChoice)
    {
        if (currentChoice != null)
            cancelCurrent();


        currentChoice = new SingleChoice(title, cards, numberOfCards, OnAccepted, OnCanceled, ForceChoice);

        titleText.text = title;

        canvas.alpha = 1;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;

        float offset = cards.Count / 2 * 5;
        for (int i = 0; i < cards.Count; i++)
        {
            SingleCardChoiceController sccc = Instantiate(cardPrefab, cardsPivot).GetComponent<SingleCardChoiceController>();
            sccc.Initialize(cards[i].ID);
            generatedCards.Add(sccc);
            float angle = i * 5 - offset;
            sccc.transform.position = cardsPivot.position;
            sccc.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.up, angle);
            sccc.transform.localScale = Vector3.one*10;
            sccc.transform.localPosition = new Vector3(sccc.transform.localPosition.x, 0, sccc.transform.localPosition.y);
            //sccc.transform.forward = sccc.transform.position - Camera.main.transform.position;
        }

    }
    
    private void acceptCurrent()
    {
        currentChoice.OnAccepted?.Invoke(currentSelected.Select(e=>e.item).ToList());
        resetView();
    }
    private void cancelCurrent()
    {
        currentChoice.OnCanceled?.Invoke();
        resetView();
    }
    private void ForceComplete()
    {
        if (currentChoice == null)
            return;
        currentChoice.ForceChoice?.Invoke();
        resetView();
    }
    private void resetView()
    {
        currentChoice = null;
        titleText.text = "";
        acceptButton.interactable = false;
        currentSelected.Clear();
        for (int i = generatedCards.Count - 1; i >= 0; i--)
            Destroy(generatedCards[i].gameObject);
        generatedCards.Clear();
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }
    public void CardClicked(SingleCardChoiceController sccc)
    {
        if (currentSelected.Contains(sccc))
        {
            sccc.transform.DOComplete();
            sccc.transform.DOLocalMoveY(0, 0.25f);
            currentSelected.Remove(sccc);
        }
        else
        {
            if (currentSelected.Count == currentChoice.numberOfCards)
                return;
            sccc.transform.DOComplete();
            sccc.transform.DOLocalMoveY(0.5f, 0.25f);
            currentSelected.Add(sccc);
        }
        acceptButton.interactable = currentSelected.Count == currentChoice.numberOfCards;
    }












    public class SingleChoice
    {
        public string title;
        public List<CardSO> cards;
        public int numberOfCards;
        public Action<List<CardSO>> OnAccepted;
        public Action OnCanceled;
        public Action ForceChoice;
        public SingleChoice(string title, List<CardSO> cards, int numberOfCards, Action<List<CardSO>> OnAccepted, Action OnCanceled, Action ForceChoice)
        {
            this.title = title;
            this.cards = cards;
            this.numberOfCards = numberOfCards;
            this.OnAccepted = OnAccepted;
            this.OnCanceled = OnCanceled;
            this.ForceChoice = ForceChoice;
        }
    }

}