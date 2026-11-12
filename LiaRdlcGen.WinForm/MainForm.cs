using System.Text.Json;
using LiaRdlcGen.Core.Services;

namespace LiaRdlcGen
{
    public partial class MainForm : Form
    {
        private readonly TemplateRdlcGenerator generator = new();
        private readonly RdlcJsonService jsonService = new();
        private readonly ComponentTemplateCompiler componentCompiler = new();
        private readonly GeminiImageLayoutService imageLayoutService = new();
        private readonly AppSettingsService settingsService = new();

        public MainForm()
        {
            InitializeComponent();
            LoadGenerationModes();
            LoadAiSettings();
            LoadSample();
            LoadTemplates();
        }

        private void LoadSampleButton_Click(object? sender, EventArgs e)
        {
            LoadSample();
        }

        private void FormatJsonButton_Click(object? sender, EventArgs e)
        {
            try
            {
                jsonTextBox.Text = componentCompiler.IsComponentDocument(jsonTextBox.Text)
                    ? componentCompiler.Format(jsonTextBox.Text)
                    : jsonService.Format(jsonTextBox.Text);
                SetStatus("JSON formatted.", false);
            }
            catch (Exception exception) when (exception is JsonException or InvalidDataException or NotSupportedException)
            {
                SetStatus(exception.Message, true);
            }
        }

        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            using SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "rdlc",
                Filter = "RDLC report (*.rdlc)|*.rdlc|All files (*.*)|*.*",
                FileName = "GeneratedReport.rdlc",
                OverwritePrompt = true,
                Title = "Create RDLC"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                if (templateComboBox.SelectedItem is not TemplateOption template)
                    throw new InvalidDataException("Select an RDLC template.");

                if (modeComboBox.SelectedItem is not GenerationModeOption mode)
                    throw new InvalidDataException("Select a generation mode.");

                generator.Generate(jsonTextBox.Text, template.Path, dialog.FileName, mode.Mode);
                SetStatus($"Created {dialog.FileName}", false);
                MessageBox.Show(this, "The RDLC model was converted successfully.", "RDLC created", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception) when (exception is JsonException or InvalidDataException or IOException or UnauthorizedAccessException or NotSupportedException)
            {
                SetStatus(exception.Message, true);
                MessageBox.Show(this, exception.Message, "Unable to create RDLC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTemplates()
        {
            templateComboBox.Items.Clear();
            foreach (string path in generator.GetAvailableTemplates())
                templateComboBox.Items.Add(new TemplateOption(path));

            if (templateComboBox.Items.Count > 0)
            {
                templateComboBox.SelectedIndex = 0;
                return;
            }

            generateButton.Enabled = false;
            SetStatus("No RDLC templates were found.", true);
        }

        private void LoadGenerationModes()
        {
            modeComboBox.Items.Add(new GenerationModeOption(TemplateGenerationMode.ReportItems, "Template items"));
            modeComboBox.Items.Add(new GenerationModeOption(TemplateGenerationMode.FullReport, "Full report"));
            modeComboBox.SelectedIndex = 0;
        }

        private void ModeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            descriptionLabel.Text = modeComboBox.SelectedItem is GenerationModeOption { Mode: TemplateGenerationMode.ReportItems }
                ? "Convert only the first report section's body items and insert them at TemplatePlaceholder."
                : "Enter the direct RDLC model JSON. It is converted and merged into the selected template.";
        }

        private void LoadAiSettings()
        {
            AppSettings settings = settingsService.Load();
            apiKeyTextBox.Text = settings.GeminiApiKey;
            modelTextBox.Text = settings.GeminiModel;
        }

        private void SaveAiSettingsButton_Click(object? sender, EventArgs e)
        {
            try
            {
                settingsService.Save(new AppSettings
                {
                    GeminiApiKey = apiKeyTextBox.Text.Trim(),
                    GeminiModel = modelTextBox.Text.Trim()
                });
                SetStatus("Gemini settings saved for this Windows user.", false);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                SetStatus(exception.Message, true);
            }
        }

        private void SelectImageButton_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new()
            {
                CheckFileExists = true,
                Filter = "Report images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select report image"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
                imagePathTextBox.Text = dialog.FileName;
        }

        private async void AnalyzeImageButton_Click(object? sender, EventArgs e)
        {
            try
            {
                analyzeImageButton.Enabled = false;
                SetStatus("Gemini is analyzing the report image...", false);
                string json = await imageLayoutService.GenerateAsync(imagePathTextBox.Text, apiKeyTextBox.Text.Trim(), modelTextBox.Text.Trim());
                componentCompiler.Compile(json);
                jsonTextBox.Text = componentCompiler.Format(json);
                SelectTemplateItemsMode();
                SetStatus("Image converted to component JSON. Review the layout before creating RDLC.", false);
            }
            catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidDataException or InvalidOperationException or IOException or UnauthorizedAccessException or TaskCanceledException)
            {
                SetStatus(exception.Message, true);
                MessageBox.Show(this, exception.Message, "Unable to analyze image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                analyzeImageButton.Enabled = true;
            }
        }

        private void SelectTemplateItemsMode()
        {
            for (int index = 0; index < modeComboBox.Items.Count; index++)
            {
                if (modeComboBox.Items[index] is GenerationModeOption { Mode: TemplateGenerationMode.ReportItems })
                {
                    modeComboBox.SelectedIndex = index;
                    return;
                }
            }
        }

        private void LoadSample()
        {
            try
            {
                bool templateItemsMode = modeComboBox.SelectedItem is GenerationModeOption { Mode: TemplateGenerationMode.ReportItems };
                jsonTextBox.Text = templateItemsMode ? ComponentTemplateCompiler.CreateSample() : jsonService.Serialize(RdlcSampleFactory.Create());
                SetStatus(templateItemsMode ? "Template report-item sample loaded." : "Full report sample loaded.", false);
            }
            catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException)
            {
                jsonTextBox.Clear();
                SetStatus($"Sample could not be loaded: {exception.Message}", true);
            }
        }

        private void SetStatus(string message, bool isError)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isError ? Color.Firebrick : SystemColors.GrayText;
        }

        private sealed record TemplateOption(string Path)
        {
            public override string ToString() => System.IO.Path.GetFileName(Path);
        }

        private sealed record GenerationModeOption(TemplateGenerationMode Mode, string DisplayName)
        {
            public override string ToString() => DisplayName;
        }
    }
}
