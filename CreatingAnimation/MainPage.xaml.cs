﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Cuboku.Resources;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Input;
using Windows.Phone.Devices.Notification;
using Debug = System.Diagnostics.Debug;
using TranslationAnimation = Cuboku.Translation<System.Windows.Media.Animation.DoubleAnimation>;
using XYZ = Cuboku.Translation<int>;
using Rectangle = System.Windows.Shapes.Rectangle;
using Plane = Cuboku.Planes.Plane;

namespace Cuboku
{
    using Mtrx = Matrix;

    // Generic 3D point. I would have used the name "Point" except
    // that this is used to contain things like DoubleAnimations as well
    // as numbers, so it more represents some displacement in space.
    public class Translation<T> where T : new()
    {
        public T x { get; set; }
        public T y { get; set; }
        public T z { get; set; }

        public Translation(T xIn, T yIn, T zIn)
        {
            x = xIn;
            y = yIn;
            z = zIn;
        }

        public Translation()
        {
            x = y = z = new T();
        }
    }

    class TranslationComparer<T> : IEqualityComparer<Translation<T>> where T : new()
    {
        public bool Equals(Translation<T> left, Translation<T> right)
        {
            return left.x.Equals(right.x) &&
                   left.y.Equals(right.y) &&
                   left.z.Equals(right.z);
        }

        private int combineHash(int seed, int addition)
        {
            return seed ^ addition + 0x68BDB4F2 + (seed << 6) + (seed >> 2);
            // Note: 0x68BDB4F2 is 31 arbitrarily chosen bits
            //       11010001 01111011 01101001 1110010_
        }

        public int GetHashCode(Translation<T> arg)
        {
            int hash = arg.x.GetHashCode();
            hash = combineHash(hash, arg.y.GetHashCode());
            return combineHash(hash, arg.z.GetHashCode());
        }
    }

    // Cell placer refers to the actual objects (translations and projections)
    // that determine where a cell is rendered.
    public class CellPlacer
    {
        TranslateTransform tran;
        PlaneProjection proj;

        public CellPlacer(TranslateTransform tranIn, PlaneProjection projIn)
        {
            tran = tranIn;
            proj = projIn;
        }

        public CellPlacer() {}

        public double x {
            set {
                tran.X = value;
            }
            get {
                return tran.X;
            }
        }

        public double y {
            set {
                tran.Y = value;
            }
            get {
                return tran.Y;
            }
        }

        public double z {
            set {
                proj.GlobalOffsetZ = value;
            }
            get {
                return proj.GlobalOffsetZ;
            }
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        MirroredCubeView<Translation<double>> coordinates;
        MirroredCubeView<Translation<double>> dragCoordinates;
        MirroredCubeView<CellPlacer> places;
        MirroredCubeView<TranslationAnimation> animations;
        MirroredCubeView<SolidColorBrush> bgColors;
        MirroredCubeView<Ref<int>> numbers;
        MirroredCubeView<Border> cells;
        MirroredCubeView<Ref<bool>> isPreset;
        
        MirroredCubeView<Ref<bool>> isWrong;
        MirroredCubeView<HashSet<XYZ>> wrongers;

        RotatableCovariant[] rotationSubscribers;

        Plane[] planes = Planes.makePlanes();
        CyclicIterator<Plane> currentPlane = null;
        bool isGlowing = false;

        TranslationComparer<int> comparePoints = new TranslationComparer<int>();

        IEnumerable<Plane>[,,] planesThrough = new IEnumerable<Plane>[3,3,3];

        bool sliderShowing = false;
        bool pickerShowing = false;
        enum Gesture { None, Unknown, DragCube, PinchCube, Tap, SwipeEdge, SwipeNumPicker };
        Gesture currentGesture = Gesture.None;

        enum Direction { Left, Right, Up, Down };

        public MainPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (System.Windows.Markup.XamlParseException e)
            {
                Debug.WriteLine("Here: {0}", e);
            }

            // Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);
            planeSelectSliderRight.Value = 3;
            
            // currentPlane = new CyclicIterator<Plane>(planes); // No, not until we need it.

            places = new MirroredCubeView<CellPlacer>(
                new CellPlacer[,,] {
                    { {new CellPlacer(cell_0_0_0_tran, cell_0_0_0_proj), new CellPlacer(cell_0_0_1_tran, cell_0_0_1_proj), new CellPlacer(cell_0_0_2_tran, cell_0_0_2_proj)}, 
                      {new CellPlacer(cell_0_1_0_tran, cell_0_1_0_proj), new CellPlacer(cell_0_1_1_tran, cell_0_1_1_proj), new CellPlacer(cell_0_1_2_tran, cell_0_1_2_proj)}, 
                      {new CellPlacer(cell_0_2_0_tran, cell_0_2_0_proj), new CellPlacer(cell_0_2_1_tran, cell_0_2_1_proj), new CellPlacer(cell_0_2_2_tran, cell_0_2_2_proj)} },
                    { {new CellPlacer(cell_1_0_0_tran, cell_1_0_0_proj), new CellPlacer(cell_1_0_1_tran, cell_1_0_1_proj), new CellPlacer(cell_1_0_2_tran, cell_1_0_2_proj)}, 
                      {new CellPlacer(cell_1_1_0_tran, cell_1_1_0_proj), new CellPlacer(cell_1_1_1_tran, cell_1_1_1_proj), new CellPlacer(cell_1_1_2_tran, cell_1_1_2_proj)}, 
                      {new CellPlacer(cell_1_2_0_tran, cell_1_2_0_proj), new CellPlacer(cell_1_2_1_tran, cell_1_2_1_proj), new CellPlacer(cell_1_2_2_tran, cell_1_2_2_proj)} },
                    { {new CellPlacer(cell_2_0_0_tran, cell_2_0_0_proj), new CellPlacer(cell_2_0_1_tran, cell_2_0_1_proj), new CellPlacer(cell_2_0_2_tran, cell_2_0_2_proj)}, 
                      {new CellPlacer(cell_2_1_0_tran, cell_2_1_0_proj), new CellPlacer(cell_2_1_1_tran, cell_2_1_1_proj), new CellPlacer(cell_2_1_2_tran, cell_2_1_2_proj)}, 
                      {new CellPlacer(cell_2_2_0_tran, cell_2_2_0_proj), new CellPlacer(cell_2_2_1_tran, cell_2_2_1_proj), new CellPlacer(cell_2_2_2_tran, cell_2_2_2_proj)} }
                });

            animations = new MirroredCubeView<TranslationAnimation>(
                new TranslationAnimation[,,] {
                    { {new TranslationAnimation(anim_0_0_0_X, anim_0_0_0_Y, anim_0_0_0_Z), new TranslationAnimation(anim_0_0_1_X, anim_0_0_1_Y, anim_0_0_1_Z), new TranslationAnimation(anim_0_0_2_X, anim_0_0_2_Y, anim_0_0_2_Z)}, 
                      {new TranslationAnimation(anim_0_1_0_X, anim_0_1_0_Y, anim_0_1_0_Z), new TranslationAnimation(anim_0_1_1_X, anim_0_1_1_Y, anim_0_1_1_Z), new TranslationAnimation(anim_0_1_2_X, anim_0_1_2_Y, anim_0_1_2_Z)}, 
                      {new TranslationAnimation(anim_0_2_0_X, anim_0_2_0_Y, anim_0_2_0_Z), new TranslationAnimation(anim_0_2_1_X, anim_0_2_1_Y, anim_0_2_1_Z), new TranslationAnimation(anim_0_2_2_X, anim_0_2_2_Y, anim_0_2_2_Z)} },
                    { {new TranslationAnimation(anim_1_0_0_X, anim_1_0_0_Y, anim_1_0_0_Z), new TranslationAnimation(anim_1_0_1_X, anim_1_0_1_Y, anim_1_0_1_Z), new TranslationAnimation(anim_1_0_2_X, anim_1_0_2_Y, anim_1_0_2_Z)}, 
                      {new TranslationAnimation(anim_1_1_0_X, anim_1_1_0_Y, anim_1_1_0_Z), new TranslationAnimation(anim_1_1_1_X, anim_1_1_1_Y, anim_1_1_1_Z), new TranslationAnimation(anim_1_1_2_X, anim_1_1_2_Y, anim_1_1_2_Z)}, 
                      {new TranslationAnimation(anim_1_2_0_X, anim_1_2_0_Y, anim_1_2_0_Z), new TranslationAnimation(anim_1_2_1_X, anim_1_2_1_Y, anim_1_2_1_Z), new TranslationAnimation(anim_1_2_2_X, anim_1_2_2_Y, anim_1_2_2_Z)} },
                    { {new TranslationAnimation(anim_2_0_0_X, anim_2_0_0_Y, anim_2_0_0_Z), new TranslationAnimation(anim_2_0_1_X, anim_2_0_1_Y, anim_2_0_1_Z), new TranslationAnimation(anim_2_0_2_X, anim_2_0_2_Y, anim_2_0_2_Z)}, 
                      {new TranslationAnimation(anim_2_1_0_X, anim_2_1_0_Y, anim_2_1_0_Z), new TranslationAnimation(anim_2_1_1_X, anim_2_1_1_Y, anim_2_1_1_Z), new TranslationAnimation(anim_2_1_2_X, anim_2_1_2_Y, anim_2_1_2_Z)}, 
                      {new TranslationAnimation(anim_2_2_0_X, anim_2_2_0_Y, anim_2_2_0_Z), new TranslationAnimation(anim_2_2_1_X, anim_2_2_1_Y, anim_2_2_1_Z), new TranslationAnimation(anim_2_2_2_X, anim_2_2_2_Y, anim_2_2_2_Z)} }
                });

            bgColors = new MirroredCubeView<SolidColorBrush>(
                new SolidColorBrush[,,] {
                    { {cell_0_0_0.Background as SolidColorBrush, cell_0_0_1.Background as SolidColorBrush, cell_0_0_2.Background as SolidColorBrush},
                      {cell_0_1_0.Background as SolidColorBrush, cell_0_1_1.Background as SolidColorBrush, cell_0_1_2.Background as SolidColorBrush},
                      {cell_0_2_0.Background as SolidColorBrush, cell_0_2_1.Background as SolidColorBrush, cell_0_2_2.Background as SolidColorBrush} },
                    { {cell_1_0_0.Background as SolidColorBrush, cell_1_0_1.Background as SolidColorBrush, cell_1_0_2.Background as SolidColorBrush},
                      {cell_1_1_0.Background as SolidColorBrush, cell_1_1_1.Background as SolidColorBrush, cell_1_1_2.Background as SolidColorBrush},
                      {cell_1_2_0.Background as SolidColorBrush, cell_1_2_1.Background as SolidColorBrush, cell_1_2_2.Background as SolidColorBrush} },
                    { {cell_2_0_0.Background as SolidColorBrush, cell_2_0_1.Background as SolidColorBrush, cell_2_0_2.Background as SolidColorBrush},
                      {cell_2_1_0.Background as SolidColorBrush, cell_2_1_1.Background as SolidColorBrush, cell_2_1_2.Background as SolidColorBrush},
                      {cell_2_2_0.Background as SolidColorBrush, cell_2_2_1.Background as SolidColorBrush, cell_2_2_2.Background as SolidColorBrush} }
                });

            cells = new MirroredCubeView<Border>(
                new Border[,,] {
                    { {cell_0_0_0, cell_0_0_1, cell_0_0_2},
                      {cell_0_1_0, cell_0_1_1, cell_0_1_2},
                      {cell_0_2_0, cell_0_2_1, cell_0_2_2} },
                    { {cell_1_0_0, cell_1_0_1, cell_1_0_2},
                      {cell_1_1_0, cell_1_1_1, cell_1_1_2},
                      {cell_1_2_0, cell_1_2_1, cell_1_2_2} },
                    { {cell_2_0_0, cell_2_0_1, cell_2_0_2},
                      {cell_2_1_0, cell_2_1_1, cell_2_1_2},
                      {cell_2_2_0, cell_2_2_1, cell_2_2_2} }
                });

            isPreset = new MirroredCubeView<Ref<bool>>(
                new Ref<bool>[,,]{
                    { {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()} },                   
                    { {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()} },
                    { {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()},
                      {new Ref<bool>(), new Ref<bool>(), new Ref<bool>()} }
                });

            Mappers.forEach(isPreset, cells, 
                            (Ref<bool> whether, Border cell) => { whether.value = presetCell(cell); });

            isWrong = new MirroredCubeView<Ref<bool>>(
                new Ref<bool>[,,]{
                    { {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)} },                   
                    { {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)} },
                    { {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)},
                      {new Ref<bool>(false), new Ref<bool>(false), new Ref<bool>(false)} }
                });

            coordinates = new MirroredCubeView<Translation<double>>();
            initializeCoordinates(coordinates);
            dragCoordinates = new MirroredCubeView<Translation<double>>();
            Mappers.forEach<Translation<double>, Translation<double>>(dragCoordinates, coordinates, Mappers.setEqual);

            numbers = new MirroredCubeView<Ref<int>>(
                new Ref<int>[,,]{
                    { {new Ref<int>(parseIntOrZero((cell_0_0_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_0_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_0_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_0_1_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_1_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_1_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_0_2_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_2_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_0_2_2.Child as TextBlock).Text))} },                   
                    { {new Ref<int>(parseIntOrZero((cell_1_0_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_0_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_0_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_1_1_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_1_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_1_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_1_2_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_2_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_1_2_2.Child as TextBlock).Text))} },
                    { {new Ref<int>(parseIntOrZero((cell_2_0_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_0_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_0_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_2_1_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_1_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_1_2.Child as TextBlock).Text))},
                      {new Ref<int>(parseIntOrZero((cell_2_2_0.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_2_1.Child as TextBlock).Text)), new Ref<int>(parseIntOrZero((cell_2_2_2.Child as TextBlock).Text))} }
                });

            wrongers = new MirroredCubeView<HashSet<XYZ>>(
                new HashSet<XYZ>[,,]{
                    { {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)} },                   
                    { {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)} },
                    { {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)},
                      {new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints), new HashSet<XYZ>(comparePoints)} }
                });

            rotationSubscribers = new RotatableCovariant[] { coordinates, dragCoordinates, cells };

            foreach (int i in Enumerable.Range(0, 3))
                foreach (int j in Enumerable.Range(0, 3))
                    foreach (int k in Enumerable.Range(0, 3))
                    {
                        planesThrough[i,j,k] = from plane in planes
                                               where plane.predicate(i, j, k)
                                               select plane;
                        foreach (var plane in planesThrough[i,j,k])
                            Debug.WriteLine("({0}, {1}, {2}) is in {3}", i, j, k, plane.name);
                    }
        }

        private int parseIntOrZero(string s)
        {
            if (s.Length == 0)
                return 0;

            try {
                return int.Parse(s);
            }
            catch {
                return 0;
            }
        }

        void doAfter(Action what, int milliseconds)
        {
            System.Threading.Timer openingDelay = new System.Threading.Timer(
                (dummy) => { 
                    Dispatcher.BeginInvoke(what);
                },
                null, milliseconds, System.Threading.Timeout.Infinite);
        }

        void startOpeningAnimation()
        {
            animationTime = 3.0;

            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, coordinates, Mappers.setToOfAnimation);
            coordinates.rotateLhX();
            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, coordinates, Mappers.setFromOfAnimation);
            coordinates.rotateRhX(); // put it back

            Twistidoo.Begin();
        }

        static double dx = 124;
        static double dy = -143;
        static double dz = -150; // -100;
        static double dxz = 43;
        static double dyz = -43;
        static Matrix projection = new Matrix(new double[,]{
            { dx,  0, dxz },
            {  0, dy, dyz },
            {  0,  0,  dz }
        });
        void initializeCoordinates(CubeView<Translation<double>> coords)
        {
            int len = CubeView<Translation<double>>.sideLength;

            for (int i = 0; i < len; ++i)
                for (int j = 0; j < len; ++j)
                    for (int k = 0; k < len; ++k)
                    {
                        Matrix current = new Matrix(new double[,] { {i}, {j}, {k} });
                        Matrix projected = projection * current;

                        Translation<double> target = coords[i, j, k];
                        target.x = projected[0, 0];
                        target.y = projected[1, 0];
                        target.z = projected[2, 0];
                    }
        }

        Border selected = null;
        bool justSelected = false;
        bool stoppingDueToGesture = false;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Twistidoo.Begin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void cell_Hold(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Hold me!");
            Border cell;

            if (e.OriginalSource.GetType() == typeof(Border))
                cell = e.OriginalSource as Border;
            else if (e.OriginalSource.GetType() == typeof(TextBlock))
                cell = (e.OriginalSource as TextBlock).Parent as Border;
            else if (e.OriginalSource.GetType() == typeof(Rectangle))
                cell = cellFromTapPad(e.OriginalSource as Rectangle);
            else 
            {
                Debug.WriteLine("Holding unexpected type {0}", e.OriginalSource.GetType());
                return;
            }

            cell_Tap(cell, null);
            //int x = (int)cell.Resources["HomeX"];
            //int y = (int)cell.Resources["HomeY"];
            //int z = (int)cell.Resources["HomeZ"];

            //if (isGlowing)
            //    return;

            SliderInOut.Begin();
            // highlightNextPlane();
        }

        private XYZ getCellXYZ(Border cell)
        {
            return new XYZ(
                (int)cell.Resources["HomeX"],
                (int)cell.Resources["HomeY"],
                (int)cell.Resources["HomeZ"]);
        }

        private XYZ getPadXYZ(Rectangle pad)
        {
            return new XYZ(
                (int)pad.Resources["HomeX"],
                (int)pad.Resources["HomeY"],
                (int)pad.Resources["HomeZ"]);
        }

        private SolidColorBrush makeFgColor(Border cell, int? xIn = null, int? yIn = null, int? zIn = null)
        {
            XYZ xyz = null;
            if (xIn == null || yIn == null || zIn == null)
                xyz = getCellXYZ(cell);

            int x = xIn ?? xyz.x;
            int y = yIn ?? xyz.y;
            int z = zIn ?? xyz.z;

            // Debug.WriteLine("Making a foreground color for ({0}, {1}, {2})", x, y, z);

            if (isPreset[x, y, z].value)
            {   // white
                return new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            }
            else if (wrongers[x, y, z].Count > 0)
            {   // red
                Debug.WriteLine("({0}, {1}, {2}) is red now", x, y, z);
                return new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0, 0));
            }
            else // non-preset cell that isn't wrong
            {   // green
                return new SolidColorBrush(Color.FromArgb(0xFF, 38, 0xFF, 0));
            }
        }

        private void unhighlight(Mappers.PointPredicate points, bool darken)
        {
            Mappers.forEachThat(cells.original,
                    points,
                    (Border c) => {
                        XYZ p = getCellXYZ(c);
                        c.Background = bgColors[p.x, p.y, p.z];
                        c.Opacity = darken ? 0.6 : 1.0;
                        (c.Child as TextBlock).Foreground = makeFgColor(c, p.x, p.y, p.z);
                    });
        }

        private void unhighlight()
        {
            if (!isGlowing)
                return;

            Mappers.PointPredicate highlightedNow = currentPlane.value.predicate;
            Mappers.PointPredicate theRest = (i,j,k) => !highlightedNow(i,j,k);

            unhighlight(highlightedNow, false);

            // And brighten up the unhighlighted ones.
            Mappers.forEachThat(cells.original, theRest,
                                (Border c) => c.Opacity = 1.0);

            isGlowing = false;
        }

        private void highlightNextPlane(bool backwards = false)
        {
            Mappers.PointPredicate oldPred;
            Mappers.PointPredicate newPred;

            if (currentPlane == null)
            {
                currentPlane = new CyclicIterator<Plane>(planes);
                newPred = currentPlane.value.predicate;
                oldPred = (i,j,k) => !newPred(i,j,k);
            }
            else
            {
                oldPred = currentPlane.value.predicate;
                if (backwards)
                    --currentPlane;
                else
                    ++currentPlane;
                newPred = currentPlane.value.predicate;

                if (!isGlowing) // darken the rest if we weren't glowing
                    oldPred = (i,j,k) => !newPred(i,j,k);
            }

            // Unhighlight old ones.
            unhighlight( (i ,j, k) => oldPred(i, j, k) && !newPred(i, j, k), 
                        true);

            // Highlight new ones.
            Mappers.forEachThat(cells.original,
                                (i, j, k) => newPred(i, j, k) && !oldPred(i, j, k),
                                (Border c) => { 
                                    c.Opacity = 1.0;
                                    c.Background = new SolidColorBrush(Colors.Cyan);
                                    (c.Child as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 47, 79, 79));
                                });

            isGlowing = true;
        }

        private void storyBoard_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("Completed handler called");
            stopRotation();
        }

        private void startRotation(Direction dir)
        {
            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, dragCoordinates, Mappers.setFromOfAnimation);

            switch(dir) {
                case Direction.Left:  foreach (var cube in rotationSubscribers) cube.rotateLhY();
                                      break;
                case Direction.Right: foreach (var cube in rotationSubscribers) cube.rotateRhY(); 
                                      break;
                case Direction.Up:    foreach (var cube in rotationSubscribers) cube.rotateRhX();
                                      break;
                case Direction.Down:  foreach (var cube in rotationSubscribers) cube.rotateLhX();
                                      break;
            }
            
            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, coordinates, Mappers.setToOfAnimation);
            Twistidoo.Begin();
        }

        private void startReturn()
        {
            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, dragCoordinates, Mappers.setFromOfAnimation);
            Mappers.forEach<TranslationAnimation, Translation<double>>(animations, coordinates, Mappers.setToOfAnimation);
            Twistidoo.Begin();
        }

        private void stopRotation()
        {
            Mappers.forEach<CellPlacer, Translation<double>>(places, coordinates, Mappers.setPlace);
            Mappers.forEach<Translation<double>, Translation<double>>(dragCoordinates, coordinates, Mappers.setEqual);
            Twistidoo.Stop();
            if (stoppingDueToGesture)
                stoppingDueToGesture = false;
            else
                background_anim.Resume();
        }

        private void doRotation(Direction dir)
        {
            if (Twistidoo.GetCurrentState() != ClockState.Stopped)
            {
                Debug.WriteLine("Stopping...");
                stoppingDueToGesture = true;  // necessary here?
                stopRotation();
                stoppingDueToGesture = false;
            }

            Debug.WriteLine("Starting...");
            startRotation(dir);
        }

        private void setAnimationTime(double seconds = 2.0)
        {
            Mappers.forEach(animations, Mappers.setAnimDuration(seconds));
        }

        private double animationTime {
            set { setAnimationTime(value); }
            get { return Mappers.animationDurationSeconds; }
        }

        private void unselectCell()
        {
            if (selected != null)
            {
                selected.BorderThickness = new Thickness(0);
            }
            selected = null;
        }

        // TODO: when the numbers aren't hard-coded, this will be useless.
        bool presetCell(Border cell)
        {
            bool ret = cell.Resources["isPreset"].ToString() == "True";
            return ret;
        }

        private Storyboard bounceCell(Border cell, EventHandler completedHandler)
        {
             PlaneProjection proj = cell.Projection as PlaneProjection;

            // (cell.Stroke as System.Windows.Media.SolidColorBrush).Color = Color.FromArgb(255, 255, 0, 0);
            
            double currentOffset = proj.GlobalOffsetZ;

            Duration duration = new Duration(TimeSpan.FromSeconds(0.6));
            Storyboard boing = new Storyboard();
            boing.FillBehavior = FillBehavior.Stop;

            DoubleAnimationUsingKeyFrames zBounce = new DoubleAnimationUsingKeyFrames();
            zBounce.Duration = duration;
            zBounce.BeginTime = new TimeSpan(0);

            LinearDoubleKeyFrame zStart = new LinearDoubleKeyFrame();
            zStart.Value = currentOffset;
            zStart.KeyTime = new TimeSpan(0);

            zBounce.KeyFrames.Add(zStart);

            EasingDoubleKeyFrame zIn = new EasingDoubleKeyFrame();
            zIn.EasingFunction = new BounceEase();
            zIn.Value = currentOffset + 300;
            zIn.KeyTime = new TimeSpan(0, 0, 0, 0, 250);

            zBounce.KeyFrames.Add(zIn);

            LinearDoubleKeyFrame zStay = new LinearDoubleKeyFrame();
            zStay.Value = zIn.Value;
            zStay.KeyTime = new TimeSpan(0, 0, 0, 0, 500);

            zBounce.KeyFrames.Add(zStay);

            LinearDoubleKeyFrame zBack = new LinearDoubleKeyFrame();
            zBack.Value = zStart.Value;
            zBack.KeyTime = new TimeSpan(0, 0, 0, 0, 600);

            zBounce.KeyFrames.Add(zBack);

            boing.Duration = duration;
            boing.Children.Add(zBounce);

            Storyboard.SetTarget(zBounce, proj);
            PropertyPath path = new PropertyPath("(PlaneProjection.GlobalOffsetZ)");
            Storyboard.SetTargetProperty(zBounce, path);

            boing.Completed += completedHandler;
            return boing;
        }

        private void cell_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Note: e is not used.

            Border cell = sender as Border;
            TextBlock block = cell.Child as TextBlock;
            XYZ p = getCellXYZ(cell);

            Debug.WriteLine("Tapped cell at ({0}, {1}, {2})", p.x, p.y, p.z); 

            if (cell == selected)
            {
                if (!presetCell(cell))
                {
                    if (block.Text.Length == 0)
                    {
                        block.Text = (numPicker.SelectedIndex + 1).ToString();
                        numbers[p.x, p.y, p.z].value = numPicker.SelectedIndex + 1;
                    }
                    else
                    {
                        block.Text = "";
                        numbers[p.x, p.y, p.z].value = 0;
                    }

                    if (!pickerShowing)
                        PickerInOut.Begin();
                }
                else
                {
                    if (pickerShowing)
                        PickerInOut.Begin();
                }
            }
            else
            {
                unselectCell();
                if (presetCell(cell))
                {
                    selected = cell;
                    if (pickerShowing)
                        PickerInOut.Begin();
                }
                else
                {
                    selected = cell;
                    string numberText = block.Text;
                    if (numberText.Length != 0)
                        numPicker.SelectedIndex = int.Parse(numberText) - 1;
                    if (!pickerShowing)
                        PickerInOut.Begin();
                }
            }

            checkCorrectness(cell);
            
            Storyboard boing = bounceCell(cell, new EventHandler(boing_Completed));
            try
            {
                // TODO: Check for any disagreements and make their borders thinly red
                cell.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 100, 149, 237));
                cell.BorderThickness = new Thickness(3);
                justSelected = true;
                boing.Begin();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}", ex);
            }

            Debug.WriteLine("Tippy tap on {0}", cell.Name);
        }

        void boing_Completed(object sender, EventArgs e)
        {
            Debug.WriteLine("The bouncy cell animation just completed.");
        }

        private bool isInRightSliderRegion(Object originalSource, Point manipOrigin)
        {
            UIElement source = originalSource as UIElement;
            var tran = source.TransformToVisual(this);
            Point actualOrigin = tran.Transform(manipOrigin);

            return inUIElement(actualOrigin, rightSliderRegion);
        }

        private void PhoneApplicationPage_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            Debug.WriteLine("Begin gesture.");
            currentGesture = Gesture.Unknown;

            //UIElement source = e.OriginalSource as UIElement;
            //var tran = source.TransformToVisual(this);
            //Point manipOrigin = tran.Transform(e.ManipulationOrigin);

            //if (inUIElement(manipOrigin, rightSliderRegion))
            if (isInRightSliderRegion(e.OriginalSource, e.ManipulationOrigin))
            {
                Debug.WriteLine("We're in slider territory.");
                currentGesture = Gesture.SwipeEdge;
            }

            background_anim.Pause();
        }

        private void PhoneApplicationPage_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            //Debug.WriteLine("PinchManipulation    {0}", Newtonsoft.Json.JsonConvert.SerializeObject(e.PinchManipulation));
            //Debug.WriteLine("DeltaManipulation    {0}", Newtonsoft.Json.JsonConvert.SerializeObject(e.DeltaManipulation));

            if (currentGesture == Gesture.SwipeEdge)
            {
                if (isInRightSliderRegion(e.OriginalSource, e.ManipulationOrigin))
                {
                    Debug.WriteLine("We're still in slider territory.");
                    return;
                }
                else
                    currentGesture = Gesture.Unknown;
            }

            if (Twistidoo.GetCurrentState() != ClockState.Stopped)
            {
                Debug.WriteLine("Stopping...");
                stoppingDueToGesture = true;
                stopRotation();
            }

            double scale = 0.25;
            const int len = 3;
            foreach (int i in Enumerable.Range(0, len))
                foreach (int j in Enumerable.Range(0, len))
                    foreach (int k in Enumerable.Range(0, len))
                    {
                        Translation<double> point = dragCoordinates.original[i, j, k];
                        Translation<double> orig = coordinates.original[i,j,k];

                        if (e.PinchManipulation == null)
                        {
                            currentGesture = Gesture.DragCube;

                            // If the user is dragging the cube around, move the front
                            // and back planes in opposite directions.
                            switch (k) {
                                case 0: point.x += e.DeltaManipulation.Translation.X * scale;
                                        point.y += e.DeltaManipulation.Translation.Y * scale;
                                        break;
                                case 2: point.x -= e.DeltaManipulation.Translation.X * scale;
                                        point.y -= e.DeltaManipulation.Translation.Y * scale;
                                        break;
                            }
                        }
                        else
                        {
                            currentGesture = Gesture.PinchCube;

                            // If the user is pinching the cube, separate the outer
                            // (top, bottom, left, right) planes from the center,
                            // or bring them closer.

                            double yDiff = Math.Abs(e.PinchManipulation.Current.PrimaryContact.Y  - e.PinchManipulation.Current.SecondaryContact.Y) -
                                           Math.Abs(e.PinchManipulation.Original.PrimaryContact.Y - e.PinchManipulation.Original.SecondaryContact.Y);

                            double xDiff = Math.Abs(e.PinchManipulation.Current.PrimaryContact.X  - e.PinchManipulation.Current.SecondaryContact.X) -
                                           Math.Abs(e.PinchManipulation.Original.PrimaryContact.X - e.PinchManipulation.Original.SecondaryContact.X);

                           switch (i) {
                                case 0: point.x = orig.x - xDiff * scale;
                                        break;
                                case 2: point.x = orig.x + xDiff * scale;
                                        break;
                            }
                            switch (j) {
                                case 0: point.y = orig.y + yDiff * scale;
                                        break;
                                case 2: point.y = orig.y - yDiff * scale;
                                        break;
                            }
                        }
                    }
            Mappers.forEach<CellPlacer, Translation<double>>(places, dragCoordinates, Mappers.setPlace);
        }

        // Length of hypotenuse
        private double hypot(double a, double b)
        {
            return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
        }

        private void PhoneApplicationPage_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Debug.WriteLine("End of gesture.");

            if (e.FinalVelocities == null)
            {
                if (currentGesture == Gesture.DragCube || currentGesture == Gesture.PinchCube)
                {
                    Debug.WriteLine("Returning to center");
                    startReturn();
                }
                return; // Not for us
            }

            Debug.WriteLine("Total scale {0}; translation {1}; isInertial {2}; expansionVelocity {3}; linearVelocity {4}",
                                               e.TotalManipulation.Scale, e.TotalManipulation.Translation, e.IsInertial,
                                               e.FinalVelocities.ExpansionVelocity, e.FinalVelocities.LinearVelocity);  
            
            double speed = hypot(e.FinalVelocities.LinearVelocity.X, e.FinalVelocities.LinearVelocity.Y);

            if (currentGesture == Gesture.SwipeEdge)
            {
                Debug.WriteLine("Congrats. You swiped the side with speed={0}", speed);
                if (speed > 100)
                    highlightNextPlane(e.FinalVelocities.LinearVelocity.Y > 0);

                return;
            }

            if (speed < /*300*/ 400) // TODO: come up for a more solid policy on speed.
            {
                Debug.WriteLine("That gesture was too slow. Not going to rotate.");
                if (currentGesture == Gesture.DragCube || currentGesture == Gesture.PinchCube)
                {
                    Debug.WriteLine("Returning to center");
                    startReturn();
                }
                else // No animation to wait for, so continue the background one.
                {
                    background_anim.Resume();
                }
            }
            else
            {
                double scale = 1.0;
                double seconds = scale * 5.0;
                
                if (speed > 500)
                    seconds = scale * (speed / (-1250) + 50.0 / 125 + 3);
                if (speed > 3000)
                    seconds = scale * 1.0;

                Debug.WriteLine("Speed {0} yields duration {1} seconds.", speed, seconds);

                if (Math.Abs(e.TotalManipulation.Translation.X) > Math.Abs(e.TotalManipulation.Translation.Y))
                {
                    animationTime = seconds;
                    if (e.TotalManipulation.Translation.X < 0)
                        doRotation(Direction.Left);
                    else
                        doRotation(Direction.Right);
                }
                else if (Math.Abs(e.TotalManipulation.Translation.Y) > Math.Abs(e.TotalManipulation.Translation.X))
                {
                    animationTime = seconds;
                    if (e.TotalManipulation.Translation.Y > 0)
                        doRotation(Direction.Up);
                    else
                        doRotation(Direction.Down);
                }
                else
                {
                    Debug.WriteLine("Can't find the greater translation direction. Not going to rotate.");
                    startReturn();
                }
            }

            currentGesture = Gesture.None;
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            Debug.WriteLine("Page orientation just changed.");
        }

        bool inUIElement(Point p, UIElement elem)
        {
            var tran = elem.TransformToVisual(this);
            Point elemPos = tran.Transform(new Point(0, 0));
            Size s = elem.RenderSize;

            Debug.WriteLine("p = {0} while the element's origin is {1} and has size {2}",
                            p, elemPos, s);

            return p.X > elemPos.X && p.X < elemPos.X + s.Width &&
                   p.Y > elemPos.Y && p.Y < elemPos.Y + s.Height;
        }

        bool inUIElement(System.Windows.Input.GestureEventArgs tap, UIElement elem)
        {
            Point p = tap.GetPosition(elem);
            Size s = elem.RenderSize;

            Debug.WriteLine("Tap at {0} relative to object. Width={1} Height={2}",
                            p, s.Width, s.Height);
            
            return p.X > 0 && p.X < s.Width &&
                   p.Y > 0 && p.Y < s.Height;
        }

        bool inCube(System.Windows.Input.GestureEventArgs tap)
        {
            return inUIElement(tap, TheCube);
        }

        bool inPicker(System.Windows.Input.GestureEventArgs tap)
        {
            return inUIElement(tap, numPicker);
        }

        private void PhoneApplicationPage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Debug.WriteLine("Was the tap within the cube? {0}", inCube(e));

            if (justSelected == true)
            {
                Debug.WriteLine("Just selected: not going to change UI.");
                justSelected = false;
            }
            else
            {       
                if (inCube(e))
                {
                    if (pickerShowing)
                        PickerInOut.Begin();
                    unhighlight();
                    unselectCell();
                }
                else if (inUIElement(e, rightSliderRegion))
                {
                    unhighlight();
                    unselectCell();
                }
                else if (pickerShowing && inPicker(e))
                {
                    Debug.WriteLine("I'm gonna kill you! OOGA BOOGA BOOGA BOOGA!");
                    double x = e.GetPosition(numPicker).X;
                    int currentIndex = numPicker.SelectedIndex;
                    double placeCloseTo = 9.0 * x / numPicker.RenderSize.Width + 0.5;
                    Debug.WriteLine("relative x={0}, ratio of width={1}, out of nine={2}, selected index={3}",
                                    x, 
                                    x / numPicker.RenderSize.Width,
                                    placeCloseTo,
                                    currentIndex);

                    numPicker.SelectedIndex = (int)Math.Round(placeCloseTo) - 1;
                }
                else if(!pickerShowing && inUIElement(e, numPickerRegion))
                {
                    Debug.WriteLine("You can't see the picker, but you can feel it.");
                    unhighlight();
                    unselectCell();
                }

                if (sliderShowing)
                    SliderInOut.Begin();
            }
        }

        private void ApplicationBarIconButton_Click_0(object sender, EventArgs e)
        {
            doRotation(Direction.Left);
        }
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            doRotation(Direction.Right);
        }
        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            doRotation(Direction.Up);
        }
        private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            doRotation(Direction.Down);
        }

        private void checkCorrectness(Border cell)
        {
            XYZ p = getCellXYZ(cell);
            int num = numbers[p.x, p.y, p.z].value;

            Debug.WriteLine("Checking correctness of cell at ({0}, {1}, {2}). It has value {3}", 
                            p.x, p.y, p.z, num);

            HashSet<XYZ> dupes = new HashSet<XYZ>(comparePoints);

            if (num == 0)
            {
                Debug.WriteLine("({0},{1},{2}) is zero", p.x, p.y, p.z);
                // Can't conflict with anything, since it doesn't have a number.
                // (Zero is the value for a blank cell).
                wrongers[p.x, p.y, p.z].Clear();
                Mappers.forEach(wrongers, (HashSet<XYZ> set) => set.Remove(p));
            }
            else
            {
                var checkSpace = planesThrough[p.x, p.y, p.z];

                // Find the coordinates of related cells that have the same value as this one.
                foreach(Plane plane in checkSpace)
                {
                    foreach(var point in plane.points)
                    {
                        if ( ! comparePoints.Equals(point, p)
                            && numbers[point.x, point.y, point.z].value == num)
                        {
                            Debug.WriteLine("I found a dupe ({0}, {1}, {2}) in plane {3}", point.x, point.y, point.z, plane.name);
                            dupes.Add(point);
                        }
                    }
                }

                Debug.WriteLine("Found {0} distinct dupes.", dupes.Count);
                //HashSet<XYZ> myWronger = wrongers[p.x, p.y, p.z];
                //myWronger.Clear();
                //myWronger.UnionWith(dupes);
                wrongers[p.x, p.y, p.z] = dupes;

                if (dupes.Count > 0) // Have to mark the dupes
                {
                    foreach (var point in dupes)
                    {
                        Debug.WriteLine("Found distinct dupe ({0}, {1}, {2})", point.x, point.y, point.z);
                        wrongers[point.x, point.y, point.z].Add(p);
                    }
                    // Clear p from the others
                    Mappers.forEachThat(wrongers,
                        (i, j, k) => ! dupes.Contains(new XYZ(i, j, k)),
                        (HashSet<XYZ> other) => other.Remove(p));
                }
            }
            // Debug.WriteLine("Break! Examine this.wrongers at this point.");
            Mappers.forEach(cells.original,
                            (Border c) => {
                                (c.Child as TextBlock).Foreground = makeFgColor(c);
                            });
        }
  
        bool numPickerChangedBefore = false;
        private void numPickerChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: keep track of when any cell number changes:
            //  null --> number
            //  number --> null
            //  number --> different number
            //
            if (numPickerChangedBefore)
            {
                justSelected = true;
                if (selected != null)
                {
                    int newNum = numPicker.SelectedIndex + 1;
                    XYZ selectedXYZ = getCellXYZ(selected);
                    numbers[selectedXYZ.x, selectedXYZ.y, selectedXYZ.z].value = newNum;

                    checkCorrectness(selected); // checks numbers
                    (selected.Child as TextBlock).Text = (newNum).ToString(); // set text to number

                    // VibrationDevice.GetDefault().Vibrate(TimeSpan.FromSeconds(0.125));
                }
            }
            else
            {
                Debug.WriteLine("THERE'S A FIRST TIME FOR EVERYTHING");
                numPickerChangedBefore = true; // Don't do anything the first time.
            }
        }
    
        private void slider_valueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            planeSelectSliderRight.Value = (int)e.NewValue;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // background_pan.Begin();

            animationTime = 3.0;
            doRotation(Direction.Up);

            doAfter(() => {
                doRotation(Direction.Down);
            }, 500);
        }

        private void numPicker_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("numPicker loaded.");
        }

        private void TitlePanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Debug.WriteLine("TitlePanel was tapped");
        }

        private void SliderInOut_Completed(object sender, EventArgs e)
        {
            DoubleAnimation anim = SliderInOut.Children[0] as DoubleAnimation;
            Nullable<double> temp = anim.From;
            anim.From = anim.To;
            anim.To = temp;

            sliderShowing = !sliderShowing;
        }

        private void PickerInOut_Completed(object sender, EventArgs e)
        {
            DoubleAnimation anim = PickerInOut.Children[0] as DoubleAnimation;
            Nullable<double> temp = anim.From;
            anim.From = anim.To;
            anim.To = temp;

            pickerShowing = !pickerShowing;
        }

        private void PageCanvas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Debug.WriteLine("Canvas tap");
        }

        private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Debug.WriteLine("LayoutRoot tap");
        }

        private Border cellFromTapPad(Rectangle pad)
        {
            XYZ p = getPadXYZ(pad);
            return cells.mirror[p.x, p.y, p.z];
        }

        private void nut_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Rectangle rec = sender as Rectangle;
            cell_Tap(cellFromTapPad(rec), null);
        }
    }
}