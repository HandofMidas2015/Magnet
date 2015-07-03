using System;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace data2C3
{
    public partial class Form1 : Form
    {
        double vx = 0, vy = 0, vz = 0;
        double sx = 0, sy = 0, sz = 0;
        int i = 0;

        string[] sp ;
        string[] f ;
        string[] x ;
        string[] y ;
        string[] z ;

        //startposision
        double endposision_x = 500;
        double endposision_y = 500;

        private static SerialPort _serialPort = null;

        // We need to use unmanaged code
        [DllImport("user32.dll")]
        // GetCursorPos() makes everything possible
        static extern bool GetCursorPos(ref Point lpPoint);
        // Variable we will need to count the traveled pixels
        static protected long totalPixels = 0;
        static protected double currX;
        static protected double currY;
        static protected double diffX;
        static protected double diffY;


        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        // SetCursorPos() makes everything possible
        private static extern bool SetCursorPos(int x, int y);

        //将枚举作为位域处理
        [Flags]
        enum MouseEventFlag : uint //设置鼠标动作的键值
        {
            Move = 0x0001,               //发生移动
            LeftDown = 0x0002,           //鼠标按下左键
            LeftUp = 0x0004,             //鼠标松开左键
            RightDown = 0x0008,          //鼠标按下右键
            RightUp = 0x0010,            //鼠标松开右键
            MiddleDown = 0x0020,         //鼠标按下中键
            MiddleUp = 0x0040,           //鼠标松开中键
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,              //鼠标轮被移动
            VirtualDesk = 0x4000,        //虚拟桌面
            Absolute = 0x8000
        }

        //设置鼠标按键和动作
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo); //UIntPtr指针多句柄类型



        public Form1()
        {
            InitializeComponent();
        }


        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string tmp = _serialPort.ReadLine();
            string tmp1 = _serialPort.ReadLine();
            this.Invoke(new EventHandler(DoUpdate));
            
        }
        private void DoUpdate(object s, EventArgs e)
        {
            i++;
            //byte[] buf = new byte[30];
            try
            {
                //this.lbText.Text = _serialPort.ReadLine();
                this.wjText.Text = _serialPort.ReadLine();
                string data = _serialPort.ReadLine();
                 sp = data.Split(',');
                 if (sp.Length < 4)
                 {
                     return;
                 }
                f = sp[0].Split('=');
                x = sp[1].Split('=');
                 y = sp[2].Split('=');
                 z = sp[3].Split('=');
            }
            catch(FormatException)
            {
                Console.Write("error");
                x[1] = "0";
                y[1] = "0";
                z[1] = "0";
            }
            //Math.Round();
            
            int fl = int.Parse(f[1]);
            double x1 = double.Parse(x[1]) * 9.8;
            double y1 = double.Parse(y[1]) * 9.8;
            double z1 = double.Parse(z[1]) * 9.8;
           
            if (x1 <= 0.25 & x1 >= -0.25 )
            {
                x1 = 0; 
            }

            if(y1 <= 0.25 & y1 >= -0.25)
            {  
                y1 =0;
            }

            if (z1 <= 9.9 & z1 >= -9.9)
            {
                z1=0;
            }

            vx += x1 * 0.03;
            vy += y1 * 0.03;
            vz += z1 * 0.03;

            sx += vx * 0.03;
            sy += vy * 0.03;
            sz += vz * 0.03;
            

            endposision_x = endposision_x + sx;
            endposision_y = endposision_y + sy;


            // New point that will be updated by the function with the current coordinates
            //Point defPnt = new Point();
            //GetCursorPos(ref defPnt);

            //endposision_x = defPnt.X + sx;
            //endposision_y = defPnt.Y + sy;

            // posision range setting
            if (endposision_x < 0)
            {
                endposision_x = 0;
            }
            if (endposision_x > 1600)
            {
                endposision_x = 1600;
            }
            if (endposision_y < 0)
            {
                endposision_y = 0;
            }
            if (endposision_y > 900)
            {
                endposision_y = 900;
            }

            SetCursorPos(Convert.ToInt32(endposision_x), Convert.ToInt32(endposision_y));

            //SetCursorPos(1000, 700);

            if (fl == 1)
            {
                mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
            }
            if(fl == 0)
            {
                mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
            }
            if(fl == 2)
            {
                SendKeys.SendWait("{Left}");

            }

           // this.wjText.Text = Convert.ToString(fl + "\n" + endposision_x + "\n" + endposision_y + "\n" + sx + "\n" + sy + "\n" + defPnt.X + "\n" + defPnt.Y);
            //this.wjText.Text = Convert.ToString(fl + "\n" + endposision_x + "\n" + endposision_y + "\n" + sx + "\n" + sy );

            
            if (i == 200)
            {
                vx = 0;
                vy = 0;
                vz = 0;
                sx = 0;
                sy = 0;
                sz = 0;
                endposision_x = 500;
                endposision_y = 500;
                i = 0;
            }
            
        }

        //

        private void btnStart_Click(object sender, EventArgs e)
        {
            _serialPort = new SerialPort("COM6", 115200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            _serialPort.Open();
            //string data = _serialPort.ReadLine();

            this.btnStart.Enabled = false;
        }
    }
}
