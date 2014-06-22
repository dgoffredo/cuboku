using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Sudokudos
{
    // I use this class as part of the implementation
    // of serializing multidimensional arrays.
    // The idea is of you have a multidimensional array with
    // length (n0, n1, n2, ...) and you want to serialize it
    // into one dimension, you have to be able to map
    // 0   -->  (0, 0, 0, ...)
    // 1   -->  (1, 0, 0, ...)
    // ...
    // n0  -->  (0, 1, 0, ...)
    // ...
    // etc.
    //
    // The array "moduli" in the constructor is the (n0, n1, ...) above.
    //
    class ModuloVector
    {
        private int[] modN;
        private int[] x;

        static T[] defaultArray<T>(int length, T defaultValue)  // Is there a better way?
        {
            return (from _ in Enumerable.Range(0, length) select defaultValue).ToArray();
        }

        public ModuloVector(int[] moduli) 
        {
            modN = (int[])moduli.Clone();

            x = defaultArray(moduli.Length, 0);
        }

        public bool isZero()
        {
            return x.All((int i) => { return i == 0; });
        }

        public static ModuloVector operator ++ (ModuloVector v)
        {
            for (int i = 0; i < v.x.Length; ++i)
            {
                v.x[i] = (v.x[i] + 1) % v.modN[i];
                if (v.x[i] != 0)
                    break;
            }

            return v;
        }

        public int[] toArray()
        {
            return (int[])x.Clone();
        }

        public int[] theArray()
        {
            return x;
        }
    }
}
