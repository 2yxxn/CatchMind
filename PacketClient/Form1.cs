using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace PacketClient
{
    public partial class Form1 : Form
    {
        private NetworkStream m_networkstream;
        private TcpClient m_client;

        private byte[] sendBuffer = new byte[1024 * 4];
        private byte[] readBuffer = new byte[1024 * 4];

        private bool m_bConnect = false;

        public Answer m_answerClass;
        public Login m_loginClass;
        public Line m_lineClass;
        public Rect m_rectClass;
        public Circle m_circleClass;

        private Thread m_thread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.m_client.Close();
            this.m_networkstream.Close();
            this.m_thread.Abort();
        }

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
            while (this.m_bConnect)
            {
                int nRead = 0;

                try
                {
                    nRead = 0;
                    nRead = this.m_networkstream.Read(readBuffer, 0, 1024 * 4);
                }
                catch
                {
                    this.m_bConnect = false;
                    this.m_networkstream = null;
                }

                Packet packet = (Packet)Packet.Desserialize(this.readBuffer);

                switch ((int)packet.Type)
                {
                    case (int)PacketType.선:
                        {

                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                Graphics g = this.panel1.CreateGraphics();
                                Pen pen = new Pen(Color.Black);
                                this.m_lineClass = (Line)Packet.Desserialize(this.readBuffer);

                                if (!this.m_lineClass.isSolid)
                                {
                                    pen.Width = 1;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                                }
                                else
                                {
                                    pen.Width = this.m_lineClass.thick;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                                }

                                g.DrawLine(pen, this.m_lineClass.point[0], this.m_lineClass.point[1]);
                            }));


                            break;
                        }
                    case (int)PacketType.사각형:
                        {

                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                Graphics g = this.panel1.CreateGraphics();
                                Pen pen = new Pen(Color.Black);
                                this.m_rectClass = (Rect)Packet.Desserialize(this.readBuffer);

                                if (!this.m_rectClass.isSolid)
                                {
                                    pen.Width = 1;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                                }
                                else
                                {
                                    pen.Width = this.m_rectClass.thick;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                                }

                                g.DrawRectangle(pen, this.m_rectClass.rect);
                            }));


                            break;
                        }
                    case (int)PacketType.원:
                        {

                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                Graphics g = this.panel1.CreateGraphics();
                                Pen pen = new Pen(Color.Black);
                                this.m_circleClass = (Circle)Packet.Desserialize(this.readBuffer);

                                if (!this.m_circleClass.isSolid)
                                {
                                    pen.Width = 1;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                                }
                                else
                                {
                                    pen.Width = this.m_circleClass.thick;
                                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                                }

                                g.DrawEllipse(pen, this.m_circleClass.rectC);
                            }));


                            break;
                        }
                    case (int)PacketType.정답:
                        {
                            this.m_answerClass = (Answer)Packet.Desserialize(this.readBuffer);

                            break;
                        }
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.m_client = new TcpClient();
            try
            {
                this.m_client.Connect(this.txtIP.Text, 7777);
                MessageBox.Show("접속 성공");
            }
            catch
            {
                MessageBox.Show("접속 에러");
                return;
            }
            this.m_bConnect = true;
            this.m_networkstream = this.m_client.GetStream();

            this.m_thread = new Thread(new ThreadStart(RUN));
            this.m_thread.Start();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!this.m_bConnect)
            {
                return;
            }
            if (this.txtID.Text == "")
            {
                MessageBox.Show("ID가 입력되지 않았습니다.");
                return;
            }
            Login login = new Login();
            login.Type = (int)PacketType.로그인;
            login.m_strID = this.txtID.Text;

            Packet.Serialize(login).CopyTo(this.sendBuffer, 0);
            this.Send();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!this.m_bConnect)
            {
                return;
            }

            if (this.m_answerClass.Data == this.txtAnswer.Text)
            {
                MessageBox.Show("정답!");

                panel1.Refresh();

                Answer answer = new Answer();
                answer.Type = (int)PacketType.정답;
                answer.Data = this.txtAnswer.Text;

                Packet.Serialize(answer).CopyTo(this.sendBuffer, 0);
                this.Send();
            }
            else
            {
                MessageBox.Show("오답입니다.");

                Answer answer = new Answer();
                answer.Type = (int)PacketType.정답;
                answer.Data = this.txtAnswer.Text;

                Packet.Serialize(answer).CopyTo(this.sendBuffer, 0);
                this.Send();
            }

        }
    }
}
