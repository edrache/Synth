# Unity Audio Synthesizer

A Unity-based audio synthesizer project featuring subtractive synthesis and a step sequencer.

## Features

- Multiple waveform types (Sine, Square, Triangle, Sawtooth, Soft Square, Soft Triangle, Soft Sawtooth)
- Volume control
- Step sequencer with 16 steps
- Accent control for each step
- Note-based object movement
- Force-based object movement
- Grid-based object placement
- Desynchronization options for movement timing
- Configurable force direction control

## Requirements

- Unity 2022.3 or later
- Basic understanding of Unity's interface

## Installation

1. Clone this repository
2. Open the project in Unity
3. Open the main scene
4. Press Play to start the synthesizer

## Usage

### VCO Component
- Controls the main oscillator
- Parameters:
  - Waveform type
  - Volume
  - BPM (Beats Per Minute)
  - Accent strength (1.0 to 2.0)

### NoteMovement Component
- Moves objects based on note pitch
- Parameters:
  - Min/Max distance
  - Move duration
  - Desynchronization range (0-1000ms)
  - Active steps selection
  - Option to always start from initial position

### NoteForceMovement Component
- Applies forces to objects based on note pitch
- Parameters:
  - Min/Max force
  - Force duration
  - Force application point (center or random)
  - Direction control (random or fixed)
  - Active steps selection

### GridPlacer Component
- Places objects on a grid
- Parameters:
  - Placement plane (XY, XZ, YZ)
  - Objects per row
  - Min/Max spacing
  - List of objects to place

## Components

### VCO
The main oscillator component that generates audio waveforms and controls the step sequencer.

### NoteMovement
Moves objects based on the current note's pitch. Objects can be moved with configurable timing and desynchronization.

### NoteForceMovement
Applies physical forces to objects based on the current note's pitch. Supports both random and fixed force directions.

### GridPlacer
A utility component for placing objects in a grid pattern with configurable spacing and layout.

## License

MIT License 