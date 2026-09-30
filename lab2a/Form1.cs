using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace lab2a
{
    public partial class Form1 : Form
    {
        private RichTextBox txtCode;
        private DataGridView gridResults;
        private Button btnLoad;
        private Button btnAnalyze;

        public Form1()
        {
            InitializeComponentCustom();
        }

        // Пустой обработчик загрузки для устранения ошибки дизайнера
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Анализатор метрик Джилба";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Кнопка загрузки файла
            btnLoad = new Button
            {
                Text = "Загрузить .js файл",
                Location = new Point(12, 12),
                Size = new Size(150, 35)
            };
            btnLoad.Click += BtnLoad_Click;

            // Кнопка расчета
            btnAnalyze = new Button
            {
                Text = "Рассчитать метрики",
                Location = new Point(168, 12),
                Size = new Size(150, 35)
            };
            btnAnalyze.Click += BtnAnalyze_Click;

            // Текстовое поле для JS-кода
            txtCode = new RichTextBox
            {
                Location = new Point(12, 55),
                Size = new Size(860, 380),
                Font = new Font("Consolas", 10f),
                WordWrap = false
            };

            // Таблица результатов
            gridResults = new DataGridView
            {
                Location = new Point(12, 445),
                Size = new Size(860, 150),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White
            };

            gridResults.Columns.Add("MetricName", "Наименование метрики");
            gridResults.Columns.Add("Designation", "Обозначение");
            gridResults.Columns.Add("Value", "Значение");

            this.Controls.Add(btnLoad);
            this.Controls.Add(btnAnalyze);
            this.Controls.Add(txtCode);
            this.Controls.Add(gridResults);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "JavaScript files (*.js)|*.js|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtCode.Text = File.ReadAllText(dialog.FileName);
                }
            }
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            string rawCode = txtCode.Text;
            if (string.IsNullOrWhiteSpace(rawCode))
            {
                MessageBox.Show("Пожалуйста, загрузите или вставьте код на JavaScript!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1. Очистка от комментариев и строковых литералов
            string cleanCode = CleanCode(rawCode);

            // 2. Расчет Absolute Complexity (CL)
            int cl = CalculateCL(cleanCode);

            // 3. Расчет общего числа операторов (N)
            int n = CalculateTotalOperators(cleanCode, cl);

            // 4. Относительная сложность (cl = CL / N)
            double relativeCl = n > 0 ? Math.Round((double)cl / n, 4) : 0;

            // 5. Максимальная глубина вложенности (CLI)
            int cli = CalculateCLI(cleanCode);

            // Вывод в таблицу
            gridResults.Rows.Clear();
            gridResults.Rows.Add("Абсолютная сложность", "CL", cl);
            gridResults.Rows.Add("Общее количество операторов", "N", n);
            gridResults.Rows.Add("Относительная сложность", "cl", relativeCl.ToString("F4"));
            gridResults.Rows.Add("Максимальный уровень вложенности", "CLI", cli);
        }

        private string CleanCode(string code)
        {
            code = Regex.Replace(code, @"//.*", "");
            code = Regex.Replace(code, @"/\*[\s\S]*?\*/", "");
            code = Regex.Replace(code, @"("".*?""|'.*?'|`.*?`)", "\"\"");
            return code;
        }

        private int CalculateCL(string code)
        {
            int ifCount = Regex.Matches(code, @"\bif\b").Count;
            int forCount = Regex.Matches(code, @"\bfor\b").Count;
            int doCount = Regex.Matches(code, @"\bdo\b").Count;

            int totalWhile = Regex.Matches(code, @"\bwhile\b").Count;
            int pureWhile = Math.Max(0, totalWhile - doCount);

            int caseCount = Regex.Matches(code, @"\bcase\b").Count;
            int ternaryCount = Regex.Matches(code, @"\?").Count;

            return ifCount + forCount + doCount + pureWhile + caseCount + ternaryCount;
        }

        private int CalculateTotalOperators(string code, int clValue)
        {
            var lines = code.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed == "{" || trimmed == "}")
                    continue;

                count++;
            }

            return Math.Max(count, clValue + 5);
        }

        private int CalculateCLI(string code)
        {
            string[] lines = code.Split('\n');
            int currentDepth = 0;
            int maxDepth = 0;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Игнорируем else if как отдельный уровень вложенности
                if (Regex.IsMatch(line, @"\b(if|for|while|do|switch)\b") && !line.Contains("else if"))
                {
                    currentDepth++;
                    if (currentDepth > maxDepth) maxDepth = currentDepth;
                }

                if (line.Contains("}"))
                {
                    currentDepth = Math.Max(0, currentDepth - 1);
                }
            }

            return maxDepth;
        }
    }
}