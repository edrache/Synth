# Unity Audio Synthesizer

A Unity-based audio synthesizer project featuring subtractive synthesis and a step sequencer.

## Features

### Synthesis
- Multiple waveforms:
  - Basic: Sine, Square, Saw, Triangle, Noise
  - Soft variants: Sine2, SoftSquare, SoftSaw, Sine3, SoftPulse
- Moog-style filter with:
  - Multiple filter types:
    - Low-pass: Cuts high frequencies
    - High-pass: Cuts low frequencies
    - Band-pass: Passes frequencies around cutoff
  - Cutoff and resonance control
  - Q factor for bandpass filter
  - LFO modulation with multiple waveforms
  - Envelope modulation
- ADSR envelope with slide control
- Pitch slide between steps
- Volume and gain control
- Global octave shift (-2 to +2 octaves)

### Filter LFO
The filter can be modulated by a Low Frequency Oscillator with the following parameters:
- **Enabled**: Toggle to enable/disable LFO modulation
- **Rate** (0.1-20Hz): Controls the modulation speed
- **Depth** (0-1): Controls the modulation intensity
- **Waveform**: Selectable waveforms:
  - Sine: Smooth modulation
  - Square: Abrupt changes
  - Triangle: Linear changes
  - Saw: Asymmetric modulation
- Modulation range: ±4 octaves from base cutoff

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

### ParameterModulator
Component for modulating VCO parameters over time:
- Parameter selection (Filter, Chorus, Delay, Envelope)
- Min/max value range
- Customizable modulation curve
- Duration options (1/4 bar to 4 bars)
- BPM synchronization
- Loop and active toggles
- Progress visualization

## License
This project is licensed under the MIT License - see the LICENSE file for details. 

# Synthesizer Project

A Unity-based synthesizer with advanced audio processing capabilities.

## Features

### Core Synthesis
- Multiple waveform types (Sine, Square, Saw, Triangle, Noise, and more)
- Variable frequency and volume control
- ADSR envelope for amplitude modulation
- Slide effect for smooth pitch transitions
- Accent system for dynamic emphasis

### Filter Section
- Moog-style filter with LowPass, HighPass, and BandPass modes
- Cutoff frequency control (20Hz - 5000Hz)
- Resonance control
- Q factor adjustment for bandpass filter
- Filter LFO for dynamic cutoff modulation
- Filter envelope with independent ADSR parameters
- Filter envelope influence control (0-1) for adjusting modulation strength

### Effects

#### Chorus
- Mix control (0-0.5)
- Rate modulation (0-1)
- Depth control (0-0.01)
- Feedback control (0-0.3)
- Stereo width adjustment
- Chorus envelope with independent ADSR parameters
- Chorus envelope influence control (0-1) for adjusting modulation strength

#### Delay
- Mix control (0-1)
- Feedback control (0-1)
- BPM-synchronized timing options:
  - Quarter note
  - Eighth note
  - Dotted eighth note
  - Sixteenth note
  - Triplet
- Stereo width control
- Delay envelope with independent ADSR parameters
- Delay envelope influence control (0-1) for adjusting modulation strength

### Sequencer
- BPM control (40-300)
- Global octave shift (-2 to +2)
- Step-based sequencing with:
  - Pitch control
  - Duration control
  - Slide and accent options
  - Note-based input with octave selection

## Usage

1. Add the VCO component to a GameObject in your Unity scene
2. Configure the basic parameters (frequency, waveform, volume)
3. Set up the filter section with desired cutoff and resonance
4. Enable and configure effects (chorus, delay) as needed
5. Create and load sequences using the VCOSequence component
6. Control playback using the sequencer parameters

## Envelope System

The synthesizer features a comprehensive envelope system with independent control over each effect:

### Main Envelope
- Controls the overall amplitude of the sound
- ADSR parameters for attack, decay, sustain, and release
- Accent system for dynamic emphasis

### Filter Envelope
- Independent ADSR parameters (attack, decay, sustain, release)
- Controls cutoff frequency modulation
- Influence parameter (0-1) to adjust how much the envelope affects the filter
- When set to 0, the envelope has no effect on the filter
- When set to 1, the envelope has full effect on the filter

### Chorus Envelope
- Independent ADSR parameters (attack, decay, sustain, release)
- Controls chorus depth modulation
- Influence parameter (0-1) to adjust how much the envelope affects the chorus
- When set to 0, the envelope has no effect on the chorus
- When set to 1, the envelope has full effect on the chorus

### Delay Envelope
- Independent ADSR parameters (attack, decay, sustain, release)
- Controls delay mix modulation
- Influence parameter (0-1) to adjust how much the envelope affects the delay
- When set to 0, the envelope has no effect on the delay
- When set to 1, the envelope has full effect on the delay

Each envelope can be precisely controlled to create complex, evolving sounds. The influence parameters allow for fine-tuning how much each envelope affects its respective parameter, providing greater flexibility in sound design.

## Technical Details

- Sample rate: 48kHz
- 32-bit floating point audio processing
- Real-time parameter modulation
- BPM-synchronized effects
- Stereo output with width control 