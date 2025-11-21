using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BPMDetector
{
    /// <summary>
    /// Main GUI form for BPM Detector application
    /// </summary>
    public partial class MainForm : Form
    {
        private TextBox filePathTextBox = null!;
        private Button browseButton = null!;
        private Label bpmLabel = null!;
        private Label statusLabel = null!;
        private ProgressBar progressBar = null!;
        private Label titleLabel = null!;
        private Panel mainPanel = null!;
        private TextBox messagesTextBox = null!;
        private Button copyMessagesButton = null!;
        private Button clearMessagesButton = null!;
        private Label messagesLabel = null!;
        private GroupBox messagesGroup = null!;
        private Button toggleMessagesButton = null!;
        private Label toggleLabel = null!;
        
        private readonly StringBuilder messages = new StringBuilder();
        private string? selectedFilePath;
        private bool messagesExpanded = false;
        private const int CollapsedHeight = 320;
        private const int ExpandedHeight = 650;

        public MainForm()
        {
            InitializeComponent();
            AddMessage("Anwendung gestartet - Bereit zur Verarbeitung");
        }

        private void AddMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string fullMessage = $"[{timestamp}] {message}";
            
            messages.AppendLine(fullMessage);
            
            // Keep only last 100 lines to avoid memory issues
            if (messages.Length > 10000)
            {
                var lines = messages.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 100)
                {
                    messages.Clear();
                    for (int i = lines.Length - 100; i < lines.Length; i++)
                    {
                        messages.AppendLine(lines[i]);
                    }
                }
            }
            
            // Update UI if handle exists
            if (IsHandleCreated && messagesTextBox != null && !messagesTextBox.IsDisposed)
            {
                BeginInvoke(() => 
                {
                    try
                    {
                        messagesTextBox.Text = messages.ToString();
                        messagesTextBox.SelectionStart = messagesTextBox.Text.Length;
                        messagesTextBox.ScrollToCaret();
                    }
                    catch
                    {
                        // Ignore UI update errors
                    }
                });
            }
        }

        private void InitializeComponent()
        {
            // Form properties
            Text = "BPM Detector";
            Size = new Size(750, CollapsedHeight);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            
            // Main panel
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };
            Controls.Add(mainPanel);

            // Title
            titleLabel = new Label
            {
                Text = "BPM Detector",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 15),
                ForeColor = Color.FromArgb(0, 102, 204)
            };
            mainPanel.Controls.Add(titleLabel);

            // File selection group
            var fileGroup = new GroupBox
            {
                Text = "Audio-Datei auswählen",
                Location = new Point(15, 55),
                Size = new Size(710, 75),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            mainPanel.Controls.Add(fileGroup);

            filePathTextBox = new TextBox
            {
                ReadOnly = true,
                Text = "Keine Datei ausgewählt",
                Location = new Point(15, 30),
                Size = new Size(575, 25),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                BackColor = Color.White
            };
            fileGroup.Controls.Add(filePathTextBox);

            browseButton = new Button
            {
                Text = "Durchsuchen...",
                Location = new Point(600, 28),
                Size = new Size(95, 28),
                Font = new Font("Segoe UI", 9),
                UseVisualStyleBackColor = true
            };
            browseButton.Click += BrowseButton_Click;
            fileGroup.Controls.Add(browseButton);

            // Analysis group (BPM Result)
            var analysisGroup = new GroupBox
            {
                Text = "BPM-Ergebnis",
                Location = new Point(15, 140),
                Size = new Size(710, 130),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            mainPanel.Controls.Add(analysisGroup);

            bpmLabel = new Label
            {
                Text = "-- BPM",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 122, 204),
                AutoSize = true,
                Location = new Point(20, 35)
            };
            analysisGroup.Controls.Add(bpmLabel);

            statusLabel = new Label
            {
                Text = "Bereit",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 90)
            };
            analysisGroup.Controls.Add(statusLabel);

            progressBar = new ProgressBar
            {
                Location = new Point(20, 108),
                Size = new Size(670, 12),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };
            analysisGroup.Controls.Add(progressBar);

            // Toggle button for messages (collapsed by default)
            toggleMessagesButton = new Button
            {
                Text = "+",
                Location = new Point(15, 280),
                Size = new Size(30, 25),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                UseVisualStyleBackColor = true,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            toggleMessagesButton.FlatAppearance.BorderColor = Color.Gray;
            toggleMessagesButton.Click += ToggleMessagesButton_Click;
            mainPanel.Controls.Add(toggleMessagesButton);

            toggleLabel = new Label
            {
                Text = "Status-Nachrichten",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(50, 285),
                Cursor = Cursors.Hand
            };
            toggleLabel.Click += ToggleMessagesButton_Click;
            mainPanel.Controls.Add(toggleLabel);

            // Messages group (initially hidden)
            messagesGroup = new GroupBox
            {
                Text = "",
                Location = new Point(15, 310),
                Size = new Size(710, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Visible = false
            };
            mainPanel.Controls.Add(messagesGroup);

            messagesLabel = new Label
            {
                Text = "Detaillierte Statusmeldungen während der Verarbeitung:",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(15, 20)
            };
            messagesGroup.Controls.Add(messagesLabel);

            messagesTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 8),
                Location = new Point(15, 45),
                Size = new Size(680, 230),
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.FixedSingle
            };
            messagesGroup.Controls.Add(messagesTextBox);

            copyMessagesButton = new Button
            {
                Text = "Kopieren",
                Location = new Point(15, 285),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 8),
                UseVisualStyleBackColor = true
            };
            copyMessagesButton.Click += CopyMessagesButton_Click;
            messagesGroup.Controls.Add(copyMessagesButton);

            clearMessagesButton = new Button
            {
                Text = "Löschen",
                Location = new Point(125, 285),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 8),
                UseVisualStyleBackColor = true
            };
            clearMessagesButton.Click += ClearMessagesButton_Click;
            messagesGroup.Controls.Add(clearMessagesButton);

            // Set tab order
            browseButton.TabIndex = 0;
        }

        private void ToggleMessagesButton_Click(object? sender, EventArgs e)
        {
            messagesExpanded = !messagesExpanded;
            
            if (messagesExpanded)
            {
                // Expand
                Size = new Size(750, ExpandedHeight);
                messagesGroup.Visible = true;
                messagesGroup.Size = new Size(710, 320);
                toggleMessagesButton.Text = "×";
                toggleMessagesButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            }
            else
            {
                // Collapse
                Size = new Size(750, CollapsedHeight);
                messagesGroup.Visible = false;
                messagesGroup.Size = new Size(710, 0);
                toggleMessagesButton.Text = "+";
                toggleMessagesButton.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            }
        }

        private async void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Audio-Dateien (*.wav;*.mp3)|*.wav;*.mp3|WAV-Dateien (*.wav)|*.wav|MP3-Dateien (*.mp3)|*.mp3|Alle Dateien (*.*)|*.*",
                Title = "Audio-Datei für BPM-Analyse auswählen"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                var fileName = Path.GetFileName(selectedFilePath);
                
                AddMessage($"Datei ausgewählt: {fileName}");
                
                try
                {
                    var fileInfo = new FileInfo(selectedFilePath);
                    var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                    AddMessage($"Dateigröße: {sizeInMB:F2} MB");
                }
                catch (Exception ex)
                {
                    AddMessage($"Warnung: {ex.Message}");
                }
                
                // Update display
                string displayName = fileName.Length > 50 ? "..." + fileName.Substring(fileName.Length - 47) : fileName;
                filePathTextBox.Text = displayName;
                filePathTextBox.ForeColor = Color.Black;
                
                statusLabel.Text = "Starte Analyse...";
                
                // Auto-start analysis immediately
                await Task.Delay(100);
                await AnalyzeFile();
            }
        }

        private async Task AnalyzeFile()
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Bitte wählen Sie zuerst eine Audio-Datei aus.", "Keine Datei",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // UI feedback
                progressBar.Visible = true;
                statusLabel.Text = "Analysiere...";
                browseButton.Enabled = false;
                bpmLabel.Text = "...";
                bpmLabel.ForeColor = Color.Gray;
                
                AddMessage("=== BPM-Analyse gestartet ===");
                AddMessage($"Verarbeite: {Path.GetFileName(selectedFilePath)}");

                // Run analysis with detailed callback
                var analysisTask = Task.Run(() =>
                {
                    return BPMDetector.DetectBPMFromFile(
                        selectedFilePath, 
                        windowSeconds: 3.0, 
                        verbose: false,
                        debugCallback: (msg) => AddMessage(msg)
                    );
                });

                // Wait with timeout (60 seconds for larger files)
                var timeoutTask = Task.Delay(60000);
                var completedTask = await Task.WhenAny(analysisTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    AddMessage("FEHLER: Zeitüberschreitung nach 60 Sekunden");
                    MessageBox.Show(
                        "Die BPM-Analyse hat zu lange gedauert und wurde abgebrochen.\n\n" +
                        "Mögliche Ursachen:\n" +
                        "• Die Datei ist zu groß\n" +
                        "• Die Sample-Rate ist sehr hoch\n" +
                        "• Unerwarteter Fehler\n\n" +
                        "Versuchen Sie eine kürzere oder kleinere Datei.",
                        "Zeitüberschreitung",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    
                    bpmLabel.Text = "-- BPM";
                    statusLabel.Text = "Analyse abgebrochen";
                    return;
                }

                var result = await analysisTask;

                // Display results
                if (result.bpm.HasValue)
                {
                    bpmLabel.Text = $"{result.bpm.Value:F1} BPM";
                    bpmLabel.ForeColor = Color.FromArgb(0, 150, 0);
                    statusLabel.Text = "Analyse erfolgreich abgeschlossen";
                    AddMessage($"✓ Ergebnis: {result.bpm.Value:F1} BPM");
                    AddMessage("=== Analyse abgeschlossen ===");
                }
                else if (!string.IsNullOrEmpty(result.error))
                {
                    bpmLabel.Text = "Fehler";
                    bpmLabel.ForeColor = Color.Red;
                    statusLabel.Text = "Analyse fehlgeschlagen";
                    AddMessage($"✗ FEHLER: {result.error}");
                    
                    MessageBox.Show(
                        $"Fehler bei der BPM-Analyse:\n\n{result.error}\n\n" +
                        "Bitte überprüfen Sie:\n" +
                        "• Ist die Datei eine gültige Audio-Datei?\n" +
                        "• Enthält die Datei Musik mit klarem Beat?\n" +
                        "• Ist die Datei nicht beschädigt?",
                        "Analyse-Fehler",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    bpmLabel.Text = "-- BPM";
                    statusLabel.Text = "Kein Ergebnis";
                    AddMessage("✗ Kein BPM erkannt");
                    
                    MessageBox.Show(
                        "Die BPM-Analyse konnte keinen Beat erkennen.\n\n" +
                        "Mögliche Ursachen:\n" +
                        "• Die Datei enthält keine Musik mit klarem Beat\n" +
                        "• Die Musik ist zu komplex oder zu leise\n" +
                        "• Das Tempo liegt außerhalb des Bereichs (40-220 BPM)\n\n" +
                        "Versuchen Sie eine andere Datei.",
                        "Kein Beat erkannt",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                bpmLabel.Text = "Fehler";
                bpmLabel.ForeColor = Color.Red;
                statusLabel.Text = "Unerwarteter Fehler";
                AddMessage($"✗ AUSNAHME: {ex.Message}");
                
                MessageBox.Show(
                    $"Ein unerwarteter Fehler ist aufgetreten:\n\n{ex.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                browseButton.Enabled = true;
            }
        }

        private void CopyMessagesButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (messages.Length > 0)
                {
                    Clipboard.SetText(messages.ToString());
                    AddMessage("✓ Nachrichten in Zwischenablage kopiert");
                    MessageBox.Show(
                        "Die Status-Nachrichten wurden in die Zwischenablage kopiert.",
                        "Kopiert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Keine Nachrichten zum Kopieren vorhanden.",
                        "Leer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Kopieren:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClearMessagesButton_Click(object? sender, EventArgs e)
        {
            messages.Clear();
            messagesTextBox.Clear();
            AddMessage("Nachrichten gelöscht - Bereit");
        }
    }
}