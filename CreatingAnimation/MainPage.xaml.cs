using System;
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
using Debug = System.Diagnostics.Debug;
using TranslationAnimation = Cuboku.Translation<System.Windows.Media.Animation.DoubleAnimation>;
using Rectangle = System.Windows.Shapes.Rectangle;

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
        MirroredCubeView<SolidColorBrush> colors;
        MirroredCubeView<Border> cells;

        RotatableCovariant[] rotationSubscribers;

        bool sliderShowing = false;
        bool pickerShowing = false;
        bool isGlowing = false;

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

            colors = new MirroredCubeView<SolidColorBrush>(
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

            coordinates = new MirroredCubeView<Translation<double>>();
            initializeCoordinates(coordinates);
            dragCoordinates = new MirroredCubeView<Translation<double>>();
            Mappers.forEach<Translation<double>, Translation<double>>(dragCoordinates, coordinates, Mappers.setEqual);

            rotationSubscribers = new RotatableCovariant[] { coordinates, dragCoordinates, cells };
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
        bool dragging = false;
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

            if (isGlowing)
                return;
            // Otherwise...

            isGlowing = !isGlowing;
            SliderInOut.Begin();

            int x = (int)cell.Resources["HomeX"];
            int y = (int)cell.Resources["HomeY"];
            int z = (int)cell.Resources["HomeZ"];

            // Mappers.forEachThat(places, Mappers.inYZDiag, (CellPlacer place) => { place.z += 200; });
            // Mappers.forEachThat(colors, Mappers.inYZDiag, (SolidColorBrush brush) => { brush.Color = Colors.Cyan; });

            Mappers.forEachThat(cells.original, Mappers.inYZDiag,
                                (Border c) => { 
                                                c.Background = new SolidColorBrush(Colors.Cyan);
                                                (c.Child as TextBlock).Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 47, 79, 79)); 
                                });
            Mappers.forEachThat(cells.original, 
                                (i, j, k) => !Mappers.inYZDiag(i, j, k), 
                                (Border c) => { c.Opacity = 0.6; });
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

        bool presetCell(Border cell)
        {
            // TODO: Use cell.Resources[Home*] instead of colors
            //Color cellColor = ((cell.Child as TextBlock).Foreground as SolidColorBrush).Color;
            //Color userSuppliedColor = Color.FromArgb(0xFF, 0x70, 0x80, 0x90);
            //bool isPreset = ! Color.Equals(cellColor, userSuppliedColor);
            //Debug.WriteLine("isPreset={0} cell color={1} user supplied color={2}", isPreset, cellColor, userSuppliedColor);
            //return isPreset;
            bool ret = cell.Resources["isPreset"].ToString() == "True";
            Debug.WriteLine("Is this cell preset? {0}", ret);
            return ret;
        }

        private void cell_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // Note: e is not used.

            Border cell = sender as Border;
            TextBlock block = cell.Child as TextBlock;
            if (cell == selected)
            {
                if (!presetCell(cell))
                {
                    if (block.Text.Length == 0)
                        block.Text = (numPicker.SelectedIndex + 1).ToString();
                    else
                        block.Text = "";
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

            boing.Completed += new EventHandler(boing_Completed);

            try
            {
                // cell.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
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

        private void PhoneApplicationPage_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            Debug.WriteLine("Begin gesture.");
            background_anim.Pause();
        }

        DateTime lastDelta;
        private void PhoneApplicationPage_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            DateTime now = DateTime.Now;
            
            Debug.WriteLine("PinchManipulation    {0}", Newtonsoft.Json.JsonConvert.SerializeObject(e.PinchManipulation));
            Debug.WriteLine("DeltaManipulation    {0}", Newtonsoft.Json.JsonConvert.SerializeObject(e.DeltaManipulation));

            if (lastDelta == null)
                lastDelta = now;

            double secs = (now - lastDelta).TotalSeconds;
            lastDelta = now; // for next time

            if (Twistidoo.GetCurrentState() != ClockState.Stopped)
            {
                Debug.WriteLine("Stopping...");
                stoppingDueToGesture = true;
                stopRotation();
            }

            dragging = true;
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
                            // If the user is pinching the cube, separate the outer
                            // (top, bottom, left, right) planes from the center,
                            // or bring them closer.
                            //double scaleX = e.DeltaManipulation.Scale.X;
                            //double scaleY = e.DeltaManipulation.Scale.Y;
                            //if (i == 0) {
                            //    point.x /= scaleX; // point.x = orig.x / scaleX;
                            //}
                            //else if (i == 2) {
                            //    point.x *= scaleX; // point.x = orig.x * scaleX;
                            //}

                            //if (j == 0) {
                            //    point.y *= scaleY; // point.y = orig.y * scaleY;
                            //}
                            //else if (j == 2) {
                            //    point.y /= scaleY; // point.y = orig.y / scaleY;
                            //}

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
                if (dragging == true)
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
            if (speed < /*300*/ 400) // TODO: come up for a more solid policy on speed.
            {
                Debug.WriteLine("That gesture was too slow. Not going to rotate.");
                if (dragging == true)
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

            dragging = false; // The manipulation just finished, so the user is not dragging now.
        }

        private void PhoneApplicationPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            Debug.WriteLine("Page orientation just changed.");
        }

        bool inCube(System.Windows.Input.GestureEventArgs tap)
        {
            Point p = tap.GetPosition(TheCube);
            Size s = TheCube.RenderSize;

            Debug.WriteLine("Tap at {0} relative to cube. Width={1} Height={2}",
                            p, s.Width, s.Height);
            
            return p.X > 0 && p.X < s.Width &&
                   p.Y > 0 && p.Y < s.Height;
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

        bool numPickerChangedBefore = false;
        private void numPickerChanged(object sender, SelectionChangedEventArgs e)
        {
            if (numPickerChangedBefore)
            {
                justSelected = true;
                if (selected != null)
                {
                    (selected.Child as TextBlock).Text = (numPicker.SelectedIndex + 1).ToString();
                }
            }
            else
                numPickerChangedBefore = true; // Don't do anything the first time.
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
            int x = (int)pad.Resources["HomeX"];
            int y = (int)pad.Resources["HomeY"];
            int z = (int)pad.Resources["HomeZ"];

            return cells.mirror[x, y, z];
        }

        private void nut_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Rectangle rec = sender as Rectangle;
            cell_Tap(cellFromTapPad(rec), null);
        }
    }
}