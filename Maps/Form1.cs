using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Npgsql;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;

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
            dBController = new DBController("postgres", "mapsdiploma", "admin");
            dBController.openConnection();
            searchHistory = dBController.searchSelect("searchhistory");
            if (searchHistory != null)
                foreach (string item in searchHistory)
                    comboBox1.Items.Add(item);
            dBController.closeConnection();
            dtRouter.Columns.Add("Øàã");
            dtRouter.Columns.Add("Íà÷. òî÷êà (latitude)");
            dtRouter.Columns.Add("Íà÷. òî÷êà (longitude)");
            dtRouter.Columns.Add("Êîí. òî÷êà (latitude)");
            dtRouter.Columns.Add("Êîí. òî÷êà (longitude)");
            dtRouter.Columns.Add("Âðåìÿ ïóòè");
            dtRouter.Columns.Add("Ðàññòîÿíèå");
            dtRouter.Columns.Add("Îïèñàíèå ìàðøðóòà");
            dataGridView1.DataSource = dtRouter;
            dataGridView1.Columns[7].Width = 250;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = false;
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


        //class Person
        //{
        //    public string Name { get; }
        //    public int Age { get; set; }
        //    public Person(string name, int age)
        //    {
        //        Name = name;
        //        Age = age;
        //    }
        //}

        private void button2_ClickAsync(object sender, EventArgs e)
        {
            //using (FileStream fs = new FileStream("user.json", FileMode.OpenOrCreate))
            //{
            //    Person tom = new Person("Tom", 37);
            //    JsonSerializer.SerializeAsync<Person>(fs, tom);
            //    MessageBox.Show("Data has been saved to file");//ÒÅÑÒÎÂÈÉ ÂÀÐ²ÀÍÒ ---- ÇÐÎÁÈÒÈ ÄÎÄÀÂÀÍÍß À ÍÅ ÏÅÐÅÏÈÑÓÂÀÍÍß ÄÆÑÎÍÀ
            //}
        }

        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            MessageBox.Show(String.Format("Marker was clicked."));
        }

         


        private void button3_Click(object sender, EventArgs e)
        {
            gMapControl1.SetPositionByKeywords(comboBox1.Text);
            PointLatLng startPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);
            gMapControl1.SetPositionByKeywords(textBox1.Text);
            PointLatLng endPoint = new PointLatLng(gMapControl1.Position.Lat, gMapControl1.Position.Lng);

            var route = OpenStreetMapProvider.Instance.GetRoute(startPoint, endPoint, false, false, 14);

            var r = new GMapRoute(route.Points, "My route");

            var routes = new GMapOverlay("Routes");

            routes.Routes.Add(r);
            gMapControl1.Overlays.Add(routes);

        }
    }
}