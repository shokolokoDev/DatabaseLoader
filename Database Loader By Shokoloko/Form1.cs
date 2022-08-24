﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Database_Loader_By_Shokoloko
{
    public partial class DatabaseLoader : Form
    {
        List<List<string>> Identities = new List<List<string>>();
        List<string> Keywords = new List<string>();
        string Seperator;
        Thread SearchProcess;
        public DatabaseLoader()
        {
            InitializeComponent();
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            if (Directory.GetFiles("data").Length == 0)
            {
                Console.WriteLine("Add databases to the data folder");
            }
            foreach (string file in Directory.GetFiles("data"))
            {
                this.DatabaseSelectMenu.Items.Add(file.Split(new char[] { '\\', '/' }, StringSplitOptions.None).Last());
            }
            if (this.DatabaseSelectMenu.Items.Count > 0)
            {
                this.DatabaseSelectMenu.SelectedIndex = 0;
            }
            this.Seperator = this.SeperatorTxtBox.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            this.Keywords = this.KeywordsTxtBox.Text.Split('\n').Select(x => x.Trim()).Where(x => x != String.Empty && x != Environment.NewLine).ToList();
            if (this.Keywords.Count == 0)
            {
                MessageBox.Show("You are not using any keywords, insert your keywords.", "Error", MessageBoxButtons.OK);
                return;
            }
            if (this.Identities.Count == 0)
            {
                MessageBox.Show("You didn't load any database.", "Error", MessageBoxButtons.OK);
                return;
            }
            this.SearchBtn.Enabled = false;
            this.StopSearchBtn.Enabled = true;
            this.SearchProcess = new Thread(new ThreadStart(() =>
            {
                List<List<string>> FoundIdentities;
                Thread SpinThread = new Thread(new ThreadStart(() =>
                {
                    var spin = new ConsoleSpinner();
                    Console.Write("Searching....");
                    while (true)
                    {
                        spin.Turn();
                    }
                }));
                SpinThread.Start();
                if(this.ExactResultsCheckBox.Checked)
                {
                    FoundIdentities = this.Identities.Where(x => this.Keywords.All(y => x.Any(z => z == y))).ToList();
                }
                else
                {
                    FoundIdentities = this.Identities.Where(x => this.Keywords.All(y => x.Any(z => z.Contains(y)))).ToList();
                }

                SpinThread.Abort();
                Console.SetCursorPosition(Console.WindowLeft, Console.CursorTop);
                Console.WriteLine($"Found {FoundIdentities.Count} results.");

                this.StopSearchBtn.Enabled = false;
                this.SearchBtn.Enabled = true;
                this.SearchProcess = null;

                if (FoundIdentities.Count == 0)
                {
                    MessageBox.Show("Found 0 matches.", "Error", MessageBoxButtons.OK);
                    return;
                }

                this.ResultsTxtBox.Text += String.Join(Environment.NewLine, FoundIdentities.Select(x => String.Join(", ", x))) + Environment.NewLine;
                

            }));
            this.SearchProcess.Start();
        }

        private void StopSearchBtn_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Stopping the search...");
            this.SearchProcess?.Abort();
            this.SearchProcess = null;
            this.StopSearchBtn.Enabled = false;
            this.SearchBtn.Enabled = true;
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            this.ResultsTxtBox.Text = String.Empty;
            this.ClearBtn.Enabled = false;
        }

        private void LoadDatabaseBtn_Click(object sender, EventArgs e)
        {
            if (DatabaseSelectMenu.Text == String.Empty)
            {
                MessageBox.Show("No database selected.", "Error", MessageBoxButtons.OK);
                return;
            }
            if (this.SeperatorTxtBox.Text == String.Empty)
            {
                this.SeperatorTxtBox.Text = ":";
            }
            
            this.Seperator = this.SeperatorTxtBox.Text;

            Console.WriteLine($"Loading database - {DatabaseSelectMenu.Text}");
            this.LoadDatabaseBtn.Enabled = false;
            this.SeperatorTxtBox.Enabled = false;
            new Thread(new ThreadStart(() =>
            {
            try
            {
                    Thread SpinThread = new Thread(new ThreadStart(() =>
                    {
                        var spin = new ConsoleSpinner();
                        Console.Write("Loading....");
                        while (true)
                        {
                            spin.Turn();
                        }
                    }));
                    SpinThread.Start();
                    this.Identities.Clear();
                    using (FileStream fs = File.Open($"data/{DatabaseSelectMenu.Text}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (BufferedStream bs = new BufferedStream(fs))
                    
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        this.Identities = sr.ReadToEnd().Split('\n').Select(x => x.Trim().Split(this.Seperator.ToCharArray()).Where(y => y != String.Empty && y != "\r").ToList()).ToList();
                    }
                    if (this.Identities.Count == 0)
                    {
                        Console.WriteLine("Load your database");
                    }
                    SpinThread.Abort();
                    Console.SetCursorPosition(Console.WindowLeft, Console.CursorTop);
                    Console.WriteLine($"Loaded database - {DatabaseSelectMenu.Text} with {this.Identities.Count} lines");
                    this.LoadDatabaseBtn.Enabled = true;
                    this.SeperatorTxtBox.Enabled = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            })).Start();
        }

        private void ReloadDatabasesBtn_Click(object sender, EventArgs e)
        {
            this.DatabaseSelectMenu.Items.Clear();
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
            }
            foreach (string file in Directory.GetFiles("data"))
            {
                this.DatabaseSelectMenu.Items.Add(file.Split(new char[] { '\\', '/' }, StringSplitOptions.None).Last());
            }
            if (this.DatabaseSelectMenu.Items.Count > 0)
            {
                this.DatabaseSelectMenu.SelectedIndex = 0;
            }
        }
        private void KeywordsTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                try
                {
                    Clipboard.SetText(Clipboard.GetText().Replace("\r", "").Replace("\n", Environment.NewLine));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void ResultsTxtBox_TextChanged(object sender, EventArgs e)
        {
            if (this.ResultsTxtBox.Text.Length > 0)
            {
                this.ClearBtn.Enabled = true;
            }
        }
    }
}
