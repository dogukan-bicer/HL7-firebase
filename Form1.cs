using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using ZedGraph;

namespace HL7_firebase
{

    public partial class Form1 : Form
    {
        GraphPane myPaneEtco2 = new GraphPane();
        PointPairList listPointsEtco2 = new PointPairList();
        LineItem myCurveEtco2;

        double zaman = 0;
        int maksm = 20, minm = 0;
        int tickStart = 0;
        string sonuc;
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "WJHVkU0kv7VZlhTd60vwJYiYg21yMpHjaBDxSVUT",
            BasePath = "https://kapno-veri-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;

        public Form1()
        {
            InitializeComponent();
            serverKontrol();
            baslıkYaz();
            deger_al();
        }

        private void GrafikHazirla()
        {
            GraphPane mypane = zedGraphControl1.GraphPane;
            mypane.Title.Text = "Etco2 grafik";
            mypane.XAxis.Title.Text = "Zaman (sn)";
            mypane.YAxis.Title.Text = "% Etco2";

            myPaneEtco2 = zedGraphControl1.GraphPane;
            myPaneEtco2.YAxis.Scale.Min = 0;
            myPaneEtco2.YAxis.Scale.Max = 100;
            myCurveEtco2 = myPaneEtco2.AddCurve(null, listPointsEtco2, Color.Red, SymbolType.None);
            //myCurveEtco2.Line.IsSmooth = true;
            //myCurveEtco2.Line.SmoothTension = 0.1f;
            myCurveEtco2.Line.Width = 3;
        }

        public void deger_al ()
        {
            var sonuc = client.Get("OBX");  //hl7 deki bilgileri çekmek icin
            hl7_data std = sonuc.ResultAs<hl7_data>();
            var etco2 = std.OBX_1.Substring(23, 3);
            etco2 = etco2.Replace("%", "" + System.Environment.NewLine);
            veri.etco2_data= etco2.Replace("|", "" + System.Environment.NewLine);
            label4.Text = "%" + veri.etco2_data;
            var fico2 = std.OBX_2.Substring(23, 3);
            fico2 = fico2.Replace("%", "" + System.Environment.NewLine);
            label6.Text = "%" + fico2.Replace("|", "" + System.Environment.NewLine);
            string etco2rr = std.OBX_3.Substring(25, 4);
            etco2rr = etco2rr.Replace("{", "" + System.Environment.NewLine);
            etco2rr = etco2rr.Replace("b", "" + System.Environment.NewLine);
            etco2rr = etco2rr.Replace("|", "" + System.Environment.NewLine);
            int deger1 = Int16.Parse(etco2rr);
            label8.Text =deger1  + " bpm";
        }

        static bool baglantiKontrol()
        {
            try
            {
                return new System.Net.NetworkInformation.Ping().Send("www.google.com", 1000).Status == System.Net.NetworkInformation.IPStatus.Success;//baglantı kontrol
            }
            catch (Exception)
            {
                return false;
            }

        }

        struct hl7_baslik
        {
            public string MSH;
            public string PID;
            public string PV1;
            public string OBR;
            public string baslik;
            public string etco2_data;
            public int fico2_data;
            public int etco2rr_data;
        }
        hl7_baslik veri = new hl7_baslik();

        public void baslıkYaz()
        {
            veri.MSH = "MSH|^~/&|PSN|PSN|||20111021175130.7370-0700||ORU^R01|92176046-e469-4d34-9e56-711ab3adeb04|P|2.4|||NE|NE$";
            veri.PID = "PID|||27^3^M10||BİCER^DOGUKAN^MIDDLE^^^^||19990307|M|||^^^^^^P~^^^^^^VACAE~^^^^^^VACAA~^^^^^^VACAC~^^^^^^VACAM~^^^^^^VACAO$";
            veri.PV1 = "PV1||I|MED/SURG^201^2$";
            veri.OBR = "OBR|1|8224e161-89a1-4fe8-aad2-a4c5578ab635^^000180ffff7f0b15^EUI-64|0609c6eb-d8ce-4180-b73d-343c922f2776 ^ ^000180ffff7f0b15 ^ EUI - 64 | 44616 - 1 ^ Pulse oximetry panel^LN |||20130328160302.2739-0500||||||||||||||||||||||||||||||||||||| 252465000 ^ Pulse oximetry ^ SCT | 255238004 ^ Continuous ^ SCT$";
            veri.baslik = veri.MSH + veri.PID + veri.PV1 + veri.OBR;
            veri.baslik = veri.baslik.Replace("$", " " + System.Environment.NewLine);
            textBox1.Text = veri.baslik;
        }
        public void serverKontrol()
        {
            //if (baglantiKontrol() == false)
            //{
            //    MessageBox.Show("Internet Bağlatısı yok");
            //}
            //else
            //{
                client = new FireSharp.FirebaseClient(config);
                if (client != null)
                {
                    MessageBox.Show("Sunucuya bağlandı!!");
                }
                else
                {
                    MessageBox.Show("Bağlantı Başarısız!!");
                }
            //}
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "*";//Varsayılan olarak görüntülenecek Dosya Adı kısmını belirliyoruz
                saveFileDialog1.Filter = "HL7 Dosyaları (*hl7)|*.hl7";//Bu kısım önemli, Kaydedilmesi gereken dosyamızın filtre değerlerini belirliyoruz
                saveFileDialog1.DefaultExt = "hl7";//Varsayılan olarak görüntülenecek dosya uzantısını belirliyoruz
                saveFileDialog1.ShowDialog();//Diyalog Penceresini kullanıcıya gösterip dosya yolu seçmesini sağlıyoruz

                StreamWriter yazmaislemi = new StreamWriter(saveFileDialog1.FileName);//System.IO kütüphanesinden faydalanarak SystemWriter olarak bir yazıcı belirliyoruz ve bu yazıcının dosya yolunu belirliyoruz
                yazmaislemi.WriteLine(textBox1.Text);//TextBoxumuzda bulunan satırları WriteLine metodu ile oluşturduğumuz txt dosyamıza kaydediyoruz
                yazmaislemi.Close();//Hata mesajı almamak için yazma işlemimizi sonlandırıp bağlantımızı kesiyoruz
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //GrafikHazirla();
            //RollingPointPairList list1 = new RollingPointPairList(1200);
            //RollingPointPairList list2 = new RollingPointPairList(1200);
            //timer1.Interval = 50;
            //mypane.XAxis.Scale.Min = 0;
            //mypane.XAxis.Scale.Max = 30;
            //mypane.XAxis.Scale.MinorStep = 1;
            //mypane.XAxis.Scale.MajorStep = 5;
            //zedGraphControl1.AxisChange();
            //tickStart = Environment.TickCount;
            //textBox2.Enabled = false;
            //hl7_data std = new hl7_data()
            //{
            //    OBX_1 = textBox2.Text,
            //    OBX_2 = textBox2.Text,
            //    OBX_3 = textBox2.Text
            //};
            //var ayarlama = client.Set("OBX",std);
            //MessageBox.Show("Veri yerleştirme başarılı");
        }

        //public void ciz(double setpoint1, double setpoint2)
        //{
        //    if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
        //        return;
        //    LineItem curve1 = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
        //    LineItem curve2 = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
        //    if (curve1 == null)
        //        return;
        //    if (curve2 == null)
        //        return;
        //    IPointListEdit list1 = curve1.Points as IPointListEdit;
        //    IPointListEdit list2 = curve2.Points as IPointListEdit;
        //    if (list1 == null)
        //        return;
        //    if (list2 == null)
        //        return;
        //    double time = (Environment.TickCount - tickStart) / 1000.0;
        //    list1.Add(time, setpoint1);
        //    list2.Add(time, Math.Sin(2.0*Math.PI*time/3.0));//Convert.ToDouble(veri.etco2_data)
        //    Scale xscale = zedGraphControl1.GraphPane.XAxis.Scale;
        //    if(time>xscale.Max - xscale.MajorStep)
        //    { }
        //    zedGraphControl1.AxisChange();
        //    zedGraphControl1.Invalidate();

        //}

        private void button3_Click_1(object sender, EventArgs e)
        {
            serverKontrol();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {
                var sonuc = client.Get("OBX");
                hl7_data std = sonuc.ResultAs<hl7_data>();
                string cikti =  std.OBX_1+ "$" + std.OBX_2 + "$" + std.OBX_3 + "$";
                cikti= cikti.Replace("$", " " + System.Environment.NewLine);
                string textbox_ilk = textBox1.Text;
                textBox1.Text = textbox_ilk+cikti;
                MessageBox.Show("Verileri alma işlemi başarılı!");

            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
                deger_al();
                zaman += 0.05;
                listPointsEtco2.Add(new PointPair(zaman, Convert.ToDouble(veri.etco2_data)));
                myPaneEtco2.XAxis.Scale.Max = zaman;
                myPaneEtco2.AxisChange();
                zedGraphControl1.Refresh();

            }

        private void button5_Click(object sender, EventArgs e)//grafigi durdur
        {
            timer1.Enabled = false;
        }

        private void button6_Click(object sender, EventArgs e)//resetle
        {
            deger_al();
            myPaneEtco2.CurveList.Clear();
            myPaneEtco2.GraphObjList.Clear();
            myCurveEtco2.Clear();
            zaman = 0;
            maksm = 20;
            minm = 0;
            tickStart = 0;
            sonuc = "";
            listPointsEtco2.Add(new PointPair(0, Convert.ToDouble(veri.etco2_data)));
            myPaneEtco2.XAxis.Scale.Max = 0;
            myPaneEtco2.AxisChange();
            zedGraphControl1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "*";
                saveFileDialog1.Filter = "bpm Dosyaları (*bmp)|*.bmp";
                saveFileDialog1.DefaultExt = "bpm";
                saveFileDialog1.ShowDialog();
                myPaneEtco2.GetImage().Save(saveFileDialog1.FileName);
            }
            catch (Exception hata)
            {
                MessageBox.Show(hata.Message);//Kullanıcıya hata oluşması durumunda hata mesajı verdirtiyoruz.
            }
        }

        private void button4_Click(object sender, EventArgs e)//grafigi başlat
        {
            timer1.Enabled = true;
            GrafikHazirla();
        }
    }
}
