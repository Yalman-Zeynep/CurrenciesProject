using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Veritabanından döviz bilgilerini almak için bu sınıfı oluşturuyoruz:

namespace WindowsFormsApp1
{
    public class CurrenciesService
    {

        public async Task<string> GetKurBilgisiAsync(int currencyId)

        {
            StringBuilder kurBilgisi = new StringBuilder();
            string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT ki.KurIsim, k.Rate, k.TarihSaat FROM Kurlar k JOIN KurIsimleri ki ON k.KurIsimleriID = ki.KurIsimleriID WHERE kİ.KurIsimleriID = @currencyId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@currencyId", currencyId);
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string kurIsim = reader["KurIsim"].ToString();

                        // Rate'i string olarak oku
                        string rateString = reader["Rate"].ToString();
                        DateTime tarihSaat = (DateTime)reader["TarihSaat"];

                        kurBilgisi.AppendLine($"Kur: {kurIsim}, Değer: {rateString}, Tarih: {tarihSaat}");



                    }
                }

                return kurBilgisi.ToString();
            }
        }
    }
}

