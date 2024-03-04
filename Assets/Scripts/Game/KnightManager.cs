using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightManager : MonoBehaviour
{
    public static KnightManager instance { get; private set; }


    private void Awake()
    {
        instance = this;
    }
    private void OnDestroy()
    {
        instance = null;
    }

    public void BeginUpgrading(KnightRecipe KR)
    {

    }
    public void BeginMobilization(KnightMobilization KM)
    {

    }








}
