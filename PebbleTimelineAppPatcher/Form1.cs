using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using Microsoft.Win32;
using System.Threading;

namespace PebbleTimelineAppPatcher
{
    public partial class MainUI : Form
    {
        public static System.Timers.Timer refreshUITimer = new System.Timers.Timer(100);

        public MainUI()
        {
            InitializeComponent();
            //Set the url default
            UrlPos.Text=(string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\RomanPort\PebbleTimelineAppPatcher", "defaultUrl", "");
        }

        private void button1_Click(object senderObj, EventArgs ee)
        {
            //This button will start the process.
            refreshUITimer.Elapsed += RefreshUI;
            refreshUITimer.Enabled = true;
            //Prompt for user file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\RomanPort\PebbleTimelineAppPatcher", "defaultPath", "c:\\");
            openFileDialog1.Filter = "Pebble PBW App Packages (*.pbw)|*.pbw|Batch Text Script (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Begin
                button1.Enabled = false;
                
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    if (openFileDialog1.FileName.ToLower().EndsWith("txt"))
                    {
                        //This is a batch request
                        string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                        Program.BeginBatch(lines, UrlPos.Text);
                    }
                    else
                    {
                        Program.BeginProcessing(openFileDialog1.FileName, UrlPos.Text); //Run in a new thread
                    }
                }).Start();
            }

                
        }

        public void RefreshUI(Object source, ElapsedEventArgs e)
        {
            
            try
            {
                this.Invoke((MethodInvoker)delegate {
                    LogTxt.Text = Program.bufferLog;
                    
                });
            } catch
            {

            }
        }

        public void SetButtonState(bool enabled)
        {
            this.Invoke((MethodInvoker)delegate {
                //Reset button
                button1.Enabled = enabled;

            });
        }
    }
}
