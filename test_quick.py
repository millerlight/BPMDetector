#!/usr/bin/env python3
"""
Quick test for BPM detection performance
"""

import sys
import os
import time
from bpm_detection.bpm_detection import read_audio, bpm_detector

def test_quick():
    """Quick test for BPM detection"""
    
    test_file = "bpm_detection/emily.mp3"
    
    if not os.path.exists(test_file):
        print("Test file not found")
        return
        
    print("Testing performance...")
    start_time = time.time()
    
    # Read audio file
    samps, fs = read_audio(test_file)
    if samps is None or fs is None:
        print("Error reading file")
        return
    
    print(f"Processing {len(samps)} samples at {fs} Hz...")
    
    # Detect BPM
    bpm, _ = bpm_detector(samps, fs, verbose=False)
    
    end_time = time.time()
    duration = end_time - start_time
    
    if bpm is not None:
        print(f"Detected BPM: {bpm:.1f}")
        print(f"Processing time: {duration:.2f} seconds")
    else:
        print("Could not detect BPM")

if __name__ == "__main__":
    test_quick()