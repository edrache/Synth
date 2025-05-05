using UnityEngine;
using UnityEngine.Audio;

namespace SynthModular.Effects
{
    public class MixerParameterController : MonoBehaviour
    {
        [Header("Mixer Settings")]
        [Tooltip("AudioMixer do kontrolowania")]
        public AudioMixer mixer;
        [Tooltip("Nazwa grupy w Mixerze (opcjonalne)")]
        public string groupName;
        [Tooltip("Nazwa parametru do kontrolowania")]
        public string parameterName;
        [Tooltip("Minimalna wartość parametru")]
        public float minValue = -80f;
        [Tooltip("Maksymalna wartość parametru")]
        public float maxValue = 0f;

        [Header("Kontrola parametru")]
        [Tooltip("Krzywa kontrolująca wartość parametru w czasie. Oś X: czas [s], oś Y: wartość znormalizowana (0-1)")]
        public AnimationCurve controlCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("Czas trwania jednego cyklu krzywej w sekundach")]
        public float cycleDuration = 1f;
        [Tooltip("Czy automatycznie powtarzać cykl")]
        public bool loop = true;
        [Tooltip("Czy uruchomić kontroler automatycznie")]
        public bool playOnStart = true;

        private float elapsedTime = 0f;
        private bool isPlaying = false;
        private AudioMixerGroup targetGroup;

        private void Start()
        {
            if (mixer != null && !string.IsNullOrEmpty(groupName))
            {
                // Znajdź grupę w mixerze
                var groups = mixer.FindMatchingGroups(groupName);
                if (groups.Length > 0)
                {
                    targetGroup = groups[0];
                }
                else
                {
                    Debug.LogWarning($"Group '{groupName}' not found in mixer!");
                }
            }

            if (playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!isPlaying || mixer == null) return;

            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / cycleDuration;

            if (loop)
            {
                normalizedTime = normalizedTime % 1f;
            }
            else if (normalizedTime >= 1f)
            {
                Stop();
                return;
            }

            float curveValue = controlCurve.Evaluate(normalizedTime);
            float parameterValue = Mathf.Lerp(minValue, maxValue, curveValue);
            
            SetParameterValue(parameterValue);
        }

        public void Play()
        {
            isPlaying = true;
            elapsedTime = 0f;
        }

        public void Stop()
        {
            isPlaying = false;
            elapsedTime = 0f;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void Resume()
        {
            isPlaying = true;
        }

        public void SetParameterValue(float value)
        {
            if (mixer == null) return;

            if (targetGroup != null)
            {
                // Ustaw parametr w grupie
                targetGroup.audioMixer.SetFloat(GetFullParameterName(), value);
            }
            else
            {
                // Ustaw parametr w głównym mixerze
                mixer.SetFloat(parameterName, value);
            }
        }

        public void SetNormalizedValue(float normalizedValue)
        {
            float value = Mathf.Lerp(minValue, maxValue, normalizedValue);
            SetParameterValue(value);
        }

        private string GetFullParameterName()
        {
            if (string.IsNullOrEmpty(groupName))
                return parameterName;
            return $"{groupName}_{parameterName}";
        }

        private void OnValidate()
        {
            if (mixer != null && !string.IsNullOrEmpty(parameterName))
            {
                string fullParamName = GetFullParameterName();
                float value;
                if (!mixer.GetFloat(fullParamName, out value))
                {
                    Debug.LogWarning($"Parameter '{fullParamName}' not found in mixer!");
                }
            }
        }
    }
} 