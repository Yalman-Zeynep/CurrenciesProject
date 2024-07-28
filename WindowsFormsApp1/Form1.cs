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
        private CurrenciesService currenciesService;
        



        public Form1()
        {
            InitializeComponent();
            currencyRequest = new CurrencyRequest();
            emailService = new EmailService();
            currenciesService = new CurrenciesService();
            InitializeTimer();
            LoadCurrencyName();
            
        }

        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 60000; // 1 saat = 3600000 ms
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

     

        private void LoadCurrencyName()
        {
            string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT KurIsimleriID, KurIsim FROM KurIsimleri";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string kurIsim = reader["KurIsim"].ToString();
                        int kurId = (int)reader["KurIsimleriID"];

                        comboBox1.Items.Add(new{ Id = kurId, Name = kurIsim });
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri yüklenirken bir hata oluştu: " + ex.Message);
                }
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            //MessageBox.Show("Timer tetiklendi ve API'den veri çekilecek."); kontrol edildi
            await currencyRequest.UpdateRatesAsync();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {


            // Kullanıcının comboBox1'den bir döviz seçip seçmediğini kontrol eder.
            if (comboBox1.SelectedItem == null)
            {
                // Kullanıcı herhangi bir döviz seçmemişse, bir mesaj kutusu ile kullanıcıyı uyarır.
                MessageBox.Show("Lütfen bir döviz seçin.");
                return; // Metodun geri kalanını çalıştırmadan çıkar.
            }

            // comboBox1'de seçilen dövizi alır.
            var selectedCurrency = (dynamic)comboBox1.SelectedItem;
            // Seçilen dövizin ID'sini alır.
            int selectedCurrencyId = selectedCurrency.Id;

            // Seçilen dövize ait kur bilgilerini almak için GetKurBilgisiAsync metodunu çağırır ve sonucu 'kurBilgisi' değişkenine atar.
            string kurBilgisi = await currenciesService.GetKurBilgisiAsync(selectedCurrencyId);
            // Kullanıcının textBox1'e girdiği e-posta adresini alır.
            string email = this.textBox1.Text;

            // E-posta adresinin geçerli olup olmadığını kontrol eder.
            if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            {
                // E-posta adresi geçersizse, bir mesaj kutusu ile kullanıcıyı uyarır.
                MessageBox.Show("Lütfen geçerli bir e-posta adresi girin.");
                return; // Metodun geri kalanını çalıştırmadan çıkar.
            }

            try
            {
                // Kur bilgisi boş değilse, e-posta gönderir.
                if (!string.IsNullOrEmpty(kurBilgisi))
                {
                    emailService.SendEmail(email, kurBilgisi); // E-posta gönderme metodunu çağırır.
                    MessageBox.Show("Kur bilgileri başarıyla gönderildi."); // Başarılı olduğunu belirten bir mesaj kutusu gösterir.
                }
                else
                {
                    MessageBox.Show("Kur bilgileri alınamadı."); // Kur bilgileri alınamazsa, bir mesaj kutusu gösterir.
                }
            }
            catch (Exception ex)
            {
                // Bir hata oluşursa, hata mesajını ve varsa iç hata mesajını gösterir.
                MessageBox.Show("Bir hata oluştu: " + ex.Message + "\nInner Exception: " + ex.InnerException?.Message);
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

    

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}