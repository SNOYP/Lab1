using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lab1_2026.Models;
using Lab1_2026.Services;

namespace Lab1_2026
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RegistrationForm());
        }
    }

    public class GradientForm : Form
    {
        public GradientForm()
        {
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true; 
            this.ResizeRedraw = true; 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.Indigo, Color.DarkMagenta, 45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        protected void CenterPanel(Panel panel)
        {
            panel.Left = (this.ClientSize.Width - panel.Width) / 2;
            panel.Top = (this.ClientSize.Height - panel.Height) / 2;
        }
    }

    // 1. ОКНО РЕГИСТРАЦИИ
    public class RegistrationForm : GradientForm
    {
        private Panel mainPanel;
        private TextBox txtLastName, txtFirstName, txtProfession;
        private ComboBox cmbGender;
        private Button btnStart;

        public RegistrationForm()
        {
            this.Text = "Лабораторна робота №1-2 - Реєстрація";
            InitializeUI();
        }

        private void InitializeUI()
        {
            mainPanel = new Panel { Width = 400, Height = 480, BackColor = Color.Transparent };
            
            Label lblTitle = new Label { Text = "Реєстрація", Top = 0, Left = 0, Width = 400, Height = 50, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 22, FontStyle.Bold), ForeColor = Color.White };
            mainPanel.Controls.Add(lblTitle);

            string[] labels = { "Прізвище:", "Ім'я:", "Професія / Спеціальність:", "Стать:" };
            int startY = 60;

            for (int i = 0; i < 3; i++)
            {
                mainPanel.Controls.Add(new Label { Text = labels[i], Top = startY + (i * 70), Left = 0, Width = 400, Height = 25, ForeColor = Color.White, Font = new Font("Arial", 12) });
            }

            txtLastName = new TextBox { Top = 90, Left = 0, Width = 400, Font = new Font("Arial", 14) };
            txtFirstName = new TextBox { Top = 160, Left = 0, Width = 400, Font = new Font("Arial", 14) };
            txtProfession = new TextBox { Top = 230, Left = 0, Width = 400, Font = new Font("Arial", 14) };
            
            mainPanel.Controls.Add(new Label { Text = labels[3], Top = 280, Left = 0, Width = 400, Height = 25, ForeColor = Color.White, Font = new Font("Arial", 12) });
            cmbGender = new ComboBox { Top = 310, Left = 0, Width = 400, Font = new Font("Arial", 14), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new string[] { "Чоловіча", "Жіноча" });
            cmbGender.SelectedIndex = 0;

            mainPanel.Controls.Add(txtLastName);
            mainPanel.Controls.Add(txtFirstName);
            mainPanel.Controls.Add(txtProfession);
            mainPanel.Controls.Add(cmbGender);

            btnStart = new Button { Text = "Розпочати тест", Top = 400, Left = 50, Width = 300, Height = 45, BackColor = Color.MediumSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;
            mainPanel.Controls.Add(btnStart);

            // ПРИВЯЗКА КЛАВИШИ ENTER К КНОПКЕ "РОЗПОЧАТИ ТЕСТ"
            this.AcceptButton = btnStart;

            this.Controls.Add(mainPanel);

            CenterPanel(mainPanel);
            this.Resize += (s, e) => CenterPanel(mainPanel);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.");
                return;
            }

            var user = new User
            {
                LastName = txtLastName.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                Profession = txtProfession.Text.Trim(),
                Gender = cmbGender.SelectedItem.ToString()
            };

            var testForm = new TestingForm(user);
            this.Hide();
            testForm.Show();
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e) => Environment.Exit(0);
    }

    // 2. ОКНО ТЕСТИРОВАНИЯ
    public class TestingForm : GradientForm
    {
        private User currentUser;
        private TestService testService = new TestService();
        private StorageService storageService = new StorageService();
        
        private string[] questions;
        private int currentIndex = 0;
        private int totalScore = 0;

        private Panel mainPanel;
        private Label lblProgress, lblQuestionText;
        private RadioButton[] rbOptions;
        private Button btnNext;

        public TestingForm(User user)
        {
            currentUser = user;
            questions = testService.GetQuestions();
            
            this.Text = "Лабораторна робота №2 - Тестування";
            InitializeUI();
            LoadQuestion();
        }

        private void InitializeUI()
        {
            mainPanel = new Panel { Width = 600, Height = 480, BackColor = Color.Transparent };

            lblProgress = new Label { Top = 0, Left = 0, Width = 600, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 12), ForeColor = Color.Plum };
            mainPanel.Controls.Add(lblProgress);

            lblQuestionText = new Label { Top = 40, Left = 0, Width = 600, Height = 100, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 14, FontStyle.Bold), ForeColor = Color.White };
            mainPanel.Controls.Add(lblQuestionText);

            rbOptions = new RadioButton[5];
            string[] optionTexts = { 
                "2 - Повністю згоден, \"Так\"", 
                "1 - Швидше \"Так\", ніж \"Ні\"", 
                "0 - Важко сказати, ні \"Так\", ні \"Ні\"", 
                "-1 - Швидше \"Ні\", ніж \"Так\"", 
                "-2 - Повністю не згоден, \"Ні\"" 
            };

            for (int i = 0; i < 5; i++)
            {
                rbOptions[i] = new RadioButton
                {
                    Text = optionTexts[i],
                    Top = 150 + (i * 45),
                    Left = 100,
                    Width = 400,
                    Height = 35,
                    Font = new Font("Arial", 12),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                mainPanel.Controls.Add(rbOptions[i]);
            }

            btnNext = new Button { Text = "Наступне питання", Top = 400, Left = 150, Width = 300, Height = 45, BackColor = Color.MediumSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += BtnNext_Click;
            mainPanel.Controls.Add(btnNext);

            // ПРИВЯЗКА КЛАВИШИ ENTER К КНОПКЕ "НАСТУПНЕ ПИТАННЯ"
            this.AcceptButton = btnNext;

            this.Controls.Add(mainPanel);

            CenterPanel(mainPanel);
            this.Resize += (s, e) => CenterPanel(mainPanel);
        }

        private void LoadQuestion()
        {
            foreach (var rb in rbOptions) rb.Checked = false;
            lblProgress.Text = $"Питання {currentIndex + 1} з {questions.Length}";
            lblQuestionText.Text = questions[currentIndex];
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            int scoreValue = 0;
            bool isSelected = false;

            if (rbOptions[0].Checked) { scoreValue = 2; isSelected = true; }
            else if (rbOptions[1].Checked) { scoreValue = 1; isSelected = true; }
            else if (rbOptions[2].Checked) { scoreValue = 0; isSelected = true; }
            else if (rbOptions[3].Checked) { scoreValue = -1; isSelected = true; }
            else if (rbOptions[4].Checked) { scoreValue = -2; isSelected = true; }

            if (!isSelected)
            {
                MessageBox.Show("Будь ласка, оберіть один з варіантів відповіді.");
                return;
            }

            totalScore += scoreValue;
            currentIndex++;

            if (currentIndex < questions.Length)
            {
                LoadQuestion();
            }
            else
            {
                string interpretation = testService.InterpretResult(totalScore);
                storageService.SaveResult(currentUser, totalScore, interpretation);
                
                var resultForm = new ResultForm(currentUser, totalScore, interpretation);
                this.Hide();
                resultForm.Show();
            }
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e) => Environment.Exit(0);
    }

    // 3. ОКНО РЕЗУЛЬТАТОВ
    public class ResultForm : GradientForm
    {
        public ResultForm(User user, int totalScore, string interpretation)
        {
            this.Text = "Результати тестування";

            Panel mainPanel = new Panel { Width = 600, Height = 450, BackColor = Color.Transparent };

            Label lblTitle = new Label { Text = "Тестування завершено!", Top = 0, Left = 0, Width = 600, Height = 50, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 22, FontStyle.Bold), ForeColor = Color.Plum };
            Label lblUser = new Label { Text = $"Користувач: {user.LastName} {user.FirstName}", Top = 80, Left = 0, Width = 600, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 14), ForeColor = Color.White };
            Label lblScore = new Label { Text = $"Загальна сума балів: {totalScore}", Top = 140, Left = 0, Width = 600, Height = 35, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 16, FontStyle.Bold), ForeColor = Color.White };
            
            Label lblConcTitle = new Label { Text = "Висновок:", Top = 210, Left = 0, Width = 600, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 14, FontStyle.Bold), ForeColor = Color.Gold };
            Label lblInterpretation = new Label { Text = interpretation, Top = 250, Left = 0, Width = 600, Height = 50, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 20, FontStyle.Bold), ForeColor = Color.Gold };
            
            Label lblSaved = new Label { Text = "Результати збережено у файл data\\results.txt", Top = 340, Left = 0, Width = 600, Height = 30, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Arial", 10, FontStyle.Italic), ForeColor = Color.LightGray };

            Button btnFinish = new Button { Text = "Завершити", Top = 390, Left = 150, Width = 300, Height = 45, BackColor = Color.MediumSlateBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 12, FontStyle.Bold), Cursor = Cursors.Hand };
            btnFinish.FlatAppearance.BorderSize = 0;
            btnFinish.Click += (s, e) => Environment.Exit(0);

            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(lblUser);
            mainPanel.Controls.Add(lblScore);
            mainPanel.Controls.Add(lblConcTitle);
            mainPanel.Controls.Add(lblInterpretation);
            mainPanel.Controls.Add(lblSaved);
            mainPanel.Controls.Add(btnFinish);

            // ПРИВЯЗКА КЛАВИШИ ENTER К КНОПКЕ "ЗАВЕРШИТИ"
            this.AcceptButton = btnFinish;

            this.Controls.Add(mainPanel);

            CenterPanel(mainPanel);
            this.Resize += (s, e) => CenterPanel(mainPanel);
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e) => Environment.Exit(0);
    }
}