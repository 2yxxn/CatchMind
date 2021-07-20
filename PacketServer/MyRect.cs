using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;

namespace PacketServer
{
    // 지금까지 그려졌던 사각형을 저장하는 클래스
    class MyRect
    {
        private Rectangle rect;
        private int thick;
        private bool isSolid;

        // 생성자
        public MyRect() {
            rect = new Rectangle();
            thick = 1;
            isSolid = true;
        }

        public void setRect(Point start, Point finish, int thick, bool isSolid) {
            rect.X = Math.Min(start.X, finish.X);
            rect.Y = Math.Min(start.Y, finish.Y);
            rect.Width = Math.Abs(start.X - finish.X);
            rect.Height = Math.Abs(start.X - finish.Y);
            this.thick = thick;
            this.isSolid = isSolid;
        }

        public Rectangle getRect() {
            return rect;
        }

        public int getThick() {
            return thick;
        }

        public bool getSolid() {
            return isSolid;
        }
    }
}
