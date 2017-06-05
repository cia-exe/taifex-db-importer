using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GeneralUtil
{

    public partial class DropDownCheckedList : CheckedListBox
    {
        //protected System.Collections.ArrayList arrColor = new System.Collections.ArrayList();
        protected List<Color> arrColor = new List<Color>();
        public DropDownCheckedList()
        {
            InitializeComponent();
            delePickColor = new MethodInvoker(pickColor);
            //this.SetStyle(ControlStyles.UserPaint, true);

            double defaultColor = 50; // after filter,  "arrColor.cont" may not equal defaultcolor.

            double brightThreshold = 0.8;  // filter out brighter color
            defaultColor /=0.75; // try and error value.

            double step = 255.0 / (Math.Pow(defaultColor, 1.0 / 3.0) - 1);

            for (double r = 0; r < 255.0; r += step)
                for (double g = 0; g < 255.0; g += step)
                    for (double b = 0; b < 255.0; b += step)
                        if ((r + g + b) < (255.0 * 3.0 * brightThreshold))
                            arrColor.Add(Color.FromArgb((int)(r+0.5),(int)(g+0.5),(int)(b+0.5)));

            // randomize sequence
            Random rand=new Random();
            int numColor = arrColor.Count;
            for (int k = 0; k < 13; k++)
                for (int i = 0; i < numColor; i++)
                {
                    int t=rand.Next(numColor);
                    var c = arrColor[i];
                    arrColor[i] = arrColor[t];
                    arrColor[t] = c;
                }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            //System.Diagnostics.Debug.Write(e+"OnpaintXXX\n");
            Color cBg = (Color)arrColor[e.Index];
            Color cFont = Color.FromArgb(255 - cBg.R, 255 - cBg.G, 255 - cBg.B);
            base.OnDrawItem(new DrawItemEventArgs(e.Graphics, e.Font, e.Bounds, e.Index, e.State, cFont,cBg));
            //base.OnDrawItem(e);
            //e.Graphics.DrawRectangle(Pens.Azure, e.Bounds.X,e.Bounds.Y,e.Bounds.Height,e.Bounds.Height);
            //e.Graphics.DrawRectangle(new Pen(Color.BlueViolet,2), e.Bounds);
        }

        /*
        protected override void OnPaint(PaintEventArgs pe)
        {
            //System.Diagnostics.Debug.Write("OnpaintXXX\n");
            //pe.Graphics.FillRectangle(Brushes.Black, this.ClientRectangle);
            base.OnPaint(pe);
        }*/

        private const int
            WM_PAINT = 0x000F,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205, 
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CAPTURECHANGED = 0x0215 // notify when click a check box or drag a scrollbar then release.
        ;


        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            
            int msg = m.Msg;

            //if (msg == WM_PAINT || msg == WM_PAINT || msg == WM_RBUTTONUP)
                //System.Diagnostics.Debug.Write(string.Format("msg={0:X} WParam={1:X} LParam={2:X}\n", msg,(int)m.WParam,(int)m.LParam));
             
            if (isShrunk())
            {
                if (msg == WM_MOUSEMOVE || msg == WM_NCMOUSEMOVE) dropDown();
            }
            else if(msg==WM_MOUSELEAVE || msg == WM_NCMOUSELEAVE || msg==WM_CAPTURECHANGED)
            {
                // 1. It is possible when WM_MOUSELEAVE occour but cursor still in control bounds.
                // WM_MOUSELEAVE just notify leaving client area.
                // 2. It is possible when WM_NCMOUSELEAVE occour but cursor move into client area.
                // e.g. from scrollbar to client area.
                // 3. When drag a scrollbar, a WM_NCMOUSELEAVE will occour.
                // when release mouse botton, a WM_CAPTURECHANGED will occour but cursor is possible in bounds or not.
                
                //System.Diagnostics.Debug.Write(System.Environment.TickCount+ "test Leave\n");
                if (!Bounds.Contains(Parent.PointToClient(Control.MousePosition))) shrinkBack();
            }

            if (msg == WM_RBUTTONDOWN && this.SelectedIndex>=0)
            {
                shrinkBack();
                this.BeginInvoke(delePickColor);
            }

             base.WndProc(ref m);
        }

        int orgHeight = 0;
        public bool isShrunk()
        {
            return orgHeight == 0;
        }

        protected void shrinkBack()
        {
            this.Height = orgHeight;
            orgHeight = 0;
            this.TopIndex = this.SelectedIndex; // scroll to selected item.
            this.SelectedIndex = -1; // clear selected highlight
        }

        protected void dropDown()
        {
            //System.Diagnostics.Debug.Write("Enter\n");
            //this.SelectedIndex = this.TopIndex;
            orgHeight = this.Height;
            this.Height = (Items.Count>0?Items.Count:1) * ItemHeight + Height - ItemHeight;
        }

        protected ColorDialog colorDialog = new ColorDialog();
        protected MethodInvoker delePickColor;
        private void pickColor()
        {
            colorDialog.ShowDialog();
        }

        public Color GetItemColor(Object item)
        {
            int idx = Items.IndexOf(item);
            if(idx<0) return Color.Empty;
            return (Color)arrColor[idx];
        }
        
    }


#if false
    public partial class CustomControl1 : CheckedListBox
    {
        public CustomControl1()
        {
            InitializeComponent();
            this.delegOnLeave = new MethodInvoker(OnLeave);
            leaveTimer =  new System.Threading.Timer(new System.Threading.TimerCallback(TimerProc));

        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void TimerProc(object state)
        {
            // The state object is the Timer object.
            //((System.Threading.Timer)state).Dispose();
            System.Diagnostics.Debug.Write("timerProc"+ bEnter+"\n");
            if (!bEnter)
            {
                this.Invoke(this.delegOnLeave);
            }
        }
        protected System.Threading.Timer leaveTimer;

        /*
        public PreProcessControlState PreProcessControlMessage(ref Message msg)
        {
            return base.PreProcessControlMessage(msg);
        }*/

        /*
        public override bool PreProcessMessage(ref Message msg)
        {
            return base.PreProcessMessage(ref msg);
        }
        */

        private const int
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_MOUSEACTIVATE = 0x0021,
            WM_NCHITTEST = 0x0084,
            WM_CAPTURECHANGED = 0x0215
        ;


        bool bEnter = false;
        bool bClickScrollBar=false;
        //long timeEnterLeave = -System.Environment.TickCount; //enter >0, leave<0
        protected override void WndProc(ref Message m)
        {

            int msg=m.Msg;
            //if(
            //    msg==WM_MOUSEACTIVATE
                //|| msg==WM_MOUSEMOVE
                //|| msg==WM_NCMOUSEMOVE||
                //msg==WM_NCHITTEST
             //   )
            //System.Diagnostics.Debug.Write(string.Format("msg={0:X} WParam={1:X} LParam={2:X}\n", msg,(int)m.WParam,(int)m.LParam));

            System.Diagnostics.Debug.Write(this.Bounds.Contains(this.Parent.PointToClient(Control.MousePosition)).ToString()+"\n");

            if (bEnter)
            {
                if (msg == WM_MOUSELEAVE || msg == WM_NCMOUSELEAVE)
                {
                    System.Diagnostics.Debug.Write("WM_NCMOUSELEAVE\n");

                    if (bClickScrollBar) bClickScrollBar = false;
                    else
                    {
                        bEnter = false;
                        leaveTimer.Change(100, System.Threading.Timeout.Infinite);
                    }

                }
                else if (msg == WM_MOUSEACTIVATE)
                {
                    System.Diagnostics.Debug.Write(string.Format("**** mouseActive: lParam={0:X}\n",(int)m.LParam));
                    if( (0xffff& (int)m.LParam) == 0x0007) bClickScrollBar = true;
                }
                else if (msg == WM_CAPTURECHANGED)
                {
                    System.Diagnostics.Debug.Write(string.Format("x={0},y={1}\n",
                    Control.MousePosition.X,
                    Control.MousePosition.Y));
                }
            }
            else if (msg == WM_MOUSEMOVE || msg == WM_NCMOUSEMOVE)
            {
                bEnter = true;
                System.Diagnostics.Debug.Write("Enter0\n");
                //this.BeginInvoke(myDelegate1);
                OnEnter();
                System.Diagnostics.Debug.Write("Enter1\n");

            }

            /*
            if (timeEnterLeave>0)
            {
                if (msg == WM_NCMOUSELEAVE)
                {
                    //System.Diagnostics.Debug.Write("leave\n");
                    timeEnterLeave = -System.Environment.TickCount;
                }
                else if (orgHeight==0 && System.Environment.TickCount - timeEnterLeave > 1000)
                {
                    orgHeight = this.Height;
                    this.Height = this.Items.Count * this.ItemHeight;
                }
            }
            else if(timeEnterLeave<0)
            {
                if (msg == WM_MOUSEMOVE || msg == WM_NCMOUSEMOVE)
                {
                    //System.Diagnostics.Debug.Write("enter\n");
                    timeEnterLeave = System.Environment.TickCount;
                }
                else if (orgHeight!=0 && System.Environment.TickCount + timeEnterLeave > 1000)
                {
                    this.Height = orgHeight;
                    orgHeight = 0;
                }
            }
            */

                /*
            if (orgHeight == 0)
            {
                if (msg == WM_MOUSEMOVE || (msg==WM_NCMOUSEMOVE && 0x12!=(int)m.WParam ))
                {
                    System.Diagnostics.Debug.Write("Enter\n");
                    orgHeight = this.Height;
                    this.Height = this.Items.Count * this.ItemHeight;
                }
            }
            else if (msg == WM_NCMOUSEMOVE && 0x12 == (int)m.WParam)  // not be detected if mouse move fast at border......
            {
                System.Diagnostics.Debug.Write("Leave\n");
                this.Height = orgHeight;
                orgHeight = 0;
            }*/

            base.WndProc(ref m);
        }

        //delegate void myDelegate();
        MethodInvoker delegOnLeave;

        int orgHeight = 0;
        protected void OnEnter()
        {
            if (orgHeight == 0)
            {
                orgHeight = this.Height;
                this.Height = this.Items.Count * this.ItemHeight;
            }
        }

        protected void OnLeave()
        {
            if (orgHeight != 0)
            {
                this.Height = orgHeight;
                orgHeight = 0;
            }
        }


        /*
                protected void dropDown(bool sw)
                {
                    System.Diagnostics.Debug.Write("dropDown0\n");

                    if(sw)
                    {
                        if (orgHeight == 0)
                        {
                            orgHeight = this.Height;
                            this.Height = this.Items.Count * this.ItemHeight;
                        }
                    }
                    else if (orgHeight != 0)
                    {
                            this.Height = orgHeight;
                            orgHeight = 0;
                    }

                    System.Diagnostics.Debug.Write("dropDown1\n");

                }
         */

    }
#endif

}
