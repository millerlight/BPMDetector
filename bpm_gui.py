#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
BPM Detector GUI Application
Ein einfaches GUI zur Erkennung von BPM aus WAV-Dateien
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import threading
import os
import sys
from pathlib import Path

# Import the BPM detection functions from the existing module
from bpm_detection.bpm_detection import read_audio, bpm_detector
import numpy as np


class BPMDetectorGUI:
    def __init__(self, root):
        self.root = root
        self.root.title("BPM Detector")
        self.root.geometry("500x280")
        self.root.resizable(False, False)
        
        # Configure style
        style = ttk.Style()
        style.theme_use('clam')
        
        # Store last used directory
        self.last_directory = str(Path.home())
        
        self.create_widgets()
        self.selected_file = None
        
    def create_widgets(self):
        # Main frame
        main_frame = ttk.Frame(self.root, padding="20")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # Title
        title_label = ttk.Label(main_frame, text="BPM Detector", font=("Arial", 16, "bold"))
        title_label.grid(row=0, column=0, columnspan=2, pady=(0, 20))
        
        # File selection frame
        file_frame = ttk.LabelFrame(main_frame, text="Audio Datei auswählen", padding="10")
        file_frame.grid(row=1, column=0, columnspan=2, sticky=(tk.W, tk.E), pady=(0, 15))
        
        # File path display
        self.file_path_var = tk.StringVar()
        self.file_path_var.set("Keine Datei ausgewählt")
        self.file_path_label = ttk.Label(file_frame, textvariable=self.file_path_var, 
                                       font=("Arial", 10), foreground="gray")
        self.file_path_label.grid(row=0, column=0, sticky=(tk.W, tk.E), padx=(0, 10))
        
        # Browse button
        self.browse_btn = ttk.Button(file_frame, text="Durchsuchen...", command=self.browse_file)
        self.browse_btn.grid(row=0, column=1)
        
        # Results frame
        results_frame = ttk.LabelFrame(main_frame, text="Ergebnisse", padding="10")
        results_frame.grid(row=2, column=0, columnspan=2, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        # BPM display
        bpm_frame = ttk.Frame(results_frame)
        bpm_frame.grid(row=0, column=0, sticky=(tk.W, tk.E), pady=(0, 10))
        
        ttk.Label(bpm_frame, text="Erkannte BPM:", font=("Arial", 12, "bold")).pack(side=tk.LEFT)
        
        self.bpm_var = tk.StringVar()
        self.bpm_var.set("--")
        self.bpm_label = ttk.Label(bpm_frame, textvariable=self.bpm_var, 
                                  font=("Arial", 14, "bold"), foreground="blue")
        self.bpm_label.pack(side=tk.LEFT, padx=(10, 0))
        
        # Progress bar
        self.progress_var = tk.DoubleVar()
        self.progress_bar = ttk.Progressbar(results_frame, variable=self.progress_var, 
                                          mode='indeterminate')
        self.progress_bar.grid(row=1, column=0, sticky=(tk.W, tk.E), pady=(5, 0))
        
        # Status label
        self.status_var = tk.StringVar()
        self.status_var.set("Bereit zur Verarbeitung")
        self.status_label = ttk.Label(results_frame, textvariable=self.status_var, 
                                    font=("Arial", 9), foreground="gray")
        self.status_label.grid(row=2, column=0, sticky=(tk.W, tk.E), pady=(5, 0))
        
        # Configure grid weights
        main_frame.columnconfigure(0, weight=1)
        file_frame.columnconfigure(0, weight=1)
        results_frame.columnconfigure(0, weight=1)
        
        # Configure row weights for expansion
        main_frame.rowconfigure(2, weight=1)
        
    def browse_file(self):
        """Open file dialog to select audio file"""
        file_types = [
            ("Audio Files", "*.wav *.mp3"),
            ("WAV Files", "*.wav"),
            ("MP3 Files", "*.mp3"),
            ("All Files", "*.*")
        ]
        
        filename = filedialog.askopenfilename(
            title="Audio-Datei auswählen",
            filetypes=file_types,
            initialdir=self.last_directory
        )
        
        if filename:
            # Remember the directory for next time
            self.last_directory = os.path.dirname(filename)
            
            self.selected_file = filename
            file_name = os.path.basename(filename)
            if len(file_name) > 30:
                display_name = "..." + file_name[-27:]
            else:
                display_name = file_name
            self.file_path_var.set(f"Ausgewählt: {display_name}")
            self.status_var.set("Datei geladen. Starte automatische Analyse...")
            
            # Automatically start BPM analysis after file selection
            self.root.after(500, self.process_file)
            
    def process_file(self):
        """Process the selected audio file in a separate thread"""
        if not self.selected_file:
            messagebox.showerror("Fehler", "Bitte wählen Sie zuerst eine Audio-Datei aus.")
            return
            
        # Start processing
        self.progress_bar.start()
        self.status_var.set("Analysiere BPM...")
        
        # Start processing in a separate thread
        thread = threading.Thread(target=self._process_audio, daemon=True)
        thread.start()
        
    def _process_audio(self):
        """Process audio file in background thread"""
        try:
            # Read audio file
            samps, fs = read_audio(self.selected_file)
            
            if samps is None or fs is None:
                self.root.after(0, self._show_error, "Fehler beim Lesen der Audio-Datei")
                return
            
            # Process audio for BPM detection
            bpm, _ = self._detect_bpm_in_chunks(samps, fs)
            
            if bpm is None or bpm <= 0:
                self.root.after(0, self._show_error, "BPM konnte nicht erkannt werden. Versuchen Sie eine andere Datei.")
                return
            
            # Update UI in main thread with BPM rounded to 2 decimal places
            self.root.after(0, self._show_result, round(bpm, 2))
            
        except Exception as e:
            self.root.after(0, self._show_error, f"Fehler bei der Verarbeitung: {str(e)}")
            
    def _detect_bpm_in_chunks(self, samps, fs, chunk_duration=3.0):
        """Detect BPM by processing audio in chunks"""
        try:
            chunk_samples = int(chunk_duration * fs)
            bpms = []
            
            for start_idx in range(0, len(samps), chunk_samples):
                chunk = samps[start_idx:start_idx + chunk_samples]
                if len(chunk) < chunk_samples // 2:  # Skip too short chunks
                    continue
                    
                try:
                    # Use the existing BPM detector function with verbose=False for cleaner processing
                    bpm, _ = bpm_detector(chunk, fs, verbose=False)
                    if bpm is not None and 60 <= bpm <= 200:  # Reasonable BPM range
                        bpms.append(bpm)
                except:
                    continue  # Skip chunks that can't be processed
            
            if not bpms:
                return None, None
                
            # Return median BPM for stability
            return np.median(bpms), None
            
        except Exception:
            return None, None
            
    def _show_result(self, bpm):
        """Display the BPM result without popup"""
        self.progress_bar.stop()
        self.bpm_var.set(f"{bpm} BPM")
        self.status_var.set("Analyse abgeschlossen")
        
    def _show_error(self, message):
        """Display error message"""
        self.progress_bar.stop()
        self.status_var.set("Fehler aufgetreten")
        messagebox.showerror("Fehler", message)


def main():
    """Main function to start the GUI application"""
    # Prevent console window on Windows
    if sys.platform.startswith('win'):
        try:
            from ctypes import windll
            windll.user32.ShowWindow(windll.kernel32.GetConsoleWindow(), 0)
        except:
            pass  # Ignore if not possible
    
    root = tk.Tk()
    app = BPMDetectorGUI(root)
    
    # Center the window on screen
    root.update_idletasks()
    x = (root.winfo_screenwidth() // 2) - (root.winfo_width() // 2)
    y = (root.winfo_screenheight() // 2) - (root.winfo_height() // 2)
    root.geometry(f"+{x}+{y}")
    
    root.mainloop()


if __name__ == "__main__":
    main()