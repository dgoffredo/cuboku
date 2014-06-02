﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XYZ = Cuboku.Translation<int>;

namespace Cuboku
{
    class CellChange
    {
        public XYZ position = new XYZ();
        public int value = 0;

        static bool validPosition(XYZ pos) {
            return pos.x >= 0 && pos.x <= 2 &&
                   pos.y >= 0 && pos.y <= 2 &&
                   pos.z >= 0 && pos.z <= 2;
        }

        public CellChange() {}

        public CellChange(XYZ pos, int val) 
        {
            if (!validPosition(pos))
                throw new Exception("You're in for a real beating, you know that?");

            position = pos;
            value = val;
        }
    }
}
