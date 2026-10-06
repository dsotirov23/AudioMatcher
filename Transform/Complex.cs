using System;
using System.Collections.Generic;
using System.Text;

namespace AudioMatcher.Transform
{
    // Complex is a custom data structure which will be used to hold the two parts of a frequency.
    // In FFT I'll need to use both parts at the exact same time to do the math
    public struct Complex
    {
        public double Real; // The real part (cosine)
        public double Imgnr; // The imaginary part (sine)

        public Complex(double real, double imaginary)
        {
            Real = real;
            Imgnr = imaginary;
        }

        // Overleoading operators tells the program how to handle operations with complex numbers.
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imgnr + b.Imgnr);
        }

        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imgnr - b.Imgnr);
        }

        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(
                (a.Real * b.Real) - (a.Imgnr * b.Imgnr), // calculates real (cosine)
                (a.Real * b.Imgnr) + (a.Imgnr * b.Real) // calculates imaginary (sine)
             );
        }

        // The final Loudness score is calculated through the Pythagorean Theorem
        public double Magnitude()
        {
            return Math.Sqrt((Real * Real) + (Imgnr * Imgnr));
        }
    }
}
