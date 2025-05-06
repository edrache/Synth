using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ObjectRotator : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public enum TweenType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        Elastic
    }

    [Header("Rotation Settings")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private float targetAngle = 90f;
    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private TweenType tweenType = TweenType.EaseInOut;
    [SerializeField] private bool relativeRotation = true;

    private Sequence currentRotation;

    public void RotateObject()
    {
        // Zatrzymaj poprzednią animację jeśli istnieje
        if (currentRotation != null)
        {
            currentRotation.Kill();
        }

        // Oblicz docelowy kąt
        Vector3 targetRotation = transform.eulerAngles;
        float finalAngle = relativeRotation ? targetRotation[(int)rotationAxis] + targetAngle : targetAngle;

        // Ustaw odpowiedni tween na podstawie wybranego typu
        Ease easeType = Ease.Linear;
        switch (tweenType)
        {
            case TweenType.Linear:
                easeType = Ease.Linear;
                break;
            case TweenType.EaseIn:
                easeType = Ease.InQuad;
                break;
            case TweenType.EaseOut:
                easeType = Ease.OutQuad;
                break;
            case TweenType.EaseInOut:
                easeType = Ease.InOutQuad;
                break;
            case TweenType.Bounce:
                easeType = Ease.OutBounce;
                break;
            case TweenType.Elastic:
                easeType = Ease.OutElastic;
                break;
        }

        // Utwórz nową sekwencję
        currentRotation = DOTween.Sequence();
        
        // Dodaj rotację do sekwencji
        currentRotation.Append(transform.DORotate(
            new Vector3(
                rotationAxis == RotationAxis.X ? finalAngle : targetRotation.x,
                rotationAxis == RotationAxis.Y ? finalAngle : targetRotation.y,
                rotationAxis == RotationAxis.Z ? finalAngle : targetRotation.z
            ),
            rotationDuration,
            RotateMode.FastBeyond360
        ).SetEase(easeType));

        // Dodaj callback po zakończeniu animacji
        currentRotation.OnComplete(() => {
            currentRotation = null;
            Debug.Log("[ObjectRotator] Rotation completed");
        });

        // Rozpocznij animację
        currentRotation.Play();
    }

    // Metoda do zatrzymania aktualnej rotacji
    public void StopRotation()
    {
        if (currentRotation != null)
        {
            currentRotation.Kill();
            currentRotation = null;
        }
    }

    // Metoda do resetowania rotacji
    public void ResetRotation()
    {
        StopRotation();
        transform.rotation = Quaternion.identity;
    }
} 