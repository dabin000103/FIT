using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using TripboxSupply_API.Models;
using TripboxSupply_API.Models.WebSite;
using Newtonsoft.Json;
using TripboxSupply_API.Models.Order;
using TripboxSupply_API.Models.Request;

namespace TripboxSupply_API.Helper
{
    public static class DataHelper
    {
        public static DataTable ToDataTable<T>(this List<T> list)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in list)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
        
        public static DataTable ExpandField(this DataTable table, string FieldName, string ColumnType)
        {
            try
            {
                DataColumn dc = new DataColumn(FieldName);
                dc.DataType = System.Type.GetType(ColumnType);
                table.Columns.Add(dc);
                return table;
            }
            catch
            {
                return table;
            }
        }

        public static DataTable ConvertColumnType(this DataTable refTable, string refField)
        {
            DataTable table = refTable.Clone();
            table.Columns[refField].DataType = typeof(Boolean);

            return table;
        }

        public static string ToJsonStringBuilder(this DataTable table)
        {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }

        #region ToObject
        public static T ToObject<T>(this DataRow dataRow) where T : new()
        {
            T item = new T();
            PropertyInfo property;
            string columnName;
            try {

                foreach (DataColumn column in dataRow.Table.Columns)
                {
                    columnName = column.ColumnName;
                    property = GetProperty(typeof(T), column.ColumnName);

                    if (property != null && dataRow[column] != DBNull.Value && dataRow[column].ToString() != "NULL")
                    {

                        if (column.ColumnName == "Zones")
                        {
                            List<WZone> list = JsonConvert.DeserializeObject<List<WZone>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "CurrentCatalog")
                        {
                            WCatalog list = JsonConvert.DeserializeObject<WCatalog>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "MenuCatalogs")
                        {
                            List<WCatalog> list = JsonConvert.DeserializeObject<List<WCatalog>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductsInCart")
                        {
                            List<WProductInCart> list = JsonConvert.DeserializeObject<List<WProductInCart>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        #region ProductInDetail
                        else if (column.ColumnName == "MeetingPoint" || column.ColumnName == "TourEndPoint")
                        {
                            WPlace list = JsonConvert.DeserializeObject<WPlace>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductItinerarys")
                        {
                            List<WProductItinerary> list = JsonConvert.DeserializeObject<List<WProductItinerary>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "Departure" || column.ColumnName == "Return")
                        {
                            List<WProductAirSchedule> list = JsonConvert.DeserializeObject<List<WProductAirSchedule>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductContents")
                        {
                            List<WProductContent> list = JsonConvert.DeserializeObject<List<WProductContent>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "OptionServices")
                        {
                            List<WOptionService> list = JsonConvert.DeserializeObject<List<WOptionService>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ReviewSummarys")
                        {
                            List<WReviewSummary> list = JsonConvert.DeserializeObject<List<WReviewSummary>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductReviews" || column.ColumnName == "ProductQAs")
                        {
                            List<WBoardArticle> list = JsonConvert.DeserializeObject<List<WBoardArticle>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductInSearchs")
                        {
                            List<WProductInSearch> list = JsonConvert.DeserializeObject<List<WProductInSearch>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "InputEntrys")
                        {
                            List<WProductInputEntry> list = JsonConvert.DeserializeObject<List<WProductInputEntry>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductInputEntrys" || column.ColumnName == "TravelerInputEntrys")
                        {
                            List<WProductInputEntry> list = JsonConvert.DeserializeObject<List<WProductInputEntry>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductImages")
                        {
                            List<WProductImage> list = JsonConvert.DeserializeObject<List<WProductImage>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ZoneMenus")
                        {
                            List<WCatalog> list = JsonConvert.DeserializeObject<List<WCatalog>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "ProductsInCart")
                        {
                            List<WProductInCart> list = JsonConvert.DeserializeObject<List<WProductInCart>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "AllTravelers")
                        {
                            List<WOrderTraveler> list = JsonConvert.DeserializeObject<List<WOrderTraveler>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "PriceCalendar")
                        {
                            List<WPriceCalendar> list = JsonConvert.DeserializeObject<List<WPriceCalendar>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        else if (column.ColumnName == "BoardArticles")
                        {
                            List<WBoardArticle> list = JsonConvert.DeserializeObject<List<WBoardArticle>>(dataRow[column].ToString());
                            property.SetValue(item, list, null);
                        }
                        #endregion
                        else
                        {
                            //property.SetValue(item, JsonSerializer.Deserialize, property.PropertyType), null);
                            property.SetValue(item, ChangeType(dataRow[column], property.PropertyType), null);
                        }
                    }
                }
            }
            catch (Exception ex) {

                Console.WriteLine(ex.Message);
                return item;
            }
            
            return item;
        }
        private static PropertyInfo GetProperty(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName);

            if (property != null)
            {
                return property;
            }

            return type.GetProperties()
                 .Where(p => p.IsDefined(typeof(DisplayAttribute), false) && p.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name == attributeName)
                 .FirstOrDefault();
        }
        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            return Convert.ChangeType(value, type);
        }
        #endregion

        #region ToList
        public static List<T> ToList<T>(this DataTable dt)
        {
            //try
            //{
                var columnNames = dt.Columns.Cast<DataColumn>()
                        .Select(c => c.ColumnName)
                        .ToList();
                var properties = typeof(T).GetProperties();
                return dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();
                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name))
                        {
                            PropertyInfo pI = objT.GetType().GetProperty(pro.Name);

                            if (pro.Name == "ProductContents")
                            {
                                List<WProductContent> list = JsonConvert.DeserializeObject<List<WProductContent>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductSearchs")
                            {
                                List<WProductInSearch> list = JsonConvert.DeserializeObject<List<WProductInSearch>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductItinerarys")
                            {
                                List<WProductItinerary> list = JsonConvert.DeserializeObject<List<WProductItinerary>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "OptionMustFees")
                            {
                                List<WMustService> list = JsonConvert.DeserializeObject<List<WMustService>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "OptionMustCharges")
                            {
                                List<WMustService> list = JsonConvert.DeserializeObject<List<WMustService>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "OptionServices")
                            {
                                List<WOptionService> list = JsonConvert.DeserializeObject<List<WOptionService>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Options")
                            {
                                List<OptionService> list = JsonConvert.DeserializeObject<List<OptionService>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "OptionItinerarys")
                            {
                                List<WOptionItinerary> list = JsonConvert.DeserializeObject<List<WOptionItinerary>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Itinerarys")
                            {
                                List<OptionItinerary> list = JsonConvert.DeserializeObject<List<OptionItinerary>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Departure" || pro.Name == "Return")
                            {
                                List<WProductAirSchedule> list = JsonConvert.DeserializeObject<List<WProductAirSchedule>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "SaleItinerarys")
                            {
                                List<WSaleItinerary> list = JsonConvert.DeserializeObject<List<WSaleItinerary>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductReviews")
                            {
                                List<WBoardArticle> list = JsonConvert.DeserializeObject<List<WBoardArticle>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ReviewSummarys")
                            {
                                List<WReviewSummary> list = JsonConvert.DeserializeObject<List<WReviewSummary>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductQAs")
                            {
                                List<WBoardArticle> list = JsonConvert.DeserializeObject<List<WBoardArticle>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "InputEntrys")
                            {
                                List<WProductInputEntry> list = JsonConvert.DeserializeObject<List<WProductInputEntry>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Inputs")
                            {
                                List<InputEntry> list = JsonConvert.DeserializeObject<List<InputEntry>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductInputEntrys" || pro.Name == "TravelerInputEntrys")
                            {
                                List<WProductInputEntry> list = JsonConvert.DeserializeObject<List<WProductInputEntry>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "MeetingPoint" || pro.Name == "TourEndPoint")
                            {
                                WPlace list = JsonConvert.DeserializeObject<WPlace>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            //else if (pro.Name == "OptionInCarts")
                            //{
                            //    List<WOptionInCart> list = JsonConvert.DeserializeObject<List<WOptionInCart>>(row[pro.Name].ToString());
                            //    pro.SetValue(objT, list);
                            //}                       
                            else if (pro.Name == "ProductImages")
                            {
                                List<WProductImage> list = JsonConvert.DeserializeObject<List<WProductImage>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            //ORDER
                            else if (pro.Name == "Sales")
                            {
                                List<WSale> list = JsonConvert.DeserializeObject<List<WSale>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            //SALE
                            else if (pro.Name == "SaleOptions")
                            {
                                List<WSaleOption> list = JsonConvert.DeserializeObject<List<WSaleOption>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "SalePolicys")
                            {
                                List<WSalePolicy> list = JsonConvert.DeserializeObject<List<WSalePolicy>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "SaleTravelers")
                            {
                                List<WSaleTraveler> list = JsonConvert.DeserializeObject<List<WSaleTraveler>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "AllTravelers")
                            {
                                List<WOrderTraveler> list = JsonConvert.DeserializeObject<List<WOrderTraveler>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Travelers")
                            {
                                List<Traveler> list = JsonConvert.DeserializeObject<List<Traveler>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            //else if (pro.Name == "SaleEntrys")
                            //{
                            //    List<SaleEntry> list = JsonConvert.DeserializeObject<List<SaleEntry>>(row[pro.Name].ToString());
                            //    pro.SetValue(objT, list);
                            //}
                            else if (pro.Name == "ZoneMenus")
                            {
                                List<WCatalog> list = JsonConvert.DeserializeObject<List<WCatalog>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "SaleInputEntrys")
                            {
                                List<WSaleInputEntry> list = JsonConvert.DeserializeObject<List<WSaleInputEntry>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "PriceOptionContents")
                            {
                                List<WProductInSearchContent> list = JsonConvert.DeserializeObject<List<WProductInSearchContent>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductInSearchContents")
                            {
                                List<WProductInSearchContent> list = JsonConvert.DeserializeObject<List<WProductInSearchContent>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ContentImages")
                            {
                                List<WProductContent> list = JsonConvert.DeserializeObject<List<WProductContent>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "AttachedFiles")
                            {
                                List<WBoardArticleImage> list = JsonConvert.DeserializeObject<List<WBoardArticleImage>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "ProductsInCart")
                            {
                                List<WProductInCart> list = JsonConvert.DeserializeObject<List<WProductInCart>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "MustServices")
                            {
                                List<WMustService> list = JsonConvert.DeserializeObject<List<WMustService>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "Childs")
                            {
                                List<WCatalog> list = JsonConvert.DeserializeObject<List<WCatalog>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "CartProcucts")
                            {
                                List<CartOrderProduct> list = JsonConvert.DeserializeObject<List<CartOrderProduct>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else if (pro.Name == "PayInfos")
                            {
                                List<ReceiveNoti> list = JsonConvert.DeserializeObject<List<ReceiveNoti>>(row[pro.Name].ToString());
                                pro.SetValue(objT, list);
                            }
                            else
                            {
                                //pro.SetValue(objT, row[pro.Name]);
                                pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : Convert.ChangeType(row[pro.Name], pI.PropertyType));
                                //pro.SetValue(objT, string.IsNullOrEmpty(row[pro.Name].ToString()) ? null : Convert.ChangeType(row[pro.Name], pI.PropertyType));
                            }
                        }
                    }
                    return objT;
                }).ToList();
           // }
            //catch (Exception ex)
            //{
            //    return null;
            //}
        }
        #endregion
        
        #region ToListItem
        public static List<T> ToListItem<T>(this DataTable dt)
        {
            List<T> data = new();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        #endregion

        #region Map To
        public static void MapProp(object sourceObj, object targetObj)
        {
            Type T1 = sourceObj.GetType();
            Type T2 = targetObj.GetType();

            PropertyInfo[] sourceProprties = T1.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyInfo[] targetProprties = T2.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var sourceProp in sourceProprties)
            {              
                object osourceVal = sourceProp.GetValue(sourceObj, null);
                int entIndex = Array.IndexOf(targetProprties, sourceProp);
                if (entIndex >= 0)
                {
                    var targetProp = targetProprties[entIndex];
                    targetProp.SetValue(targetObj, osourceVal);
                }
            }
        }
        public static void MatchAndMap<TSource, TDestination>(this TSource source, TDestination destination)
           where TSource : class, new()
           where TDestination : class, new()
        {
            if (source != null && destination != null)
            {
                List<PropertyInfo> sourceProperties = source.GetType().GetProperties().ToList<PropertyInfo>();
                List<PropertyInfo> destinationProperties = destination.GetType().GetProperties().ToList<PropertyInfo>();

                foreach (PropertyInfo sourceProperty in sourceProperties)
                {
                    PropertyInfo destinationProperty = destinationProperties.Find(item => item.Name == sourceProperty.Name);

                    if (destinationProperty != null)
                    {
                        try
                        {
                            destinationProperty.SetValue(destination, sourceProperty.GetValue(source, null), null);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

        }

        public static TDestination MapProperties<TDestination>(this object source)
            where TDestination : class, new()
        {
            var destination = Activator.CreateInstance<TDestination>();
            MatchAndMap(source, destination);

            return destination;
        }
        #endregion
    }

    public class MyExtensionMethods
    {
        public static Boolean IsValidUri(String url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
