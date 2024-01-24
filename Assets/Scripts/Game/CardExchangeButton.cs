using UnityEngine;
using UnityEngine.UI;

public class CardExchangeButton : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Image icon;


    [SerializeField]
    private Transform materialsPivot;

    [SerializeField]
    private GameObject materialPrefab;
    public void Initialize(RecipedCard config)
    {
        icon.sprite = config.icon;

        int index = 0;
        foreach (var mat in config.materials)
        {
            GameObject go = Instantiate(materialPrefab, materialsPivot);
            go.GetComponent<RectTransform>().anchoredPosition = Vector2.right * index * 0.25f;
            go.GetComponent<CardExchangeMaterial>().Initialize(mat);
            index++;
        }
        button.onClick.AddListener(() => { config.OnUsed(); });
    }
}
