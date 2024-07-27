using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class EmailService
    {
        public async Task<string> GetKurBilgisiAsync()
        {
            StringBuilder kurBilgisi = new StringBuilder();
            string connectionString = "Server=ZYNPYLMN\\MSSQLSERVER01;Initial Catalog=DatabaseKur;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT ki.KurIsim, k.Rate, k.TarihSaat FROM Kurlar k JOIN KurIsimleri ki ON k.KurIsimleriId = ki.KurIsimleriID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        string kurIsim = reader["KurIsim"].ToString();
                        decimal rate = (decimal)reader["Rate"];
                        DateTime tarihSaat = (DateTime)reader["TarihSaat"];

                        kurBilgisi.AppendLine($"Kur: {kurIsim}, Değer: {rate}, Tarih: {tarihSaat}");
                    }
                }
            }

            return kurBilgisi.ToString();
        }

        public void SendEmail(string email, string kurBilgisi)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("zynpylmnzynpylmn@outlook.com");
                mail.To.Add(email);
                mail.Subject = "Güncel Kur Bilgileri";
                mail.Body = kurBilgisi;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("zynpylmnzynpylmn@outlook.com", "vcfclypdjfoalinu");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
        }
    }
}

