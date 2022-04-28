using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace ClPP.Praas.Function
{
    public static class Function1
    {
        [FunctionName("AddPromotion")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var promotion = JsonConvert.DeserializeObject<Promotion>(requestBody);
            //dynamic promotion = JsonConvert.DeserializeObject(req.Body.ToString());
            var str = Environment.GetEnvironmentVariable("sqldb_connection");
            using (SqlConnection conn = new SqlConnection(str))
            {
                var query  = @"INSERT INTO dbo.Promotion (PromotionOccasion, ValidFrom, ValidTill, CustomerId, Description) 
                   VALUES (@PromotionOccasion, @ValidFrom, @ValidTill, @CustomerId, @Description) ";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // define parameters and their values
                    cmd.Parameters.Add("@PromotionOccasion", SqlDbType.VarChar, 50).Value = promotion.PromotionOccasion;
                    cmd.Parameters.Add("@ValidFrom", SqlDbType.DateTime, 10).Value = promotion.ValidFrom; //DateTime.ParseExact( promotion.ValidFrom.ToString(), "yyyy-", CultureInfo.InvariantCulture);
                    cmd.Parameters.Add("@ValidTill", SqlDbType.DateTime).Value = promotion.ValidTill;// DateTime.ParseExact(promotion.ValidTill.ToString(), "dd-mm-yyyy", CultureInfo.InvariantCulture); ;
                    cmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = promotion.CustomerId;
                    cmd.Parameters.Add("@Description", SqlDbType.VarChar, 50).Value = promotion.Description?? string.Empty;
                    // open connection, execute INSERT, close connection
                    conn.Open();
                    await cmd.ExecuteNonQueryAsync();
                    conn.Close();
                }
               
            }
        }
    }

    public class Promotion
    {
        public string PromotionOccasion { get; set; }
        public DateTime ValidFrom { get; set; }
        public string ValidTill { get; set; }
        public int CustomerId { get; set; }
        public string Description { get; set; }
    }
}
