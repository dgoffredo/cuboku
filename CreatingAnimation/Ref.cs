using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuboku
{
    class Ref<T>
    {
        public T value;
        public Ref(T initValue) { value = initValue; }
        public Ref() {}
    }
}
