# PyCharm Setup für BPM Detector

## Virtual Environment konfigurieren

Das Projekt enthält ein Virtual Environment (`.venv`), das alle benötigten Dependencies enthält.

### PyCharm Python Interpreter konfigurieren:

1. **Öffne PyCharm**
2. **Datei → Einstellungen** (Windows/Linux) oder **PyCharm → Einstellungen** (macOS)
3. **Wähle Project: the-BPM-detector-python** in der linken Sidebar
4. **Klicke auf "Python Interpreter"**
5. **Klicke auf das Zahnrad-Symbol (⚙️) → "Add"**
6. **Wähle "System Interpreter"**
7. **Browse zum Virtual Environment:**
   - **Windows:** `C:\Users\Horyzen\PycharmProjects\the-BPM-detector-python\.venv\Scripts\python.exe`
   - **Oder:** `C:\Users\Horyzen\PycharmProjects\the-BPM-detector-python\.venv\python.exe`
8. **Klicke "OK"**

### Alternative: System Interpreter verwenden
Falls das Virtual Environment nicht funktioniert, verwende den System Interpreter:
- **Windows:** `C:\Users\Horyzen\anaconda3\python.exe`
- **Oder:** `C:\Users\Horyzen\anaconda3\python.exe` (Anaconda)

## Dependencies
Das Virtual Environment enthält alle benötigten Module:
- matplotlib
- numpy
- scipy
- PyWavelets (pywt)
- pydub

## Test
Nach der Konfiguration sollte der Code ohne "No module named" Fehler funktionieren.

## Fehlerbehebung
Falls Module immer noch nicht gefunden werden:
1. Überprüfe, ob der richtige Python Interpreter in PyCharm eingestellt ist
2. Neuinstallation der Dependencies im richtigen Umfeld
3. PyCharm neu starten