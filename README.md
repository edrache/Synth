# Synth

A Unity-based audio synthesizer project featuring subtractive synthesis and a step sequencer.

## Features

- Multiple waveforms (Sine, Square, Saw, Triangle, Noise, Sine2, SoftSquare, SoftSaw, Sine3, SoftPulse)
- Step sequencer with note accenting capabilities
- Audio parameter control (volume, gain, accents)
- 3D object interaction (movement and force-based)
- Customizable step activation for object movement
- Soft waveforms for gentle, ear-friendly sounds

## Requirements

- Unity 2022.3 or newer
- .NET 4.x

## Installation

1. Clone the repository
2. Open the project in Unity
3. Open the scene from Assets/Scenes

## Usage

1. Add the VCO component to any GameObject
2. Configure audio parameters in the inspector:
   - Select waveform type
   - Adjust volume and gain
   - Set accent strength (0.001-2.0)
3. Set up sequencer steps
4. For object movement:
   - Add NoteMovement or NoteForceMovement component
   - Configure movement parameters
   - Select active steps for movement
5. Play the scene to hear the sound and see object movement

## Components

### VCO (Voltage Controlled Oscillator)
- Main sound generation component
- Controls waveform type, volume, and accent strength
- Includes step sequencer functionality

### NoteMovement
- Moves objects based on note pitch
- Configurable movement distance and speed
- Selective step activation

### NoteForceMovement
- Applies physics-based force to objects
- Force strength based on note pitch
- Customizable force application points

## License

MIT 