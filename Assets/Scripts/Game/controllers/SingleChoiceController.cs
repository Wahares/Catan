using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleChoiceController : CardView
{
    public void OnMouseDown()
    {
        CardChoiceManager.instance.CardClicked(this);
    }
}
