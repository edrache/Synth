# Synth Game Project - FMOD Implementation Plan

## Obecny System (Unity Native Audio)

### Główne Funkcje
- Synteza subtraktywna w czasie rzeczywistym
- System sekwencera (16 kroków)
- Efekty (chorus, delay)
- System modulacji parametrów
- Kontrola ruchu obiektów bazująca na dźwięku

### Szczegóły Techniczne
- Implementacja w C# używając OnAudioFilterRead
- Własny system syntezy
- Synchronizacja z BPM
- System presetów i sekwencji

## Planowana Implementacja FMOD

### Cele Migracji
1. Lepsza wydajność audio
2. Łatwiejsze zarządzanie samplami
3. Zaawansowane efekty przestrzenne
4. Lepsze miksowanie

### Struktura FMOD Studio

#### Banks
1. Master Bank
   - Podstawowe sample
   - Wspólne efekty
   
2. Instruments Bank
   - Dungeon Synth Instruments
   - Ambient Pads
   - Atmospheric Effects
   
3. Effects Bank
   - Reverb Presets
   - Delay Patterns
   - Modulation Effects

#### Events
1. Synthesizer Events
   ```
   /Synth/
   ├── BaseSynth
   ├── LeadSynth
   └── Pads
   ```

2. Atmospheric Events
   ```
   /Atmosphere/
   ├── Background
   ├── OneShots
   └── Transitions
   ```

3. Effect Events
   ```
   /Effects/
   ├── Reverb
   ├── Delays
   └── Modulation
   ```

### Parametry FMOD

#### Globalne
- Master Volume
- Reverb Amount
- Delay Time
- Modulation Rate

#### Per-Instrument
- Note/Pitch
- Velocity
- Filter Cutoff
- Resonance
- LFO Rate
- Effect Sends

### Integracja z Unity

#### Komponenty
1. FMODSynthController
   - Zarządzanie eventami FMOD
   - Kontrola parametrów
   - Synchronizacja z sekwencerem

2. FMODSequencer
   - Obsługa kroków sekwencji
   - Kontrola BPM
   - Triggering sampli

3. FMODParameterModulator
   - Modulacja parametrów FMOD
   - Krzywe modulacji
   - Synchronizacja z BPM

4. FMODAtmosphereManager
   - Zarządzanie ambientem
   - Przejścia między stanami
   - Kontrola warstw dźwiękowych

### Sample i Presety

#### Dungeon Synth Collection
1. Lead Instruments
   - Vintage Synth Leads
   - FM Bell Tones
   - Crystal Sounds

2. Pad Sounds
   - Dark Ambient Pads
   - Atmospheric Textures
   - Drone Bases

3. Percussion
   - Synthetic Drums
   - Ambient Hits
   - Textural Percussion

#### Efekty
1. Reverb Spaces
   - Dungeon Hall
   - Crystal Cave
   - Dark Chamber

2. Delay Types
   - Echo Chamber
   - Tape Delay
   - Granular Delay

3. Modulation
   - Chorus Ensemble
   - Phaser Effects
   - Flanger

### System Kontroli Parametrów

#### Real-time Controls
- Filter Envelope
- Amplitude Modulation
- Effect Parameters
- Mix Controls

#### Automation
- Parameter Curves
- Modulation Paths
- Transition States

### Integracja z Istniejącym Systemem

#### Zachowane Funkcje
- System sekwencera (16 kroków)
- Kontrola ruchu obiektów
- System presetów
- Interface użytkownika

#### Nowe Możliwości
- Zaawansowane efekty przestrzenne
- Lepsze zarządzanie CPU
- Więcej warstw dźwiękowych
- Bardziej złożone przejścia

## Wymagania Techniczne

### FMOD Studio
- Wersja: Najnowsza stabilna
- Licencja: Darmowa (do $200k)

### Unity Integration
- FMOD Unity Integration Package
- Minimum Unity 2022.3

### Asset Management
- System organizacji sampli
- Struktura banków
- System wersjonowania

## Workflow Development

1. Przygotowanie Projektu
   - Instalacja FMOD Studio
   - Konfiguracja Unity Integration
   - Utworzenie podstawowej struktury

2. Implementacja Podstawowa
   - Stworzenie podstawowych eventów
   - Implementacja kontrolerów
   - Integracja z UI

3. Rozwój Funkcjonalności
   - Dodanie zaawansowanych efektów
   - Implementacja systemu modulacji
   - Integracja z systemem ruchu

4. Optymalizacja
   - Profiling audio
   - Optymalizacja CPU
   - Zarządzanie pamięcią

## Następne Kroki

1. Konfiguracja Środowiska
   - Instalacja FMOD Studio
   - Konfiguracja projektu Unity
   - Przygotowanie struktury folderów

2. Podstawowa Implementacja
   - Stworzenie pierwszych eventów
   - Implementacja podstawowego kontrolera
   - Testy integracji

3. Rozwój Systemu
   - Dodawanie sampli
   - Implementacja efektów
   - Rozbudowa funkcjonalności

4. Testy i Optymalizacja
   - Testy wydajności
   - Optymalizacja zasobów
   - Finalne dostrojenie 