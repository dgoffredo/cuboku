using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudokudos
{
    class Planes
    {
        public const int size = 3;
        public class Plane
        {
            Translation<int>[] _points = new Translation<int>[size*size];
            Mappers.PointPredicate _predicate;
            public string name { get; set; }

            // Will throw a bounds exception if the number of points
            // for which pred -> true is greater than size*size.
            public Plane(Mappers.PointPredicate pred, string nameIn)
            {
                name = nameIn;
                _predicate = pred;
                int index = 0;
                for (int i = 0; i < size; ++i)
                    for (int j = 0; j < size; ++j)
                        for (int k = 0; k < size; ++k)
                            if (pred(i, j, k))
                                _points[index++] = new Translation<int>(i, j, k);
            }

            public Translation<int>[] points {
                get { return _points; }
            }

            public Mappers.PointPredicate predicate {
                get { return _predicate; }
            }
        }

        static readonly Plane xEquals0 = new Plane(Mappers.inYZPlane(0), "xEquals0");
        static readonly Plane xEquals1 = new Plane(Mappers.inYZPlane(1), "xEquals1");
        static readonly Plane xEquals2 = new Plane(Mappers.inYZPlane(2), "xEquals2");
        static readonly Plane yEquals0 = new Plane(Mappers.inXZPlane(0), "yEquals0");
        static readonly Plane yEquals1 = new Plane(Mappers.inXZPlane(1), "yEquals1");
        static readonly Plane yEquals2 = new Plane(Mappers.inXZPlane(2), "yEquals2");
        static readonly Plane zEquals0 = new Plane(Mappers.inXYPlane(0), "zEquals0");
        static readonly Plane zEquals1 = new Plane(Mappers.inXYPlane(1), "zEquals1");
        static readonly Plane zEquals2 = new Plane(Mappers.inXYPlane(2), "zEquals2");
        static readonly Plane xEqualsY = new Plane(Mappers.inXYDiag, "xEqualsY");
        static readonly Plane xEqualsZ = new Plane(Mappers.inXZDiag, "xEqualsZ");
        static readonly Plane yEqualsZ = new Plane(Mappers.inYZDiag, "yEqualsZ");

        static public Plane[] makePlanes() {
            return new Plane[] { xEquals0, xEquals1, xEquals2, yEquals0, yEquals1, yEquals2,
                                 zEquals0, zEquals1, zEquals2, /*xEqualsY,*/ xEqualsZ, yEqualsZ }; 
        }
    }
}
