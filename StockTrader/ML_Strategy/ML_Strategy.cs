using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiaNet;
using SiaNet.Data;
using SiaNet.Engine;
using SiaNet.Events;
using SiaNet.Initializers;
using SiaNet.Layers;
using Deedle;
using SQLiteAccessLibrary;
using IEXDataLibrary;

namespace StockTrader.ML_Strategy
{
    class ML_Strategy
    {
     
        Sequential seq_model;

        private static void SetEngine()
        {
            Global.UseEngine(SiaNet.Backend.TensorFlowLib.SiaNetBackend.Instance, DeviceType.CPU);

        }
        private static DataFrame2D LoadDataFromFile(String filename)
        {
            var frame = DataFrame2D.ReadCsv(filename);

        }
        private static DataFrame2D LoadDataFromQuery()
        {

        }
        private static BuildModel()
        {
            //seq_model.Add(SiaNet.Layers.)
        }
        
        
    }
}
