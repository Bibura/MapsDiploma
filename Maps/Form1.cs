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
            dtRouter.Columns.Add("���");
            dtRouter.Columns.Add("���. ����� (latitude)");
            dtRouter.Columns.Add("���. ����� (longitude)");
            dtRouter.Columns.Add("���. ����� (latitude)");
            dtRouter.Columns.Add("���. ����� (longitude)");
            dtRouter.Columns.Add("����� ����");
            dtRouter.Columns.Add("����������");
            dtRouter.Columns.Add("�������� ��������");
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
            //    MessageBox.Show("Data has been saved to file");//�������� ��в��� ---- ������� ��������� � �� ������������� ������
            //}
        }

        private void gMapControl1_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            MessageBox.Show(String.Format("Marker was clicked."));
        }

         


        private void button3_Click(object sender, EventArgs e)
        {
            //GMapOverlay routes = new GMapOverlay("routes");
            //List<PointLatLng> points = new List<PointLatLng>();
            //points.Add(new PointLatLng(48.866383, 2.323575));
            //points.Add(new PointLatLng(48.863868, 2.321554));
            //points.Add(new PointLatLng(48.861017, 2.330030));
            //GMapRoute route = new GMapRoute(points, "A walk in the park");
            //route.Stroke = new Pen(Color.Red, 3);
            //routes.Routes.Add(route);
            //gMapControl1.Overlays.Add(routes);
            //MessageBox.Show(route.Distance.ToString());
            string url = string.Format(
        "http://maps.googleapis.com/maps/api/directions/xml?origin={0},&destination={1}&sensor=false&language=ru&mode={2}",
        Uri.EscapeDataString(comboBox1.Text), Uri.EscapeDataString(textBox1.Text), Uri.EscapeDataString("driving"));

            System.Net.HttpWebRequest request =
        (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);

            System.Net.WebResponse response =
        request.GetResponse();

            System.IO.Stream dataStream =
        response.GetResponseStream();

            System.IO.StreamReader sreader =
        new System.IO.StreamReader(dataStream);

            string responsereader = sreader.ReadToEnd();

            response.Close();

            System.Xml.XmlDocument xmldoc =
        new System.Xml.XmlDocument();

            xmldoc.LoadXml(responsereader);

            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
            {
                System.Xml.XmlNodeList nodes =
                    xmldoc.SelectNodes("//leg//step");

                //��������� ������ ��� ���������� � �������.
                object[] dr;
                for (int i = 0; i < nodes.Count; i++)
                {
                    //��������� ��� ������ ����� �������� �� 
                    //������ ��������.
                    dr = new object[8];
                    //����� ����.
                    dr[0] = i;
                    //��������� ��������� ������ �������.
                    dr[1] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[2] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //��������� ��������� ����� �������.
                    dr[3] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[4] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //��������� ������� ������������ ��� ����������� ����� �������.
                    dr[5] = xmldoc.SelectNodes("//duration").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //��������� ����������, ������������ ���� ��������.
                    dr[6] = xmldoc.SelectNodes("//distance").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //��������� ���������� ��� ����� ����, �������������� � ���� ��������� ������ HTML.
                    dr[7] = HtmlToPlainText(xmldoc.SelectNodes("//html_instructions").Item(i).InnerText.ToString());
                    //���������� ���� � �������.
                    dtRouter.Rows.Add(dr);
                }

                //������� � ��������� ���� ����� ������ ����.
                comboBox1.Text = xmldoc.SelectNodes("//leg//start_address").Item(0).InnerText.ToString();
                //������� � ��������� ���� ����� ����� ����.
                textBox1.Text = xmldoc.SelectNodes("//leg//end_address").Item(0).InnerText.ToString();
                //������� � ��������� ���� ����� � ����.
                //textBox3.Text = xmldoc.GetElementsByTagName("duration")[nodes.Count].ChildNodes[1].InnerText;
                //������� � ��������� ���� ���������� �� ��������� �� �������� �����.
                //textBox4.Text = xmldoc.GetElementsByTagName("distance")[nodes.Count].ChildNodes[1].InnerText;

                //���������� ��� �������� ��������� ������ � ����� ����.
                double latStart = 0.0;
                double lngStart = 0.0;
                double latEnd = 0.0;
                double lngEnd = 0.0;

                //��������� ��������� ������ ����.
                latStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[0].InnerText);
                lngStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[1].InnerText);
                //��������� ��������� �������� �����.
                latEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[0].InnerText);
                lngEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[1].InnerText);

                //������� � ��������� ���� ���������� ������ ����.
                //textBox5.Text = latStart + ";" + lngStart;
                //������� � ��������� ���� ���������� �������� �����.
                //textBox6.Text = latEnd + ";" + lngEnd;

                //������������� ����������� ������� � �������� ���������.
                dataGridView1.DataSource = dtRouter;

                //������������� ������� ����� �� ������ ����.
                gMapControl1.Position = new GMap.NET.PointLatLng(latStart, lngStart);

                //������� ����� ������ ��������, � ��������� ���������� 
                //� ������� ��� ����� �������������� � ��������� ������.
                GMap.NET.WindowsForms.GMapOverlay markersOverlay =
                    new GMap.NET.WindowsForms.GMapOverlay("marker");

                //������������� ������ �������� �������, � ��������� ��������� ������ ����.
                GMap.NET.WindowsForms.Markers.GMarkerGoogle markerG =
                    new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                    new GMap.NET.PointLatLng(latStart, lngStart), GMarkerGoogleType.green);
                markerG.ToolTip =
                    new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerG);

                //���������, ��� ��������� �������, ���������� ���������� ������.
                markerG.ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.Always;

                //��������� ��������� ��� �������.
                string[] wordsG = textBox1.Text.Split(',');
                string dataMarkerG = string.Empty;
                foreach (string word in wordsG)
                {
                    dataMarkerG += word + ";\n";
                }

                //������������� ����� ��������� �������.               
                markerG.ToolTipText = dataMarkerG;

                //������������� ������ �������� �������, � ��������� ��������� ����� ����.
                GMap.NET.WindowsForms.Markers.GMarkerGoogle markerR =
                    new GMap.NET.WindowsForms.Markers.GMarkerGoogle(
                    new GMap.NET.PointLatLng(latEnd, lngEnd), GMarkerGoogleType.red);
                markerG.ToolTip =
                    new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerG);

                //���������, ��� ��������� �������, ���������� ���������� ������.
                markerR.ToolTipMode = GMap.NET.WindowsForms.MarkerTooltipMode.Always;

                //��������� ��������� ��� �������.
                string[] wordsR = textBox1.Text.Split(',');
                string dataMarkerR = string.Empty;
                foreach (string word in wordsR)
                {
                    dataMarkerR += word + ";\n";
                }

                //����� ��������� �������.               
                markerR.ToolTipText = dataMarkerR;

                //��������� ������� � ������ ��������.
                markersOverlay.Markers.Add(markerG);
                markersOverlay.Markers.Add(markerR);

                //������� ������ �������� ����������.
                gMapControl1.Overlays.Clear();

                //������� ������ ����������� ����� ��� ��������� ��������.
                ArrayList list = new ArrayList();

                //���������� �� ������������ �������� ��� ���������
                //��������� ����������� ����� �������� � ���������� ��
                //� ������ ���������.
                for (int i = 0; i < dtRouter.Rows.Count; i++)
                {
                    double dbStartLat = double.Parse(dtRouter.Rows[i].ItemArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbStartLng = double.Parse(dtRouter.Rows[i].ItemArray[2].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new GMap.NET.PointLatLng(dbStartLat, dbStartLng));

                    double dbEndLat = double.Parse(dtRouter.Rows[i].ItemArray[3].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbEndLng = double.Parse(dtRouter.Rows[i].ItemArray[4].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new GMap.NET.PointLatLng(dbEndLat, dbEndLng));
                }

                //������� ��� ��������.
                markersOverlay.Routes.Clear();

                //������� ������� �� ������ ������ ����������� �����.
                GMap.NET.WindowsForms.GMapRoute r = new GMap.NET.WindowsForms.GMapRoute((IEnumerable<PointLatLng>)list, "Route");

                //���������, ��� ������ ������� ������ ������������.
                r.IsVisible = true;

                //������������� ���� ��������.
                r.Stroke.Color = Color.DarkGreen;

                //��������� �������.
                markersOverlay.Routes.Add(r);

                //��������� � ���������, ������ �������� � ���������.
                gMapControl1.Overlays.Add(markersOverlay);

                //���������, ��� ��� �������� ����� ����� �������������� 
                //9�� ������� �����������.
                gMapControl1.Zoom = 9;

                //��������� �����.
                gMapControl1.Refresh();
            }
        }
        public string HtmlToPlainText(string html)
        {
            html = html.Replace("/b", "");
            return html.Replace("b", "");
        }
    }
}