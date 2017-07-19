using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfApplication1;
using System.Threading;
using System.Reflection;
using RoverGround;

namespace RoverGround.TouchControl
{
    public class TouchControl
    {
        public TouchControl()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            engineCtlForm = new WpfApplication1.MainWindow(this);
            directCtrlForm = new WpfApplication1.Window1(this);
        }
        static readonly object syncObj = new object();

        public double Speed = 1.0;
        public double Linear = 0;
        public double Angular = 0;

        WpfApplication1.MainWindow engineCtlForm;
        WpfApplication1.Window1 directCtrlForm;
        public void PubMsg()
        {
            //RoverGround.MainWindow.Instance.pubCmdMsg(Linear, Angular, Speed);
        }
        public void ShowForm()
        {

            engineCtlForm.Topmost = true;
            engineCtlForm.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            engineCtlForm.Left = System.Windows.SystemParameters.PrimaryScreenWidth - engineCtlForm.Width-60;
            engineCtlForm.Top= (System.Windows.SystemParameters.PrimaryScreenHeight - engineCtlForm.Height) / 2;
            engineCtlForm.Show();

            directCtrlForm.Topmost = true;
            directCtrlForm.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            directCtrlForm.Left = 20;
            directCtrlForm.Top = (System.Windows.SystemParameters.PrimaryScreenHeight - directCtrlForm.Height) / 2;
            directCtrlForm.Show();
        }
        public void CloseForm()
        {
            engineCtlForm.Close();
            directCtrlForm.Close();
        }
    }

}
