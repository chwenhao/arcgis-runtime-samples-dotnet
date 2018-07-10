// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.FeatureLayerSelection
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer selection",
        "Layers",
        "This sample demonstrates how to select features in a feature layer by tapping a MapView.",
        "")]
    public partial class FeatureLayerSelection : ContentPage
    {
        //Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        public FeatureLayerSelection()
        {
            InitializeComponent ();

            Title = "Feature layer selection";
            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create envelope to be used as a target extent for map's initial viewpoint
            Envelope myEnvelope = new Envelope(
                -1131596.019761, 3893114.069099, 3926705.982140, 7977912.461790, 
                SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(myEnvelope);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create Uri for the feature service
            Uri featureServiceUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable)
            {

                // Set the selection color for feature layer
                SelectionColor = Colors.Cyan,

                // Set the selection width
                SelectionWidth = 3
            };

            // Make sure that used feature layer is loaded before we hook into the tapped event
            // This prevents us trying to do selection on the layer that isn't initialized
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map
            if (_featureLayer.LoadStatus == LoadStatus.Loaded)
            {
                // Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer);

                // Add tap event handler for mapview
                MyMapView.GeoViewTapped += OnMapViewTapped;
            }           
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Define the selection tolerance
                double tolerance = 5;

                // Convert the tolerance to map units
                double mapTolerance = tolerance * MyMapView.UnitsPerPixel;

                // Get the tapped point
                MapPoint geometry = e.Location;

                // Normalize the geometry if wrap-around is enabled
                //    This is necessary because of how wrapped-around map coordinates are handled by Runtime
                //    Without this step, querying may fail because wrapped-around coordinates are out of bounds.
                if (MyMapView.IsWrapAroundEnabled) { geometry = (MapPoint)GeometryEngine.NormalizeCentralMeridian(geometry); }

                // Define the envelope around the tap location for selecting features
                Envelope selectionEnvelope = new Envelope(geometry.X - mapTolerance, geometry.Y - mapTolerance, geometry.X + mapTolerance,
                    geometry.Y + mapTolerance, MyMapView.Map.SpatialReference);

                // Define the query parameters for selecting features
                QueryParameters queryParams = new QueryParameters
                {

                    // Set the geometry to selection envelope for selection by geometry
                    Geometry = selectionEnvelope
                };

                // Select the features based on query parameters defined above
                await _featureLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sample error", ex.ToString(),"OK");
            }
        }
    }
}
