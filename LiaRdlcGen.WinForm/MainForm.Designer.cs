namespace LiaRdlcGen
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label titleLabel;
        private Label descriptionLabel;
        private FlowLayoutPanel templatePanel;
        private Label templateLabel;
        private ComboBox templateComboBox;
        private Label modeLabel;
        private ComboBox modeComboBox;
        private FlowLayoutPanel aiPanel;
        private Label apiKeyLabel;
        private TextBox apiKeyTextBox;
        private Label modelLabel;
        private TextBox modelTextBox;
        private Button saveAiSettingsButton;
        private TextBox imagePathTextBox;
        private Button selectImageButton;
        private Button analyzeImageButton;
        private TextBox jsonTextBox;
        private FlowLayoutPanel actionPanel;
        private Button loadSampleButton;
        private Button formatJsonButton;
        private Button generateButton;
        private Label statusLabel;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            mainLayout = new TableLayoutPanel();
            titleLabel = new Label();
            descriptionLabel = new Label();
            templatePanel = new FlowLayoutPanel();
            templateLabel = new Label();
            templateComboBox = new ComboBox();
            modeLabel = new Label();
            modeComboBox = new ComboBox();
            aiPanel = new FlowLayoutPanel();
            apiKeyLabel = new Label();
            apiKeyTextBox = new TextBox();
            modelLabel = new Label();
            modelTextBox = new TextBox();
            saveAiSettingsButton = new Button();
            imagePathTextBox = new TextBox();
            selectImageButton = new Button();
            analyzeImageButton = new Button();
            jsonTextBox = new TextBox();
            actionPanel = new FlowLayoutPanel();
            loadSampleButton = new Button();
            formatJsonButton = new Button();
            generateButton = new Button();
            statusLabel = new Label();
            mainLayout.SuspendLayout();
            templatePanel.SuspendLayout();
            aiPanel.SuspendLayout();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(titleLabel, 0, 0);
            mainLayout.Controls.Add(descriptionLabel, 0, 1);
            mainLayout.Controls.Add(templatePanel, 0, 2);
            mainLayout.Controls.Add(aiPanel, 0, 3);
            mainLayout.Controls.Add(jsonTextBox, 0, 4);
            mainLayout.Controls.Add(actionPanel, 0, 5);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Padding = new Padding(16);
            mainLayout.RowCount = 6;
            mainLayout.RowStyles.Add(new RowStyle());
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            titleLabel.Margin = new Padding(0, 0, 0, 4);
            titleLabel.Text = "Lia RDLC Generator";
            // 
            // descriptionLabel
            // 
            descriptionLabel.AutoEllipsis = true;
            descriptionLabel.Dock = DockStyle.Fill;
            descriptionLabel.ForeColor = SystemColors.GrayText;
            descriptionLabel.Margin = new Padding(0, 0, 0, 12);
            descriptionLabel.Text = "Enter the direct RDLC model JSON. It is validated and converted to a 2010/01 RDLC document.";
            descriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // templatePanel
            // 
            templatePanel.AutoSize = true;
            templatePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            templatePanel.Controls.Add(templateLabel);
            templatePanel.Controls.Add(templateComboBox);
            templatePanel.Controls.Add(modeLabel);
            templatePanel.Controls.Add(modeComboBox);
            templatePanel.Dock = DockStyle.Fill;
            templatePanel.FlowDirection = FlowDirection.LeftToRight;
            templatePanel.Margin = new Padding(0, 0, 0, 8);
            templatePanel.WrapContents = false;
            // 
            // templateLabel
            // 
            templateLabel.AutoSize = true;
            templateLabel.Margin = new Padding(0, 6, 8, 0);
            templateLabel.Text = "Template:";
            // 
            // templateComboBox
            // 
            templateComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            templateComboBox.FormattingEnabled = true;
            templateComboBox.Width = 320;
            // 
            // modeLabel
            // 
            modeLabel.AutoSize = true;
            modeLabel.Margin = new Padding(16, 6, 8, 0);
            modeLabel.Text = "Mode:";
            // 
            // modeComboBox
            // 
            modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modeComboBox.FormattingEnabled = true;
            modeComboBox.Width = 150;
            modeComboBox.SelectedIndexChanged += ModeComboBox_SelectedIndexChanged;
            // 
            // aiPanel
            // 
            aiPanel.AutoSize = true;
            aiPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            aiPanel.Controls.Add(apiKeyLabel);
            aiPanel.Controls.Add(apiKeyTextBox);
            aiPanel.Controls.Add(modelLabel);
            aiPanel.Controls.Add(modelTextBox);
            aiPanel.Controls.Add(saveAiSettingsButton);
            aiPanel.Controls.Add(imagePathTextBox);
            aiPanel.Controls.Add(selectImageButton);
            aiPanel.Controls.Add(analyzeImageButton);
            aiPanel.Dock = DockStyle.Fill;
            aiPanel.Margin = new Padding(0, 0, 0, 8);
            aiPanel.WrapContents = true;
            // 
            // apiKeyLabel
            // 
            apiKeyLabel.AutoSize = true;
            apiKeyLabel.Margin = new Padding(0, 7, 8, 0);
            apiKeyLabel.Text = "Gemini key:";
            // 
            // apiKeyTextBox
            // 
            apiKeyTextBox.UseSystemPasswordChar = true;
            apiKeyTextBox.Width = 180;
            // 
            // modelLabel
            // 
            modelLabel.AutoSize = true;
            modelLabel.Margin = new Padding(12, 7, 8, 0);
            modelLabel.Text = "Model:";
            // 
            // modelTextBox
            // 
            modelTextBox.Width = 150;
            // 
            // saveAiSettingsButton
            // 
            saveAiSettingsButton.AutoSize = true;
            saveAiSettingsButton.Text = "Save AI settings";
            saveAiSettingsButton.UseVisualStyleBackColor = true;
            saveAiSettingsButton.Click += SaveAiSettingsButton_Click;
            // 
            // imagePathTextBox
            // 
            imagePathTextBox.ReadOnly = true;
            imagePathTextBox.Width = 260;
            // 
            // selectImageButton
            // 
            selectImageButton.AutoSize = true;
            selectImageButton.Text = "Select image...";
            selectImageButton.UseVisualStyleBackColor = true;
            selectImageButton.Click += SelectImageButton_Click;
            // 
            // analyzeImageButton
            // 
            analyzeImageButton.AutoSize = true;
            analyzeImageButton.Text = "Generate JSON";
            analyzeImageButton.UseVisualStyleBackColor = true;
            analyzeImageButton.Click += AnalyzeImageButton_Click;
            // 
            // jsonTextBox
            // 
            jsonTextBox.AcceptsReturn = true;
            jsonTextBox.AcceptsTab = true;
            jsonTextBox.Dock = DockStyle.Fill;
            jsonTextBox.Font = new Font("Cascadia Mono", 10F);
            jsonTextBox.Multiline = true;
            jsonTextBox.ScrollBars = ScrollBars.Both;
            jsonTextBox.WordWrap = false;
            // 
            // actionPanel
            // 
            actionPanel.Controls.Add(loadSampleButton);
            actionPanel.Controls.Add(formatJsonButton);
            actionPanel.Controls.Add(generateButton);
            actionPanel.Controls.Add(statusLabel);
            actionPanel.AutoSize = true;
            actionPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            actionPanel.Dock = DockStyle.Fill;
            actionPanel.FlowDirection = FlowDirection.LeftToRight;
            actionPanel.Padding = new Padding(0, 8, 0, 4);
            actionPanel.WrapContents = false;
            // 
            // loadSampleButton
            // 
            loadSampleButton.AutoSize = true;
            loadSampleButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            loadSampleButton.MinimumSize = new Size(0, 34);
            loadSampleButton.Padding = new Padding(12, 2, 12, 2);
            loadSampleButton.Text = "Load sample";
            loadSampleButton.UseVisualStyleBackColor = true;
            loadSampleButton.Click += LoadSampleButton_Click;
            // 
            // formatJsonButton
            // 
            formatJsonButton.AutoSize = true;
            formatJsonButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            formatJsonButton.MinimumSize = new Size(0, 34);
            formatJsonButton.Padding = new Padding(12, 2, 12, 2);
            formatJsonButton.Text = "Format JSON";
            formatJsonButton.UseVisualStyleBackColor = true;
            formatJsonButton.Click += FormatJsonButton_Click;
            // 
            // generateButton
            // 
            generateButton.AutoSize = true;
            generateButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            generateButton.MinimumSize = new Size(0, 34);
            generateButton.Padding = new Padding(12, 2, 12, 2);
            generateButton.Text = "Create RDLC...";
            generateButton.UseVisualStyleBackColor = true;
            generateButton.Click += GenerateButton_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Margin = new Padding(12, 9, 0, 0);
            statusLabel.Text = "Ready";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 700);
            Controls.Add(mainLayout);
            MinimumSize = new Size(720, 480);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Lia RDLC Generator";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            templatePanel.ResumeLayout(false);
            templatePanel.PerformLayout();
            aiPanel.ResumeLayout(false);
            aiPanel.PerformLayout();
            actionPanel.ResumeLayout(false);
            actionPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
