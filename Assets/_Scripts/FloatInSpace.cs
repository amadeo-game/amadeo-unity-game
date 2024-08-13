using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatInSpace : MonoBehaviour
{  
    public float amplitude = 0.5f;
    public float speed = 1f;
    public float randomOffset = 1f;
    
    
    private Vector3 startPosition;
    
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
     float xOffset = Mathf.PerlinNoise(Time.time * speed, randomOffset)-0.5f;
     float yOffset = Mathf.PerlinNoise(randomOffset, Time.time * speed)-0.5f;
     
     
     Vector3 newPos = startPosition + new Vector3(xOffset, yOffset, 0) * amplitude;
        transform.position = newPos;
    }
}
