using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Sudokudos
{
    [DataContract]
    class Ref<T>
    {
        [DataMember]
        public T value;

        public Ref(T initValue) { 
            value = initValue; 
        }

        public Ref(Ref<T> initValue) { 
            value = initValue.value; 
        }

        public Ref() {}

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
