using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Itinero;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Itinero.Profiles;
using Npgsql;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using static System.Windows.Forms.AxHost;

namespace Maps
{
    public partial class Form1 : Form
    {

        List<CPoint> listWithPointsOfHospitals = new List<CPoint>();
        List<CPoint> listWithPointsOfParks = new List<CPoint>();
        List<CPoint> listWithPointsOfMcDonalds = new List<CPoint>();

        GMapOverlay listOfHospitals = new GMapOverlay();
        GMapOverlay listOfParks = new GMapOverlay();
        GMapOverlay listOfMcDonalds = new GMapOverlay();

        DBController dBController;
        List<string> searchHistory;
        DataTable dtRouter = new DataTable();

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            //dBController = new DBController("postgres", "mapsdiploma", "admin");
            //dBController.openConnection();
            //searchHistory = dBController.searchSelect("searchhistory");
            //if (searchHistory != null)
            //    foreach (string item in searchHistory)
            //        comboBox1.Items.Add(item);
            //dBController.closeConnection();
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly; //downloading mode - online of ofline from package
                gMapControl1.MapProvider = OpenStreetMapProvider.Instance; //provider choose
            }
            else if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.CacheOnly; //downloading mode - online of ofline from package
                gMapControl1.MapProvider = OpenStreetMapProvider.Instance; //provider choose
            }
            gMapControl1.MinZoom = 2; //min zoom
            gMapControl1.MaxZoom = 32; //max zoom
            gMapControl1.Zoom = 10; //opening zoom
                                    //gMapControl1.Position = new GMap.NET.PointLatLng(50.45, 30.524167);
            gMapControl1.SetPositionByKeywords("Kyiv, Ukraine");//center point on load
            gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter; //zooming mode
            gMapControl1.CanDragMap = true; //dragging on/off
            gMapControl1.DragButton = MouseButtons.Left; //dragging button
            gMapControl1.ShowCenter = false; //show red cross in center
            gMapControl1.ShowTileGridLines = false; //show or not tile grid
            gMapControl1.Bearing = 0;
            gMapControl1.PolygonsEnabled = true;
            gMapControl1.RoutesEnabled = true;
            gMapControl1.MarkersEnabled = true;

            gMapControl1.Overlays.Add(new GMapOverlay("Temporary markers"));
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "PNG (*.png)|*.png";
                    dialog.FileName = "IMAGE";
                    Image image = this.gMapControl1.ToImage();
                    if (image != null)
                    {
                        using (image)
                        {
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                string filename = dialog.FileName;
                                if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                                {
                                    filename += ".png";
                                }
                                image.Save(filename);
                                MessageBox.Show("File saved! Saved in path : " + Environment.NewLine +
                                    dialog.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error poped! " + Environment.NewLine + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void find_on_map_click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                gMapControl1.SetPositionByKeywords(comboBox1.Text);
                gMapControl1.Zoom = 14;
                for (int i = 0; i < gMapControl1.Overlays.Count; i++)
                {
                    if (gMapControl1.Overlays[i].Id.Equals("Temporary markers"))
                    {
                        gMapControl1.Overlays[i].Clear();

                        GMarkerGoogle gMarkerGoogle = new GMarkerGoogle(new PointLatLng(
                            gMapControl1.Position.Lat, gMapControl1.Position.Lng), GMarkerGoogleType.pink);
                        gMarkerGoogle.ToolTip = new GMapRoundedToolTip(gMarkerGoogle);
                        gMarkerGoogle.ToolTipText = comboBox1.Text;
                        gMapControl1.Overlays[i].Markers.Add(gMarkerGoogle);
                        break;
                    }
                }

                dBController = new DBController("postgres", "mapsdiploma", "admin");
                dBController.openConnection();

                int count = dBController.basicSelect("searchhistory").Count;

                dBController.closeConnection();

                dBController.openConnection();

                int id = Convert.ToInt32(dBController.basicSelect("searchhistory")[count - 1]) + 1;

                dBController.closeConnection();

                dBController.openConnection();

                string date = @"'" + DateTime.Now.ToString("dd/MM/yyyy") + @"'";

                List<string> values = new List<string>{ id.ToString(),
                    @"'" + comboBox1.Text + @"'", date };
                dBController.basicInsertIntoSearchTable("searchhistory", values);
                dBController.closeConnection();
                comboBox1.Items.Clear();

                dBController.openConnection();

                searchHistory.Clear();

                searchHistory = dBController.searchSelect("searchhistory");

                if (searchHistory != null)
                    foreach (string item in searchHistory)
                        comboBox1.Items.Add(item);

                dBController.closeConnection();
            }
        }

        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            MessageBox.Show(String.Format("Marker was clicked."));
        }


        GMapOverlay routOverlay = new GMapOverlay("AtoB");

        private void button3_Click(object sender, EventArgs e)
        {
            gMapControl1.Overlays.Add(routOverlay);

            using (var stream = new FileInfo(@"ukraine-latest.routerdb").Open(FileMode.Open))
            {
                var routeDB = RouterDb.Deserialize(stream);

                var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
                var router = new Router(routeDB);

                gMapControl1.SetPositionByKeywords(comboBox2.Text);
                PointLatLng endPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);
                GMarkerGoogle destinationPoint = new GMarkerGoogle(endPoint, GMarkerGoogleType.red_big_stop);
                destinationPoint.ToolTip = new GMapRoundedToolTip(destinationPoint);
                destinationPoint.ToolTipText = "Destination";
                var latEnd = Convert.ToSingle(endPoint.Lat);
                var lngEnd = Convert.ToSingle(endPoint.Lng);
                gMapControl1.SetPositionByKeywords(comboBox1.Text);
                PointLatLng startPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);
                GMarkerGoogle startingPoint = new GMarkerGoogle(startPoint, GMarkerGoogleType.green_big_go);
                startingPoint.ToolTip = new GMapRoundedToolTip(startingPoint);
                startingPoint.ToolTipText = "Start";
                var latStart = Convert.ToSingle(startPoint.Lat);
                var lngStart = Convert.ToSingle(startPoint.Lng);

                var start = router.Resolve(profile, latStart, lngStart, 100);

                var end = router.Resolve(profile, latEnd, lngEnd, 100);

                var route = router.Calculate(profile, start, end);

                List<PointLatLng> points = new List<PointLatLng>();

                foreach (var item in route.Shape)
                {
                    PointLatLng tmpPoint = new PointLatLng(Convert.ToDouble(item.Latitude), Convert.ToDouble(item.Longitude));
                    points.Add(tmpPoint);
                }
                GMapRoute gMapRoute = new GMapRoute(points, "My route");

                routOverlay.Routes.Clear();
                routOverlay.Routes.Add(gMapRoute);
                routOverlay.Markers.Add(destinationPoint);
                routOverlay.Markers.Add(startingPoint);
                gMapControl1.Overlays.Add(routOverlay);
            }
        }

        private void downloadOfflineMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var routerDb = new RouterDb();
            using (var stream = new FileInfo(@"ukraine-latest.osm.pbf").OpenRead())
            {
                routerDb.LoadOsmData(stream, Itinero.Osm.Vehicles.Vehicle.Car);
            }

            using (var stream = new FileInfo(@"ukraine-latest.routerdb").Open(FileMode.Create))
            {
                routerDb.Serialize(stream);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gMapControl1.SetPositionByKeywords(comboBox2.Text);
            PointLatLng endPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);
            List<PointLatLng> points = new List<PointLatLng>();
            GMarkerGoogle destinationPoint = new GMarkerGoogle(endPoint, GMarkerGoogleType.red_big_stop);
            destinationPoint.ToolTip = new GMapRoundedToolTip(destinationPoint);
            destinationPoint.ToolTipText = "Destination";
            gMapControl1.SetPositionByKeywords(comboBox1.Text);
            PointLatLng startPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);
            GMarkerGoogle startingPoint = new GMarkerGoogle(startPoint, GMarkerGoogleType.green_big_go);
            startingPoint.ToolTip = new GMapRoundedToolTip(startingPoint);
            startingPoint.ToolTipText = "Start";

            var googleRoute = OpenStreetMapProvider.Instance.GetRoute(startPoint, endPoint, false, false, 14);
            using (var stream = new FileInfo(@"ukraine-latest.routerdb").Open(FileMode.Open))
            {
                var routeDB = RouterDb.Deserialize(stream);

                var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
                var router = new Router(routeDB);
            for(int i = 0; i < googleRoute.Points.Count - 1; i++)
            {

                var start = router.Resolve(profile, Convert.ToSingle(googleRoute.Points[i].Lat), Convert.ToSingle(googleRoute.Points[i].Lng), 100);

                var end = router.Resolve(profile, Convert.ToSingle(Convert.ToSingle(googleRoute.Points[i+1].Lat)), Convert.ToSingle(googleRoute.Points[i+1].Lng), 100);

                var OSMroute = router.Calculate(profile, start, end);

                foreach(var a in OSMroute.Shape)
                {
                    points.Add(new PointLatLng(a.Latitude, a.Longitude));
                }

            var routeOnMap = new GMapRoute(points, "My route");
            routOverlay.Routes.Add(routeOnMap);
            gMapControl1.Overlays.Add(routOverlay);

            }

            }
            MessageBox.Show(points.Count.ToString());
            MessageBox.Show(googleRoute.Points.Count.ToString());
        }
    }
}