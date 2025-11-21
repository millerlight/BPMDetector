#!/usr/bin/env python3
"""
Test script to verify the BPM detection fix for silence at the beginning
"""

import sys
import os
from bpm_detection.bpm_detection import read_audio, bpm_detector

def test_bpm_files():
    """Test BPM detection on emily and emily2 files"""
    
    # Check if test files exist
    test_files = [
        "bpm_detection/emily.mp3",
        "bpm_detection/emily2.mp3"
    ]
    
    for filename in test_files:
        if not os.path.exists(filename):
            print(f"Test file not found: {filename}")
            continue
            
        print(f"\nTesting: {filename}")
        
        # Read audio file
        samps, fs = read_audio(filename)
        if samps is None or fs is None:
            print(f"  Error reading file: {filename}")
            continue
            
        print(f"  Sample rate: {fs} Hz")
        print(f"  Total samples: {len(samps)}")
        
        # Detect BPM
        bpm, _ = bpm_detector(samps, fs, verbose=True)
        
        if bpm is not None:
            # Handle both scalar and array cases
            if hasattr(bpm, '__iter__'):
                bpm_value = float(bpm[0]) if len(bpm) > 0 else None
            else:
                bpm_value = float(bpm)
            
            if bpm_value is not None:
                print(f"  Detected BPM: {bpm_value:.1f}")
            else:
                print("  Could not detect BPM")
        else:
            print("  Could not detect BPM")

if __name__ == "__main__":
    test_bpm_files()