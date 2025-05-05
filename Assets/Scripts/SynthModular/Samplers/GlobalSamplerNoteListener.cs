using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SynthModular.Samplers
{
    // Event wywoływany przy zagraniu nuty przez dowolny sampler
    [System.Serializable]
    public class NoteEvent : UnityEvent<Sampler, int> { }

    public class GlobalSamplerNoteListener : MonoBehaviour
    {
        [Tooltip("Lista nut (MIDI), dla których mają być wywoływane eventy")]
        public List<int> notesToListen = new List<int>();

        [Tooltip("Event wywoływany, gdy zagrana zostanie nuta z listy przez dowolny Sampler")]
        public NoteEvent onNotePlayed;

        private void OnEnable()
        {
            Sampler.OnAnyNotePlayed += HandleNotePlayed;
        }

        private void OnDisable()
        {
            Sampler.OnAnyNotePlayed -= HandleNotePlayed;
        }

        private void HandleNotePlayed(Sampler sampler, int midiNote)
        {
            if (notesToListen.Contains(midiNote))
            {
                onNotePlayed?.Invoke(sampler, midiNote);
            }
        }
    }
} 