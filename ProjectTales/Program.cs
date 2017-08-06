using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ProjectTales
{
    class Program
    {
        static void Main(string[] args)
        {
            String path;
            Console.WriteLine("Please input the name of the folder to search for companies stocks, or hit enter for defalt omegacgl.");
            path = Console.ReadLine();
            Console.WriteLine("Generating data...");
            CompanyGroup companyGroup;
            if(path == "")
            {
                companyGroup = new CompanyGroup("omegacgl");
            }
            else
            {
                companyGroup = new CompanyGroup(path);
            }
            Console.WriteLine("Read all companies in group successfully.");
            companyGroup.CreateDataMatrix();
            Console.ReadLine();
        }
    }
}
