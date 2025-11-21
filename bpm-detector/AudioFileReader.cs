using System;
using System.IO;
using System.Linq;
using NAudio.Wave;

namespace BPMDetector
{
    /// <summary>
    /// Handles reading audio files (WAV and MP3) and converting them to audio samples
    /// Enhanced with debug output
    /// </summary>
    public class AudioFileReader
    {
        /// <summary>
        /// Read audio file and return samples and sample rate
        /// </summary>
        public static (double[]? samples, int sampleRate) ReadAudioFile(string filePath, Action<string>? debugCallback = null)
        {
            debugCallback?.Invoke($"=== AudioFileReader: ReadAudioFile ===");
            debugCallback?.Invoke($"File path: {filePath}");
            
            if (!File.Exists(filePath))
            {
                debugCallback?.Invoke($"ERROR: File not found: {filePath}");
                return (null, 0);
            }

            // Get file info
            try
            {
                var fileInfo = new FileInfo(filePath);
                debugCallback?.Invoke($"File exists: {fileInfo.Length:N0} bytes");
                debugCallback?.Invoke($"File created: {fileInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                debugCallback?.Invoke($"File modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                debugCallback?.Invoke($"Warning: Could not get file info: {ex.Message}");
            }

            string extension = Path.GetExtension(filePath).ToLower();
            debugCallback?.Invoke($"Detected extension: {extension}");
            
            (double[]? samples, int sampleRate) result = extension switch
            {
                ".wav" => ReadWavFile(filePath, debugCallback),
                ".mp3" => ReadMp3File(filePath, debugCallback),
                _ => throw new NotSupportedException($"Unsupported file format: {extension}")
            };
            
            if (result.samples != null)
            {
                debugCallback?.Invoke($"✓ Audio file loaded successfully");
                debugCallback?.Invoke($"Sample rate: {result.sampleRate} Hz");
                debugCallback?.Invoke($"Total samples: {result.samples.Length:N0}");
                debugCallback?.Invoke($"Duration: {result.samples.Length / (double)result.sampleRate:F2} seconds");
                debugCallback?.Invoke($"Sample range: [{result.samples.Min():F6} to {result.samples.Max():F6}]");
                debugCallback?.Invoke($"AudioFileReader: SUCCESS");
            }
            else
            {
                debugCallback?.Invoke($"✗ Audio file loading failed");
            }
            
            return result;
        }

        private static (double[]? samples, int sampleRate) ReadWavFile(string filePath, Action<string>? debugCallback = null)
        {
            debugCallback?.Invoke($"  Reading WAV file...");
            
            try
            {
                using var audioFile = new NAudio.Wave.AudioFileReader(filePath);
                var waveFormat = audioFile.WaveFormat;
                
                debugCallback?.Invoke($"    WAV format: {waveFormat.SampleRate}Hz, {waveFormat.Channels} channel(s), {waveFormat.BitsPerSample} bit");
                
                // Get total samples - this might be the byte count, need to be careful
                long totalSamples = audioFile.Length;
                debugCallback?.Invoke($"    File length: {totalSamples:N0} bytes");
                
                // Calculate actual sample count based on format
                int bytesPerSample = waveFormat.BitsPerSample / 8;
                int sampleCount = (int)(totalSamples / bytesPerSample / waveFormat.Channels);
                debugCallback?.Invoke($"    Calculated sample count: {sampleCount:N0}");
                
                // Create buffer for reading - use calculated sample count
                var buffer = new float[sampleCount];
                int samplesRead = audioFile.Read(buffer, 0, buffer.Length);
                debugCallback?.Invoke($"    Actually read: {samplesRead:N0} samples");
                
                // Take only the samples we actually read
                var actualBuffer = buffer.Take(samplesRead).ToArray();
                
                // Convert to double array
                var samples = actualBuffer.Select(f => (double)f).ToArray();
                debugCallback?.Invoke($"    Converted to double array: {samples.Length:N0} samples");
                
                // If stereo, convert to mono
                if (waveFormat.Channels == 2)
                {
                    debugCallback?.Invoke($"    Converting stereo to mono...");
                    var monoSamples = new double[samples.Length / 2];
                    for (int i = 0; i < samples.Length / 2; i++)
                    {
                        monoSamples[i] = (samples[i * 2] + samples[i * 2 + 1]) / 2.0;
                    }
                    debugCallback?.Invoke($"    Mono samples: {monoSamples.Length:N0}");
                    return (monoSamples, waveFormat.SampleRate);
                }
                
                debugCallback?.Invoke($"    Returning {samples.Length:N0} samples at {waveFormat.SampleRate} Hz");
                return (samples, waveFormat.SampleRate);
            }
            catch (Exception ex)
            {
                debugCallback?.Invoke($"    ERROR reading WAV file: {ex.GetType().Name}: {ex.Message}");
                debugCallback?.Invoke($"    Stack trace: {ex.StackTrace}");
                return (null, 0);
            }
        }

        private static (double[]? samples, int sampleRate) ReadMp3File(string filePath, Action<string>? debugCallback = null)
        {
            debugCallback?.Invoke($"  Reading MP3 file...");
            
            try
            {
                using var audioFile = new NAudio.Wave.AudioFileReader(filePath);
                var waveFormat = audioFile.WaveFormat;
                
                debugCallback?.Invoke($"    MP3 format: {waveFormat.SampleRate}Hz, {waveFormat.Channels} channel(s)");
                
                // Get total samples - this might be the byte count, need to be careful
                long totalSamples = audioFile.Length;
                debugCallback?.Invoke($"    File length: {totalSamples:N0} bytes");
                
                // Calculate actual sample count based on format
                int bytesPerSample = waveFormat.BitsPerSample / 8;
                int sampleCount = (int)(totalSamples / bytesPerSample / waveFormat.Channels);
                debugCallback?.Invoke($"    Calculated sample count: {sampleCount:N0}");
                
                // Create buffer for reading - use calculated sample count
                var buffer = new float[sampleCount];
                int samplesRead = audioFile.Read(buffer, 0, buffer.Length);
                debugCallback?.Invoke($"    Actually read: {samplesRead:N0} samples");
                
                // Take only the samples we actually read
                var actualBuffer = buffer.Take(samplesRead).ToArray();
                
                // Convert to double array
                var samples = actualBuffer.Select(f => (double)f).ToArray();
                debugCallback?.Invoke($"    Converted to double array: {samples.Length:N0} samples");
                
                // If stereo, convert to mono
                if (waveFormat.Channels == 2)
                {
                    debugCallback?.Invoke($"    Converting stereo to mono...");
                    var monoSamples = new double[samples.Length / 2];
                    for (int i = 0; i < samples.Length / 2; i++)
                    {
                        monoSamples[i] = (samples[i * 2] + samples[i * 2 + 1]) / 2.0;
                    }
                    debugCallback?.Invoke($"    Mono samples: {monoSamples.Length:N0}");
                    return (monoSamples, waveFormat.SampleRate);
                }
                
                debugCallback?.Invoke($"    Returning {samples.Length:N0} samples at {waveFormat.SampleRate} Hz");
                return (samples, waveFormat.SampleRate);
            }
            catch (Exception ex)
            {
                debugCallback?.Invoke($"    ERROR reading MP3 file: {ex.GetType().Name}: {ex.Message}");
                debugCallback?.Invoke($"    Stack trace: {ex.StackTrace}");
                return (null, 0);
            }
        }
    }
}