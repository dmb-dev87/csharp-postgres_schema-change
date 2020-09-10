using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Npgsql;
using System.IO;

namespace PostgreDBSchema
{
    /*public class TableInfo
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public override string ToString()
        {
            return "SchemaName: " + SchemaName + "   TableName: " + TableName;
        }
    }*/

    public partial class Schema_Form : Form
    {
        private string str_ipaddr;
        private string str_portNum;
        private string str_usrName;
        private string str_password;
        private string str_dbName;
        private string str_schemaName;
        private bool bAllSchemas;
        private NpgsqlConnection db_conn;

        private StreamWriter sw;

        public Schema_Form()
        {
            InitializeComponent();

            bAllSchemas = true;
            cb_allschmas.Checked = true;
            txt_schemaname.Enabled = false;

            //Pass the filepath and filename to the StreamWriter Constructor
            sw = new StreamWriter("log.txt");
        }

        private bool validateValues()
        {
            if (txt_ipaddr.Text == "")
            {
                MessageBox.Show("Please enter the ip address of Database.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                str_ipaddr = txt_ipaddr.Text;
            }

            if (txt_portnum.Text == "")
            {
                MessageBox.Show("Please enter the port number of Database.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                str_portNum = txt_portnum.Text;
            }

            if (txt_username.Text == "")
            {
                MessageBox.Show("Please input the user name of Database.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                str_usrName = txt_username.Text;
            }

            if (txt_password.Text == "")
            {
                MessageBox.Show("Please input the password of Database.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                str_password = txt_password.Text;
            }

            if (txt_dbname.Text == "")
            {
                MessageBox.Show("Please input the name of Database.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else
            {
                str_dbName = txt_dbname.Text;
            }

            if (bAllSchemas == false && txt_schemaname.Text == "")
            {
                MessageBox.Show("Please input the name of Schema.", "Information", MessageBoxButtons.OK);
                return false;
            }
            else if (bAllSchemas == false)
            {
                str_schemaName = txt_schemaname.Text;
            }
            else
            {
                str_schemaName = "";
            }

            return true;
        }

        private bool connect_db()
        {
            try
            {
                db_conn = new NpgsqlConnection("Server=" + str_ipaddr + "; Port=" + str_portNum + "; User Id=" + str_usrName + "; Password=" + str_password + "; Database=" + str_dbName);
                label_status.Text = "DB Connection: Success.";
            }
            catch(Exception ex)
            {
                label_status.Text = "Error occured.";
                return false;
            }            

            return true;
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            bool val_data = validateValues();

            bool con_db = false;
            
            if (val_data)
            {
                con_db = connect_db();
            }

            if (con_db)
            {
                processTables();
            }
        }

        private void processTables()
        {
            string schema_name = null;
            string table_name = null;

            List<TableInfo> names = new List<TableInfo>();

            try
            {                
                db_conn.Open();

                if (bAllSchemas)
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema!='pg_catalog' AND table_schema!='information_schema' AND table_type='BASE TABLE'", db_conn);
                    NpgsqlDataReader dr = command.ExecuteReader();

                    //change table name
                    while (dr.Read())
                    {
                        schema_name = dr.GetString(0);
                        table_name = dr.GetString(1);
                        names.Add(new TableInfo() { SchemaName = schema_name, TableName = table_name });
                    }
                }
                else
                {
                    NpgsqlCommand command = new NpgsqlCommand("SELECT table_schema, table_name FROM information_schema.tables WHERE table_schema!='pg_catalog' AND table_schema!='information_schema' AND table_type='BASE TABLE' AND table_schema = '" + str_schemaName + "'", db_conn);
                    NpgsqlDataReader dr = command.ExecuteReader();

                    //change table name
                    while (dr.Read())
                    {
                        schema_name = dr.GetString(0);
                        table_name = dr.GetString(1);
                        names.Add(new TableInfo() { SchemaName = schema_name, TableName = table_name });
                    }
                }

                db_conn.Close();
            }
            catch(Exception ex)
            {
                label_status.Text = "Error occured.";
                return;
            }            

            //change table names
            processTables(names);

            label_status.Text = "Process Success";

            sw.Close();

            return;
        }

        private void processTables(List<TableInfo> names)
        {
            try
            {
                for (int i = 0; i < names.Count; i++)
                {
                    label_status.Text = "Process the table: " + names[i];

                    processColumns(names[i].TableName);

                    db_conn.Open();

                    string new_name = names[i].TableName + "2020";
                    NpgsqlCommand changCmd = new NpgsqlCommand();
                    changCmd.Connection = db_conn;
                    changCmd.CommandText = "ALTER TABLE " + names[i].SchemaName + "." + names[i].TableName + " RENAME TO " + new_name;
                    changCmd.ExecuteNonQuery();

                    sw.WriteLine(str_dbName + ", " + names[i].ToString() + " : Change tabel name to " + new_name + ".");

                    db_conn.Close();
                }
            }
            catch(Exception ex)
            {
                label_status.Text = "Error occured.";
                return;
            }
            
        }

        private void processColumns(string tableName)
        {
            string value = null;
            List<string> names = new List<string>();

            try
            {
                db_conn.Open();

                NpgsqlCommand command = new NpgsqlCommand("SELECT column_name FROM information_schema.columns WHERE table_name='" + tableName + "'", db_conn);
                NpgsqlDataReader dr = command.ExecuteReader();

                //get column names
                while (dr.Read())
                {
                    value = dr.GetString(0);
                    names.Add(value);
                }

                db_conn.Close();
            }
            catch(Exception ex)
            {
                label_status.Text = "Error occured.";
                return;
            }

            changeColumnNames(names, tableName);
        }

        private void changeColumnNames(List<string> names, string tableName)
        {
            try
            {
                db_conn.Open();

                for (int i = 0; i < names.Count; i++)
                {
                    string new_name = names[i] + "2020";
                    NpgsqlCommand changCmd = new NpgsqlCommand();
                    changCmd.Connection = db_conn;
                    changCmd.CommandText = "ALTER TABLE " + tableName + " RENAME " + names[i] + " TO " + new_name;
                    changCmd.ExecuteNonQuery();

                    sw.WriteLine(str_dbName + ", " + tableName + ", " + names[i] + " : Change column name to " + new_name + ".");
                }

                db_conn.Close();
            }
            catch(Exception ex)
            {
                label_status.Text = "Error occured.";
                return;
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void cb_allschmas_CheckedChanged(object sender, EventArgs e)
        {
            bAllSchemas = cb_allschmas.Checked;
            txt_schemaname.Enabled = !bAllSchemas;
        }
    }
}
