using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetraClashDec24
{
    public class BlockQueue
    {
        private readonly Block[] blocks = new Block[]
        {
            new IBlock(),
            new JBlock(),
            new LBlock(),
            new OBlock(),
            new SBlock(),
            new TBlock(),
            new ZBlock(),
        };

        private LCGGenerator LCGGen;

        public Block NextBlock { get; private set; }

        public BlockQueue(int seed)
        {
            LCGGen = new LCGGenerator(seed);
            NextBlock = RandomBlock();
        }

        public Block RandomBlock()
        {
            return blocks[LCGGen.NextTetrominoValue()];
        }

        public Block GetAndUpdate()
        {
            Block block = NextBlock;

            do
            {
                NextBlock = RandomBlock();
            }
            while (block.TetrominoID == NextBlock.TetrominoID);

            return block;
        }
    }
}
