using UnityEngine;

[System.Serializable]
public class VisibilityLayer
{
    [SerializeField] private int radius = 1;
    [SerializeField, Range(0f, 1f)] private float alpha = 1f;

    public int Radius 
    { 
        get => radius;
        set => radius = value;
    }
    
    public float Alpha 
    { 
        get => alpha;
        set => alpha = value;
    }
} 