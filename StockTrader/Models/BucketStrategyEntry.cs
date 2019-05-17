using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader.Models
{
    class BucketStrategyEntry : INotifyPropertyChanged
    {
        public string BucketStrategyName { get; set; }

        public BucketStrategyEntry(string bsn)
        {
            BucketStrategyName = bsn;
            OnPropertyChanged("BucketStrategyName");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
