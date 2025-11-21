BPM Detector in Python
======================
Implementation of a Beats Per Minute (BPM) detection algorithm, as presented in the paper of G. Tzanetakis, G. Essl and P. Cook titled: "Audio Analysis using the Discrete Wavelet Transform".

You can find it here: http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.63.5712

Based on the work done in the MATLAB code located at github.com/panagiop/the-BPM-detector-python.

Process .wav or .mp3 file to determine the Beats Per Minute.

## Usage
```bash
python bpm_detection/bpm_detection.py --filename audiofile.mp3 --window 3
python bpm_detection/bpm_detection.py --filename audiofile.wav --window 3
```

## Requirements
Tested with Python 3.12+. Key Dependencies: scipy, numpy, pywavelets, matplotlib, pydub. See requirements.txt
