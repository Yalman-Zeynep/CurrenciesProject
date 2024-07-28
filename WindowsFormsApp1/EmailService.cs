using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class EmailService
    {
        public void SendEmail(string email1, string kurBilgisi1)
        {
            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("zynpylmnzynpylmn@outlook.com");
                mail.To.Add(email1);
                mail.Subject = "Güncel Kur Bilgileri";
                mail.Body = kurBilgisi1;
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

