using System;
using System.Collections.Generic;
using System.ComponentModel;
using BruTile.Predefined;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Projection;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;
using RoadBookXF.RBLayers;
using static  RoadBookXF.RBGlobals;
using Xamarin.Forms;

namespace MapsuiTouchTimings
{
    public partial class MainPage : ContentPage
    {
        public RBLayer RBLayer;

        public bool initialised = false;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            if (!initialised)
            {
                initialised = true;

                RBG_DebugLabel = DebugLabel;

                Map Map = new Map
                {
                    CRS = "EPSG:3857",
                    Transformation = new MinimalTransformation()
                };

                TileLayer MapTileLayer = OpenStreetMap.CreateTileLayer();
                Map.Layers.Add(MapTileLayer);


                RBLayer = new RBLayer("RBLayer");

                Map.Widgets.Add(new ScaleBarWidget(Map)
                {
                    TextAlignment = Mapsui.Widgets.Alignment.Center,
                    HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right,
                    VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top
                });

                MapsuiView.IsMyLocationButtonVisible = false;
                MapsuiView.IsNorthingButtonVisible = true;
                MapsuiView.MyLocationEnabled = false;
                MapsuiView.MyLocationLayer.Enabled = false;

                if (Device.RuntimePlatform == Device.WPF)
                {
                    // don't work anyway
                    MapsuiView.IsZoomButtonVisible = false;
                    MapsuiView.IsNorthingButtonVisible = false;
                }

                if (Device.RuntimePlatform == Device.UWP)
                {
                    // no need !
                    MapsuiView.IsNorthingButtonVisible = false;
                }

                MapsuiView.Map = Map;

                MapsuiView.UseDoubleTap = false;
                DoubleTapButton.BackgroundColor = Color.MistyRose;
                DoubleTapButton.Text = "Enable DoubleTap";

                RBLayer.IsMapInfoLayer = false;
                MapInfoButton.BackgroundColor = Color.MistyRose;
                MapInfoButton.Text = "Enable MapInfo";

                MapsuiView.MapClicked += MapClickedAsync;

                // not working on Android
                MapsuiView.MapLongClicked += MapLongClickedAsync;

                // dragging via points
                MapsuiView.TouchStarted += MapTouchStarted;
                MapsuiView.TouchMove += MapTouchMouve;
                MapsuiView.TouchEnded += MapTouchEnded;

                // if we touch feature of enables infolayxer
                //MapsuiView.Info += MapInfoAsync;
            }
        }

        private async void MapTouchEnded(object sender, TouchedEventArgs e)
        {
            await RBLayer.MapTouchEnd(sender, e);
        }

        private async void MapTouchMouve(object sender, TouchedEventArgs e)
        {
            await RBLayer.MapTouchMove(sender, e);
        }

        private async void MapTouchStarted(object sender, TouchedEventArgs e)
        {
            await RBLayer.MapTouchStarted(sender, e);
        }

        private async void MapLongClickedAsync(object sender, MapLongClickedEventArgs e)
        {
            // block Mapsui map updates for test !
            e.Handled = true;
            await RBLayer.MapLongClickedAsync(sender, e);
        }

        private async void MapClickedAsync(object sender, MapClickedEventArgs e)
        {
            e.Handled = true;
            await RBLayer.MapClickedAsync(sender, e);
        }

        private void SearchTextCompleted(object sender, EventArgs e)
        {

        }

        private void ToggleDoubleTap(object sender, EventArgs e)
        {
            if (MapsuiView.UseDoubleTap)
            {
                MapsuiView.UseDoubleTap = false;
                ((Button)sender).BackgroundColor = Color.MistyRose;
                ((Button)sender).Text = "Enable DoubleTap";
            }
            else
            {
                MapsuiView.UseDoubleTap = true;
                ((Button)sender).BackgroundColor = Color.LightGreen;
                ((Button)sender).Text = "Disable DoubleTap";
            }
        }

        private void ToggleMapInfo(object sender, EventArgs e)
        {
            if (RBLayer.IsMapInfoLayer)
            {
                RBLayer.IsMapInfoLayer = false;
                ((Button)sender).BackgroundColor = Color.MistyRose;
                ((Button)sender).Text = "Enable MapInfo";
            }
            else
            {
                RBLayer.IsMapInfoLayer = true;
                ((Button)sender).BackgroundColor = Color.LightGreen;
                ((Button)sender).Text = "Disable MapInfo";
            }
        }

        private void ClearLabel(object sender, EventArgs e)
        {
            DebugLabel.Text = "";
        }
    }
}
