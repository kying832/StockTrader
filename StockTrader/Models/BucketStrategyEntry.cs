using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader.Models
{
    class BucketStrategyEntry : StrategyEntry, INotifyPropertyChanged
    {
        public BucketStrategyEntry(string bsn)
        {
            TypeName = "BucketStrategy";
            StrategyName = bsn;
            OnPropertyChanged("BucketStrategyName");
        }

        public string GetTypeName()
        {
            return TypeName;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
