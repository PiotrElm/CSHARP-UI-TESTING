using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GrblEngineerProject.Partials;

namespace GrblEngineerProject
{
    
    public partial class App : Application
    {
        public static CNCConnection myGlobalConnection = new CNCConnection();
    }
}
