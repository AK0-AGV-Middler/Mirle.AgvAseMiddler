﻿using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferCmds;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;
using System.Linq;
using Mirle.Agv.Controller.Tools;

namespace Mirle.Agv.Controller
{
    public class MapHandler
    {
        private string rootDir;
        private LoggerAgent loggerAgent;
        private MapConfig mapConfig;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        private MapInfo theMapInfo = new MapInfo();
        private float SectionWidth { get; set; } = 5;
        private float AddressArea { get; set; } = 5;

        public MapHandler(MapConfig mapConfig)
        {
            this.mapConfig = mapConfig;
            loggerAgent = LoggerAgent.Instance;
            rootDir = mapConfig.RootDir;
            SectionPath = Path.Combine(rootDir, mapConfig.SectionFileName);
            AddressPath = Path.Combine(rootDir, mapConfig.AddressFileName);
            BarcodePath = Path.Combine(rootDir, mapConfig.BarcodeFileName);

            LoadBarcodeLineCsv();
            LoadAddressCsv();
            LoadSectionCsv();
        }

        public void LoadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    return;
                }
                var mapSections = theMapInfo.mapSections;
                var allMapSections = theMapInfo.allMapSections;
                Dictionary<string, int> dicSectionIndexes = new Dictionary<string, int>(); //theMapInfo.dicSectionIndexes;
                mapSections.Clear();
                allMapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, FromAddress, ToAddress, Distance, Speed, Type, PermitDirection, FowardBeamSensorEnable, BackwardBeamSensorEnable   
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicSectionIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSection oneRow = new MapSection();
                    oneRow.Id = getThisRow[dicSectionIndexes["Id"]];
                    oneRow.HeadAddress = theMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["FromAddress"]]];
                    oneRow.TailAddress = theMapInfo.allMapAddresses[getThisRow[dicSectionIndexes["ToAddress"]]];
                    oneRow.Distance = float.Parse(getThisRow[dicSectionIndexes["Distance"]]);
                    oneRow.Speed = float.Parse(getThisRow[dicSectionIndexes["Speed"]]);
                    oneRow.Type = oneRow.SectionTypeConvert(getThisRow[dicSectionIndexes["Type"]]);
                    oneRow.PermitDirection = oneRow.PermitDirectionConvert(getThisRow[dicSectionIndexes["PermitDirection"]]);
                    oneRow.FowardBeamSensorEnable = bool.Parse(getThisRow[dicSectionIndexes["FowardBeamSensorEnable"]]);
                    oneRow.BackwardBeamSensorEnable = bool.Parse(getThisRow[dicSectionIndexes["BackwardBeamSensorEnable"]]);

                    mapSections.Add(oneRow);
                    allMapSections.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void LoadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    return;
                }
                var mapAddresses = theMapInfo.mapAddresses;
                var allMapAddresses = theMapInfo.allMapAddresses;
                Dictionary<string, int> dicAddressIndexes = new Dictionary<string, int>(); // theMapInfo.dicAddressIndexes;
                mapAddresses.Clear();
                allMapAddresses.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, Barcode, PositionX, PositionY, 
                //IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,
                //IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicAddressIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAddress oneRow = new MapAddress();
                    oneRow.Id = getThisRow[dicAddressIndexes["Id"]];
                    oneRow.Barcode = float.Parse(getThisRow[dicAddressIndexes["Barcode"]]);
                    oneRow.Position.X = float.Parse(getThisRow[dicAddressIndexes["PositionX"]]);
                    oneRow.Position.Y = float.Parse(getThisRow[dicAddressIndexes["PositionY"]]);
                    oneRow.IsWorkStation = bool.Parse(getThisRow[dicAddressIndexes["IsWorkStation"]]);
                    oneRow.CanLeftLoad = bool.Parse(getThisRow[dicAddressIndexes["CanLeftLoad"]]);
                    oneRow.CanLeftUnload = bool.Parse(getThisRow[dicAddressIndexes["CanLeftUnload"]]);
                    oneRow.CanRightLoad = bool.Parse(getThisRow[dicAddressIndexes["CanRightLoad"]]);
                    oneRow.CanRightUnload = bool.Parse(getThisRow[dicAddressIndexes["CanRightUnload"]]);
                    oneRow.IsCharger = bool.Parse(getThisRow[dicAddressIndexes["IsCharger"]]);
                    oneRow.CouplerId = getThisRow[dicAddressIndexes["CouplerId"]];
                    oneRow.ChargeDirection = oneRow.ChargeDirectionConvert(getThisRow[dicAddressIndexes["ChargeDirection"]]);
                    oneRow.IsSegmentPoint = bool.Parse(getThisRow[dicAddressIndexes["IsSegmentPoint"]]);
                    oneRow.CanSpin = bool.Parse(getThisRow[dicAddressIndexes["CanSpin"]]);
                    oneRow.PioDirection = oneRow.PioDirectionConvert(getThisRow[dicAddressIndexes["PioDirection"]]);

                    mapAddresses.Add(oneRow);
                    allMapAddresses.Add(oneRow.Id, oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void LoadBarcodeLineCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodePath))
                {
                    return;
                }
                var mapBarcodeLines = theMapInfo.mapBarcodeLines;
                Dictionary<string, int> dicBarcodeIndexes = new Dictionary<string, int>(); // theMapInfo.dicBarcodeIndexes;
                var allBarcodes = theMapInfo.allBarcodes;
                mapBarcodeLines.Clear();
                allBarcodes.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
                    string className = GetType().Name;
                    string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                    string classMethodName = className + ":" + methodName;
                    LogFormat logFormat = new LogFormat("Error", "1", classMethodName, "Device", "CarrierID", "There are no barcodes in file");
                    loggerAgent.LogMsg("Error", logFormat);

                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, Direction
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicBarcodeIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapBarcodeLine oneRow = new MapBarcodeLine();
                    string Id = getThisRow[dicBarcodeIndexes["Id"]];
                    int HeadNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeHeadNum"]]);
                    int TailNum = int.Parse(getThisRow[dicBarcodeIndexes["BarcodeTailNum"]]);
                    float HeadX = float.Parse(getThisRow[dicBarcodeIndexes["HeadX"]]);
                    float HeadY = float.Parse(getThisRow[dicBarcodeIndexes["HeadY"]]);
                    float TailX = float.Parse(getThisRow[dicBarcodeIndexes["TailX"]]);
                    float TailY = float.Parse(getThisRow[dicBarcodeIndexes["TailY"]]);
                    int Direction = oneRow.BarcodeDirectionConvert(getThisRow[dicBarcodeIndexes["Direction"]]);
                    float OffsetX = float.Parse(getThisRow[dicBarcodeIndexes["OffsetX"]]);
                    float OffsetY = float.Parse(getThisRow[dicBarcodeIndexes["OffsetY"]]);

                    oneRow.Id = Id;
                    oneRow.HeadBarcode.Number = HeadNum;
                    oneRow.HeadBarcode.Position.X = HeadX;
                    oneRow.HeadBarcode.Position.Y = HeadY;
                    oneRow.TailBarcode.Number = TailNum;
                    oneRow.TailBarcode.Position.X = TailX;
                    oneRow.TailBarcode.Position.Y = TailY;
                    oneRow.Direction = Direction;
                    oneRow.Offset.X = OffsetX;
                    oneRow.Offset.Y = OffsetY;

                    int count = TailNum - HeadNum;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
                        //TODO: Log BarcodeLineNum mod 3 is not zero
                        break;
                    }
                    if (count < 0)
                    {
                        count = -count;
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = TailNum + j;
                            mapBarcode.Position.X = (j * HeadX + (count - j) * TailX) / count;
                            mapBarcode.Position.Y = (j * HeadY + (count - j) * TailY) / count;
                            mapBarcode.Offset.X = OffsetX;
                            mapBarcode.Offset.Y = OffsetY;
                            mapBarcode.Direction = Direction;
                            mapBarcode.LineId = Id;

                            allBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }
                    else
                    {
                        for (int j = 0; j <= count; j += 3)
                        {
                            MapBarcode mapBarcode = new MapBarcode();
                            mapBarcode.Number = HeadNum + j;
                            mapBarcode.Position.X = (j * TailX + (count - j) * HeadX) / count;
                            mapBarcode.Position.Y = (j * TailY + (count - j) * HeadY) / count;
                            mapBarcode.Offset.X = OffsetX;
                            mapBarcode.Offset.Y = OffsetY;
                            mapBarcode.Direction = Direction;
                            mapBarcode.LineId = Id;

                            allBarcodes.Add(mapBarcode.Number, mapBarcode);
                        }
                    }

                    mapBarcodeLines.Add(oneRow);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public MapInfo GetMapInfo()
        {
            return theMapInfo;
        }

        public bool IsPositionInThisSection(MapPosition aPosition, MapSection aSection)
        {
            MapAddress headAdr = aSection.HeadAddress;
            MapAddress tailAdr = aSection.TailAddress;

            if (IsPositionInThisAddress(aPosition, headAdr))
            {
                return true;
            }

            if (IsPositionInThisAddress(aPosition, tailAdr))
            {
                return true;
            }

            switch (aSection.Type)
            {
                case EnumSectionType.Horizontal:
                    {
                        float diffY = Math.Abs(aPosition.Y - headAdr.Position.Y);
                        if (diffY <= SectionWidth)
                        {
                            if (aPosition.X > tailAdr.Position.X || aPosition.X < headAdr.Position.X)
                            {
                                return false;
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case EnumSectionType.Vertical:
                    {
                        float diffX = Math.Abs(aPosition.X - headAdr.Position.X);
                        if (diffX <= SectionWidth)
                        {
                            if (aPosition.Y > tailAdr.Position.Y || aPosition.Y < headAdr.Position.Y)
                            {
                                return false;
                            }
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case EnumSectionType.R2000:
                    //TODO: Analysis diff <= SectionWidth?
                    //TODO: Analysis position is in the R2000 rectangle(sin45/cos45)
                    break;
                case EnumSectionType.None:
                default:
                    break;
            }

            return true;
        }

        public bool IsPositionInThisAddress(MapPosition aPosition, MapAddress anAddress)
        {
            var diffX = Math.Abs(aPosition.X - anAddress.Position.X);
            var diffY = Math.Abs(aPosition.Y - anAddress.Position.Y);
            return diffX * diffX + diffY * diffY <= AddressArea * AddressArea;
        }

    }

}
