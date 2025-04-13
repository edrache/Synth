# Unity Audio Synthesizer

A Unity-based audio synthesizer project featuring subtractive synthesis and a step sequencer.

## Features

### Synthesis
- Multiple waveforms:
  - Basic: Sine, Square, Saw, Triangle, Noise
  - Soft variants: Sine2, SoftSquare, SoftSaw, Sine3, SoftPulse
- Moog-style filter with cutoff and resonance
- ADSR envelope with slide control
- Pitch slide between steps
- Volume and gain control
- Global octave shift (-2 to +2 octaves)

### Effects
- Chorus effect with parameters:
  - Amount (0-1): Effect intensity
  - Rate (0.1-20Hz): Modulation speed
  - Depth (0-0.02s): Modulation depth
  - Feedback (0-1): Signal feedback
  - Width (0-1): Stereo spread
- BPM-synchronized delay:
  - Multiple timing options: Quarter note, Eighth note, Dotted eighth, Sixteenth note, Triplet
  - Amount (0-1): Effect mix
  - Feedback (0-1): Echo repetitions
  - Width (0-1): Stereo spread
  - Automatic sync with sequencer tempo

### Sequencer
- 16-step sequencer with adjustable BPM
- Per-step controls:
  - Note selection (C through B with sharps)
  - Octave selection
  - Duration (16th note, 8th note, dotted 8th, quarter note, or mute)
  - Accent toggle
  - Slide toggle
- Visual step indicator showing:
  - Current playing step
  - Note name with '#' for sharps
  - Octave number
  - Duration symbol (♬, ♪, ♪., ♩, ×)
  - Accent and slide indicators

### Object Movement
- Note-triggered object movement
- Customizable movement parameters:
  - Distance range
  - Movement duration
  - Desync offset
- Selectable active steps for movement
- Force-based movement variant available

## ADSR Envelope

The synthesizer features a full ADSR (Attack, Decay, Sustain, Release) envelope with the following parameters:

- **Attack Time** (0.001-2s): Controls how quickly the sound reaches its peak
- **Decay Time** (0.001-2s): Controls how quickly the sound falls to the sustain level
- **Sustain Level** (0-1): Controls the volume level while the note is held
- **Release Time** (0.001-2s): Controls how quickly the sound fades out after release
- **Slide Time** (0.001-1s): Controls the time it takes to slide between notes
- **Accent Strength** (1-2): Controls the volume boost for accented notes

## Requirements
- Unity 2022.3 or newer
- Basic audio setup in Unity

## Installation
1. Clone the repository
2. Open the project in Unity
3. Open the demo scene
4. Play to start the synthesizer

## Components

### VCO (Voltage Controlled Oscillator)
Main synthesizer component handling:
- Sound generation
- Filter processing
- Sequencer logic
- Effect processing

### NoteMovement
Handles object movement based on sequencer steps:
- Position-based movement
- Configurable movement parameters
- Step selection system

### NoteForceMovement
Alternative movement system using physics:
- Force-based movement
- Directional control (random or fixed)
- Center of mass options

### SequenceUI
Manages the sequencer visualization:
- Step state display
- Current step indication
- Musical notation display

## License
This project is licensed under the MIT License - see the LICENSE file for details. 