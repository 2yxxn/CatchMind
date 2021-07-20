using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;

namespace PacketClient
{
    public enum PacketType
    {
        정답 = 0,
        로그인,
        선,
        사각형,
        원
    }

    [Serializable]
    public class Packet
    {
        public int Length;
        public int Type;

        public Packet()
        {
            this.Length = 0;
            this.Type = 0;
        }

        // 보낼 때
        public static byte[] Serialize(Object o)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();
        }

        public static Object Desserialize(byte[] bt)
        {
            MemoryStream ms = new MemoryStream(1024 * 4);
            foreach (byte b in bt)
            {
                ms.WriteByte(b);
            }

            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            Object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }

    [Serializable]
    public class Answer : Packet
    {
        public string Data;
        public Answer()
        {
            this.Data = null;
        }
    }

    [Serializable]
    public class Login : Packet
    {
        public string m_strID;
        public Login()
        {
            this.m_strID = null;
        }
    }

    [Serializable]
    public class Line : Packet
    {
        public Point[] point = new Point[2];
        public int thick;
        public bool isSolid;
        public Line()
        {
            point[0] = new Point();
            point[1] = new Point();
            thick = 1;
            isSolid = true;
        }
    }

    [Serializable]
    public class Rect : Packet
    {
        public Rectangle rect;
        public int thick;
        public bool isSolid;
        public Rect()
        {
            rect = new Rectangle();
            thick = 1;
            isSolid = true;
        }
    }

    [Serializable]
    public class Circle : Packet
    {
        public Rectangle rectC;
        public int thick;
        public bool isSolid;

        // 생성자
        public Circle()
        {
            rectC = new Rectangle();
            thick = 1;
            isSolid = true;
        }
    }
}
