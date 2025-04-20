# Dungeon Synth Interactive Sound System

## Założenia Projektu

### Cel
Stworzenie niskopoziomowego, reaktywnego systemu muzyki ambientowej w stylistyce Dungeon Synth, zintegrowanego z Unity przy użyciu FMOD Studio. System ma generować unikalną ścieżkę dźwiękową w zależności od kontekstu gry, z minimalnym zużyciem zasobów i pełną kompatybilnością z platformami PC i konsolowymi.

### Główne Funkcje
- Reaktywny system audio oparty na parametrach gry
- Minimalistyczny zestaw sampli o jednolitym charakterze
- Dynamiczna modulacja parametrów dźwięku
- Efektywne zarządzanie zasobami
- Pełna integracja z systemem gry

## Struktura Audio w FMOD Studio

### Banks
1. Master Bank
   - Podstawowe sample
   - Wspólne efekty
   - Główne miksy
   - Routing grup
   
2. Instruments Bank
   - Pad / Dron (1-2 sample)
   - Lead synth / melodyczny (1 sample)
   - Organ / Choir / Strings (1-2 sample)
   - Pluck / Bell FX (2-3 sample)
   - Ambient tła (1 sample)
   
3. Effects Bank
   - Reverb Presets
   - Delay Patterns
   - Modulation Effects

### Events
1. Instrument Events
   ```
   /Instruments/
   ├── Drones
   │   ├── DarkPad
   │   └── AmbientDrone
   ├── Leads
   │   ├── MainSynth
   │   └── MelodicVoice
   ├── Textures
   │   ├── Organ
   │   ├── Choir
   │   └── Strings
   └── FX
       ├── Pluck
       ├── Bell
       └── Crystal
   ```

2. Ambient Events
   ```
   /Ambient/
   ├── Background
   │   ├── Cave
   │   ├── Wind
   │   └── Mystical
   └── Transitions
       ├── LocationChange
       ├── TimeOfDay
       └── StateChange
   ```

### Parametry FMOD
1. Globalne Parametry
   - Location (0-1)
   - Tension (0-1)
   - Lightness (0-1)
   - TimeOfDay (0-1)
   - PlayerState (0-1)

2. Per-Instrument Parametry
   - Pitch
   - Filter Cutoff
   - Volume
   - Effect Sends

3. Modulatory
   - LFO (Rate, Depth)
   - Envelope (AHDSR)
   - Random Modulation

## System Kontroli w Unity

### Komponenty
1. FMODGameStateController
   ```csharp
   public class FMODGameStateController : MonoBehaviour
   {
       // Parametry gry
       [SerializeField] private float location;
       [SerializeField] private float tension;
       [SerializeField] private float lightness;
       [SerializeField] private float timeOfDay;
       [SerializeField] private float playerState;

       // Event References
       [SerializeField] private string[] instrumentPaths;
       [SerializeField] private string[] ambientPaths;
   }
   ```

2. FMODParameterModulator
   ```csharp
   public class FMODParameterModulator : MonoBehaviour
   {
       [System.Serializable]
       public class ModulationSettings
       {
           public string parameterName;
           public AnimationCurve modulationCurve;
           public float rate;
           public float depth;
       }

       [SerializeField] private ModulationSettings[] modulations;
   }
   ```

3. FMODBusManager
   ```csharp
   public class FMODBusManager : MonoBehaviour
   {
       [SerializeField] private string[] busPaths = new string[]
       {
           "bus:/Drones",
           "bus:/Melodies",
           "bus:/FX",
           "bus:/Ambients"
       };
   }
   ```

### Sample i Presety

#### Wymagane Sample
1. Drones (1-2 sample)
   - Dark ambient pad
   - Atmospheric drone
   - Format: 44.1kHz/16bit WAV
   - Długość: 4-8s loopable

2. Leads (1 sample)
   - Vintage synth lead
   - Format: 44.1kHz/16bit WAV
   - Długość: 2-4s

3. Textures (1-2 sample)
   - Organ/Choir/Strings
   - Format: 44.1kHz/16bit WAV
   - Długość: 4-8s loopable

4. FX (2-3 sample)
   - Pluck/Bell sounds
   - Format: 44.1kHz/16bit WAV
   - Długość: 1-2s

5. Ambient (1 sample)
   - Background texture
   - Format: 44.1kHz/16bit WAV
   - Długość: 8-16s loopable

#### Efekty
1. Reverb Spaces
   - Small Dungeon (RT: ~1.5s)
   - Large Hall (RT: ~2.5s)
   - Crystal Cave (RT: ~3.5s)

2. Delay Types
   - Sync delay (BPM)
   - Tape echo emulation
   - Granular delay

3. Modulation
   - Classic chorus
   - Vintage phaser
   - Crystal flanger

## Interfejs Użytkownika

### Główne Elementy
1. State Controls
   - Location slider
   - Tension meter
   - Lightness control
   - Time of day selector
   - Player state indicators

2. Parameter Controls
   - Instrument mix
   - Effect sends
   - Modulation depth
   - Bus routing

3. Visualization
   - Parameter meters
   - State indicators
   - Effect feedback

## Wymagania Techniczne

### FMOD Studio
- Wersja: Najnowsza stabilna
- Licencja: Darmowa (do $200k)
- Minimum 16 kanałów audio
- Obsługa modulacji LFO i Envelope

### Unity
- Wersja: 2022.3 lub nowsza
- FMOD Unity Integration Package
- Minimum 2GB RAM
- DirectX 11 lub nowszy

### Zasoby
- Total sample size: ~100MB
- Format: 44.1kHz/16bit WAV
- Loop points w metadata
- Crossfade dla loopów

## Plan Rozwoju

1. Setup (2-3 dni)
   - Instalacja FMOD Studio
   - Konfiguracja Unity
   - Przygotowanie struktury
   - Import sampli

2. Podstawy (4-5 dni)
   - Implementacja kontrolerów
   - Podstawowe eventy
   - Testy integracji

3. Rozwój (10-14 dni)
   - System modulacji
   - Efekty i routing
   - Integracja z grą

4. Finalizacja (5-7 dni)
   - Optymalizacja
   - Testy wydajności
   - Dokumentacja

## Dobre Praktyki

1. Audio Design
   - Jednolity poziom głośności
   - Płynne loopowanie
   - Unikanie transientów
   - Spójne brzmienie

2. Implementacja
   - Efektywne routing
   - Optymalne użycie modulacji
   - Minimalne zużycie CPU
   - Czytelna struktura

3. Optymalizacja
   - Voice limiting
   - Resource pooling
   - Memory management
   - Performance monitoring

## Następne Kroki

1. Przygotowanie Projektu
   - Setup FMOD
   - Import sampli
   - Podstawowa struktura

2. Pierwsze Testy
   - Podstawowe eventy
   - Kontrolery stanu
   - Testy wydajności

3. Iteracyjny Rozwój
   - System modulacji
   - Efekty i routing
   - Integracja z grą 