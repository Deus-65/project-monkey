using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class textInteractable : Interactable
{
    public override void OnFocus()
    {
        print("LOOKING AT " + gameObject.name);
    }

    public override void OnInteract()
    {
        print("INTERACTED WITH " + gameObject.name);
    }

    public override void OnLoseFocus()
    {
        print("STOPPED LOOOKÝNG AT " + gameObject.name);
    }
}
