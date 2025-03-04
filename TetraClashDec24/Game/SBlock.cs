﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class SBlock : Block
    {
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new (0,1), new (0,2), new (1,0), new (1,1) },
            new Position[] { new (0,1), new (1,1), new (1,2), new (2,2) }
        };

        public override int TetrominoID => 5;

        protected override Position StartOffset => new Position(0, 3);

        protected override Position[][] Tiles => tiles;
    }
}
