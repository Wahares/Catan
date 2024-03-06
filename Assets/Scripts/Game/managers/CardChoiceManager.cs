using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardChoiceManager : MonoBehaviour
{
    public static CardChoiceManager instance;
    [SerializeField]
    private GameObject cardPrefab, avatarPrefab;
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
    public SingleChoiceBase currentChoice { get; private set; }

    private List<SingleChoiceController> generatedCards = new();
    private List<SingleChoiceController> currentSelected = new();

    public void CreateChoice(string title, List<CardSO> cards, int numberOfCards, Action<List<CardSO>> OnAccepted, Action OnCanceled, Action ForceChoice, bool canCancel)
    {
        CreateChoice<CardSO>(title, cards, numberOfCards, OnAccepted, OnCanceled, ForceChoice, canCancel);
    }
    public void CreatePlayerChoice(string title, List<int> players, int numberOfCards, Action<List<int>> OnAccepted, Action OnCanceled, Action ForceChoice, bool canCancel)
    {
        CreateChoice<int>(title, players, numberOfCards, OnAccepted, OnCanceled, ForceChoice, canCancel);
    }

    private void CreateChoice<T>(string title, List<T> cards, int numberOfCards, Action<List<T>> OnAccepted, Action OnCanceled, Action ForceChoice, bool canCancel)
    {
        if (currentChoice != null)
            cancelCurrent();

        if (typeof(T) == typeof(CardSO))
            currentChoice = new SingleChoice<CardSO>(title, cards as List<CardSO>
                , numberOfCards, OnAccepted as Action<List<CardSO>>, OnCanceled, ForceChoice);
        else if (typeof(T) == typeof(int))
            currentChoice = new SingleChoice<int>(title, cards as List<int>
                , numberOfCards, OnAccepted as Action<List<int>>, OnCanceled, ForceChoice);
        else { Debug.LogError("Wrong type of choice"); return; }



        titleText.text = title;

        cancelButton.gameObject.SetActive(canCancel);

        canvas.alpha = 1;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;

        if (numberOfCards == 0)
            acceptButton.interactable = true;

        float offset = cards.Count / 2 * 5;
        for (int i = 0; i < cards.Count; i++)
        {

            SingleChoiceController scc;

            if (typeof(T) == typeof(CardSO))
            {
                scc = Instantiate(cardPrefab, cardsPivot).GetComponent<SingleCardChoiceController>();
                scc.Initialize((cards[i] as CardSO).ID);
            }
            else if (typeof(T) == typeof(int))
            {
                scc = Instantiate(avatarPrefab, cardsPivot).GetComponent<SingleCardChoiceController>();
                scc.Initialize((cards as List<int>)[i]);
            }
            else { Debug.LogError("Wrong type of choice"); return; }

            generatedCards.Add(scc);
            float angle = i * 5 - offset;
            scc.transform.position = cardsPivot.position;
            scc.transform.RotateAround(Camera.main.transform.position, Camera.main.transform.up, angle);
            scc.transform.localScale = Vector3.one * 10;
            scc.transform.localPosition = new Vector3(scc.transform.localPosition.x, 0, scc.transform.localPosition.y);
            //sccc.transform.forward = sccc.transform.position - Camera.main.transform.position;
        }

    }

    private void acceptCurrent()
    {
        (currentChoice as SingleChoice<CardSO>)?.OnAccepted?.Invoke(currentSelected.Select(e => e.item).ToList());
        (currentChoice as SingleChoice<int>)?.OnAccepted?.Invoke(currentSelected.Select(e => e.ID).ToList());
        resetView();
    }
    private void cancelCurrent()
    {
        currentChoice?.OnCanceled?.Invoke();
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

        PlayerCardsOptionsController.isBeingUsed = false;
    }
    public void CardClicked(SingleChoiceController sccc)
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










    public abstract class SingleChoiceBase
    {
        public string title;
        public int numberOfCards;
        public Action OnCanceled;
        public Action ForceChoice;
    }

    public class SingleChoice<T> : SingleChoiceBase
    {
        public List<T> options;
        public Action<List<T>> OnAccepted;
        public SingleChoice(string title, List<T> options, int numberOfCards, Action<List<T>> OnAccepted, Action OnCanceled, Action ForceChoice)
        {
            this.title = title;
            this.options = options;
            this.numberOfCards = numberOfCards;
            this.OnAccepted = OnAccepted;
            this.OnCanceled = OnCanceled;
            this.ForceChoice = ForceChoice;
        }
    }

}