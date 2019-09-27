using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightSpeedAutoUtility
{
    class Ado
    {
        public void ZeroPickedByDateRange(string startDate, string endDate, Dictionary<int, string> branchList)
        {          
            DataSet ds = new DataSet();
            ds.Tables.Add(new DataTable());
            ds.Tables[0].Columns.Add("Branch", typeof(string));
            ds.Tables[0].Columns.Add("Total Picked", typeof(int));
            ds.Tables[0].Columns.Add("Single Picked", typeof(int));
            ds.Tables[0].Columns.Add("Cases Picked", typeof(string));
            ds.Tables[0].Columns.Add("Note", typeof(string));
            ds.Tables[0].Columns["Total Picked"].DefaultValue = 0;
            ds.Tables[0].Columns["Single Picked"].DefaultValue = 0;
            ds.Tables[0].Columns["Cases Picked"].DefaultValue = "0";
            ds.Tables[0].Columns["Note"].DefaultValue = "";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["LightSpeedServerConn"].ToString()))
            {           
                SqlDataAdapter da = new SqlDataAdapter();
                foreach (var branchId in branchList)
                {
                    ds.Tables[0].Columns["Branch"].DefaultValue = branchId.Value;
                    SqlCommand cmd = new SqlCommand("ZeroPickedByDateRange", con);
                    cmd.Parameters.AddWithValue("@sdate", startDate);
                    cmd.Parameters.AddWithValue("@edate", endDate);
                    cmd.Parameters.AddWithValue("@routeIDs", string.Empty);
                    cmd.Parameters.AddWithValue("@BranchId", branchId.Key);
                    cmd.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand = cmd;
                    da.Fill(ds.Tables[0]);                 
                }
                con.Close();
            }
            ZeroPickedWritetoDwDB(ds.Tables[0]);
           // ZeroPickedWritetoStaging(ds.Tables[0]);
        }

        public void ProductByDateRange(string startDate, string endDate, Dictionary<int, string> branchList)
        {
            DataSet dsProduct = new DataSet();
            dsProduct.Tables.Add(new DataTable());
            dsProduct.Tables[0].Columns.Add("Branch", typeof(string));
            dsProduct.Tables[0].Columns.Add("Order Count", typeof(string));
            dsProduct.Tables[0].Columns.Add("Row Coil Count", typeof(string));
            dsProduct.Tables[0].Columns.Add("Note", typeof(string));
            dsProduct.Tables[0].Columns["Order Count"].DefaultValue = "0";
            dsProduct.Tables[0].Columns["Row Coil Count"].DefaultValue = "0";
            dsProduct.Tables[0].Columns["Note"].DefaultValue = "";
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["LightSpeedServerConn"].ToString()))
            {
                SqlDataAdapter da = new SqlDataAdapter();
                foreach (var branchId in branchList)
                {
                    dsProduct.Tables[0].Columns["Branch"].DefaultValue = branchId.Value;
                    SqlCommand cmd = new SqlCommand("ProductByDateRange", con);
                    cmd.Parameters.AddWithValue("@sdate", startDate);
                    cmd.Parameters.AddWithValue("@edate", endDate);
                    cmd.Parameters.AddWithValue("@routeID", string.Empty);
                    cmd.Parameters.AddWithValue("@ZoneID", 0);
                    cmd.Parameters.AddWithValue("@category", string.Empty);
                    cmd.Parameters.AddWithValue("@branchId", branchId.Key);
                    cmd.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand = cmd;
                    da.Fill(dsProduct.Tables[0]);
                    dsProduct.Tables[0].Columns.Remove("category");
                    dsProduct.Tables[0].Columns.Remove("productCode");
                    dsProduct.Tables[0].Columns.Remove("product");
                    dsProduct.Tables[0].Columns.Remove("ProviderProductID");
                    dsProduct.Tables[0].Columns.Remove("caseSize");
                    dsProduct.Tables[0].Columns.Remove("zone");                  
                }
                con.Close();
            }        
            ProductByWritetoDwDB(dsProduct.Tables[0]);
           // ProductByWritetoStaging(dsProduct.Tables[0]);
        }

        //Zero Picked By Date Range
        public static void ZeroPickedWritetoDwDB(DataTable daTable)
        {
            //Get connection string from app.config

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DwConn"].ToString()))
            {
                con.Open();
                using (SqlBulkCopy bk = new SqlBulkCopy(con, SqlBulkCopyOptions.TableLock, null))
                {
                    string destinationTable = ConfigurationSettings.AppSettings["DWTableName"].ToString();
                    bk.DestinationTableName = destinationTable;
                    foreach (var col in daTable.Columns)
                    {
                        if (col.ToString() == "product")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Product");
                        }
                        else if (col.ToString() == "numOrders")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Order Count");
                        }
                        else if (col.ToString() == "numOccurances")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Row Coil Count");
                        }
                        else if (col.ToString() == "amountOrdered")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Quantity Ordered");
                        }
                        else if (col.ToString() == "startDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "StartDate");
                        }
                        else if (col.ToString() == "endDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "EndDate");
                        }
                        else if (col.ToString() == "Branch")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Branch");
                        }
                        else if (col.ToString() == "Total Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Total Picked");
                        }
                        else if (col.ToString() == "Single Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Singles Picked");
                        }
                        else if (col.ToString() == "Cases Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Cases Picked");
                        }
                        else if (col.ToString() == "Note")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Note");
                        }
                    }

                    try
                    {
                        bk.WriteToServer(daTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    con.Close();
                }

            }
        }

    
        //Product picked bt date range
        public static void ProductByWritetoDwDB(DataTable daTable)
        {
            //Get connection string from app.config
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DwConn"].ToString()))
            {
                con.Open();
                using (SqlBulkCopy bk = new SqlBulkCopy(con, SqlBulkCopyOptions.TableLock, null))
                {
                    string destinationTable = ConfigurationSettings.AppSettings["DWTableName"].ToString();
                    bk.DestinationTableName = destinationTable;
                    foreach (var col in daTable.Columns)
                    {
                       if (col.ToString() == "Branch")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Branch");
                        }
                        else if (col.ToString() == "Order Count")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Order Count");
                        }
                        else if (col.ToString() == "Row Coil Count")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Row Coil Count");
                        }
                        else if (col.ToString() == "StartDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "StartDate");
                        }
                        else if (col.ToString() == "EndDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "EndDate");
                        }
                        else if (col.ToString() == "codeProduct")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Product");
                        }
                        else if (col.ToString() == "OrderedQuantity")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Quantity Ordered");
                        }
                        else if (col.ToString() == "PickedQuantity")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Total Picked");
                        }                                                               
                        else if (col.ToString() == "casesPicked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Cases Picked");
                        }
                        else if (col.ToString() == "singlesPicked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Singles Picked");
                        }
                        else if (col.ToString() == "Note")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Note");
                        }
                        else
                        {
                            continue;
                        }                     
                    }
                    try
                    {
                        bk.WriteToServer(daTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    con.Close();
                }
            }
        }


        //Below section is not useable for now // maybe it will use in future.
        //staging table 
        public static void ProductByWritetoStaging(DataTable daTable)
        {
            //Get connection string from app.config
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["StagingConn"].ToString()))
            {
                con.Open();
                using (SqlBulkCopy bk = new SqlBulkCopy(con, SqlBulkCopyOptions.TableLock, null))
                {
                    string destinationTable = ConfigurationSettings.AppSettings["StagingTableName"].ToString();
                    bk.DestinationTableName = destinationTable;
                    foreach (var col in daTable.Columns)
                    {
                        if (col.ToString() == "Branch")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Branch");
                        }
                        else if (col.ToString() == "Order Count")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Order Count");
                        }
                        else if (col.ToString() == "Row Coil Count")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Row Coil Count");
                        }
                        else if (col.ToString() == "StartDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "StartDate");
                        }
                        else if (col.ToString() == "EndDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "EndDate");
                        }
                        else if (col.ToString() == "codeProduct")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Product");
                        }
                        else if (col.ToString() == "OrderedQuantity")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Quantity Ordered");
                        }
                        else if (col.ToString() == "PickedQuantity")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Total Picked");
                        }
                        else if (col.ToString() == "casesPicked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Cases Picked");
                        }
                        else if (col.ToString() == "singlesPicked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Singles Picked");
                        }
                        else if (col.ToString() == "Note")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Note");
                        }
                        else
                        {
                            continue;
                        }
                    }
                    try
                    {
                        bk.WriteToServer(daTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    con.Close();
                }
            }
        }

        //staging table 
        public static void ZeroPickedWritetoStaging(DataTable daTable)
        {
            //Get connection string from app.config

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["StagingConn"].ToString()))
            {
                con.Open();
                using (SqlBulkCopy bk = new SqlBulkCopy(con, SqlBulkCopyOptions.TableLock, null))
                {
                    string destinationTable = ConfigurationSettings.AppSettings["StagingTableName"].ToString();
                    bk.DestinationTableName = destinationTable;
                    foreach (var col in daTable.Columns)
                    {
                        if (col.ToString() == "product")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Product");
                        }
                        else if (col.ToString() == "numOrders")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Order Count");
                        }
                        else if (col.ToString() == "numOccurances")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Row Coil Count");
                        }
                        else if (col.ToString() == "amountOrdered")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Quantity Ordered");
                        }
                        else if (col.ToString() == "startDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "StartDate");
                        }
                        else if (col.ToString() == "endDate")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "EndDate");
                        }
                        else if (col.ToString() == "Branch")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Branch");
                        }
                        else if (col.ToString() == "Total Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Total Picked");
                        }
                        else if (col.ToString() == "Single Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Singles Picked");
                        }
                        else if (col.ToString() == "Cases Picked")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Cases Picked");
                        }
                        else if (col.ToString() == "Note")
                        {
                            bk.ColumnMappings.Add(col.ToString(), "Note");
                        }
                    }

                    try
                    {
                        bk.WriteToServer(daTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    con.Close();
                }

            }
        }
    }
}
