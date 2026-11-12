using System.Text.Json;
using LiaRdlcGen.Core.Services;

namespace LiaRdlcGen
{
    public partial class MainForm : Form
    {
        private readonly TemplateRdlcGenerator generator = new();
        private readonly RdlcJsonService jsonService = new();

        public MainForm()
        {
            InitializeComponent();
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
                jsonTextBox.Text = jsonService.Format(jsonTextBox.Text);
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

                generator.Generate(jsonTextBox.Text, template.Path, dialog.FileName);
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

        private void LoadSample()
        {
            try
            {
                jsonTextBox.Text = jsonService.Serialize(RdlcSampleFactory.Create());
                SetStatus("Direct RDLC model sample loaded.", false);
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
    }
}
