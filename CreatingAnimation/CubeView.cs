using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Cuboku
{
    class Rotations
    {
        public static readonly Matrix rhZ = new Matrix(new double[,] { {  0, 1, 0 },
                                                                       { -1, 0, 0 },
                                                                       {  0, 0, 1 } });

        public static readonly Matrix lhZ = new Matrix(new double[,] { { 0, -1, 0 },
                                                                       { 1,  0, 0 },
                                                                       { 0,  0, 1 } });
        
        public static readonly Matrix lhY = new Matrix(new double[,] { {  0, 0, 1 },
                                                                       {  0, 1, 0 },
                                                                       { -1, 0, 0 } });

        public static readonly Matrix rhY = new Matrix(new double[,] { { 0, 0, -1 },
                                                                       { 0, 1,  0 },
                                                                       { 1, 0,  0 } });

        public static readonly Matrix rhX = new Matrix(new double[,] { { 1,  0, 0 },
                                                                       { 0,  0, 1 },
                                                                       { 0, -1, 0 } });

        public static readonly Matrix lhX = new Matrix(new double[,] { { 1, 0,  0 },
                                                                       { 0, 0, -1 },
                                                                       { 0, 1,  0 } });
    }

    public interface RotatableCovariant
    {
        void rotateLhZ();
        void rotateRhZ();
        void rotateLhY();
        void rotateRhY();
        void rotateLhX();
        void rotateRhX();
    }

    public interface  RotatableContravariant
    {
        void rotateLhZConjugate();
        void rotateRhZConjugate();
        void rotateLhYConjugate();
        void rotateRhYConjugate();
        void rotateLhXConjugate();
        void rotateRhXConjugate();
    }

    public interface Rotatable : RotatableCovariant, RotatableContravariant {}

    class CubeView<T> where T : new()
    {
        protected T[,,] target;
        protected Matrix transformation;

        public static int sideLength {
            get { return 3; }
        }
        public static int dimension  {
            get { return 3; }
        }

        private void initTransformation() {
            transformation = Matrix.IdentityMatrix(dimension, dimension); 
        }

        public CubeView(T[,,] dataToView)
        {
            initTransformation(); 
            target = dataToView;
        }

        public CubeView()
        {
            initTransformation();
            target = new T[sideLength, sideLength, sideLength];
            foreach (int i in Enumerable.Range(0, 3))
                foreach (int j in Enumerable.Range(0, 3))
                    foreach (int k in Enumerable.Range(0, 3))
                        target[i, j, k] = new T();
        }

        int[] transformedIndex(int i, int j, int k)
        {
            Matrix before = new Matrix(new double[,]{ { i - 1 }, { j - 1 }, { k - 1 } });
            Matrix after = transformation * before;
            
            return new int[3] { (int)after[0, 0] + 1, (int)after[1, 0] + 1, (int)after[2, 0] + 1 };
        }

        public T this[int i, int j, int k] {
            get {
                int[] idx = transformedIndex(i, j, k);
                return target[idx[0], idx[1], idx[2]];
            }
            set {
                int[] idx = transformedIndex(i, j, k);
                target[idx[0], idx[1], idx[2]] = value;
            }
        }
    }

    class RotatableCubeView<T> : CubeView<T>, Rotatable where T : new()
    {
        public RotatableCubeView(T[,,] dataToView) : base(dataToView)
        {}

        public RotatableCubeView() : base()
        {}

        public void rotateLhZ() { transformation = Rotations.lhZ * transformation; }
        public void rotateRhZ() { transformation = Rotations.rhZ * transformation; }
        public void rotateLhY() { transformation = Rotations.lhY * transformation; }
        public void rotateRhY() { transformation = Rotations.rhY * transformation; }
        public void rotateLhX() { transformation = Rotations.lhX * transformation; }
        public void rotateRhX() { transformation = Rotations.rhX * transformation; }

        public void rotateLhZConjugate() { transformation = transformation * Rotations.lhZ; }
        public void rotateRhZConjugate() { transformation = transformation * Rotations.rhZ; }
        public void rotateLhYConjugate() { transformation = transformation * Rotations.lhY; }
        public void rotateRhYConjugate() { transformation = transformation * Rotations.rhY; }
        public void rotateLhXConjugate() { transformation = transformation * Rotations.lhX; }
        public void rotateRhXConjugate() { transformation = transformation * Rotations.rhX; }
        public RotatableCubeView<T> invert() { transformation = transformation.Invert(); return this; }

        public T[,,] data {
            get { return target; }
        }
    }

    class MirroredCubeView<T> : RotatableCovariant where T : new()
    {
        RotatableCubeView<T> _primary;
        RotatableCubeView<T> _mirror;
        CubeView<T> _original;

        public static int sideLength {
            get { return RotatableCubeView<T>.sideLength; }
        }
        public static int dimension  {
            get { return RotatableCubeView<T>.dimension; }
        }

        public static implicit operator CubeView<T>(MirroredCubeView<T> mcv) 
        { 
            return mcv._primary; 
        }

        public CubeView<T> mirror { get { return _mirror; } }
        public CubeView<T> original { get { return _original; } }

        public T[,,] data { get { return _primary.data; } } // _original, _primary, and _mirror all share the same data.

        public void rotateLhZ() { _primary.rotateLhZ(); _mirror.rotateRhZConjugate(); }
        public void rotateRhZ() { _primary.rotateRhZ(); _mirror.rotateLhZConjugate(); }
        public void rotateLhY() { _primary.rotateLhY(); _mirror.rotateRhYConjugate(); }
        public void rotateRhY() { _primary.rotateRhY(); _mirror.rotateLhYConjugate(); }
        public void rotateLhX() { _primary.rotateLhX(); _mirror.rotateRhXConjugate(); }
        public void rotateRhX() { _primary.rotateRhX(); _mirror.rotateLhXConjugate(); }

        public MirroredCubeView(T[,,] dataToView)
        {
            _primary = new RotatableCubeView<T>(dataToView);
            _mirror =  new RotatableCubeView<T>(dataToView);
            _original =  new RotatableCubeView<T>(dataToView);
        }

        public MirroredCubeView()
        {
            _primary = new RotatableCubeView<T>();
            _mirror = new RotatableCubeView<T>(_primary.data);
            _original = new RotatableCubeView<T>(_primary.data);
        }

        public T this[int i, int j, int k] {
            get {
                return _primary[i, j, k];
            }
            set {
                _primary[i, j, k] = value;
            }
        }
    }
}
