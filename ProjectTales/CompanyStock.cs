using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Globalization;

namespace ProjectTales
{
    public struct DataPoint
    {
        public string name;
        public DateTime date;
        public double open;
        public double high;
        public double low;
        public double close;
        public double volume;

        public DataPoint(string name, DateTime date, double open, double high, double low, double close, double volume)
        {
            this.name = name;
            this.date = date;
            this.open = open;
            this.high = high;
            this.low = low;
            this.close = close;
            this.volume = volume;
        }

        public DataPoint(DataPoint copy)
        {
            this.name = copy.name;
            this.date = copy.date;
            this.open = copy.open;
            this.high = copy.high;
            this.low = copy.low;
            this.close = copy.close;
            this.volume = copy.volume;
        }

        public DataPoint(DateTime date) : this("null", date, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN) { }
        public DataPoint(String companyName, DateTime date) : this(companyName, date, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN) { }
        public bool IsNull { get { return double.IsNaN(close); } }
    }

    public class CompanyStock
    {
        public String name;
        public Dictionary<DateTime,DataPoint> stockData;

        public CompanyStock(String fileName)
        {
            this.name = Path.GetFileNameWithoutExtension(fileName);
            this.stockData = new Dictionary<DateTime, DataPoint>();

            StreamReader file = new StreamReader(fileName);   
            
            //Get rid of the first line, its only headers.
            string line;
            line = file.ReadLine();

            while((line = file.ReadLine()) != null){
                //Parse the comma separated file.
                String[] separatedValues = line.Split(',');

                //Create the DataPoint and store it in the Company's array.
                DateTime tempDate = DateTime.ParseExact(separatedValues[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                DataPoint tempPoint = new DataPoint(this.name, tempDate, Convert.ToDouble(separatedValues[2]), Convert.ToDouble(separatedValues[3]), Convert.ToDouble(separatedValues[4]), Convert.ToDouble(separatedValues[5]), Convert.ToDouble(separatedValues[6]));

                stockData.Add(tempDate, tempPoint);
            }
        }   
    }
}

