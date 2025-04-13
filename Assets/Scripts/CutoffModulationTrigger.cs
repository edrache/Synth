using UnityEngine;

public class CutoffModulationTrigger : MonoBehaviour
{
    public CutoffModulator modulator;
    public VCO vco;

    void Start()
    {
        // Automatycznie znajdź komponenty jeśli nie są przypisane
        if (modulator == null) modulator = GetComponent<CutoffModulator>();
        if (vco == null) vco = GetComponent<VCO>();
    }

    void Update()
    {
        // Przykład: uruchom modulację po naciśnięciu spacji
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (modulator != null && vco != null)
            {
                modulator.StartModulation(vco.bpm);
            }
        }
    }
} 