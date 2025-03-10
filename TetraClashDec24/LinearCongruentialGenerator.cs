﻿namespace TetraClashDec24
{
    public class LCGGenerator
    {
        // The current seed for the generator.
        private long seed;

        // Constants for the LCG algorithm.
        private const long a = 1664525;
        private const long c = 1013904223;
        private const long m = 2147483648; // 2^31

        // Constructor to initialize the generator with a seed.
        public LCGGenerator(long _seed)
        {
            seed = _seed;
        }

        // Generates a pseudo-random number between 1 and 7.
        public int NextTetrominoValue()
        {
            // Update the seed using the LCG formula.
            seed = (a * seed + c) % m;

            return (int)(seed % 7);
        }
    }
}
