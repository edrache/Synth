using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class PresetManager : MonoBehaviour
{
    public ModularVoice modularVoice;
    public string presetFilePath = "preset.json";
    public ModularSynth modularSynth;
    private string presetDirectory = "Assets/Presets";
    public string presetFileName = "preset.json"; // Enter the file name here
    public TextAsset presetFile; // Drag and drop the preset file here

    [ContextMenu("Save Preset")]
    public void SavePreset()
    {
        SavePresetAs(presetFileName);
    }

    public void SavePresetAs(string fileName)
    {
        if (!Directory.Exists(presetDirectory))
        {
            Directory.CreateDirectory(presetDirectory);
        }

        string fullPath = Path.Combine(presetDirectory, fileName);
        SynthPreset preset = modularSynth.SavePreset();
        Debug.Log("Serializing preset with Newtonsoft.Json:");
        string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
        Debug.Log("Serialized JSON:");
        Debug.Log(json);
        File.WriteAllText(fullPath, json);
        Debug.Log("Preset saved to " + fullPath);
    }

    [ContextMenu("Load Preset")]
    public void LoadPreset()
    {
        if (presetFileName != null && presetFileName != "")
        {
            string fullPath = Path.Combine(presetDirectory, presetFileName);
            if (File.Exists(fullPath))
            {
                string json = File.ReadAllText(fullPath);
                SynthPreset preset = JsonConvert.DeserializeObject<SynthPreset>(json);
                if (modularSynth != null)
                {
                    modularSynth.LoadPreset(preset);
                    Debug.Log("Preset loaded from " + fullPath);
                }
                else
                {
                    Debug.LogWarning("ModularSynth is not assigned.");
                }
            }
            else
            {
                Debug.LogWarning("Preset file not found: " + fullPath);
            }
        }
        else
        {
            Debug.LogWarning("No preset file name provided.");
        }
    }
} 