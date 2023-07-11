using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Data.Common;
using System.Data;
using System.Diagnostics;
using EllipsePaymentService.Models;
using System.Numerics;
using System.Text;
using System.Drawing;
using System.Diagnostics.Metrics;
using System.Security.Principal;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EllipsePaymentService.Services
{   

    public class DbService
    {       

        private string _connectionString;
        public DbService(IConfiguration configuration) {
            _connectionString = configuration.GetValue<string>("MySql");
        }
        public void InitDatabase()
        {
            var query = $"CREATE TABLE IF NOT EXISTS `fyst`(" +
                $"`id` int(11) NOT NULL AUTO_INCREMENT," +
                $"`fyst_id` int(11) NOT NULL DEFAULT '0'," +
                $"`socialclubid` varchar(64) NOT NULL," +
                $"`sum` FLOAT NOT NULL DEFAULT '0'," +
                $"`status` varchar(16) NOT NULL DEFAULT 'PROCESSING'," +
                $"`created` datetime NOT NULL DEFAULT NOW()," +
                $"`updated` datetime NOT NULL DEFAULT NOW()," +
                $"PRIMARY KEY(`id`)" +
                $")ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
            Query(query);
        }
        public bool TryGetAccoutn(string login, out Account account)
        {
            var response = QueryRead("SELECT `socialclubid`, `password`, `gocoins`  FROM `accounts` WHERE `login`=@prop0", login);
            if (response.Rows.Count == 0)
            {
                account = null;
                return false;
            }
            account = new Account { 
                Login = login,
                Password = response.Rows[0]["password"].ToString(),
                SocialclubId = response.Rows[0]["socialclubid"].ToString(),
                Gocoins = Convert.ToSingle(response.Rows[0]["gocoins"]),
                PaymentHistory = GetPaymentHistory(response.Rows[0]["socialclubid"].ToString())
            };
            return true;
        }

        public void AddCoins(string amount, string socialclubId)
        {
            Query("UPDATE `accounts` SET `gocoins`= `gocoins` + @prop0 FROM  WHERE `socialclubid`=@prop1", amount, socialclubId);           
        }

        private List<PaymentInfo> GetPaymentHistory(string socialClubId)
        {
            var result  = new List<PaymentInfo>();
            var response = QueryRead("SELECT `updated`, `status`, `sum`  FROM `fyst` WHERE `socialclubid`=@prop0", socialClubId);
            foreach (DataRow item in response.Rows)
            {
                result.Add(new PaymentInfo
                {
                    Date = Convert.ToDateTime(item["updated"]),
                    Amont = item["sum"].ToString(),
                    Status = item["status"].ToString()
                });
            }
            return result;
        }

        public Order GetOrderById(string id)
        {
            var response = QueryRead("SELECT `updated`, `status`, `socialclubid`, `fyst_id`, `id`, `sum` FROM `fyst` WHERE `id`=@prop0", id);
            var row = response.Rows[0];
            return new Order
            {
                Date = Convert.ToDateTime(row["updated"]),
                SocialclubId = row["socialclubid"].ToString(),
                Status = row["status"].ToString(),
                FystId = row["fyst_id"].ToString(),
                Sum = row["sum"].ToString(),
                Id = Convert.ToInt32(row["id"].ToString())
            };
        }

        public void UpdateOrder(Order order)
        {
            Query("UPDATE `fyst` SET `updated`=@prop0, `status`=@prop1, `fyst_id`=@prop2, WHERE `id`=@prop3", order.Date.ToString("s"), order.Status, order.FystId, order.Id);            
        }


        private void Query(string command, params object[] args)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(command);
                    cmd.Connection = connection;
                    LoadArguments(cmd, args);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                var trace = new System.Diagnostics.StackTrace();
            }
        }
        private DataTable QueryRead(MySqlCommand command)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                command.Connection = connection;

                DbDataReader reader = command.ExecuteReader();
                DataTable result = new DataTable();
                result.Load(reader);

                return result;
            }
        }
       
        private DataTable QueryRead(string command, params object[] args)
        {
            using (MySqlCommand cmd = new MySqlCommand(command))
            {
                LoadArguments(cmd, args);
                return QueryRead(cmd);
            }
        }

        private void LoadArguments(MySqlCommand cmd, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@prop{i}", args[i]);
            }
        }

        internal int CreateNewOrder(BasePaymentRequest request, string socialclubid)
        {
            var data = QueryRead($"INSERT INTO `fyst` (`sum`,`socialclubid`) VALUES(@prop0, @prop1); SELECT @@identity;", request.amount, socialclubid);
            var id = Convert.ToInt32(data.Rows[0][0]);
            return id;
        }
    }
}
