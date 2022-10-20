using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using Npgsql;

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

        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            dBController = new DBController("postgres", "mapsdiploma", "admin");
            dBController.openConnection();
            List<string> searchHistory = dBController.searchSelect("searchhistory");
            if (searchHistory != null)
                foreach (string item in searchHistory)
                    comboBox1.Items.Add(item);
            dBController.closeConnection();
        }

        private void gMapControl1_Load(object sender, EventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly; //downloading mode - online of ofline from package
            gMapControl1.MapProvider = OpenStreetMapProvider.Instance; //provider choose
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

        private void button1_Click(object sender, EventArgs e)
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

                List<string> searchHistory = dBController.searchSelect("searchhistory");

                if (searchHistory != null)
                    foreach (string item in searchHistory)
                        comboBox1.Items.Add(item);

                dBController.closeConnection();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {


            //NpgsqlDataReader? reader = dBController.basicSelect("searchhistory");
            //if(reader != null)
            //while (reader.Read())
            //{
            //    MessageBox.Show($"{reader[0]} {reader[1]} {reader[2]}");
            //}else MessageBox.Show($"Nothing to read!");

        }
    }
}