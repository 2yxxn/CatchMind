using System.Collections.Generic;
using System.Drawing;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Net;

namespace PacketServer
{
    public partial class Form1 : Form
    {
        // Server 멤버 변수
        private NetworkStream m_networkstream;
        private TcpListener m_listener;

        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];

        private bool m_bClientOn = false;

        private Thread m_thread;

        public Answer m_answerClass;
        public Login m_loginClass;

        private List<string> suggestions = new List<string>();

        // 그림판 변수
        private bool line;
        private bool rect;
        private bool circle;
        private Point start;
        private Point finish;
        private Pen pen;
        private int nline;
        private int nrect;
        private int ncircle;
        private int i;
        private int thick;
        private bool isSolid;
        private MyLines[] mylines;
        private MyRect[] myrect;
        private MyCircle[] mycircle;

        public void Send()
        {
            this.m_networkstream.Write(this.sendBuffer, 0, this.sendBuffer.Length);
            this.m_networkstream.Flush();

            for (int i = 0; i < 1024 * 4; i++)
            {
                this.sendBuffer[i] = 0;
            }
        }

        public void RUN()
        {
            // 제시어
            suggestions.Add("배구공");
            suggestions.Add("안경");
            suggestions.Add("버스");
            suggestions.Add("비빔밥");
            suggestions.Add("강아지");
            suggestions.Add("거울");

            this.m_listener = new TcpListener(7777);
            this.m_listener.Start();

            if (!this.m_bClientOn)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    // 연결이 안됐을 경우
                    // ToolBar, 전송 Button, 그림판 Panel 비활성화
                    toolBar1.Enabled = false;
                    btnSend.Enabled = false;
                    panel1.Enabled = false;
                }));
            }

            TcpClient client = this.m_listener.AcceptTcpClient();

            if (client.Connected)
            {
                // 연결이 됐을 경우
                this.m_bClientOn = true;
                m_networkstream = client.GetStream();
            }

            int nRead = 0;
            while (this.m_bClientOn)
            {
                try
                {
                    nRead = 0;
                    nRead = this.m_networkstream.Read(readBuffer, 0, 1024 * 4);
                }
                catch
                {
                    this.m_bClientOn = false;
                    this.m_networkstream = null;
                }

                Packet packet = (Packet)Packet.Desserialize(this.readBuffer);

                switch ((int)packet.Type)
                {
                    case (int)PacketType.로그인:
                        {
                            this.m_loginClass = (Login)Packet.Desserialize(this.readBuffer);
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                this.txtBoxID.Text = this.m_loginClass.m_strID;
                            }));

                            toolBar1.Enabled = true;
                            btnSend.Enabled = true;
                            panel1.Enabled = true;

                            Random rand = new Random();
                            int number = rand.Next(suggestions.Count);
                            txtBoxSg.Text = suggestions[number];

                            Answer answer = new Answer();
                            answer.Type = (int)PacketType.정답;
                            answer.Data = txtBoxSg.Text;

                            Packet.Serialize(answer).CopyTo(this.sendBuffer, 0);
                            this.Send();

                            break;
                        }
                    case (int)PacketType.정답:
                        {
                            this.m_answerClass = (Answer)Packet.Desserialize(this.readBuffer);
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                if (this.m_answerClass.Data == txtBoxSg.Text)
                                {
                                    Random rand = new Random();
                                    int number = rand.Next(suggestions.Count);
                                    txtBoxSg.Text = suggestions[number];
                                }
                            }));

                            Answer answer = new Answer();
                            answer.Type = (int)PacketType.정답;
                            answer.Data = txtBoxSg.Text;

                            Packet.Serialize(answer).CopyTo(this.sendBuffer, 0);
                            this.Send();

                            break;
                        }

                }
            }
        }

        public Form1()
        {
            InitializeComponent();

            SetupVar();
        }

        private void SetupVar()
        {
            i = 0;
            thick = 1;
            isSolid = true;
            line = false;
            rect = false;
            circle = false;
            start = new Point(0, 0);
            finish = new Point(0, 0);
            pen = new Pen(Color.Black);
            mylines = new MyLines[100];
            myrect = new MyRect[100];
            mycircle = new MyCircle[100];
            nline = 0;
            nrect = 0;
            ncircle = 0;
            line0Button.Pushed = false;
            line1Button.Pushed = true;
            line2Button.Pushed = false;
            line3Button.Pushed = false;

            SetupMine(); // 저장 클래스 초기화
        }

        private void SetupMine()
        {
            for (i = 0; i < 100; i++)
                mylines[i] = new MyLines();

            for (i = 0; i < 100; i++)
                myrect[i] = new MyRect();

            for (i = 0; i < 100; i++)
                mycircle[i] = new MyCircle();
        }


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            start.X = e.X;
            start.Y = e.Y;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((start.X == 0) && (start.Y == 0))
                return;

            finish.X = e.X;
            finish.Y = e.Y;

            if (line == true)
            {
                mylines[nline].setPoint(start, finish, thick, isSolid);
            }

            if (rect == true)
            {
                myrect[nrect].setRect(start, finish, thick, isSolid);
            }

            if (circle == true)
            {
                mycircle[ncircle].setRect(start, finish, thick, isSolid);
            }

            panel1.Invalidate(true);
            panel1.Update();
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (line == true)
                nline++;
            if (rect == true)
                nrect++;
            if (circle == true)
                ncircle++;

            start.X = 0;
            start.Y = 0;
            finish.X = 0;
            finish.Y = 0;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 현재 저장된 선, 사각형, 원 그리기
            for (i = 0; i <= nline; i++)
            {
                if (!mylines[i].getSolid())
                {
                    pen.Width = 1;
                    pen.DashStyle = DashStyle.Dot;
                }
                else
                {
                    pen.Width = mylines[i].getThick();
                    pen.DashStyle = DashStyle.Solid;
                }

                e.Graphics.DrawLine(pen, mylines[i].getPoint(), mylines[i].getPoint2());
            }

            for (i = 0; i <= nrect; i++)
            {
                if (!myrect[i].getSolid())
                {
                    pen.Width = 1;
                    pen.DashStyle = DashStyle.Dot;
                }
                else
                {
                    pen.Width = myrect[i].getThick();
                    pen.DashStyle = DashStyle.Solid;
                }

                e.Graphics.DrawRectangle(pen, myrect[i].getRect());
            }

            for (i = 0; i <= ncircle; i++)
            {
                if (!mycircle[i].getSolid())
                {
                    pen.Width = 1;
                    pen.DashStyle = DashStyle.Dot;
                }
                else
                {
                    pen.Width = mycircle[i].getThick();
                    pen.DashStyle = DashStyle.Solid;
                }

                e.Graphics.DrawEllipse(pen, mycircle[i].getRectC());
            }

            pen.Width = thick;
            pen.DashStyle = DashStyle.Solid;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.m_thread = new Thread(new ThreadStart(RUN));
            this.m_thread.Start();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.m_listener.Stop();
            this.m_networkstream.Close();
            this.m_thread.Abort();
        }

        private void toolBar1_ButtonClick_1(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button == newButton)
            {
                this.lineButton.Pushed = false;
                this.rectButton.Pushed = false;
                this.circleButton.Pushed = false;

                SetupVar();
                panel1.Refresh();
            }

            if (e.Button == lineButton)
            {
                line = true;
                rect = false;
                circle = false;

                this.lineButton.Pushed = true;
                this.rectButton.Pushed = false;
                this.circleButton.Pushed = false;
            }

            if (e.Button == rectButton)
            {
                line = false;
                rect = true;
                circle = false;

                this.lineButton.Pushed = false;
                this.rectButton.Pushed = true;
                this.circleButton.Pushed = false;
            }

            if (e.Button == circleButton)
            {
                line = false;
                rect = false;
                circle = true;

                this.lineButton.Pushed = false;
                this.rectButton.Pushed = false;
                this.circleButton.Pushed = true;
            }

            if (e.Button == line0Button)
            {
                isSolid = false;
                thick = 1;

                this.line0Button.Pushed = true;
                this.line1Button.Pushed = false;
                this.line2Button.Pushed = false;
                this.line3Button.Pushed = false;
            }

            if (e.Button == line1Button)
            {
                isSolid = true;
                thick = 1;

                this.line0Button.Pushed = false;
                this.line1Button.Pushed = true;
                this.line2Button.Pushed = false;
                this.line3Button.Pushed = false;
            }

            if (e.Button == line2Button)
            {
                isSolid = false;
                thick = 3;

                this.line0Button.Pushed = false;
                this.line1Button.Pushed = false;
                this.line2Button.Pushed = true;
                this.line3Button.Pushed = false;
            }

            if (e.Button == line3Button)
            {
                isSolid = false;
                thick = 5;

                this.line0Button.Pushed = false;
                this.line1Button.Pushed = false;
                this.line2Button.Pushed = false;
                this.line3Button.Pushed = true;
            }
        }

        private void btnSend_Click_1(object sender, EventArgs e)
        {
            if (!this.m_bClientOn)
            {
                return;
            }

            for (i = 0; i <= nline; i++)
            {
                Line line = new Line();
                line.Type = (int)PacketType.선;
                line.point[0] = mylines[i].getPoint();
                line.point[1] = mylines[i].getPoint2();
                line.isSolid = mylines[i].getSolid();
                line.thick = mylines[i].getThick();

                Packet.Serialize(line).CopyTo(this.sendBuffer, 0);
                this.Send();
            }
            for (i = 0; i <= nrect; i++)
            {
                Rect rect = new Rect();
                rect.Type = (int)PacketType.사각형;
                rect.rect.X = myrect[i].getRect().X;
                rect.rect.Y = myrect[i].getRect().Y;
                rect.rect.Width = myrect[i].getRect().Width;
                rect.rect.Height = myrect[i].getRect().Height;
                rect.thick = myrect[i].getThick();
                rect.isSolid = myrect[i].getSolid();

                Packet.Serialize(rect).CopyTo(this.sendBuffer, 0);
                this.Send();
            }
            for (i = 0; i <= ncircle; i++)
            {
                Circle circle = new Circle();
                circle.Type = (int)PacketType.원;
                circle.rectC.X = mycircle[i].getRectC().X;
                circle.rectC.Y = mycircle[i].getRectC().Y;
                circle.rectC.Width = mycircle[i].getRectC().Width;
                circle.rectC.Height = mycircle[i].getRectC().Height;
                circle.thick = mycircle[i].getThick();
                circle.isSolid = mycircle[i].getSolid();

                Packet.Serialize(circle).CopyTo(this.sendBuffer, 0);
                this.Send();
            }
        }
    }
}
