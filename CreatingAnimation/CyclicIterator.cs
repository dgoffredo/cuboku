using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuboku
{
    class CyclicIterator<T>
    {
        T[] _arr;
        int _index = 0;

        public CyclicIterator(T[] arr) {
            _arr = arr;
        }

        public static CyclicIterator<T> operator ++ (CyclicIterator<T> iter) {
            iter._index = (iter._index + 1) % iter._arr.Length;
            return iter;
        }

        public static CyclicIterator<T> operator -- (CyclicIterator<T> iter) {
            if (iter._index == 0)
                iter._index = iter._arr.Length - 1;
            else
                iter._index = (iter._index - 1) % iter._arr.Length;
            return iter;
        }

        public T value { get { return _arr[_index]; } }
        public void goHome() { _index = 0; }
    }
}
