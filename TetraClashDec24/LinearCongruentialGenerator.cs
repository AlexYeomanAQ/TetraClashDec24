namespace TetraClashDec24
{
    // The LCGGenerator class implements a Linear Congruential Generator (LCG),
    // a widely used algorithm for generating pseudo-random numbers.
    public class LCGGenerator
    {
        // The current seed for the generator.
        // This value is updated with each call to generate a new pseudo-random number.
        private long seed;

        // Constants for the LCG algorithm.
        // These constants are chosen to help achieve a good distribution of pseudo-random numbers.
        private const long a = 1664525;       // Multiplier constant.
        private const long c = 1013904223;    // Increment constant.
        private const long m = 2147483648;      // Modulus constant, equal to 2^31.

        // Constructor to initialize the generator with a specific seed.
        // A different seed will produce a different sequence of pseudo-random numbers.
        public LCGGenerator(long _seed)
        {
            seed = _seed;
        }

        // Generates a pseudo-random integer corresponding to a tetromino type.
        public int NextTetrominoValue()
        {
            // Update the seed using the LCG formula:
            seed = (a * seed + c) % m;

            // The modulus operation (seed % 7) ensures the result is between 0 and 6.
            return (int)(seed % 7);
        }
    }
}
