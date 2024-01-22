using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDefiner : MonoBehaviour
{
    public static ObjectDefiner instance { get; private set; }

    [field: SerializeField]
    public List<CardSO> availableCards { get; private set; }

    [field: SerializeField]
    public List<CardSO> availableBuildingRecipes { get; private set; }


    private void Awake()
    {
        instance = this;
    }




}
