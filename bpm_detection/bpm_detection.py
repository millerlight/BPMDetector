# Copyright 2012 Free Software Foundation, Inc.
#
# This file is part of The BPM Detector Python
#
# The BPM Detector Python is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 3, or (at your option)
# any later version.
#
# The BPM Detector Python is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with The BPM Detector Python; see the file COPYING.  If not, write to
# the Free Software Foundation, Inc., 51 Franklin Street,
# Boston, MA 02110-1301, USA.

import argparse
import array
import math
import wave
import os
import warnings

import matplotlib.pyplot as plt
import numpy
import pywt
from scipy import signal

# Suppress all warnings for clean output
warnings.filterwarnings("ignore")

try:
    from pydub import AudioSegment
    PYDUB_AVAILABLE = True
except ImportError:
    PYDUB_AVAILABLE = False


def read_wav(filename):
    # open file, get metadata for audio
    try:
        wf = wave.open(filename, "rb")
    except IOError as e:
        print(e)
        return

    # typ = choose_type( wf.getsampwidth() ) # TODO: implement choose_type
    nsamps = wf.getnframes()
    assert nsamps > 0

    fs = wf.getframerate()
    assert fs > 0

    # Read entire file and make into an array
    samps = list(array.array("i", wf.readframes(nsamps)))

    try:
        assert nsamps == len(samps)
    except AssertionError:
        # Silent fail - no output for clean BPM-only result
        pass

    return samps, fs


def read_mp3(filename):
    """Read MP3 file and convert to audio data"""
    if not PYDUB_AVAILABLE:
        print("Error: pydub is required for MP3 support. Install with: pip install pydub")
        return None, None
    
    try:
        # Load MP3 file
        audio = AudioSegment.from_mp3(filename)
        
        # Convert to numpy array
        # pydub uses 16-bit signed integers, so we need to convert to int32
        samps = numpy.array(audio.get_array_of_samples(), dtype=numpy.int32)
        
        # If stereo, convert to mono by taking the mean of left and right channels
        if audio.channels == 2:
            samps = samps.reshape((-1, 2)).mean(axis=1).astype(numpy.int32)
        
        # Get sample rate
        fs = audio.frame_rate
        
        return samps.tolist(), fs
        
    except Exception as e:
        print(f"Error reading MP3 file {filename}: {e}")
        return None, None


def read_audio(filename):
    """Read audio file (WAV or MP3) based on file extension"""
    ext = os.path.splitext(filename)[1].lower()
    
    if ext == '.wav':
        return read_wav(filename)
    elif ext == '.mp3':
        return read_mp3(filename)
    else:
        print(f"Unsupported file format: {ext}. Supported formats: .wav, .mp3")
        return None, None


# print an error when no data can be found
def no_audio_data():
    print("No audio data for sample, skipping...")
    return None, None


# simple peak detection
def peak_detect(data):
    max_val = numpy.amax(abs(data))
    peak_ndx = numpy.where(data == max_val)
    if len(peak_ndx[0]) == 0:  # if nothing found then the max must be negative
        peak_ndx = numpy.where(data == -max_val)
    return peak_ndx


def bpm_detector(data, fs, verbose=True):
    cA = []
    cD = []
    correl = []
    cD_sum = []
    levels = 4
    max_decimation = 2 ** (levels - 1)
    min_ndx = math.floor(60.0 / 220 * (fs / max_decimation))
    max_ndx = math.floor(60.0 / 40 * (fs / max_decimation))

    for loop in range(0, levels):
        cD = []
        # 1) DWT
        if loop == 0:
            [cA, cD] = pywt.dwt(data, "db4")
            cD_minlen = len(cD) / max_decimation + 1
            cD_sum = numpy.zeros(math.floor(cD_minlen))
        else:
            [cA, cD] = pywt.dwt(cA, "db4")

        # 2) Filter
        cD = signal.lfilter([0.01], [1 - 0.99], cD)

        # 4) Subtract out the mean.

        # 5) Decimate for reconstruction later.
        cD = abs(cD[:: (2 ** (levels - loop - 1))])
        cD = cD - numpy.mean(cD)

        # 6) Recombine the signal before ACF
        #    Essentially, each level the detail coefs (i.e. the HPF values) are concatenated to the beginning of the array
        cD_sum = cD[0 : math.floor(cD_minlen)] + cD_sum

    if [b for b in cA if b != 0.0] == []:
        return no_audio_data()

    # Adding in the approximate data as well...
    cA = signal.lfilter([0.01], [1 - 0.99], cA)
    cA = abs(cA)
    cA = cA - numpy.mean(cA)
    cD_sum = cA[0 : math.floor(cD_minlen)] + cD_sum

    # ACF
    correl = numpy.correlate(cD_sum, cD_sum, "full")

    midpoint = math.floor(len(correl) / 2)
    correl_midpoint_tmp = correl[midpoint:]
    peak_ndx = peak_detect(correl_midpoint_tmp[min_ndx:max_ndx])
    if len(peak_ndx) > 1:
        return no_audio_data()

    peak_ndx_adjusted = peak_ndx[0] + min_ndx
    bpm = 60.0 / peak_ndx_adjusted * (fs / max_decimation)
    
    if verbose:
        print(bpm)
    
    return bpm, correl


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Process .wav or .mp3 file to determine the Beats Per Minute.")
    parser.add_argument("audio_file", help="Audio file for processing (.wav or .mp3)")
    parser.add_argument(
        "--window",
        type=float,
        default=3,
        help="Size of the the window (seconds) that will be scanned to determine the bpm. Typically less than 10 seconds. [3]",
    )
    parser.add_argument(
        "--verbose",
        action="store_true",
        help="Verbose mode - show detailed output and plot"
    )

    args = parser.parse_args()
    samps, fs = read_audio(args.audio_file)
    data = []
    correl = []
    bpm = 0
    n = 0
    nsamps = len(samps)
    window_samps = int(args.window * fs)
    samps_ndx = 0  # First sample in window_ndx
    max_window_ndx = math.floor(nsamps / window_samps)
    bpms = numpy.zeros(max_window_ndx)

    # Iterate through all windows
    for window_ndx in range(0, max_window_ndx):

        # Get a new set of samples
        # print(n,":",len(bpms),":",max_window_ndx_int,":",fs,":",nsamps,":",samps_ndx)
        data = samps[samps_ndx : samps_ndx + window_samps]
        if not ((len(data) % window_samps) == 0):
            raise AssertionError(str(len(data)))

        bpm, correl_temp = bpm_detector(data, fs, verbose=args.verbose)
        if bpm is None:
            continue
        # Convert bpm to scalar to avoid NumPy deprecation warning
        if hasattr(bpm, 'item'):
            bpms[window_ndx] = bpm.item()
        else:
            bpms[window_ndx] = bpm
        correl = correl_temp

        # Iterate at the end of the loop
        samps_ndx = samps_ndx + window_samps

        # Counter for debug...
        n = n + 1

    bpm = numpy.median(bpms)
    
    if args.verbose:
        # Verbose mode with full output
        print("Completed!  Estimated Beats Per Minute:", bpm)

        n = range(0, len(correl))
        plt.plot(n, abs(correl))
        plt.show(block=True)
    else:
        # Silent mode - output only rounded BPM
        print(round(bpm))
