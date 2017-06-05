using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//using System.Windows.Threading;
using System.Threading;
using GeneralUtil;

namespace DbImporter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    using DbImporter.Future;
    public partial class Window1 : Window
    {
        DBImportFacade faced = new DBImportFacade();
        public Window1()
        {
            InitializeComponent();
            GeneralUtil.TxtLog.logBox = this.logTextBox;
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            if (btnImport.Content.Equals("Stop"))
            {
                faced.RequestStop();
            }
            else
            {
                //TxtLog.report(sender.ToString() + " : " + e);
                faced.import(()=>
                {
                    this.Dispatcher.BeginInvoke(new ThreadStart(() => this.btnImport.Content = "Import"));
                });

                this.btnImport.Content = "Stop";
            }
        }

        private void btnAutoImportAll_Click(object sender, RoutedEventArgs e)
        {
            if (btnImport.Content.Equals("Stop"))
            {
                faced.RequestStop();
            }
            else
            {
                //TxtLog.report(sender.ToString() + " : " + e);
                faced.AutoImportAll(() =>
                {
                    this.Dispatcher.BeginInvoke(new ThreadStart(() => this.btnImport.Content = "Auto All"));
                });

                this.btnImport.Content = "Stop";
            }

        }
    }
}
