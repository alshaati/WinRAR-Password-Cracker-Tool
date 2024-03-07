using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace WinRAR_Password_Crack
{
    public partial class Form1 : Form
    {
        private Timer opacityTimer;
        private bool isCracking;
        private Task crackingTask;
        private const int transitionDuration = 2000;
        private const int timerInterval = 20;
        private double targetOpacity = 1.0;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();   
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Clicks == 1 && e.Y <= this.Height && e.Y >= 0)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            StartOpacityTransition();

        }
        private void StartOpacityTransition()
        {
            timer1 = new Timer();
            timer1.Interval = timerInterval;
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double opacityIncrement = timerInterval / (double)transitionDuration;
            Opacity += opacityIncrement;

            if (Opacity >= targetOpacity)
            {
                Opacity = targetOpacity;
                timer1.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "Archive Files (*.rar;*.zip;*.7z)|*.rar;*.zip;*.7z|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
            {
                MessageBox.Show("Normal Method Started.");
                await Task.Run(() => SimplePasswordCrack());
            }
            else if (!checkBox1.Checked && checkBox2.Checked && !checkBox3.Checked)
            {
                MessageBox.Show("Paralel Method Started");
                await Task.Run(() => ParallelPasswordCrack());
            }
            else if (!checkBox1.Checked && !checkBox2.Checked && checkBox3.Checked)
            {
                MessageBox.Show("List Method Started");
                await Task.Run(() => DictionaryPasswordCrack());
            }
            else if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked && checkBox4.Checked)
            {
                MessageBox.Show("Sequential Method Started");
                await Task.Run(() => SequentialMethod());
            }
            else if (checkBox1.Checked && checkBox2.Checked && !checkBox3.Checked)
            {
                MessageBox.Show("Please select only one cracking method.");
            }
            else if ((checkBox1.Checked && checkBox3.Checked) || (checkBox2.Checked && checkBox3.Checked))
            {
                MessageBox.Show("Please select only one cracking method.");
            }
            else if (checkBox1.Checked && checkBox2.Checked && checkBox3.Checked)
            {
                MessageBox.Show("Please select only one cracking method.");
            }
            else
            {
                MessageBox.Show("Please select a cracking method.");
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void SimplePasswordCrack()
        {
            isCracking = true;
            string fileName = textBox1.Text;
            string filePath = Path.GetDirectoryName(fileName);
            string password = "0";

            while (isCracking)
            {
                string destination = Path.Combine(Path.GetTempPath(), new Random().Next(1, 100000).ToString());
                Directory.CreateDirectory(destination);

                string command = $@"unrar e -inul -p{password} ""{fileName}"" ""{destination}""";
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(psi);
                process.WaitForExit();
                int result = process.ExitCode;

                if (result == 0)
                {
                    MessageBox.Show("Password found: " + password);
                    break;
                }

                password = (int.Parse(password) + 1).ToString();
                Directory.Delete(destination, true);
            }
        }
        private void ParallelPasswordCrack()
        {
            isCracking = true;
            string fileName = textBox1.Text;
            string filePath = Path.GetDirectoryName(fileName);

            Parallel.For(0, 9999, (i, state) =>
            {
                if (isCracking) 
                {
                    string password = i.ToString("D4");
                    if (UnrarFile(fileName, filePath, password))
                    {
                        MessageBox.Show("Password found: " + password);
                        state.Stop();
                    }
                    else
                    {
                        Console.WriteLine("Tried password: " + password);
                    }
                }
                else
                {
                    state.Stop();
                }
            });

            MessageBox.Show("Password cracking finished.");
        }

        
        private bool UnrarFile(string rarFilePath, string destinationPath, string password)
        {
            if (!isCracking) 
                return false;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "unrar.exe",
                Arguments = $"e -p{password} \"{rarFilePath}\" \"{destinationPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                isCracking = false;
                return true; // Password found
            }

            return false; // Password incorrect
        }

        private void SequentialMethod()
        {
            isCracking = true;
            string fileName = textBox1.Text;
            string filePath = Path.GetDirectoryName(fileName);
            int passwordNumber = 1;

            while (isCracking)
            {
                string password = passwordNumber.ToString();
                string destination = Path.Combine(Path.GetTempPath(), new Random().Next(1, 100000).ToString());
                Directory.CreateDirectory(destination);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "unrar",
                    Arguments = $"e -inul -p{password} \"{Path.Combine(filePath, fileName)}\" \"{destination}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = Process.Start(startInfo);
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    MessageBox.Show("PASSWORD FOUND!\nFILE = " + fileName + "\nPASSWORD = " + password);
                    break;
                }

                passwordNumber++;

                Directory.Delete(destination, true);
            }
        }


        private string GenerateNextPassword(string currentPassword)
        {
            int nextPassword = int.Parse(currentPassword) + 1;
            return nextPassword.ToString();
        }

        private void DictionaryPasswordCrack()
        {
            isCracking = true;
            string fileName = textBox1.Text;
            string filePath = Path.GetDirectoryName(fileName);

            string[] passwords = File.ReadAllLines("passwords.txt");

            foreach (string password in passwords)
            {
                if (isCracking)
                {
                    if (UnrarFile(fileName, filePath, password))
                    {
                        MessageBox.Show("Password found: " + password);
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Tried password: " + password);
                    }
                }
                else
                {
                    break;
                }
            }

            MessageBox.Show("Password cracking finished.");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isCracking = false;
            if (crackingTask != null && !crackingTask.IsCompleted)
            {
                crackingTask.Wait();
            }
            MessageBox.Show("Cracking process stopped.");
        }
    }
}