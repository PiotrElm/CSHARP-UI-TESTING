using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GrblEngineerProject
{
    public static class GlobalVariables
    {
        public static string programStatus;
        public static string PositionAnswer;
        public static string MachineStatus;
        public static string MachinePos;
        public static string WorkPos;
       public static point MachinePositionAsPoint = new point();
       public static point WorkingPositionAsPoint = new point();
    }
    public class point
    {
        public string x;
        public string y;
        public string z;
    }
}
