using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[ExecuteInEditMode]
public class GridPlacer : MonoBehaviour
{
    public enum Plane { XY, XZ, YZ }
    public Plane placementPlane = Plane.XZ;
    
    [Range(1, 100)]
    public int objectsPerRow = 5;
    
    [Range(0.1f, 10f)]
    public float minSpacing = 1f;
    
    [Range(0.1f, 10f)]
    public float maxSpacing = 2f;
    
    public List<GameObject> objectsToPlace = new List<GameObject>();
    
    public void PlaceObjects()
    {
        if (objectsToPlace.Count == 0)
        {
            Debug.LogWarning("Brak obiektów do umieszczenia!");
            return;
        }
        
        // Oblicz liczbę kolumn
        int columns = Mathf.CeilToInt((float)objectsToPlace.Count / objectsPerRow);
        
        // Rozmieść obiekty
        for (int i = 0; i < objectsToPlace.Count; i++)
        {
            if (objectsToPlace[i] == null)
            {
                Debug.LogWarning($"Obiekt o indeksie {i} jest null!");
                continue;
            }
            
            // Oblicz pozycję w siatce
            int row = i % objectsPerRow;
            int col = i / objectsPerRow;
            
            // Losuj odstęp
            float spacing = Random.Range(minSpacing, maxSpacing);
            
            // Oblicz pozycję w zależności od wybranej płaszczyzny
            Vector3 position = Vector3.zero;
            switch (placementPlane)
            {
                case Plane.XY:
                    position = new Vector3(row * spacing, col * spacing, 0);
                    break;
                case Plane.XZ:
                    position = new Vector3(row * spacing, 0, col * spacing);
                    break;
                case Plane.YZ:
                    position = new Vector3(0, row * spacing, col * spacing);
                    break;
            }
            
            // Przesuń obiekt na nową pozycję
            objectsToPlace[i].transform.position = transform.position + position;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridPlacer))]
public class GridPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GridPlacer gridPlacer = (GridPlacer)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Umieść obiekty"))
        {
            gridPlacer.PlaceObjects();
        }
    }
}
#endif 