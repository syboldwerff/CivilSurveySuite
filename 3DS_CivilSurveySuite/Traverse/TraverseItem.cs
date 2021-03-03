﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace _3DS_CivilSurveySuite.Traverse
{
    public class TraverseItem : INotifyPropertyChanged
    {
        #region Private Members
        private double index;
        private double bearing;
        private double distance;
        private DMS dms;
        #endregion

        #region Properties
        public DMS DMSBearing { get => dms; private set { dms = value; NotifyPropertyChanged(); } }

        public double Index { get => index; set { index = value; NotifyPropertyChanged(); } }

        public double Bearing { get => bearing; 
            set 
            {
                if (DMS.IsValid(value))
                {
                    bearing = value;
                    DMSBearing = new DMS(value);
                    NotifyPropertyChanged();
                }
                else bearing = 0;
            } 
        }
        public double Distance { get => distance; set { distance = value; NotifyPropertyChanged(); } }
        #endregion

        #region Constructors

        public TraverseItem() { }

        public TraverseItem(double bearing, double distance)
        {
            DMSBearing = new DMS(bearing);
            Distance = distance;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public static void UpdateIndex(ICollection<TraverseItem> collection)
        {
            int i = 0;
            foreach (TraverseItem item in collection)
            {
                item.Index = i;
                i++;
            }
        }
    }
}
