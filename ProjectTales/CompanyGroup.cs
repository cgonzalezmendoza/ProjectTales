using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Globalization;


namespace ProjectTales
{
    class CompanyGroup
    {
        public List<CompanyStock> companies;
        public string name;
        List<List<DataPoint>> dataMatrix;
        public int maxNa;

        public CompanyGroup(String fileName)
        {
            this.name = Path.GetFileNameWithoutExtension(fileName);
            this.companies = new List<CompanyStock>();


            if (File.Exists(fileName))
            {
                // This path is a file
                ProcessFile(fileName);
            }
            else if (Directory.Exists(fileName))
            {
                // This path is a directory
                ProcessDirectory(fileName);
                this.maxNa = this.companies.Capacity / 2;

            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", fileName);
            }
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory);
            }

        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            CompanyStock companyStock = new CompanyStock(path);
            this.companies.Add(companyStock);
        }

        public void CreateDataMatrix()
        {
            //Printing the instructions neccessary to create the stock data matrix.
            Console.WriteLine("You will be creating a text file with the stock data information starting from a starting date.");
            Console.WriteLine("Please input the year (yyyy format):");
            string year = Console.ReadLine();
            Console.WriteLine("Please input the month (mm format):");
            string month = Console.ReadLine();
            Console.WriteLine("Please input the day (dd format):");
            string day = Console.ReadLine();

            //Create the starting date as a DateTime to start getting data from.
            string date = year + month + day;
            if(date == "")
            {
                date = "20170501";
            }
            DateTime startingDate = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime currentDate = DateTime.Now;

            //Create the Lists that make up the dataMatrix and the print matrix.
            List<string> headers = new List<string>();
            List<List<string>> printMatrix = new List<List<string>>();
            dataMatrix = new List<List<DataPoint>>();

            //Add the headers to our print matrix.
            headers.Add("Date");
            headers.AddRange(GetListOfCompanies());
            printMatrix.Add(headers);

            //Keep track of how many times we find an N/A for that DAY. 
            //If half or more have NA, we dont record that day.
            int naCount;

            //Get each of the dataPoints necessary to populate dataMatrix
            while (startingDate.Date <= currentDate.Date)
            {
                naCount = 0;
                //Check the date is not a weekend.
                if ((startingDate.DayOfWeek != DayOfWeek.Saturday) && (startingDate.DayOfWeek != DayOfWeek.Sunday))
                {
                    List<DataPoint> dataColumn = new List<DataPoint>();

                    foreach (CompanyStock company in companies)
                    {
                        if (company.stockData.ContainsKey(startingDate))
                        {
                            dataColumn.Add(company.stockData[startingDate]);
                        }
                        else
                        {
                            naCount++;
                            dataColumn.Add(new DataPoint(company.name, startingDate));
                        }
                    }
                    if (naCount < this.maxNa)
                    {
                        dataMatrix.Add(dataColumn);
                    }
                }
                startingDate = startingDate.AddDays(1);
            }

            //Filtering the data from the matrix, and then printing it out.
            FillMatrixHoles();
            PrintDataMatrix();
            Console.WriteLine("Finished creating the data text file.");
        }

        public void PrintDataMatrix()
        {
            Console.WriteLine("Which parameter do you want to get: Open (o), High (h), Low(l), Close(c) or Volume(v)?");
            string parameter = Console.ReadLine();

            using (StreamWriter file = new StreamWriter("StockData.csv"))
            {
                file.Write("Date,");
                foreach(DataPoint dataPoint in dataMatrix[0])
                {
                    file.Write("{0},", dataPoint.name);
                }
                file.WriteLine();

                foreach (List<DataPoint> row in dataMatrix)
                {
                    file.Write("{0},", row[0].date.Date.ToString());
                    foreach (DataPoint dataPoint in row)
                    {
                        switch (parameter)
                        {
                            case "o":
                                file.Write("{0},", dataPoint.open);
                                break;
                            case "h":
                                file.Write("{0},", dataPoint.high);
                                break;
                            case "l":
                                file.Write("{0},", dataPoint.low);
                                break;
                            case "c":
                                file.Write("{0},", dataPoint.close);
                                break;
                            case "v":
                                file.Write("{0},", dataPoint.volume);
                                break;
                        }
                    }
                    file.WriteLine();
                }
            }
        }

        //Returns the names of all of the companies in the group as a List of strings.
        public List<string> GetListOfCompanies()
        {
            List<string> companyNamesList = new List<string>();
            foreach (CompanyStock company in companies)
            {
                companyNamesList.Add(company.name);
            }

            return companyNamesList;
        }

        //Prints the string matrix to a "StockData" CSV file.
        public void PrintCompanyGroupMatrix(List<List<string>> matrix)
        {
            using (StreamWriter file = new StreamWriter("StockData.csv"))
            {
                foreach (List<string> column in matrix)
                {
                    foreach (string value in column)
                    {
                        file.Write("{0}, ", value);
                    }
                    file.WriteLine();
                }
            }
        }

        //Approximates holes in data with the two nearest data points, if none is found, 
        //it deletes that company from the data matrix.
        public void FillMatrixHoles()
        {
            int lastDataIndex;
            HashSet<int> columnsToDelete = new HashSet<int>();

            for (int col = 0; col < dataMatrix[0].Count; ++col)
            {
                //Index used to keep track of the last non N/A dataPoint
                lastDataIndex = -1;
                for (int row = 0; row < dataMatrix.Count; ++row)
                {
                    if (dataMatrix[row][col].IsNull)
                    {
                        if (row + 1 >= dataMatrix.Count)
                        {
                            //The case were we reach the last dataPoint, and it is N/A.
                            if (lastDataIndex != -1)
                            {
                                dataMatrix[row][col] = new DataPoint(dataMatrix[row - 1][col]);
                            }
                            //We reach the last dataPoint and we never found a non N/a. We delete this column.
                            else
                            {
                                //Delete this company
                                columnsToDelete.Add(col);
                                break;
                            }
                        }
                        else if (lastDataIndex != -1)
                        {
                            //We found an N/A, and we cannot approximate it with neighboring data points.
                            if (row - lastDataIndex > 1 || dataMatrix[row+1][col].IsNull)
                            { 
                                //Delete this company
                                columnsToDelete.Add(col);
                                break;
                            }
                            //We found an N/A but we two neighboring data points we can approximate it with.
                            else
                            {
                                //Approximate data with the two neighbors data points.
                                dataMatrix[row][col] = AveragePoints(dataMatrix[row - 1][col], dataMatrix[row + 1][col]);
                                lastDataIndex = row;
                            }
                        }
                    }
                    else
                    {
                        lastDataIndex = row;
                    }
                }
            }

            //Removing the columns(companies) from our previous set.
            //New matrix with filtered data.
            List<List<DataPoint>> newMatrix = new List<List<DataPoint>>();

            foreach (List<DataPoint> day in dataMatrix)
            {
                List<DataPoint> newDay = new List<DataPoint>();
                for(int i = 0; i < day.Count; i++)
                {
                    if (!columnsToDelete.Contains(i))
                    {
                        newDay.Add(day[i]);
                    }
                }
                newMatrix.Add(newDay);
            }
            dataMatrix = newMatrix;
            Console.WriteLine("Finished filtering data");
        }

        //Add day method that takes care of weekends.
        public DateTime AddDay(DateTime originalDate)
        {
            if(originalDate.DayOfWeek == DayOfWeek.Friday)
            {
                return originalDate.AddDays(3);
            }
            return originalDate.AddDays(1);
        }

        //Approximate a data point given two nehigboring data Points.
        public DataPoint AveragePoints(DataPoint first, DataPoint second)
        {
            DateTime newDate = AddDay(first.date);
            double avgOpen = (first.open + second.open) / 2 ;
            double avgHigh = (first.high + second.high) / 2; 
            double avgLow = (first.low + second.low) / 2; 
            double avgClose = (first.close + second.close) / 2; 
            double avgVolume = (first.volume + second.volume) / 2; 

            return new DataPoint(first.name,newDate, avgOpen, avgHigh, avgLow, avgClose, avgVolume);
        }
    }
}
