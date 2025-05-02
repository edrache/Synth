using UnityEngine;

[ExecuteAlways]
public class StretchToCollider : MonoBehaviour
{
    [Tooltip("Obiekt, który ma być rozciągnięty (jeśli null, użyje tego samego obiektu)")]
    public GameObject targetObject;
    [Tooltip("BoxCollider, do którego ma być dopasowany rozmiar")]
    public BoxCollider referenceCollider;
    [Tooltip("Czy dopasować skalę tylko na osi X?")]
    public bool stretchX = true;
    [Tooltip("Czy dopasować skalę tylko na osi Z?")]
    public bool stretchZ = true;
    [Tooltip("Czy dopasować skalę na starcie?")]
    public bool stretchOnStart = true;
    [Tooltip("Czy dopasowywać skalę w czasie rzeczywistym (w Update)?")]
    public bool liveUpdate = false;

    private void Start()
    {
        if (stretchOnStart)
            Stretch();
    }

    private void Update()
    {
        if (liveUpdate)
            Stretch();
    }

    [ContextMenu("Stretch Now")]
    public void Stretch()
    {
        if (referenceCollider == null)
        {
            Debug.LogError("Brak przypisanego BoxCollider!");
            return;
        }
        GameObject obj = targetObject != null ? targetObject : gameObject;
        Vector3 scale = obj.transform.localScale;
        Vector3 refSize = Vector3.Scale(referenceCollider.size, referenceCollider.transform.lossyScale);
        if (stretchX)
            scale.x = refSize.x;
        if (stretchZ)
            scale.z = refSize.z;
        obj.transform.localScale = scale;
    }
} 