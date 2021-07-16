﻿using UnityEngine;

public class Wagon : MonoBehaviour
{
    private float timeInTrigger;

    private void OnTriggerStay(Collider other)
    {
        var h = other.GetComponentInParent<Horse>();
        if (h.BasePV.IsMine && h.wag == gameObject && !h.Wagon)
        {
            timeInTrigger += Time.fixedDeltaTime;
            if (timeInTrigger >= 2f)
            {
                h.Owner.TryConnectWagon();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        timeInTrigger = 0f;
    }
}