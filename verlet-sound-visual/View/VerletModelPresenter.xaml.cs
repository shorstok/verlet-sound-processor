using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using verlet_sound_visual.Verlet;
using verlet_sound_visual.Vm;

namespace verlet_sound_visual.View
{
    /// <summary>
    /// Interaction logic for VerletModelPresenter.xaml
    /// </summary>
    public partial class VerletModelPresenter : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
            "Model", typeof(VerletModel), typeof(VerletModelPresenter), new PropertyMetadata(default(VerletModel),ModelChanged));

        private static void ModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(!(d is VerletModelPresenter presenter))
                return;

            presenter.SimulatedModel = e.NewValue as VerletModel;
        }

        public VerletModel Model
        {
            get => (VerletModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }
        
        public static readonly DependencyProperty SimulatedModelProperty = DependencyProperty.Register(
            "SimulatedModel", typeof(VerletModel), typeof(VerletModelPresenter), new PropertyMetadata(default(VerletModel)));

        public VerletModel SimulatedModel
        {
            get => (VerletModel) GetValue(SimulatedModelProperty);
            set => SetValue(SimulatedModelProperty, value);
        }

        public static readonly DependencyProperty ShowSimulationProperty = DependencyProperty.Register(
            "ShowSimulation", typeof(bool), typeof(VerletModelPresenter), new PropertyMetadata(true, OnShowSimulationChanged));

        private static void OnShowSimulationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(!(d is VerletModelPresenter presenter))
                return;

            presenter._cancellationTokenSource.Cancel();
        }

        public bool ShowSimulation
        {
            get { return (bool) GetValue(ShowSimulationProperty); }
            set { SetValue(ShowSimulationProperty, value); }
        }

        public ICommand RunSimulationCommand { get; }
        public VerletModelPresenter()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            RunSimulationCommand = new DelegateCommand(t => Model != null, RunSimulationAsync);
        }

        private VerletModel ActiveModel => ShowSimulation ? SimulatedModel : Model;
        
        WriteableBitmap entitiesBitmap, inputBitmap, outputBitmap;


        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if(!IsLoaded)
                return;

            if (null == entitiesBitmap)
            {
                entitiesBitmap = BitmapFactory.New((int) ActiveModel.XSpan, (int) ActiveModel.YSpan);
                entitiesBitmap.Clear(Colors.Black);
                SimRenderTarget.Source = entitiesBitmap;
            }

            if (null == inputBitmap || Math.Abs(inputBitmap.Width - InputRenderTarget.ActualWidth) > 1)
            {
                inputBitmap = BitmapFactory.New((int) InputRenderTarget.ActualWidth, (int) InputRenderTarget.ActualHeight);
                inputBitmap.Clear(Colors.Black);
                InputRenderTarget.Source = inputBitmap;
            }

            
            if (null == outputBitmap || Math.Abs(outputBitmap.Width - OutputRenderTarget.ActualWidth) > 1)
            {
                outputBitmap = BitmapFactory.New((int) OutputRenderTarget.ActualWidth, (int) OutputRenderTarget.ActualHeight);
                outputBitmap.Clear(Colors.Black);
                OutputRenderTarget.Source = outputBitmap;
            }

            using (entitiesBitmap.GetBitmapContext())
            {
                entitiesBitmap.Clear(Colors.Black);

                RenderConstraints();
                RenderEntities();
                RenderGraphs();
            }

            
        }

        public int SimResultsCountMax = 10;

        private void RenderGraphs()
        {
            int xc = 0;

            SimResultsCountMax = Math.Min(inputBitmap.PixelWidth, outputBitmap.PixelWidth);

            inputBitmap.Clear(Colors.Black);
            outputBitmap.Clear(Colors.Black);

            foreach (var result in _simResults)
            {
                xc++;

                var pxInputValue = inputBitmap.PixelHeight / 2.0 + result.Item1 * (inputBitmap.PixelHeight-1) / 2;
                var pxOutputValue = outputBitmap.PixelHeight / 2.0 + result.Item2 * (outputBitmap.PixelHeight-1) / 2;

                if (pxInputValue < 0)
                    pxInputValue = 0;
                if (pxOutputValue < 0)
                    pxOutputValue = 0;
                if (pxInputValue >= inputBitmap.PixelHeight - 1)
                    pxInputValue = inputBitmap.PixelHeight - 1;
                if (pxOutputValue >= outputBitmap.PixelHeight - 1)
                    pxOutputValue = outputBitmap.PixelHeight - 1;

                inputBitmap.SetPixel(xc,(int) pxInputValue,Colors.GreenYellow);
                outputBitmap.SetPixel(xc,(int) pxOutputValue,Colors.GreenYellow);

                if(xc>=inputBitmap.PixelWidth)
                    break;
            }

        }

        private void RenderEntities()
        {
            foreach (var entity in ActiveModel.Entities)
            {
                switch (entity)
                {
                    case ISensor deltaMovementSensor:
                        entitiesBitmap.FillEllipseCentered((int) entity.X,
                            (int) entity.Y,
                            (int) 3,
                            (int) 3,
                            Colors.GreenYellow);
                        break;
                    case DirectionalStaticActuator dsa:
                        entitiesBitmap.FillEllipseCentered((int) entity.X,
                            (int) entity.Y,
                            (int) 3,
                            (int) 3,
                            Colors.Red);

                        entitiesBitmap.DrawLineAa(
                            (int) (dsa.OriginX - dsa.ActuatorDirectionX * dsa.Amplitude),
                            (int) (dsa.OriginY - dsa.ActuatorDirectionY * dsa.Amplitude),
                            (int) (dsa.OriginX + dsa.ActuatorDirectionX * dsa.Amplitude),
                            (int) (dsa.OriginY + dsa.ActuatorDirectionY * dsa.Amplitude),
                            Color.FromArgb(255,81,227,255),
                            1);

                        break;
                    case IActuator actuator:
                        entitiesBitmap.FillEllipseCentered((int) entity.X,
                            (int) entity.Y,
                            (int) 3,
                            (int) 3,
                            Colors.Red);
                        break;
                    default:
                        entitiesBitmap.FillEllipseCentered((int) entity.X,
                            (int) entity.Y,
                            (int) 3,
                            (int) 3,
                            Colors.DarkMagenta);
                        break;
                }
            }
        }

        private void RenderConstraints()
        {
            foreach (var entity in ActiveModel.Entities)
            {
                foreach (var constraint in entity.Constraints)
                {
                    switch (constraint)
                    {
                        case FixedLinkConstraint fixedLinkConstraint:
                            entitiesBitmap.DrawLineAa(
                                (int) constraint.Source.X,
                                (int) constraint.Source.Y,
                                (int) constraint.Target.X,
                                (int) constraint.Target.Y,
                                Colors.DarkGray,
                                3);
                            break;
                        case InelasticBallConstraint inelasticBallConstraint:
                            entitiesBitmap.DrawEllipseCentered((int) constraint.Source.X,
                                (int) constraint.Source.Y,
                                (int) inelasticBallConstraint.Radius,
                                (int) inelasticBallConstraint.Radius,
                                Colors.White);
                            break;
                        case ElasticLinkConstraint springLinkConstraint:
                            entitiesBitmap.DrawLineAa(
                                (int) constraint.Source.X,
                                (int) constraint.Source.Y,
                                (int) constraint.Target.X,
                                (int) constraint.Target.Y,
                                Colors.GreenYellow,
                                1);
                            break;
                        case FixedSpringConstraint fixedSpringConstraint:
                            entitiesBitmap.DrawLineAa(
                                (int) constraint.Source.X,
                                (int) constraint.Source.Y,
                                (int) fixedSpringConstraint.X0,
                                (int) fixedSpringConstraint.Y0,
                                Colors.AntiqueWhite,
                                1);
                            break;
                        case SpringLinkConstraint springConstraint:
                            entitiesBitmap.DrawLineAa(
                                (int) constraint.Source.X,
                                (int) constraint.Source.Y,
                                (int) constraint.Target.X,
                                (int) constraint.Target.Y,
                                Colors.DarkGreen,
                                1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(constraint));
                    }
                }
            }
        }

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private Thread _simulatorThread = null;

        private readonly ConcurrentQueue<Tuple<double,double>> _simResults = new ConcurrentQueue<Tuple<double, double>>();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private async void RunSimulationAsync(object obj)
        {
            ShowSimulation = true;

            _cancellationTokenSource.Cancel();
            await _semaphore.WaitAsync(1000);

            if(_simulatorThread?.IsAlive == true)
                _simulatorThread?.Join();
            
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                SimulatedModel = Model.Clone();

                _simulatorThread = new Thread(RunSimulation){IsBackground = true};

                _simulatorThread.Start(SimulatedModel);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void RunSimulation(object obj)
        {
            var model = obj as VerletModel ?? throw new ArgumentException();

            var stopwatch = Stopwatch.StartNew();

            var dt = 1000/200;

            var prevTime = stopwatch.ElapsedMilliseconds;
            var accumulator = 0;
            
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var newTime = stopwatch.ElapsedMilliseconds;
                var frameTime = newTime - prevTime;
                prevTime = newTime;
                
                accumulator = (int) (accumulator + frameTime);

                while (accumulator > dt)
                {
                    var input = newTime % 500 > 250 ? 1.0 : -1.0;

                    for (int i = 0; i < 10; i++)
                    {
                        var rz = model.Step(input);

                        _simResults.Enqueue((input,rz).ToTuple());

                        while (_simResults.Count > SimResultsCountMax) 
                            _simResults.TryDequeue(out var _);
                    }

                    accumulator -= dt;
                }
                
                Thread.Sleep(1);
            }
        }

        
    }
}
