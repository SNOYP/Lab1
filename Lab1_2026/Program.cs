using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lab1_2026.Services;
using Lab1_2026.Models;

namespace Lab1_2026
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RegistrationForm());
        }
    }

    // --- БАЗОВА ФОРМА З ГРАДІЄНТОМ ---
    public class GradientForm : Form
    {
        public GradientForm()
        {
            this.DoubleBuffered = true;
            this.Resize += (s, e) => this.Invalidate(); // Перемальовуємо градієнт при зміні розміру
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.ClientRectangle.Width > 0 && this.ClientRectangle.Height > 0)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(15, 15, 20), Color.FromArgb(85, 20, 140), 45F))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }
    }

    // --- 1. ВІКНО РЕЄСТРАЦІЇ ---
    public class RegistrationForm : GradientForm
    {
        private Panel mainPanel;
        private TextBox txtLastName, txtFirstName, txtProfession;
        private ComboBox cmbGender;

        public RegistrationForm()
        {
            this.Text = "Лабораторна робота №1 - Реєстрація";
            this.Size = new Size(600, 600);
            this.MinimumSize = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Панель-контейнер, яка завжди буде по центру
            mainPanel = new Panel() { Size = new Size(420, 450), BackColor = Color.Transparent };
            
            var lblTitle = new Label() { Text = "Реєстрація респондента", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(420, 40), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            
            int currentY = 60;
            var lbl1 = CreateLabel("Прізвище:", currentY);
            txtLastName = CreateTextBox(currentY + 25);

            currentY += 70;
            var lbl2 = CreateLabel("Ім'я:", currentY);
            txtFirstName = CreateTextBox(currentY + 25);

            currentY += 70;
            var lbl3 = CreateLabel("Професія / Спеціальність:", currentY);
            txtProfession = CreateTextBox(currentY + 25);

            currentY += 70;
            var lbl4 = CreateLabel("Стать:", currentY);
            cmbGender = new ComboBox() { Location = new Point(0, currentY + 25), Size = new Size(420, 25), Font = new Font("Segoe UI", 11), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGender.Items.AddRange(new string[] { "Чоловіча", "Жіноча" });
            cmbGender.SelectedIndex = 0;

            var btnStart = new Button() { Text = "Розпочати тест", Location = new Point(100, currentY + 90), Size = new Size(220, 45), BackColor = Color.MediumSlateBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += BtnStart_Click;

            mainPanel.Controls.AddRange(new Control[] { lblTitle, lbl1, txtLastName, lbl2, txtFirstName, lbl3, txtProfession, lbl4, cmbGender, btnStart });
            this.Controls.Add(mainPanel);

            this.Resize += (s, e) => CenterPanel();
            CenterPanel();
        }

        private void CenterPanel()
        {
            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
        }

        private Label CreateLabel(string text, int top)
        {
            return new Label() { Text = text, Location = new Point(0, top), Size = new Size(420, 20), Font = new Font("Segoe UI", 10, FontStyle.Regular), ForeColor = Color.Thistle };
        }

        private TextBox CreateTextBox(int top)
        {
            return new TextBox() { Location = new Point(0, top), Size = new Size(420, 25), Font = new Font("Segoe UI", 11) };
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Будь ласка, заповніть прізвище та ім'я!", "Попередження", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = new User
            {
                LastName = txtLastName.Text.Trim(),
                FirstName = txtFirstName.Text.Trim(),
                Profession = txtProfession.Text.Trim(),
                Gender = cmbGender.SelectedItem?.ToString()
            };

            this.Hide();
            var testForm = new TestingForm(user);
            testForm.Closed += (s, args) => this.Close();
            testForm.Show();
        }
    }

    // --- 2. ВІКНО ТЕСТУВАННЯ ---
    public class TestingForm : GradientForm
    {
        private Panel mainPanel;
        private User currentUser;
        private TestService testService = new TestService();
        private StorageService storageService = new StorageService();
        private Question[] questions;
        private int currentIndex = 0;
        private int totalScore = 0;

        private Label lblProgress, lblQuestionText;
        private RadioButton[] rbOptions;

        public TestingForm(User user)
        {
            currentUser = user;
            questions = testService.GetShubertQuestions();

            this.Text = "Лабораторна робота №1 - Тестування (Варіант 11)";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(650, 500);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainPanel = new Panel() { Size = new Size(600, 450), BackColor = Color.Transparent };

            lblProgress = new Label() { Text = "", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(600, 30), ForeColor = Color.MediumSlateBlue, TextAlign = ContentAlignment.MiddleCenter };
            lblQuestionText = new Label() { Text = "", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 40), Size = new Size(600, 100), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };

            string[] optionTexts = {
                "2 - Повністю згоден, \"Так\"",
                "1 - Швидше \"Так\", ніж \"Ні\"",
                "0 - Важко сказати, ні \"Так\", ні \"Ні\"",
                "-1 - Швидше \"Ні\", ніж \"Так\"",
                "-2 - Повністю не згоден, \"Ні\""
            };

            rbOptions = new RadioButton[5];
            int topOffset = 160;
            for (int i = 0; i < 5; i++)
            {
                rbOptions[i] = new RadioButton() { Text = optionTexts[i], Location = new Point(50, topOffset), Size = new Size(500, 35), Font = new Font("Segoe UI", 12), ForeColor = Color.White, Cursor = Cursors.Hand };
                mainPanel.Controls.Add(rbOptions[i]);
                topOffset += 45;
            }

            var btnNext = new Button() { Text = "Наступне питання", Location = new Point(200, 390), Size = new Size(200, 45), BackColor = Color.MediumSlateBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += BtnNext_Click;

            mainPanel.Controls.AddRange(new Control[] { lblProgress, lblQuestionText, btnNext });
            this.Controls.Add(mainPanel);

            this.Resize += (s, e) => CenterPanel();
            CenterPanel();
            LoadQuestion();
        }

        private void CenterPanel()
        {
            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
        }

        private void LoadQuestion()
        {
            if (currentIndex < questions.Length)
            {
                lblProgress.Text = $"Питання {currentIndex + 1} з {questions.Length}";
                lblQuestionText.Text = questions[currentIndex].Text;
                rbOptions[0].Checked = true;
            }
        }

        private void BtnNext_Click(object? sender, EventArgs e)
        {
            int scoreValue = 2;
            if (rbOptions[1].Checked) scoreValue = 1;
            else if (rbOptions[2].Checked) scoreValue = 0;
            else if (rbOptions[3].Checked) scoreValue = -1;
            else if (rbOptions[4].Checked) scoreValue = -2;

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

                // Виклик нашого нового красивого вікна результатів
                var resultForm = new ResultForm(currentUser, totalScore, interpretation);
                this.Hide();
                resultForm.Closed += (s, args) => this.Close();
                resultForm.Show();
            }
        }
    }

    // --- 3. КРАСИВЕ ВІКНО РЕЗУЛЬТАТІВ ---
    public class ResultForm : GradientForm
    {
        private Panel mainPanel;

        public ResultForm(User user, int score, string interpretation)
        {
            this.Text = "Результати тестування";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainPanel = new Panel() { Size = new Size(450, 350), BackColor = Color.Transparent };

            var lblTitle = new Label() { Text = "Тестування завершено!", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(0, 0), Size = new Size(450, 40), ForeColor = Color.Violet, TextAlign = ContentAlignment.MiddleCenter };
            var lblUser = new Label() { Text = $"Користувач: {user.LastName} {user.FirstName}", Font = new Font("Segoe UI", 12), Location = new Point(0, 60), Size = new Size(450, 30), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            var lblScore = new Label() { Text = $"Загальна сума балів: {score}", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(0, 110), Size = new Size(450, 30), ForeColor = Color.LightSkyBlue, TextAlign = ContentAlignment.MiddleCenter };
            var lblResult = new Label() { Text = $"Висновок:\n{interpretation}", Font = new Font("Segoe UI", 16, FontStyle.Bold), Location = new Point(0, 160), Size = new Size(450, 70), ForeColor = Color.Gold, TextAlign = ContentAlignment.MiddleCenter };
            var lblSaved = new Label() { Text = "Результати збережено у файл data\\results.txt", Font = new Font("Segoe UI", 10, FontStyle.Italic), Location = new Point(0, 240), Size = new Size(450, 30), ForeColor = Color.Thistle, TextAlign = ContentAlignment.MiddleCenter };

            var btnClose = new Button() { Text = "Завершити", Location = new Point(125, 290), Size = new Size(200, 45), BackColor = Color.MediumSlateBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            mainPanel.Controls.AddRange(new Control[] { lblTitle, lblUser, lblScore, lblResult, lblSaved, btnClose });
            this.Controls.Add(mainPanel);

            this.Resize += (s, e) => CenterPanel();
            CenterPanel();
        }

        private void CenterPanel()
        {
            mainPanel.Left = (this.ClientSize.Width - mainPanel.Width) / 2;
            mainPanel.Top = (this.ClientSize.Height - mainPanel.Height) / 2;
        }
    }
}