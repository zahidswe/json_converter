using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Dapper;
using System.Configuration;
using Newtonsoft.Json;
using System.Web.Hosting;
using System.IO;

namespace JsonConvertor
{
    public partial class Form1 : Form
    {
        private SqlConnection  con;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Text File";
            openFileDialog.Filter = "TXT files|*.txt";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                try
                {
                    string filename = openFileDialog.FileName;

                    string[] filelines = File.ReadAllLines(filename);


                    for (int a = 0; a < filelines.Length; a++)
                    {
                        string[] tables = filelines[a].Split(',');

                        foreach (var table in tables)
                        {
                            GetAllInfo(table);
                    
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
               
      
            }
        }
        public static void GetAllInfo(string table)
        {
            using (IDbConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                if (db.State == ConnectionState.Closed)
                    db.Open();

                var items = db.Query<object>("select * from "+table).ToArray();
                try
                {
                    string json = JsonConvert.SerializeObject(items, Formatting.Indented, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });
                    string path = System.IO.Path.GetFullPath("~");

                    System.IO.File.WriteAllText(table + ".json", json);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }
    }
}
