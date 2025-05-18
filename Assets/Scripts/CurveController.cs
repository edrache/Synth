using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class CurveController
{
    private const int POINTS_COUNT = 33; // 0 to 16 with 0.5 step
    private const float MIN_X = 0f;
    private const float MAX_X = 16f;
    private const float STEP = 0.5f;

    private float[] currentValues;
    private float[] targetValues;
    private Tweener[] activeTweens;
    private AnimationCurve curve;

    public CurveController(float minY, float maxY)
    {
        currentValues = new float[POINTS_COUNT];
        targetValues = new float[POINTS_COUNT];
        activeTweens = new Tweener[POINTS_COUNT];
        curve = new AnimationCurve();

        // Initialize curve with evenly spaced points
        for (int i = 0; i < POINTS_COUNT; i++)
        {
            float x = MIN_X + (i * STEP);
            curve.AddKey(new Keyframe(x, 0f, 0f, 0f));
        }
    }

    public void SetPointValue(int index, float value, float duration = 0f)
    {
        if (index < 0 || index >= POINTS_COUNT)
        {
            Debug.LogError($"[CurveController] Index {index} out of range!");
            return;
        }

        targetValues[index] = value;

        if (duration <= 0f)
        {
            // Immediate change
            currentValues[index] = value;
            UpdateCurvePoint(index);
        }
        else
        {
            // Animated change
            if (activeTweens[index] != null && activeTweens[index].IsActive())
            {
                activeTweens[index].Kill();
            }

            activeTweens[index] = DOTween.To(
                () => currentValues[index],
                x => {
                    currentValues[index] = x;
                    UpdateCurvePoint(index);
                },
                value,
                duration
            );
        }
    }

    public void SetAllPoints(float[] values, float duration = 0f)
    {
        if (values.Length != POINTS_COUNT)
        {
            Debug.LogError($"[CurveController] Invalid values array length! Expected {POINTS_COUNT}, got {values.Length}");
            return;
        }

        for (int i = 0; i < POINTS_COUNT; i++)
        {
            SetPointValue(i, values[i], duration);
        }
    }

    public void InterpolateToCurve(AnimationCurve targetCurve, float duration)
    {
        for (int i = 0; i < POINTS_COUNT; i++)
        {
            float x = MIN_X + (i * STEP);
            float targetValue = targetCurve.Evaluate(x);
            SetPointValue(i, targetValue, duration);
        }
    }

    public float GetPointValue(int index)
    {
        if (index < 0 || index >= POINTS_COUNT)
        {
            Debug.LogError($"[CurveController] Index {index} out of range!");
            return 0f;
        }

        return currentValues[index];
    }

    public float[] GetAllPointValues()
    {
        return (float[])currentValues.Clone();
    }

    public AnimationCurve GetCurve()
    {
        return curve;
    }

    private void UpdateCurvePoint(int index)
    {
        float x = MIN_X + (index * STEP);
        Keyframe key = curve.keys[index];
        key.value = currentValues[index];
        curve.MoveKey(index, key);
    }

    public void KillAllTweens()
    {
        for (int i = 0; i < POINTS_COUNT; i++)
        {
            if (activeTweens[i] != null && activeTweens[i].IsActive())
            {
                activeTweens[i].Kill();
            }
        }
    }
} 