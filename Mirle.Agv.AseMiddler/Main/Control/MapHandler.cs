﻿using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.AseMiddler.Model.Configs;
using System.Linq;

using System.Reflection;
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class MapHandler
    {
        private MirleLogger mirleLogger;
        private MapConfig mapConfig;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        public string SectionBeamDisablePath { get; set; }
        public MapInfo theMapInfo { get; private set; } = new MapInfo();
        private double AddressAreaMm { get; set; } = 30;

        private string lastReadBcrLineId = "";
        private string lastReadBcrId = "";
        private string lastReadAdrId = "";
        private string lastReadSecId = "";

        public MapHandler(MapConfig mapConfig)
        {
            this.mapConfig = mapConfig;
            mirleLogger = MirleLogger.Instance;
            SectionPath = Path.Combine(Environment.CurrentDirectory, mapConfig.SectionFileName);
            AddressPath = Path.Combine(Environment.CurrentDirectory, mapConfig.AddressFileName);
            BarcodePath = Path.Combine(Environment.CurrentDirectory, mapConfig.BarcodeFileName);
            SectionBeamDisablePath = Path.Combine(Environment.CurrentDirectory, mapConfig.SectionBeamDisablePathFileName);
            AddressAreaMm = mapConfig.AddressAreaMm;

            LoadMapInfo();
        }

        public void LoadMapInfo()
        {
            ReadBarcodeLineCsv();
            ReadAddressCsv();
            ReadSectionCsv();
            //WriteBarcodeBackup();
            //WriteAddressBackup();
            //WriteSectionBackup();
        }

        public void ReadBarcodeLineCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodePath))
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , $"IsBarcodePathNull={string.IsNullOrWhiteSpace(BarcodePath)}"));
                    return;
                }
                theMapInfo.barcodeLineMap.Clear();
                theMapInfo.barcodeMap.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , "There are no barcodes in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, BarcodeHeadNum, HeadX, HeadY, BarcodeTailNum, TailX, TailY, OffsetX, OffsetY
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');

                    MapBarcodeLine oneRow = new MapBarcodeLine();
                    try
                    {
                        oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                        oneRow.HeadBarcode.LineId = oneRow.Id;
                        oneRow.HeadBarcode.Number = int.Parse(getThisRow[dicHeaderIndexes["BarcodeHeadNum"]]);
                        oneRow.HeadBarcode.Position.X = double.Parse(getThisRow[dicHeaderIndexes["HeadX"]]);
                        oneRow.HeadBarcode.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["HeadY"]]);
                        oneRow.TailBarcode.LineId = oneRow.Id;
                        oneRow.TailBarcode.Number = int.Parse(getThisRow[dicHeaderIndexes["BarcodeTailNum"]]);
                        oneRow.TailBarcode.Position.X = double.Parse(getThisRow[dicHeaderIndexes["TailX"]]);
                        oneRow.TailBarcode.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["TailY"]]);
                        oneRow.Offset.X = double.Parse(getThisRow[dicHeaderIndexes["OffsetX"]]);
                        oneRow.Offset.Y = double.Parse(getThisRow[dicHeaderIndexes["OffsetY"]]);
                        oneRow.Material = oneRow.BarcodeMaterialParse(getThisRow[dicHeaderIndexes["Material"]]);

                        lastReadBcrLineId = oneRow.Id;

                        //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        //  , $"LoadBarcodeLineCsv oneRow ok. [lastReadBcrLineId={lastReadBcrLineId}]"));

                    }
                    catch (Exception ex)
                    {
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load lineBarcode read oneRow. [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }


                    int count = oneRow.TailBarcode.Number - oneRow.HeadBarcode.Number;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                            , $"BarcodeLineNum mod 3 is not zero, [Id = {oneRow.Id}][HeadNum={oneRow.HeadBarcode.Number}][TailNum={oneRow.TailBarcode.Number}]"));
                        break;
                    }
                    if (count < 0)
                    {
                        try
                        {
                            count = -count;
                            for (int j = 0; j <= count; j += 3)
                            {
                                MapBarcode mapBarcode = new MapBarcode();
                                mapBarcode.Number = oneRow.TailBarcode.Number + j;
                                mapBarcode.Position.X = (j * oneRow.HeadBarcode.Position.X + (count - j) * oneRow.TailBarcode.Position.X) / count;
                                mapBarcode.Position.Y = (j * oneRow.HeadBarcode.Position.Y + (count - j) * oneRow.TailBarcode.Position.Y) / count;
                                mapBarcode.Offset.X = oneRow.Offset.X;
                                mapBarcode.Offset.Y = oneRow.Offset.Y;
                                mapBarcode.LineId = oneRow.Id;
                                mapBarcode.Material = oneRow.Material;

                                lastReadBcrId = mapBarcode.Number.ToString();
                                theMapInfo.barcodeMap.Add(mapBarcode.Number, mapBarcode);
                            }
                        }
                        catch (Exception ex)
                        {
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load barcode count < 0, [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                        }
                    }
                    else
                    {
                        try
                        {
                            for (int j = 0; j <= count; j += 3)
                            {
                                MapBarcode mapBarcode = new MapBarcode();
                                mapBarcode.Number = oneRow.HeadBarcode.Number + j;
                                mapBarcode.Position.X = (j * oneRow.TailBarcode.Position.X + (count - j) * oneRow.HeadBarcode.Position.X) / count;
                                mapBarcode.Position.Y = (j * oneRow.TailBarcode.Position.Y + (count - j) * oneRow.HeadBarcode.Position.Y) / count;
                                mapBarcode.Offset.X = oneRow.Offset.X;
                                mapBarcode.Offset.Y = oneRow.Offset.Y;
                                mapBarcode.LineId = oneRow.Id;
                                mapBarcode.Material = oneRow.Material;
                                lastReadBcrId = mapBarcode.Number.ToString();
                                theMapInfo.barcodeMap.Add(mapBarcode.Number, mapBarcode);
                            }
                        }
                        catch (Exception ex)
                        {
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load barcode count < 0, [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                        }
                    }

                    //loggerAgent.LogMsg("Debug", new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    // , $"LoadBarcodeCsv oneRow ok. [lastReadBcrId={lastReadBcrId}]"));

                    lastReadBcrLineId = oneRow.Id;
                    theMapInfo.barcodeLineMap.Add(oneRow.Id, oneRow);
                }

                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Load Barcode File Ok. [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"[lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void ReadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                       , $"IsAddressPathNull={string.IsNullOrWhiteSpace(AddressPath)}"));
                    return;
                }
                theMapInfo.addressMap.Clear();
                theMapInfo.chargerAddressMap.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"There are no address in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapAddress oneRow = new MapAddress();
                    MapAddressOffset offset = new MapAddressOffset();
                    try
                    {
                        oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                        oneRow.Position.X = double.Parse(getThisRow[dicHeaderIndexes["PositionX"]]);
                        oneRow.Position.Y = double.Parse(getThisRow[dicHeaderIndexes["PositionY"]]);
                        if (dicHeaderIndexes.ContainsKey("TransferPortDirection"))
                        {
                            oneRow.TransferPortDirection = oneRow.AddressDirectionParse(getThisRow[dicHeaderIndexes["TransferPortDirection"]]);
                        }
                        if (dicHeaderIndexes.ContainsKey("PortNumber"))
                        {
                            oneRow.PortNumber = getThisRow[dicHeaderIndexes["PortNumber"]];
                        }
                        if (dicHeaderIndexes.ContainsKey("ChargeDirection"))
                        {
                            oneRow.ChargeDirection = oneRow.AddressDirectionParse(getThisRow[dicHeaderIndexes["ChargeDirection"]]);
                        }
                        if (dicHeaderIndexes.ContainsKey("PioDirection"))
                        {
                            oneRow.PioDirection = oneRow.AddressDirectionParse(getThisRow[dicHeaderIndexes["PioDirection"]]);
                        }
                        oneRow.CouplerId = getThisRow[dicHeaderIndexes["CouplerId"]];
                        oneRow.CanSpin = bool.Parse(getThisRow[dicHeaderIndexes["CanSpin"]]);
                        oneRow.IsTR50 = bool.Parse(getThisRow[dicHeaderIndexes["IsTR50"]]);
                        if (dicHeaderIndexes.ContainsKey("InsideSectionId"))
                        {
                            oneRow.InsideSectionId = FitZero(getThisRow[dicHeaderIndexes["InsideSectionId"]]);
                        }
                        if (dicHeaderIndexes.ContainsKey("OffsetX"))
                        {
                            offset.OffsetX = double.Parse(getThisRow[dicHeaderIndexes["OffsetX"]]);
                            offset.OffsetY = double.Parse(getThisRow[dicHeaderIndexes["OffsetY"]]);
                            offset.OffsetTheta = double.Parse(getThisRow[dicHeaderIndexes["OffsetTheta"]]);
                        }
                        oneRow.AddressOffset = offset;
                        if (dicHeaderIndexes.ContainsKey("VehicleHeadAngle"))
                        {
                            oneRow.VehicleHeadAngle = double.Parse(getThisRow[dicHeaderIndexes["VehicleHeadAngle"]]);
                        }
                    }
                    catch (Exception ex)
                    {
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadAddressCsv read oneRow : [lastReadAdrId={lastReadAdrId}]"));
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    lastReadAdrId = oneRow.Id;
                    theMapInfo.addressMap.Add(oneRow.Id, oneRow);
                    if (oneRow.IsCharger())
                    {
                        theMapInfo.chargerAddressMap.Add(oneRow);
                    }
                    theMapInfo.portNumberMap.Add(oneRow.Id, oneRow.PortNumber);

                }

                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load Address File Ok. [lastReadAdrId={lastReadAdrId}]"));

            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadAddressCsv : [lastReadAdrId={lastReadAdrId}]"));

                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private string FitZero(string v)
        {
            int sectionIdToInt = int.Parse(v);
            return sectionIdToInt.ToString("0000");
        }

        public void ReadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionPathNull={string.IsNullOrWhiteSpace(SectionPath)}"));
                    return;
                }
                theMapInfo.sectionMap.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"There are no section in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, FromAddress, ToAddress, Speed, Type, PermitDirection
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSection oneRow = new MapSection();
                    try
                    {
                        oneRow.Id = getThisRow[dicHeaderIndexes["Id"]];
                        if (!theMapInfo.addressMap.ContainsKey(getThisRow[dicHeaderIndexes["FromAddress"]]))
                        {
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail, headAddress is not in the map : [secId={oneRow.Id}][headAddress={getThisRow[dicHeaderIndexes["FromAddress"]]}]"));
                        }
                        oneRow.HeadAddress = theMapInfo.addressMap[getThisRow[dicHeaderIndexes["FromAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.HeadAddress);
                        if (!theMapInfo.addressMap.ContainsKey(getThisRow[dicHeaderIndexes["ToAddress"]]))
                        {
                            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail, tailAddress is not in the map : [secId={oneRow.Id}][tailAddress={getThisRow[dicHeaderIndexes["ToAddress"]]}]"));
                        }
                        oneRow.TailAddress = theMapInfo.addressMap[getThisRow[dicHeaderIndexes["ToAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.TailAddress);
                        oneRow.HeadToTailDistance = GetDistance(oneRow.HeadAddress.Position, oneRow.TailAddress.Position);
                        oneRow.Speed = double.Parse(getThisRow[dicHeaderIndexes["Speed"]]);
                        oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicHeaderIndexes["Type"]]);
                        oneRow.PermitDirection = oneRow.PermitDirectionParse(getThisRow[dicHeaderIndexes["PermitDirection"]]);

                    }
                    catch (Exception ex)
                    {
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail : [lastReadSecId={lastReadSecId}]"));
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    lastReadSecId = oneRow.Id;
                    theMapInfo.sectionMap.Add(oneRow.Id, oneRow);
                }

                LoadBeamSensorDisable();

                AddInsideAddresses();

                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"Load Section File Ok. [lastReadSecId={lastReadSecId}]"));

            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv : [lastReadSecId={lastReadSecId}]"));
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void WriteBarcodeBackup()
        {
            var directionName = Path.GetDirectoryName(BarcodePath);
            if (!Directory.Exists(directionName))
            {
                Directory.CreateDirectory(directionName);
            }

            var barcodeBackupPath = Path.ChangeExtension(BarcodePath, ".backup.csv");

            string titleRow = "Id,BarcodeHeadNum,HeadX,HeadY,BarcodeTailNum,TailX,TailY,OffsetX,OffsetY,Material" + Environment.NewLine;
            File.WriteAllText(barcodeBackupPath, titleRow);

            List<string> barcodeLineInfos = new List<string>();
            foreach (var item in theMapInfo.barcodeLineMap.Values)
            {
                var head = item.HeadBarcode;
                var tail = item.TailBarcode;
                var barcodeLineInfo = string.Format("{0},{1},{2:F2},{3:F2},{4},{5:F2},{6:F2},{7:F2},{8:F2},{9}", item.Id, head.Number, head.Position.X, head.Position.Y, tail.Number, tail.Position.X, tail.Position.Y, item.Offset.X, item.Offset.Y, item.Material);
                barcodeLineInfos.Add(barcodeLineInfo);
            }
            File.AppendAllLines(barcodeBackupPath, barcodeLineInfos);
        }

        private void WriteAddressBackup()
        {
            var directionName = Path.GetDirectoryName(AddressPath);
            if (!Directory.Exists(directionName))
            {
                Directory.CreateDirectory(directionName);
            }

            var backupPath = Path.ChangeExtension(AddressPath, ".backup.csv");

            string titleRow = "Id,PositionX,PositionY,TransferPortDirection,PortNumber,PioDirection,ChargeDirection,CouplerId,CanSpin,IsTR50,InsideSectionId,OffsetX,OffsetY,OffsetTheta,VehicleHeadAngle" + Environment.NewLine;
            File.WriteAllText(backupPath, titleRow);
            List<string> lineInfos = new List<string>();
            foreach (var item in theMapInfo.addressMap.Values)
            {
                var lineInfo = string.Format("{0},{1:F0},{2:F0},{3},{4},{5},{6},{7},{8},{9},{10:0000},{11:F2},{12:F2},{13:F2},{14:N0}",
                    item.Id, item.Position.X, item.Position.Y, item.TransferPortDirection, item.PortNumber, item.PioDirection, item.ChargeDirection, item.CouplerId,
                    item.CanSpin, item.IsTR50,
                   int.Parse(item.InsideSectionId), item.AddressOffset.OffsetX, item.AddressOffset.OffsetY, item.AddressOffset.OffsetTheta,
                   item.VehicleHeadAngle
                    );
                lineInfos.Add(lineInfo);
            }
            File.AppendAllLines(backupPath, lineInfos);
        }

        private void WriteSectionBackup()
        {
            var directionName = Path.GetDirectoryName(SectionPath);
            if (!Directory.Exists(directionName))
            {
                Directory.CreateDirectory(directionName);
            }

            var backupPath = Path.ChangeExtension(SectionPath, ".backup.csv");

            string titleRow = "Id,FromAddress,ToAddress,Speed, Type,PermitDirection" + Environment.NewLine;
            File.WriteAllText(backupPath, titleRow);
            List<string> lineInfos = new List<string>();
            foreach (var item in theMapInfo.sectionMap.Values)
            {
                var lineInfo = string.Format("{0},{1},{2},{3},{4},{5}",
                    item.Id, item.HeadAddress.Id, item.TailAddress.Id, item.Speed, item.Type, item.PermitDirection
                    );
                lineInfos.Add(lineInfo);
            }
            File.AppendAllLines(backupPath, lineInfos);
        }

        private void AddInsideAddresses()
        {
            try
            {
                foreach (var adr in theMapInfo.addressMap.Values)
                {
                    if (theMapInfo.sectionMap.ContainsKey(adr.InsideSectionId))
                    {
                        theMapInfo.sectionMap[adr.InsideSectionId].InsideAddresses.Add(adr);
                    }
                }

                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"AddInsideAddresses Ok."));
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"AddInsideAddresses FAIL at Sec[{lastReadSecId}] and Adr[{lastReadAdrId}]" + ex.StackTrace));
            }
        }

        public void LoadBeamSensorDisable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionBeamDisablePath))
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionBeamDisablePathNull={string.IsNullOrWhiteSpace(SectionBeamDisablePath)}"));
                    return;
                }

                string[] allRows = File.ReadAllLines(SectionBeamDisablePath);
                if (allRows == null || allRows.Length < 2)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"There are no beam-disable in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id, FromAddress, ToAddress, Speed, Type, PermitDirection
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        dicHeaderIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    MapSectionBeamDisable oneRow = new MapSectionBeamDisable();
                    try
                    {
                        oneRow.SectionId = getThisRow[dicHeaderIndexes["SectionId"]];
                        oneRow.Min = double.Parse(getThisRow[dicHeaderIndexes["Min"]]);
                        oneRow.Max = double.Parse(getThisRow[dicHeaderIndexes["Max"]]);
                        oneRow.FrontDisable = bool.Parse(getThisRow[dicHeaderIndexes["FrontDisable"]]);
                        oneRow.BackDisable = bool.Parse(getThisRow[dicHeaderIndexes["BackDisable"]]);
                        oneRow.LeftDisable = bool.Parse(getThisRow[dicHeaderIndexes["LeftDisable"]]);
                        oneRow.RightDisable = bool.Parse(getThisRow[dicHeaderIndexes["RightDisable"]]);
                    }
                    catch (Exception ex)
                    {
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadBeamSensorDisable read oneRow, [SecId={oneRow.SectionId}][Max={(int)oneRow.Max}][Min={(int)oneRow.Min}]"));
                        mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    AddMapSectionBeamDisableIntoList(oneRow);
                }

                mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load BeamDisable File Ok."));

            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void AddMapSectionBeamDisableIntoList(MapSectionBeamDisable oneRow)
        {
            try
            {
                if (!theMapInfo.sectionMap.ContainsKey(oneRow.SectionId))
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Section[{oneRow.SectionId}]加入Beam Sensor Disable清單失敗，圖資不包含Section[{oneRow.SectionId}]"));

                    return;
                }
                MapSection mapSection = theMapInfo.sectionMap[oneRow.SectionId];
                if (oneRow.Min < -30)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Min < 0. [SectionId={oneRow.SectionId}][Min={oneRow.Min}]"));
                    return;
                }
                if (oneRow.Max > mapSection.HeadToTailDistance + 31)
                {
                    mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Max > Distance. [SectionId={oneRow.SectionId}][Max={oneRow.Max}][Distance={mapSection.HeadToTailDistance}]"));

                    return;
                }
                if (oneRow.Min == 0 && oneRow.Max == 0)
                {
                    oneRow.Min = -30;
                    oneRow.Max = mapSection.HeadToTailDistance + 30;
                }

                mapSection.BeamSensorDisables.Add(oneRow);
            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public bool IsPositionInThisSection(MapSection aSection, MapPosition position)
        {
            try
            {
                MapPosition aPosition = position;

                #region NotInSection 2019.09.23
                double secMinX, secMaxX, secMinY, secMaxY;

                if (aSection.HeadAddress.Position.X >= aSection.TailAddress.Position.X)
                {
                    secMaxX = aSection.HeadAddress.Position.X + AddressAreaMm;
                    secMinX = aSection.TailAddress.Position.X - AddressAreaMm;
                }
                else
                {
                    secMaxX = aSection.TailAddress.Position.X + AddressAreaMm;
                    secMinX = aSection.HeadAddress.Position.X - AddressAreaMm;
                }

                if (aSection.HeadAddress.Position.Y >= aSection.TailAddress.Position.Y)
                {
                    secMaxY = aSection.HeadAddress.Position.Y + AddressAreaMm;
                    secMinY = aSection.TailAddress.Position.Y - AddressAreaMm;
                }
                else
                {
                    secMaxY = aSection.TailAddress.Position.Y + AddressAreaMm;
                    secMinY = aSection.HeadAddress.Position.Y - AddressAreaMm;
                }


                if (!(aPosition.X <= secMaxX && aPosition.X >= secMinX && aPosition.Y <= secMaxY && aPosition.Y >= secMinY))
                {
                    return false;
                }
                #endregion

                #region In Section                   
                return true;
                #endregion

            }
            catch (Exception ex)
            {
                mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                return false;
            }
        }

        private double GetVectorRatio(MapPosition aPosition, MapSection mapSection)
        {
            var headPosition = mapSection.HeadAddress.Position;
            var tailPosition = mapSection.TailAddress.Position;
            var num1 = (tailPosition.X - headPosition.X) * (aPosition.Y - headPosition.Y);
            var num2 = (tailPosition.Y - headPosition.Y) * (aPosition.X - headPosition.X);
            return Math.Abs(num1 - num2);
        }

        public bool IsPositionInThisAddress(MapPosition aPosition, MapPosition addressPosition)
        {
            return Math.Abs(aPosition.X - addressPosition.X) <= AddressAreaMm && Math.Abs(aPosition.Y - addressPosition.Y) <= AddressAreaMm;
        }

        public double GetDistance(MapPosition aPosition, MapPosition bPosition)
        {
            var diffX = Math.Abs(aPosition.X - bPosition.X);
            var diffY = Math.Abs(aPosition.Y - bPosition.Y);
            return Math.Sqrt((diffX * diffX) + (diffY * diffY));
        }
    }

}
