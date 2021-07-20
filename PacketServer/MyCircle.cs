using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;

namespace PacketServer
{
    /// <summary>
    /// 지금까지 그려졌던 원들을 저장하는 클래스
    /// </summary>
    class MyCircle
    {
        private Rectangle rectC;
        private int thick;
        private bool isSolid;

        // 생성자
        public MyCircle() {
            rectC = new Rectangle();
            thick = 1;
            isSolid = true;
        }

        public void setRect(Point start, Point finish, int thick, bool isSolid) {
            rectC.X = Math.Min(start.X, finish.X);
            rectC.Y = Math.Min(start.Y, finish.Y);
            rectC.Width = Math.Abs(start.X - finish.X);
            rectC.Height = Math.Abs(start.X - finish.Y);
            this.thick = thick;
            this.isSolid = isSolid;
        }

        public Rectangle getRectC() {
            return rectC;
        }

        public int getThick() {
            return thick;
        }

        public bool getSolid() {
            return isSolid;
        }
    }
}
