using UnityEngine;
using System;

namespace SynthModular.Effects
{
    [Serializable]
    public class Saturator : MonoBehaviour
    {
        [Header("Saturator Parameters")]
        [Range(1f, 10f)]
        [Tooltip("Ilość zniekształceń (1 = czysty sygnał, 10 = maksymalne nasycenie)")]
        public float drive = 1f;

        [Range(0f, 1f)]
        [Tooltip("Balans między czystym a przetworzonym sygnałem")]
        public float mix = 1f;

        [Range(0f, 1f)]
        [Tooltip("Kontrola barwy zniekształceń (0 = ciemne, 1 = jasne)")]
        public float tone = 0.5f;

        [Range(1f, 4f)]
        [Tooltip("Kształt krzywej nasycenia (1 = łagodny, 4 = agresywny)")]
        public float curve = 1f;

        private float lastDrive = 1f;
        private float lastTone = 0.5f;
        private float lastCurve = 1f;
        private float[] toneFilter = new float[2];

        private void Start()
        {
            UpdateParameters();
        }

        private void UpdateParameters()
        {
            lastDrive = drive;
            lastTone = tone;
            lastCurve = curve;
        }

        public void ProcessBuffer(float[] buffer)
        {
            if (buffer == null || buffer.Length == 0) return;

            // Sprawdź czy parametry się zmieniły
            if (lastDrive != drive || lastTone != tone || lastCurve != curve)
            {
                UpdateParameters();
            }

            // Normalizacja wejścia przed przetwarzaniem
            float maxInput = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                maxInput = Mathf.Max(maxInput, Mathf.Abs(buffer[i]));
            }

            float inputGain = maxInput > 1f ? 1f / maxInput : 1f;

            // Przetwarzanie próbek
            for (int i = 0; i < buffer.Length; i++)
            {
                // Normalizacja wejścia
                float normalizedInput = buffer[i] * inputGain;

                // Zachowaj oryginalny sygnał do mixu
                float dry = normalizedInput;

                // Zastosuj drive z mniejszym wzmocnieniem
                float driven = normalizedInput * (drive * 0.5f);

                // Zastosuj nieliniowe zniekształcenia (krzywa nasycenia)
                float saturated = ProcessSaturation(driven);

                // Zastosuj filtr tonalny
                saturated = ProcessTone(saturated);

                // Kompensacja głośności po saturacji
                float compensationGain = 1f / Mathf.Max(1f, drive * 0.25f);
                saturated *= compensationGain;

                // Mix między czystym a przetworzonym sygnałem
                buffer[i] = Mathf.Lerp(dry, saturated, mix);

                // Końcowa normalizacja
                buffer[i] = Mathf.Clamp(buffer[i], -1f, 1f);
            }
        }

        private float ProcessSaturation(float input)
        {
            // Różne krzywe nasycenia w zależności od parametru curve
            switch (Mathf.FloorToInt(curve))
            {
                case 1: // Łagodne nasycenie (tanh)
                    return (float)Math.Tanh(input);

                case 2: // Średnie nasycenie (arkus tangens)
                    return (2f / Mathf.PI) * Mathf.Atan(input);

                case 3: // Mocne nasycenie (cubic soft clipping)
                    if (input > 1f) return 1f;
                    if (input < -1f) return -1f;
                    return (3f/2f) * input - (input * input * input) / 2f;

                case 4: // Bardzo mocne nasycenie (hard clipping)
                    return Mathf.Clamp(input, -1f, 1f);

                default:
                    return input;
            }
        }

        private float ProcessTone(float input)
        {
            // Prosty filtr pierwszego rzędu
            float alpha = tone;
            toneFilter[0] = (1f - alpha) * input + alpha * toneFilter[1];
            toneFilter[1] = toneFilter[0];
            return toneFilter[0];
        }

        private void OnDisable()
        {
            // Reset parametrów
            toneFilter[0] = 0f;
            toneFilter[1] = 0f;
        }
    }
} 