using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Debug = System.Diagnostics.Debug;

namespace Cuboku
{
    [DataContract]
    class SerialArray<T>
    {
        [DataMember]
        public T[] data;

        [DataMember]
        public int rank;

        [DataMember]
        public int[] lengths;

        static int[] getLengths(Array a)
        {
            return (from i in Enumerable.Range(0, a.Rank)
                       select a.GetLength(i)).ToArray();
        }

        public void readFrom(Array a)
        {
            lengths = getLengths(a);
            rank = a.Rank;

            int len = lengths.Aggregate((x, y) => x * y);

            List<T> readData = new List<T>();
            ModuloVector mv = new ModuloVector(lengths);
            do {
                T item = (T)a.GetValue(mv.theArray());
                readData.Add(item);
            } while ( !(++mv).isZero() );

            data = readData.ToArray();
        }

        public void writeTo(Array a)
        {
            if (a.Rank != rank)
                throw new Exception(String.Format("Cannot write a rank {0} array into a rank {1} target", rank, a.Rank));

            if (!lengths.SequenceEqual(getLengths(a)))
                throw new Exception("Cannot write to array, since the lengths of its dimensions are different from mine.");

            ModuloVector mv = new ModuloVector(lengths);
            foreach (T item in data)
            {
                a.SetValue(item, mv.theArray());
                ++mv;
            }

            if (!mv.isZero())
                throw new Exception("We should have exhausted (overflowed) the ModuloVector, but we didn't.");
        }
    }
}
