using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using Debug = System.Diagnostics.Debug;

namespace Cuboku
{
    class Mappers
    {
        public static void setFromOfAnimation(Translation<DoubleAnimation> anim, Translation<Double> point)
        {
            anim.x.From = point.x;
            anim.y.From = point.y;
            anim.z.From = point.z;
        }

        public static void setToOfAnimation(Translation<DoubleAnimation> anim, Translation<Double> point)
        {
            anim.x.To = point.x;
            anim.y.To = point.y;
            anim.z.To = point.z;
        }

        public static void setEqual<T>(Translation<T> left, Translation<T> right) where T : new()
        {
            left.x = right.x;
            left.y = right.y;
            left.z = right.z;
        }

        public static void setPlace(CellPlacer place, Translation<Double> point)
        {
            place.x = point.x;
            place.y = point.y;
            place.z = point.z;
        }

        public static double animationDurationSeconds = 2.0; // TODO: this is not the place for this.
        public static void setDurationOfAnimation(Translation<DoubleAnimation> anim)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(animationDurationSeconds));
            anim.x.Duration = anim.y.Duration = anim.z.Duration = duration;
        }

        public static UnaryActor<Translation<DoubleAnimation>> setAnimDuration(double seconds)
        {
            return (Translation<DoubleAnimation> anim) => {
                Duration duration = new Duration(TimeSpan.FromSeconds(seconds));
                anim.x.Duration = anim.y.Duration = anim.z.Duration = duration;
            };
        }

        public delegate void BinaryActor<T1, T2>(T1 left, T2 right)
            where T1 : new()
            where T2 : new();

        public static void forEach<T1, T2>(CubeView<T1> left, CubeView<T2> right, BinaryActor<T1, T2> actor)
            where T1 : new()
            where T2 : new() 
        {
            const int sideLength = 3;

            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                    for (int k = 0; k < sideLength; ++k)
                        actor(left[i, j, k], right[i, j, k]);
        }

        public delegate void UnaryActor<T1>(T1 left)
            where T1 : new();

        public static void forEach<T1>(CubeView<T1> cube, UnaryActor<T1> actor)
            where T1 : new()
        {
            const int sideLength = 3;

            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                    for (int k = 0; k < sideLength; ++k)
                        actor(cube[i, j, k]);
        }

        public delegate bool PointPredicate(int i, int j, int k);

        public static void forEachThat<T1>(CubeView<T1> cube, PointPredicate pred, UnaryActor<T1> actor)
            where T1 : new()
        {
            const int sideLength = 3;

            for (int i = 0; i < sideLength; ++i)
                for (int j = 0; j < sideLength; ++j)
                    for (int k = 0; k < sideLength; ++k)
                        if (pred(i, j, k))
                            actor(cube[i, j, k]);
        }

        public static PointPredicate inXZPlane(int y) {
            return (int i, int j, int k) => {
                return j == y;
            };
        }
        public static PointPredicate inYZPlane(int x) {
            return (int i, int j, int k) => {
                return i == x;
            };
        }
        public static PointPredicate inXYPlane(int z) {
            return (int i, int j, int k) => {
                return k == z;
            };
        }
        public static bool inYZDiag(int i, int j, int k) {
            return j == k;
        }
        public static bool inXZDiag(int i, int j, int k) {
            return i == k;
        }
        public static bool inXYDiag(int i, int j, int z) { // Won't be used.
            return i == j;
        }
    }
}
