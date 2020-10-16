using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    List<Slot> isCloseTo = new List<Slot>();
    string occupyingPlayer;
    bool isPlaceable = true;
    
    
    public void Unplaceable()
    {
        isPlaceable = false;
    }
    
    public bool GetIsPlaceable()
    {
        return isPlaceable;
    }

}
