using UnityEngine;
using UnityEngine.UI;

public class CardExchangeButton : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private SpriteRenderer[] materials;
    [SerializeField]
    private SpriteRenderer icon;


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
            for (int i = 0; i < mat.number; i++)
            {
                GameObject go = Instantiate(materialPrefab, materialsPivot);
                go.transform.localPosition = Vector3.right * i * 0.2f + Vector3.forward * -0.01f;
                index++;
            }
        }
        button.onClick.AddListener(() => { config.OnUsed(); });
    }
}
