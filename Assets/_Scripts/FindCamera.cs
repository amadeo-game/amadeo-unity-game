using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindCamera : MonoBehaviour
{
 
    void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

 
    
    
}
