using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;

namespace WindowsFormsApp1
{

    //currencyRate döviz kurunlarını temsil eder. 
    public class CurrencyRate
    {
        public string CurrencyName { get; set; } // kurun adi
        public float Rate { get; set; }   //kurun degeri
      
    }


    //CurrencyRequest sınıfı, döviz kurlarını API'den çekmek ve veritabanına kaydetmekle sorumludur. 
    public class CurrencyRequest
    {
        private static readonly HttpClient client = new HttpClient(); //HttpClient sınıfı, API istekleri yapmak için kullanılır.


        //Main metodu asenkron olarak çalışır ve döviz kurlarını almak için API'ye bir GET isteği gönderir. Yanıt response değişkeninde saklanır.
        public async Task UpdateRatesAsync()
        {
            try
            {
                var response = await client.GetStringAsync("https://api.vatcomply.com/rates?base=TRY");

                // JSON yanıtını ayrıştır
                var json = JObject.Parse(response);
                var rates = json["rates"] as JObject;

                // Döviz kurları bir listeye dönüştürülür. Her bir döviz kuru, CurrencyRate nesnesine dönüştürülür ve listeye eklenir.
                var currencyRates = new List<CurrencyRate>();

                foreach (var rate in rates)
                {
                    var currencyName = rate.Key;
                    var rateValue = rate.Value.ToObject<float>();
                    currencyRates.Add(new CurrencyRate
                    {
                        CurrencyName = currencyName,
                        Rate = rateValue,
                       
                    }); 
                }
              



                //Veritabanına bağlanmak için gerekli bağlantı dizesi tanımlanır. SqlConnection nesnesi kullanılarak veritabanına bağlanılır ve bağlantı asenkron olarak açılır.
                string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    //tüm kur isimlerini tabloya ekleme
                    foreach (var rate in currencyRates)
                    {
                        // Kur ismini veritabanında kontrol et
                        string checkCurrencyQuery = "SELECT COUNT(*) FROM KurIsimleri WHERE KurIsim = @KurIsim";
                        using (SqlCommand checkCommand = new SqlCommand(checkCurrencyQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@KurIsim", rate.CurrencyName);
                            int count = (int)await checkCommand.ExecuteScalarAsync();

                            // Eğer kur ismi veritabanında yoksa ekle
                            if (count == 0)
                            {
                                string insertCurrencyQuery = "INSERT INTO KurIsimleri (KurIsim) VALUES (@KurIsim)";
                                using (SqlCommand insertCommand = new SqlCommand(insertCurrencyQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@KurIsim", rate.CurrencyName);
                                    await insertCommand.ExecuteNonQueryAsync();
                                }
                            }
                 
                        }
                    }    




                    // kur Id si lazım
                    foreach (var rate in currencyRates)
                        {
                        // rate değişkeninin tersini x olarak hesaplama
                        float x = 1 / rate.Rate;

                        // rate değişkenini x olarak güncelleme
                        rate.Rate = x;

                        // Döviz kurunun ismine göre KurIsimleri tablosundan ilgili kaydı alacak SQL sorgusu hazırlanır
                        string foreach_kurlar = "select top 10 * from KurIsimleri where KurIsim= '" + rate.CurrencyName + "'";
                        SqlCommand kur_getir_command = new SqlCommand(foreach_kurlar, connection);

                        // SQL sorgusu asenkron olarak yürütülür ve sonuçlar bir SqlDataReader nesnesinde saklanır
                        SqlDataReader reader = await kur_getir_command.ExecuteReaderAsync();

                        // Sonuçları okumak için SqlDataReader nesnesi kullanılır
                        var dataReaded = await reader.ReadAsync();

                        // Eğer bir sonuç döndüyse (yani döviz kuru ismi veritabanında bulunuyorsa)
                        if (dataReaded)
                        {
                            // KurIsimleriID alanı okunur ve bir değişkene atanır
                            int kur_id = Convert.ToInt32(reader["KurIsimleriID"]);

                            // SqlDataReader nesnesi kapatılır
                            reader.Close();




                            // Kurlar tablosuna yeni bir kayıt eklemek için SQL sorgusu hazırlanır
                            string query = "INSERT INTO Kurlar(KurDegeri, TarihSaat, Rate, KurIsimleriId) VALUES (@KurDegeri, @TarihSaat, @Rate, @KurIsimleriId)";

                            // Yeni bir SqlCommand nesnesi oluşturulur ve parametreler sorguya eklenir
                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                // Parametreler sorguya eklenir
                                command.Parameters.AddWithValue("@KurDegeri", 0);
                                command.Parameters.AddWithValue("@TarihSaat", DateTime.Now);
                                command.Parameters.AddWithValue("@Rate", Convert.ToDecimal(rate.Rate));
                                command.Parameters.AddWithValue("@KurIsimleriId", kur_id);
                                
                                // SQL sorgusu asenkron olarak yürütülür ve veri veritabanına eklenir
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }

                }

                Console.WriteLine("Döviz kurları başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}


