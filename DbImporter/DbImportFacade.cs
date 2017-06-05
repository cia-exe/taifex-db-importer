using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using GeneralUtil;
namespace DbImporter.Future
{
    using Database;
    class DBImportFacade
    {
        DbImporter FuDbImporter = new DbImporter();

        public void RequestStop()
        {
            FuDbImporter.requestStop();
        }

        public bool IsBusy()
        {
            return FuDbImporter.IsBusy();
        }

        public void import(Action taskComplete)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            openFileDialog1.Filter = "CSV Files|*.csv";
            //openFileDialog1.FilterIndex = 2 ;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                //WinForm may has problem with WPF. debuger does not break at unhandled exception line after show WinForm Dialog.

                // Dictionary<int, int> tt = new Dictionary<int, int>(); int ttt = tt[100]; //for test unhandled exception.
                //FuDbImporter.Connect();
                FuDbImporter.ImportCSV(openFileDialog1.FileName, taskComplete);
            }

        }

        Thread thread;
        public void AutoImportAll(Action taskComplete)
        {
            if (thread != null && thread.IsAlive) return;

            string dir = GeneralUtil.Helper.getRootDirPath("SOF") + @"data.raw/";
            bool bShowWarning=false;
            thread = new Thread(() =>
            {
                try
                {
                    if(FuDbImporter.ImportCSV(dir + "1.csv")<0) bShowWarning=true;
                    TxtLog.showLog("****************************************");
                    if(FuDbImporter.ImportCSV(dir + "2.csv")<0)  bShowWarning=true;
                    TxtLog.showLog("****************************************");
                    if(FuDbImporter.ImportCSV(dir + "3.csv")<0) bShowWarning=true;
                    TxtLog.showLog("****************************************");
                }
                catch (Exception e)
                {
                    TxtLog.report(e.ToString());
                    bShowWarning = true;
                }

                if(bShowWarning)
                    System.Windows.MessageBox.Show("Something may be wrong, please check log.", "Warning");

                if (taskComplete != null) taskComplete();
                thread = null; // should release at final execution
            });

            thread.Start();

        }


    }


}
