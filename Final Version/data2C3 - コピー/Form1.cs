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
        //
        double vx = 0, vy = 0, vz = 0;
        double sx = 0, sy = 0, sz = 0;
        double[] a_x = { 0, 0, 0, 0 }, a_z = { 0, 0, 0, 0 };
        int i = 0;
        bool HardDownPressed;
        DateTime lastPress;

        int ch = 0;
        int pres = 0, Press_old = 0;
        
        

        //startposision
        double endposision_x;
        double endposision_y;

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

        public struct WorldCoords
        {
            public double x;
            public double y;
            public double z;
            public int p;
        }


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
        [DllImport("User32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        //public static extern void keybd_event(byte bVk, byte bScan, byte dwFlags, int dwExtraInfo);//键盘事件，上面是鼠标



        public Form1()
        {
            InitializeComponent();
        }


        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Invoke(new EventHandler(DoUpdate));
        }
        private void DoUpdate(object s, EventArgs e)
        {
            WorldCoords coords = new WorldCoords();
            //byte[] buf = new byte[30];
            i+=1;

            this.wjText.Text = _serialPort.ReadLine();
            string data = _serialPort.ReadLine();
            //this.wjText.Text = data;
            string[] sp = data.Split('\t');
            if (sp.Length<1)
            {
                return ;
            }
            //these might be unneccessary if one always follows the other, just fall in line once, then alternate aworld/euler
            if (String.Compare(sp[0], "aw") == 0)
                coords = ParseWorldCoord(sp);
            //if (String.Compare(sp[0], "euler") == 0)
            //    ParseEulerCoords();
            if (i>=70)
            {
                pres = coords.p;


                if (Press_old != pres)
                {
                    ch = 1;
                    Press_old = pres;
                }
                if(pres == 9)
                {
                    a_x[0] = a_x[1];
                    a_z[0] = a_z[1];
                    a_x[1] = a_x[2];
                    a_z[1] = a_z[2];
                    a_x[2] = a_x[3];
                    a_z[2] = a_z[3];

                    a_x[3] = coords.x * 9.8;
                    double y = coords.y * 9.8;
                    a_z[3] = coords.z * 9.8;

                    double x = (a_x[0] + a_x[1] + a_x[2] + a_x[3]) / 4;//-------------------------filter
                    double z = (a_z[0] + a_z[1] + a_z[2] + a_z[3]) / 4;
                    
                    vx -= x * 0.7;//----------------------------------積分
                    vy += y * 0.1;
                    vz -= z * 0.7;
                    //sx += vx * 0.1;
                    //sy += vy * 0.2;
                    //sz += vz * 0.1;

                    UpdateCursor(Convert.ToInt32(vx), Convert.ToInt32(vz));//必须是整型吗？？？？？？？？？？
                }

                if (pres == 7)
                {
                    if (!HardDownPressed)
                    {
                        HardDownPressed = true;
                    }
            //        lastPress = DateTime.Now;
                    //leftclick(1);
                    //return;
                    
                    a_x[0] = a_x[1];
                    a_z[0] = a_z[1];
                    a_x[1] = a_x[2];
                    a_z[1] = a_z[2];
                    a_x[2] = a_x[3];
                    a_z[2] = a_z[3];

                    a_x[3] = coords.x * 9.8;
                    double y = coords.y * 9.8;
                    a_z[3] = coords.z * 9.8;

                    double x = (a_x[0] + a_x[1] + a_x[2] + a_x[3]) / 4;
                    double z = (a_z[0] + a_z[1] + a_z[2] + a_z[3]) / 4;

                    vx -= x * 0.7;//----------------------------------積分
                    vy += y * 0.1;
                    vz -= z * 0.7;

                    UpdateCursor(Convert.ToInt32(vx), Convert.ToInt32(vz));//必须是整型吗？？？？？？？？？？
                    mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
  //                  mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                if ((pres != 7) && HardDownPressed)
                {
                    mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                }

                if(ch == 1)
                {
                    if (pres == 2)
                    {
                        mouse_event(MouseEventFlag.RightDown, 0, 0, 0, UIntPtr.Zero);//扩大
                        mouse_event(MouseEventFlag.RightUp, 0, 0, 0, UIntPtr.Zero); 
                        keybd_event((byte)Keys.Z, 0, 0, 0);
                        keybd_event((byte)Keys.Z, 0, 2, 0);
                        
                    }
                    if (pres == 1)
                    {
                        keybd_event((byte)Keys.Escape, 0, 0, 0);
                        keybd_event((byte)Keys.Escape, 0, 2, 0);//还原
                    }
                    if (pres == 3)
                    {
                        keybd_event((byte)Keys.Right, 0, 0, 0);
                        keybd_event((byte)Keys.Right, 0, 2, 0);//下页
                    }
                    if (pres == 4)
                    {
                        keybd_event((byte)Keys.Left, 0, 0, 0);
                        keybd_event((byte)Keys.Left, 0, 2, 0);//上页
                    }
                    if (pres == 8)
                    {
                        keybd_event((byte)Keys.ControlKey, 0, 0, 0);//コメント
                        keybd_event((byte)Keys.P, 0, 0, 0);
                        keybd_event((byte)Keys.ControlKey, 0, 2, 0);  
                        keybd_event((byte)Keys.P, 0, 2, 0);                     
                    }
                    if (pres == 0)//不按时积分清零
                    {

                        vx = 0;
                        sy = 0;
                        vz = 0;
                    }
                    ch = 0;
                }
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            _serialPort = new SerialPort("COM7", 57600, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.RequestToSendXOnXOff;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            _serialPort.Open();

            this.btnStart.Enabled = false;
        }
        private void UpdateCursor(int xdelta, int ydelta)
        {
            // New point that will be updated by the function with the current coordinates
            Point defPnt = new Point();
            GetCursorPos(ref defPnt);


            double posx = defPnt.X + xdelta;// +xdelta * 0;
            double posy = defPnt.Y + ydelta;// +ydelta * 0;

            // posision range setting
            if (endposision_x < 0)
            {
                endposision_x = 0;
            }
            if (endposision_x > 1920)
            {
                endposision_x = 1920;
            }
            if (endposision_y < 0)
            {
                endposision_y = 0;
            }
            if (endposision_y > 1080)
            {
                endposision_y = 1080;
            }
           // posx = 200 ;
           // posy = 200 ;
          //  SetCursorPos(Convert.ToInt32(posx), Convert.ToInt32(posy));
            this.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(Convert.ToInt32(posx), Convert.ToInt32(posy));
        }
        WorldCoords ParseWorldCoord(string[] indata)
        {
            WorldCoords dummy = new WorldCoords();
            if (indata.Length < 4)
                return dummy;
            dummy.x = Convert.ToDouble(indata[1])/3900d ;/// 16384;
            dummy.y = Convert.ToDouble(indata[2]) / 3900d;// / 16384;
            string[] modi = indata[3].Split('\r');
            dummy.z = Convert.ToDouble(indata[3]) / 3900d;
            dummy.p = Convert.ToInt32(indata[4]);
            return dummy;
        }
    }
}
