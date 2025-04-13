using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New VCO Sequence", menuName = "Synth/VCO Sequence")]
public class VCOSequence : ScriptableObject
{
    public string sequenceName = "New Sequence";
    public float bpm = 120f;
    public List<VCO.Step> steps = new List<VCO.Step>();
} 