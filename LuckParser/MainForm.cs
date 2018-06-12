﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LuckParser.Controllers;

public delegate void Logger(int i, string msg, int prog);
public delegate void Cancellation(int i, DoWorkEventArgs e);

namespace LuckParser
{
    public partial class MainForm : Form
    {

        BackgroundWorker m_oWorker;
        private SettingsForm setFrm;
        //public bool[] settingArray = { true, true, true, true, true, false, true, true };
        bool completedOp = false;
        List<string> _logsFiles;
        Controller1 controller = new Controller1();
        public MainForm()
        {
            InitializeComponent();

            _logsFiles = new List<string>();

            btnCancel.Enabled = false;
            btnParse.Enabled = false;
            m_oWorker = new BackgroundWorker();

            // Create a background worker thread that ReportsProgress &
            // SupportsCancellation
            // Hook up the appropriate events.
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.ProgressChanged += new ProgressChangedEventHandler
                    (m_oWorker_ProgressChanged);
            m_oWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker_RunWorkerCompleted);
            m_oWorker.WorkerReportsProgress = true;
            m_oWorker.WorkerSupportsCancellation = true;


        }

        public MainForm(string[] args)
        {
            InitializeComponent();
            listView1_AddItems(args);
            m_DoWork(log_Console, null, null);
        }

        // On completed do the appropriate task

        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lblStatusValue.Text = "Task Cancelled.";
            }
            // Check to see if an error occurred in the background process.
            else if (e.Error != null)
            {
                lblStatusValue.Text = "Error while performing background operation.";
            }
            else
            {
                // Everything completed normally.
                lblStatusValue.Text = "Task Completed";
                // Flash window until it recieves focus
                FlashWindow.Flash(this);
            }

            //Change the status of the buttons on the UI accordingly
            btnParse.Enabled = true;
            btnCancel.Enabled = false;
        }

        // Notification is performed here to the progress bar
        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pgbProgress.Value = e.ProgressPercentage;
            string[] progstr = (string[])e.UserState;
            if (progstr[1] == "Cancel")
            {
                _logsFiles.Clear();
                //listView1.Items.Clear();
                completedOp = true;
                btnClear.Enabled = true;
                lblStatusValue.Text = "Canceled Parsing";
            }
            else if (progstr[1] == "Finish")
            {
                _logsFiles.Clear();
                //listView1.Items.Clear();
                completedOp = true;
                btnClear.Enabled = true;
                btnParse.Enabled = false;
                lblStatusValue.Text = "Finished Parsing";
            }
            else
            {
                lblStatusValue.Text = "Parsing...";
                lvFileList.Items[Int32.Parse(progstr[0])].SubItems[1].Text = progstr[1];
            }
        }

        private void log_Console(int i, string msg, int prog)
        {
            Console.WriteLine(msg);
        }

        private void log_Worker(int i, string msg, int prog)
        {
            string[] reportObject = { i.ToString(), msg };
            m_oWorker.ReportProgress(prog, reportObject);
        }

        private void cancel_Worker(int i, DoWorkEventArgs e)
        {
            if (m_oWorker.CancellationPending)
            {
                // Set the e.Cancel flag so that the WorkerCompleted event
                // knows that the process was cancelled.
                e.Cancel = true;
                string[] reportObject = new string[] { i.ToString(), "Cancel" };
                m_oWorker.ReportProgress(0, reportObject);
                btnParse.Enabled = false;
                return;
            }
        }

        /// Time consuming operations go here </br>
        /// i.e. Database operations,Reporting
        void m_DoWork(Logger logger, Cancellation cancel, DoWorkEventArgs e)
        {
            //globalization
            System.Globalization.CultureInfo before = System.Threading.Thread.CurrentThread.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture =
                    new System.Globalization.CultureInfo("en-US");
                // Proceed with specific code

                bool[] settingsSnap = new bool[] {
                    Properties.Settings.Default.DPSGraphTotals,
                    Properties.Settings.Default.PlayerGraphTotals,
                    Properties.Settings.Default.PlayerGraphBoss,
                    Properties.Settings.Default.PlayerBoonsUniversal,
                    Properties.Settings.Default.PlayerBoonsImpProf,
                    Properties.Settings.Default.PlayerBoonsAllProf,
                    Properties.Settings.Default.PlayerRot,
                    Properties.Settings.Default.PlayerRotIcons,
                    Properties.Settings.Default.EventList,
                    Properties.Settings.Default.BossSummary,
                    Properties.Settings.Default.SimpleRotation,
                    Properties.Settings.Default.ShowAutos,
                    Properties.Settings.Default.LargeRotIcons,
                    Properties.Settings.Default.ShowEstimates
                };
                for (int i = 0; i < lvFileList.Items.Count; i++)
                {
                    FileInfo fInfo = new FileInfo(_logsFiles[i]);
                    if (!fInfo.Exists)
                    {
                        logger(i, "File does not exist", 100);
                        continue;
                    }

                    logger(i, "Working...", 20);
                    Controller1 control = new Controller1();

                    if (fInfo.Extension.Equals(".evtc", StringComparison.OrdinalIgnoreCase) || 
                        fInfo.Name.EndsWith(".evtc.zip", StringComparison.OrdinalIgnoreCase))
                    {
                        //Process evtc here
                        logger(i, "Reading Binary...", 40);
                        control.ParseLog(fInfo.FullName);

                        //Creating File
                        //save location
                        DirectoryInfo saveDirectory;
                        if (Properties.Settings.Default.SaveAtOut || Properties.Settings.Default.OutLocation == null)
                        {
                            //Default save directory
                            saveDirectory = fInfo.Directory;
                        }
                        else
                        {
                            //Customised save directory
                            saveDirectory = new DirectoryInfo(Properties.Settings.Default.OutLocation);
                        }

                        string bossid = control.getBossData().getID().ToString();
                        string result = "fail";

                        if (control.getLogData().getBosskill())
                        {
                            result = "kill";
                        }

                        string outputType = Properties.Settings.Default.SaveOutHTML ? "html" : "csv";
                        string outputFile = Path.Combine(
                            saveDirectory.FullName, 
                            $"{fInfo.Name}_{control.GetLink(bossid + "-ext")}_{result}.{outputType}"
                        );

                        logger(i, "Creating File...", 60);
                        using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                        {
                            logger(i, $"Writing {outputType.ToUpper()}...", 80);
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                if (Properties.Settings.Default.SaveOutHTML)
                                {
                                    control.CreateHTML(sw, settingsSnap);
                                }
                                else
                                {
                                    control.CreateCSV(sw, ",");
                                }
                            }
                        }

                        logger(i, $"{outputType.ToUpper()} Generated!", 100); //keeping here for console usage
                        logger(i, outputFile, 100);
                    }
                    else
                    {
                        logger(i, "Not EVTC...", 100);
                    }

                    cancel?.Invoke(i, e);
                }

                //Report 100% completion on operation completed
                logger(0, "Finish", 100);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = before;
            }
        }
        
        /// Worker job
        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            m_DoWork(log_Worker, cancel_Worker, e);
        }

        private void btnStartAsyncOperation_Click(object sender, EventArgs e)
        {
            //Change the status of the buttons on the UI accordingly
            //The start button is disabled as soon as the background operation is started
            //The Cancel button is enabled so that the user can stop the operation 
            //at any point of time during the execution
            if (_logsFiles.Count > 0)
            {
                btnParse.Enabled = false;
                btnCancel.Enabled = true;
                btnClear.Enabled = false;

                m_oWorker.RunWorkerAsync();
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (m_oWorker.IsBusy)
            {
                // Notify the worker thread that a cancel has been requested.
                // The cancel will not actually happen until the thread in the
                // DoWork checks the m_oWorker.CancellationPending flag. 

                m_oWorker.CancelAsync();
            }
        }

        private void listView1_AddItems(string[] filesArray)
        {
            foreach (string file in filesArray)
            {
                if (_logsFiles.Contains(file))
                {
                    //Don't add doubles
                    continue;
                }
                _logsFiles.Add(file);

                ListViewItem lvItem = new ListViewItem(file);
                lvItem.SubItems.Add(" ");
                lvFileList.Items.Add(lvItem);
            }
        }
       
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            btnParse.Enabled = true;
            if (completedOp)
            {
                lvFileList.Items.Clear();
                completedOp = false;
            }
            //Get files as list
            string[] filesArray = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            listView1_AddItems(filesArray);
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Yellow, e.Bounds);
            e.DrawText();
        }

        private void settingsbtn_Click(object sender, EventArgs e)
        {
            setFrm = new SettingsForm(/*settingArray,this*/);
            setFrm.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            btnParse.Enabled = false;
            _logsFiles.Clear();
            lvFileList.Items.Clear();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void listView1_MouseMove_1(object sender, MouseEventArgs e)
        {
            var hit = lvFileList.HitTest(e.Location);
            if (hit.SubItem != null && hit.SubItem == hit.Item.SubItems[1]) lvFileList.Cursor = Cursors.Hand;
            else lvFileList.Cursor = Cursors.Default;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = lvFileList.HitTest(e.Location);
            if (hit.SubItem != null && hit.SubItem == hit.Item.SubItems[1])
            {
                string fileLoc = hit.SubItem.Text;
                if (File.Exists(fileLoc))
                {
                    System.Diagnostics.Process.Start(fileLoc);
                }
                //var url = new Uri(hit.SubItem.Text);
                // etc..
            }
        }
    }
}
