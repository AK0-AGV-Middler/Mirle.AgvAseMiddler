﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.Model
{
    [Serializable]
    public class MapInfo
    {
        public Dictionary<string, MapAddress> allMapAddresses = new Dictionary<string, MapAddress>();
        public Dictionary<string, MapSection> allMapSections = new Dictionary<string, MapSection>();
        public Dictionary<int, MapBarcode> allMapBarcodes = new Dictionary<int, MapBarcode>();
        public Dictionary<string, MapBarcodeLine> allMapBarcodeLines = new Dictionary<string, MapBarcodeLine>();

        public MapInfo()
        {
        }
    }
}
