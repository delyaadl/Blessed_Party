using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace egitlab_PotionNetCore
{
    public class QueryData
    {
        readonly string strCon = @"Data Source=DESKTOP-RV5F2H7;Initial Catalog=Blessed_Party;Persist Security Info=True;Integrated Security=SSPI;";
        readonly string strConPub = @"Data Source=SQL5105.site4now.net,1433;Initial Catalog=db_a7d8a1_blessedparty;User Id=db_a7d8a1_blessedparty_admin;Password=blessed123;";

        public DataTable GetDataSql(string strsql)
        {

            using (SqlConnection con = new SqlConnection(strCon))
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(strsql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                con.Close();
                return dt;
            }

        }

    }
}