using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI;
using Mapsui.UI.Forms;
using RoadBookXF.RBDebugTools;
using static RoadBookXF.RBGlobals;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RoadBookXF.RBLayers
{
    public class RBLayer : Layer
    {
        internal List<Feature> Features;
        private RBLogger logger = new RBLogger();
        public RBLayer(string name) : base()
        {
            Name = name;

            // disable white default circle on pins
            Style = null;

            // neededed for dragging
            // by default is not enabled
            // can be changed by xxxxLayer ...
            IsMapInfoLayer = false;



            Clear();
        }

        public void Clear()
        {
            Features = new List<Feature>();

            Draw();
        }

        public void Draw()
        {
            this.DataSource = new MemoryProvider(Features)
            {
                CRS = "EPSG:4326" // The DataSource CRS needs to be set
            };
            this.DataHasChanged();
        }


        public async Task MapClickedAsync(object sender, MapClickedEventArgs e)
        {
            logger.Log($" MapClickedAsync : Test for Charlenni MapClickedAsync {e.NumOfTaps} tap(s) from Mapsui");
        }

        public async Task MapLongClickedAsync(object sender, MapLongClickedEventArgs e)
        {
            logger.Log($" MapLongClickedAsync : Test for Charlenni Long Tap from Mapsui");
        }

        private int TouchMoveCount = 0;

        // this one to tell that all following events up to TouchStart are to be discarded
        private bool userActionCompleted = true;

        private bool touchEndInsideFirsTimerOccured = false;
        private bool touchEndInsideSecondTimerForClickOccured = false;

        private bool secondTouchStartedInsideFirsTimerOccured = false;
        private bool secondTouchStartedInsideSecondClickTimerTimerOccured = false;

        private bool waitingFirstTimer = false;
        private bool waitingSecondTimerForClick = false;

        private object savedSender;
        private TouchedEventArgs savedTouchedEventArgs;

        public async Task MapTouchStarted(object sender, TouchedEventArgs e)
        {
            // if we have a touch start, all move and TouchEnd events are processed
            // this can be:
            //  - a new user action (click, doubleClick, drag, longTap)
            //  - a second click following a first TouchStart, (move .... ), TouchEnd

            if (!waitingFirstTimer && !waitingSecondTimerForClick)
            {
                TouchMoveCount = 0;

                userActionCompleted = false;

                touchEndInsideFirsTimerOccured = false;
                touchEndInsideSecondTimerForClickOccured = false;

                secondTouchStartedInsideFirsTimerOccured = false;
                secondTouchStartedInsideSecondClickTimerTimerOccured = false;

                waitingFirstTimer = true;
                waitingSecondTimerForClick = false;

                savedSender = sender;
                savedTouchedEventArgs = e;

                logger.Clear();
                logger.Log($" MapTouchStarted : Double Tap Enabled : {((MapControl)sender).UseDoubleTap}");
                logger.Log($" MapTouchStarted : First MapTouchStarted : Starting first timer {RBGlobals.RBG_MillisecondsBeforeDragging} ms");

                Device.StartTimer(TimeSpan.FromMilliseconds(RBGlobals.RBG_MillisecondsBeforeDragging), MapTouchFirstTimerHandler);

                Device.StartTimer(TimeSpan.FromMilliseconds(2000), () =>
                {
                    logger.FlushOnLabel(RBG_DebugLabel);
                    logger.FlushOnDiagnosticDebug();
                    return false;
                });
            }
            else
            {
                if (waitingFirstTimer)
                {
                    logger.Log($" MapTouchStarted : Second MapTouchStarted : Waiting first timer still active");
                    secondTouchStartedInsideFirsTimerOccured = true;
                }
                else if (waitingSecondTimerForClick)
                {
                    logger.Log($" MapTouchStarted : Second MapTouchStarted : Waiting first timer is elapsed and second timer active waiting for second click");
                    secondTouchStartedInsideSecondClickTimerTimerOccured = true;
                }
                else
                {
                    throw new Exception("RBLayer: MapTouchStarted : Having both timers active is not possible, there is a fault in the logic !");
                }
            }
        }

        private bool MapTouchFirstTimerHandler()
        {
            Device.BeginInvokeOnMainThread(async () =>
               {
                   waitingFirstTimer = false;
                   if (secondTouchStartedInsideFirsTimerOccured)
                   {
                       // fast user, both clicks while waiting for first moveend !
                       waitingSecondTimerForClick = false;
                       logger.Log($" MapTouchFirstTimerHandler : Second MapTouchStarted during the first timer occured -> it is a double click during first timer");
                       //  disguard the following events
                       userActionCompleted = true;
                       //await ((IRBLayer)this).LayerDoubleClickedAsync(savedSender, savedTouchedEventArgs);
                   }
                   else
                   {
                       if (touchEndInsideFirsTimerOccured)
                       {
                           logger.Log($" MapTouchFirstTimerHandler : TouchEnd inside the first timer occured with NO Second MapTouchStarted  -> No drag/longTap but is it a single/double click ! ");

                           waitingSecondTimerForClick = true;
                           touchEndInsideFirsTimerOccured = false;
                           logger.Log($" MapTouchFirstTimerHandler : Starting second timer for waiting single/double click {RBGlobals.RBG_MaxMillisecondsForDoubleClick} ms");
                           Device.StartTimer(TimeSpan.FromMilliseconds(RBGlobals.RBG_MaxMillisecondsForDoubleClick), MapTouchSecondTimerForClickHandlerForDoubleClick);
                       }
                       else
                       {
                           waitingSecondTimerForClick = false;
                           logger.Log($" MapTouchFirstTimerHandler : First timer {RBGlobals.RBG_MillisecondsBeforeDragging} ms expired -> It is drag or long tap ?");
                           await MapLongTapEndOrDraggingStarted(savedSender, savedTouchedEventArgs);
                       }
                   }
               });
            return false; //  false to stop
        }

        private bool MapTouchSecondTimerForClickHandlerForDoubleClick()
        {
            Device.BeginInvokeOnMainThread(async () =>
           {
               waitingSecondTimerForClick = false;
               if (secondTouchStartedInsideSecondClickTimerTimerOccured)
               {
                   logger.Log($" MapTouchSecondTimerForClickHandlerForDoubleClick : SecondTouch after first timer elapsed but occured while second timer still running -> it is double click");
                   //  disguard the following events
                   userActionCompleted = true;
                   //await ((IRBLayer)this).LayerDoubleClickedAsync(savedSender, savedTouchedEventArgs);
               }
               else
               {
                   logger.Log($" MapTouchSecondTimerForClickHandlerForDoubleClick : NO SecondTouch after first timer occured during second timer -> it is single click");
                   //  disguard the following events
                   userActionCompleted = true;
                   //await ((IRBLayer)this).LayerSingleClickedAsync(savedSender, savedTouchedEventArgs);
               }
           });
            return false;
        }

        private async Task MapLongTapEndOrDraggingStarted(object sender, TouchedEventArgs e)
        {
            // no touchend since delay sarted, we assume it is drag !
            logger.Log($" MapLongTapEndOrDraggingStarted : TODO Handle lonTap versus drag ....");
            //await ((IRBLayer)this).LayerTouchStarted(sender, e);
        }

        public async Task MapTouchMove(object sender, TouchedEventArgs e)
        {
            TouchMoveCount++;
            logger.Log($" MapTouchMove # {TouchMoveCount} : ");
        }

        public async Task MapTouchEnd(object sender, TouchedEventArgs e)
        {
            if (waitingFirstTimer)
            {
                touchEndInsideFirsTimerOccured = true;
                logger.Log($" MapTouchEnd : Occured during first timer");
            }
            else if (waitingSecondTimerForClick)
            {
                touchEndInsideSecondTimerForClickOccured = true;
                logger.Log($" MapTouchEnd : Occured during second timer");
            }
            else if (userActionCompleted == true)
            {
                logger.Log($" MapTouchEnd : MapTouchEnd occured with no timer active and useraction terminated !");
            }
            else
            {
                logger.Log($" MapTouchEnd : Occured with NO timer active");
            }
        }

        public async Task MapInfoAsync(object sender, MapInfoEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"---------------- {DateTime.Now:fff} MapInfoAsync ");
        }
    }
}
