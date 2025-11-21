using System;
using System.Linq;

namespace BPMDetector
{
    /// <summary>
    /// BPM detection using energy-based onset detection and autocorrelation
    /// Robuster Algorithmus für zuverlässige BPM-Erkennung
    /// </summary>
    public class BPMDetector
    {
        private const double MinBPM = 40.0;
        private const double MaxBPM = 220.0;

        /// <summary>
        /// BPM detection result structure
        /// </summary>
        public class BPMResult
        {
            public double? bpm { get; set; }
            public string? error { get; set; }
            public bool success => bpm.HasValue;
        }

        /// <summary>
        /// Detect BPM from audio file - Main entry point
        /// </summary>
        public static BPMResult DetectBPMFromFile(string filePath, double windowSeconds = 3.0, bool verbose = false, Action<string>? debugCallback = null)
        {
            try
            {
                debugCallback?.Invoke($"=== BPM-Erkennung aus Datei ===");
                debugCallback?.Invoke($"Datei: {System.IO.Path.GetFileName(filePath)}");

                // Read audio file
                var (samples, sampleRate) = AudioFileReader.ReadAudioFile(filePath, debugCallback);
                if (samples == null || sampleRate == 0)
                {
                    debugCallback?.Invoke("FEHLER: Audiodatei konnte nicht gelesen werden");
                    return new BPMResult { error = "Audiodatei konnte nicht gelesen werden" };
                }

                debugCallback?.Invoke($"Audiodatei geladen: {samples.Length:N0} Samples, {sampleRate} Hz");
                debugCallback?.Invoke($"Dauer: {samples.Length / (double)sampleRate:F2} Sekunden");

                // Detect BPM from entire file using robust algorithm
                var bpm = DetectBPMRobust(samples, sampleRate, debugCallback);
                
                if (bpm > 0 && bpm >= MinBPM && bpm <= MaxBPM)
                {
                    debugCallback?.Invoke($"=== Ergebnis ===");
                    debugCallback?.Invoke($"Erkannte BPM: {bpm:F2}");
                    debugCallback?.Invoke($"=== BPM-Erkennung abgeschlossen ===");
                    
                    if (verbose)
                        Console.WriteLine($"BPM: {bpm:F2}");
                    
                    return new BPMResult { bpm = Math.Round(bpm, 2) };
                }
                else
                {
                    debugCallback?.Invoke($"FEHLER: Ungültige BPM: {bpm:F2}");
                    return new BPMResult { error = "Kein gültiger BPM erkannt" };
                }
            }
            catch (Exception ex)
            {
                debugCallback?.Invoke($"AUSNAHME: {ex.GetType().Name}: {ex.Message}");
                return new BPMResult { error = $"Fehler: {ex.Message}" };
            }
        }

        /// <summary>
        /// Robust BPM detection using energy envelope and autocorrelation
        /// </summary>
        private static double DetectBPMRobust(double[] samples, int sampleRate, Action<string>? debugCallback)
        {
            debugCallback?.Invoke("=== Robuste BPM-Erkennung ===");
            
            // Step 1: Calculate energy envelope with short windows
            debugCallback?.Invoke("Berechne Energie-Hüllkurve...");
            var envelope = CalculateEnergyEnvelope(samples, sampleRate);
            debugCallback?.Invoke($"Hüllkurve: {envelope.Length:N0} Samples");
            
            // Step 2: Enhance onsets with derivative
            debugCallback?.Invoke("Verstärke Beat-Onsets...");
            var onsets = EnhanceOnsets(envelope);
            debugCallback?.Invoke($"Onset-Signal: {onsets.Length:N0} Samples");
            
            // Step 3: Autocorrelation to find tempo
            debugCallback?.Invoke("Berechne Autokorrelation...");
            var tempogram = ComputeTempogram(onsets, sampleRate);
            debugCallback?.Invoke($"Tempogram: {tempogram.Length:N0} Werte");
            
            // Step 4: Find peak in valid BPM range
            debugCallback?.Invoke($"Suche Peak im Bereich {MinBPM}-{MaxBPM} BPM...");
            var bpm = FindBPMFromTempogram(tempogram, sampleRate, debugCallback);
            
            debugCallback?.Invoke($"Gefundene BPM: {bpm:F2}");
            
            return bpm;
        }

        /// <summary>
        /// Calculate energy envelope using RMS in short windows
        /// </summary>
        private static double[] CalculateEnergyEnvelope(double[] samples, int sampleRate)
        {
            const int windowSize = 2048;  // ~46ms at 44100Hz
            const int hopSize = 512;       // ~11ms hop
            
            var numFrames = (samples.Length - windowSize) / hopSize + 1;
            var envelope = new double[numFrames];
            
            for (int i = 0; i < numFrames; i++)
            {
                int start = i * hopSize;
                double energy = 0;
                
                for (int j = 0; j < windowSize && start + j < samples.Length; j++)
                {
                    double sample = samples[start + j];
                    energy += sample * sample;
                }
                
                envelope[i] = Math.Sqrt(energy / windowSize);
            }
            
            return envelope;
        }

        /// <summary>
        /// Enhance beat onsets using first-order difference and half-wave rectification
        /// </summary>
        private static double[] EnhanceOnsets(double[] envelope)
        {
            var onsets = new double[envelope.Length];
            
            // First-order difference (derivative)
            for (int i = 1; i < envelope.Length; i++)
            {
                double diff = envelope[i] - envelope[i - 1];
                // Half-wave rectification: only keep positive changes (increasing energy)
                onsets[i] = Math.Max(0, diff);
            }
            
            // Normalize
            double max = onsets.Max();
            if (max > 0)
            {
                for (int i = 0; i < onsets.Length; i++)
                {
                    onsets[i] /= max;
                }
            }
            
            return onsets;
        }

        /// <summary>
        /// Compute tempogram using autocorrelation
        /// </summary>
        private static double[] ComputeTempogram(double[] onsets, int sampleRate)
        {
            // Calculate hop size used in envelope (for timing)
            const int hopSize = 512;
            double frameRate = (double)sampleRate / hopSize;
            
            // Calculate lag range for BPM range
            int minLag = (int)(60.0 / MaxBPM * frameRate);
            int maxLag = (int)(60.0 / MinBPM * frameRate);
            
            minLag = Math.Max(1, minLag);
            maxLag = Math.Min(onsets.Length - 1, maxLag);
            
            var tempogram = new double[maxLag - minLag + 1];
            
            // Autocorrelation in valid lag range
            for (int lag = minLag; lag <= maxLag; lag++)
            {
                double sum = 0;
                int count = 0;
                
                for (int i = 0; i < onsets.Length - lag; i++)
                {
                    sum += onsets[i] * onsets[i + lag];
                    count++;
                }
                
                tempogram[lag - minLag] = count > 0 ? sum / count : 0;
            }
            
            return tempogram;
        }

        /// <summary>
        /// Find BPM from tempogram by finding the strongest peak
        /// </summary>
        private static double FindBPMFromTempogram(double[] tempogram, int sampleRate, Action<string>? debugCallback)
        {
            const int hopSize = 512;
            double frameRate = (double)sampleRate / hopSize;
            
            int minLag = (int)(60.0 / MaxBPM * frameRate);
            
            // Find maximum peak
            double maxValue = double.MinValue;
            int maxIndex = 0;
            
            for (int i = 0; i < tempogram.Length; i++)
            {
                if (tempogram[i] > maxValue)
                {
                    maxValue = tempogram[i];
                    maxIndex = i;
                }
            }
            
            // Convert lag to BPM
            int actualLag = maxIndex + minLag;
            double bpm = 60.0 * frameRate / actualLag;
            
            debugCallback?.Invoke($"Peak bei Lag {actualLag} -> BPM: {bpm:F2}");
            
            // Check for harmonic ambiguity (double-tempo)
            // If BPM is around half of what it should be, check double-tempo
            if (bpm < 90)
            {
                double doubleBPM = bpm * 2;
                if (doubleBPM >= MinBPM && doubleBPM <= MaxBPM)
                {
                    debugCallback?.Invoke($"Erkenne mögliches Half-Tempo: {bpm:F2} -> Prüfe {doubleBPM:F2}");
                    
                    // Check if double-tempo peak exists
                    int doubleTempoLag = actualLag / 2;
                    int doubleTempoIndex = doubleTempoLag - minLag;
                    
                    if (doubleTempoIndex >= 0 && doubleTempoIndex < tempogram.Length)
                    {
                        double doubleTempoValue = tempogram[doubleTempoIndex];
                        
                        // If double-tempo peak is significant (>70% of main peak), use it
                        if (doubleTempoValue > maxValue * 0.7)
                        {
                            debugCallback?.Invoke($"Double-Tempo bestätigt: {doubleBPM:F2} (Peak: {doubleTempoValue:F4} vs {maxValue:F4})");
                            bpm = doubleBPM;
                        }
                    }
                }
            }
            
            return bpm;
        }

        /// <summary>
        /// Legacy method for compatibility - calls robust detection
        /// </summary>
        public static BPMResult DetectBPM(double[] data, int sampleRate, bool verbose = false, Action<string>? debugCallback = null)
        {
            try
            {
                var bpm = DetectBPMRobust(data, sampleRate, debugCallback);
                
                if (bpm > 0 && bpm >= MinBPM && bpm <= MaxBPM)
                {
                    return new BPMResult { bpm = Math.Round(bpm, 2) };
                }
                else
                {
                    return new BPMResult { error = "Kein gültiger BPM erkannt" };
                }
            }
            catch (Exception ex)
            {
                debugCallback?.Invoke($"AUSNAHME: {ex.Message}");
                return new BPMResult { error = ex.Message };
            }
        }
    }
}