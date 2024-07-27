using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private CurrencyRequest currencyRequest;
        private EmailService emailService;

        public Form1()
        {
            InitializeComponent();
            currencyRequest = new CurrencyRequest();
            emailService = new EmailService();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 3600000; // 1 saat = 3600000 ms
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("Timer tetiklendi ve API'den veri çekilecek."); kontrol edildi
            await currencyRequest.UpdateRatesAsync();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            string kurBilgisi = await emailService.GetKurBilgisiAsync();
            string email = this.textBox1.Text;

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                MessageBox.Show("Lütfen geçerli bir e-posta adresi girin.");
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(kurBilgisi))
                {
                    emailService.SendEmail(email, kurBilgisi);
                    MessageBox.Show("Kur bilgileri başarıyla gönderildi.");
                }
                else
                {
                    MessageBox.Show("Kur bilgileri alınamadı.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mail = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private void chart1_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";
            string queryEur = "SELECT TarihSaat, Rate FROM Kurlar WHERE KurIsimleriID=35";
            var xValues = new List<DateTime>();
            var yValues = new List<decimal>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryEur, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    xValues.Add(Convert.ToDateTime(reader["TarihSaat"]));
                    yValues.Add(Convert.ToDecimal(reader["Rate"]));
                }
                reader.Close();
            }

            chart1.Series.Clear(); // Varolan serileri temizleyin
            Series series1 = new Series
            {
                Name = "EurKuru",
                IsVisibleInLegend = false,
                ChartType = SeriesChartType.Line
            };

            // Verileri seriye ekleyin
            for (int i = 0; i < xValues.Count; i++)
            {
                series1.Points.AddXY(xValues[i], yValues[i]);
            }

            // Seriyi chart kontrolüne ekleyin
            chart1.Series.Add(series1);

            // ChartArea ekleyin
            if (chart1.ChartAreas.Count == 0)
            {
                chart1.ChartAreas.Add(new ChartArea("Default"));
            }

            chart1.Invalidate(); // Chart'ı güncelleyin
        }

        private void chart2_Click(object sender, EventArgs e)
        {
            string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";
            string queryUsd = "SELECT TarihSaat, Rate FROM Kurlar WHERE KurIsimleriID=36";
            var xValues = new List<DateTime>();
            var yValues = new List<decimal>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryUsd, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    xValues.Add(Convert.ToDateTime(reader["TarihSaat"]));
                    yValues.Add(Convert.ToDecimal(reader["Rate"]));
                }
                reader.Close();
            }

            chart2.Series.Clear(); // Varolan serileri temizleyin
            Series series2 = new Series
            {
                Name = "UsdKuru",
                IsVisibleInLegend = false,
                ChartType = SeriesChartType.Line
            };

            // Verileri seriye ekleyin
            for (int i = 0; i < xValues.Count; i++)
            {
                series2.Points.AddXY(xValues[i], yValues[i]);
            }

            // Seriyi chart kontrolüne ekleyin
            chart2.Series.Add(series2);

            // ChartArea ekleyin
            if (chart2.ChartAreas.Count == 0)
            {
                chart2.ChartAreas.Add(new ChartArea("Default"));
            }

            chart2.Invalidate(); // Chart'ı güncelleyin
        }
    }
}