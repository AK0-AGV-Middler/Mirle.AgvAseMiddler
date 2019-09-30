﻿using Mirle.Agv.Model;
using Mirle.Agv.Model.TransferSteps;
using System;
using System.Collections.Generic;
using System.IO;
using Mirle.Agv.Model.Configs;
using System.Linq;
using Mirle.Agv.Controller.Tools;
using System.Reflection;

namespace Mirle.Agv.Controller
{
    public class MapHandler
    {
        private LoggerAgent loggerAgent;
        private MapConfig mapConfig;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string BarcodePath { get; set; }
        public string SectionBeamDisablePath { get; set; }
        public MapInfo TheMapInfo { get; private set; } = new MapInfo();
        private double AddressAreaMm { get; set; } = 30;
        private Vehicle theVehicle = Vehicle.Instance;

        private string lastReadBcrLineId = "";
        private string lastReadBcrId = "";
        private string lastReadAdrId = "";
        private string lastReadSecId = "";


        public MapHandler(MapConfig mapConfig)
        {
            this.mapConfig = mapConfig;
            loggerAgent = LoggerAgent.Instance;
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
        }

        public void ReadBarcodeLineCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(BarcodePath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                        , $"IsBarcodePathNull={string.IsNullOrWhiteSpace(BarcodePath)}"));
                    return;
                }
                TheMapInfo.allMapBarcodeLines.Clear();
                TheMapInfo.allMapBarcodes.Clear();

                string[] allRows = File.ReadAllLines(BarcodePath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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

                        loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                          , $"LoadBarcodeLineCsv oneRow ok. [lastReadBcrLineId={lastReadBcrLineId}]"));

                    }
                    catch (Exception ex)
                    {
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load lineBarcode read oneRow. [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }


                    int count = oneRow.TailBarcode.Number - oneRow.HeadBarcode.Number;
                    int absCount = Math.Abs(count);
                    if (absCount % 3 != 0)
                    {
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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
                                TheMapInfo.allMapBarcodes.Add(mapBarcode.Number, mapBarcode);
                            }
                        }
                        catch (Exception ex)
                        {
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load barcode count < 0, [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
                                TheMapInfo.allMapBarcodes.Add(mapBarcode.Number, mapBarcode);
                            }
                        }
                        catch (Exception ex)
                        {
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"Load barcode count < 0, [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                        }
                    }

                    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"LoadBarcodeCsv oneRow ok. [lastReadBcrId={lastReadBcrId}]"));

                    lastReadBcrLineId = oneRow.Id;
                    TheMapInfo.allMapBarcodeLines.Add(oneRow.Id, oneRow);
                }

                WriteBarcodeBackup();
                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Load Barcode File Ok. [lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"[lastReadBcrLineId={lastReadBcrLineId}][lastReadBcrId={lastReadBcrId}]"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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

            string titleRow = "Id,BarcodeHeadNum,HeadX,HeadY,BarcodeTailNum,TailX,TailY,OffsetX,OffsetY,Material";
            File.WriteAllText(barcodeBackupPath, titleRow);

            List<string> barcodeLineInfos = new List<string>();
            barcodeLineInfos.Add(Environment.NewLine);
            foreach (var item in TheMapInfo.allMapBarcodeLines.Values)
            {
                var head = item.HeadBarcode;
                var tail = item.TailBarcode;
                var barcodeLineInfo = string.Format("{0},{1},{2:F2},{3:F2},{4},{5:F2},{6:F2},{7:F2},{8:F2},{9}", item.Id, head.Number, head.Position.X, head.Position.Y, tail.Number, tail.Position.X, tail.Position.Y, item.Offset.X, item.Offset.Y, item.Material);
                barcodeLineInfos.Add(barcodeLineInfo);
            }
            File.AppendAllLines(barcodeBackupPath, barcodeLineInfos);
        }

        public void ReadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                       , $"IsAddressPathNull={string.IsNullOrWhiteSpace(AddressPath)}"));
                    return;
                }
                TheMapInfo.allMapAddresses.Clear();
                TheMapInfo.allCouples.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"There are no address in file"));
                    return;
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
                //Id,PositionX,PositionY,
                //IsWorkStation,CanLeftLoad,CanLeftUnload,CanRightLoad,CanRightUnload,
                //IsCharger,CouplerId,ChargeDirection,IsSegmentPoint,CanSpin,IsTR50
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
                        oneRow.IsWorkStation = bool.Parse(getThisRow[dicHeaderIndexes["IsWorkStation"]]);
                        oneRow.CanLeftLoad = bool.Parse(getThisRow[dicHeaderIndexes["CanLeftLoad"]]);
                        oneRow.CanLeftUnload = bool.Parse(getThisRow[dicHeaderIndexes["CanLeftUnload"]]);
                        oneRow.CanRightLoad = bool.Parse(getThisRow[dicHeaderIndexes["CanRightLoad"]]);
                        oneRow.CanRightUnload = bool.Parse(getThisRow[dicHeaderIndexes["CanRightUnload"]]);
                        oneRow.IsCharger = bool.Parse(getThisRow[dicHeaderIndexes["IsCharger"]]);
                        oneRow.CouplerId = getThisRow[dicHeaderIndexes["CouplerId"]];
                        oneRow.ChargeDirection = oneRow.ChargeDirectionParse(getThisRow[dicHeaderIndexes["ChargeDirection"]]);
                        oneRow.IsSegmentPoint = bool.Parse(getThisRow[dicHeaderIndexes["IsSegmentPoint"]]);
                        oneRow.CanSpin = bool.Parse(getThisRow[dicHeaderIndexes["CanSpin"]]);
                        oneRow.PioDirection = oneRow.PioDirectionParse(getThisRow[dicHeaderIndexes["PioDirection"]]);
                        oneRow.IsTR50 = bool.Parse(getThisRow[dicHeaderIndexes["IsTR50"]]);
                        if (dicHeaderIndexes.ContainsKey("InsideSectionId"))
                        {
                            oneRow.InsideSectionId = getThisRow[dicHeaderIndexes["InsideSectionId"]];
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
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadAddressCsv read oneRow : [lastReadAdrId={lastReadAdrId}]"));
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    lastReadAdrId = oneRow.Id;
                    TheMapInfo.allMapAddresses.Add(oneRow.Id, oneRow);
                    if (oneRow.IsCharger)
                    {
                        TheMapInfo.allCouples.Add(oneRow);
                    }

                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load Address File Ok. [lastReadAdrId={lastReadAdrId}]"));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadAddressCsv : [lastReadAdrId={lastReadAdrId}]"));

                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public void ReadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionPathNull={string.IsNullOrWhiteSpace(SectionPath)}"));
                    return;
                }
                TheMapInfo.allMapSections.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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
                        if (!TheMapInfo.allMapAddresses.ContainsKey(getThisRow[dicHeaderIndexes["FromAddress"]]))
                        {
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail, headAddress is not in the map : [secId={oneRow.Id}][headAddress={getThisRow[dicHeaderIndexes["FromAddress"]]}]"));
                        }
                        oneRow.HeadAddress = TheMapInfo.allMapAddresses[getThisRow[dicHeaderIndexes["FromAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.HeadAddress);
                        if (!TheMapInfo.allMapAddresses.ContainsKey(getThisRow[dicHeaderIndexes["ToAddress"]]))
                        {
                            loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail, tailAddress is not in the map : [secId={oneRow.Id}][tailAddress={getThisRow[dicHeaderIndexes["ToAddress"]]}]"));
                        }
                        oneRow.TailAddress = TheMapInfo.allMapAddresses[getThisRow[dicHeaderIndexes["ToAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.TailAddress);
                        oneRow.Distance = GetDistance(oneRow.HeadAddress.Position, oneRow.TailAddress.Position);
                        oneRow.Speed = double.Parse(getThisRow[dicHeaderIndexes["Speed"]]);
                        oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicHeaderIndexes["Type"]]);
                        oneRow.PermitDirection = oneRow.PermitDirectionParse(getThisRow[dicHeaderIndexes["PermitDirection"]]);

                    }
                    catch (Exception ex)
                    {
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv read oneRow fail : [lastReadSecId={lastReadSecId}]"));
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    lastReadSecId = oneRow.Id;
                    TheMapInfo.allMapSections.Add(oneRow.Id, oneRow);
                }

                LoadBeamSensorDisable();

                AddInsideAddresses();

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"Load Section File Ok. [lastReadSecId={lastReadSecId}]"));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadSectionCsv : [lastReadSecId={lastReadSecId}]"));
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void AddInsideAddresses()
        {
            try
            {
                foreach (var adr in TheMapInfo.allMapAddresses.Values)
                {
                    lastReadAdrId = adr.Id;
                    lastReadSecId = adr.InsideSectionId;
                    if (TheMapInfo.allMapSections.ContainsKey(adr.InsideSectionId))
                    {
                        TheMapInfo.allMapSections[adr.InsideSectionId].InsideAddresses.Add(adr);
                    }
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                  , $"AddInsideAddresses Ok."));
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"AddInsideAddresses FAIL at Sec[{lastReadSecId}] and Adr[{lastReadAdrId}]" + ex.StackTrace));
            }
        }

        public void LoadBeamSensorDisable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionBeamDisablePath))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                         , $"IsSectionBeamDisablePathNull={string.IsNullOrWhiteSpace(SectionBeamDisablePath)}"));
                    return;
                }

                string[] allRows = File.ReadAllLines(SectionBeamDisablePath);
                if (allRows == null || allRows.Length < 2)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
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
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", $"LoadBeamSensorDisable read oneRow, [SecId={oneRow.SectionId}][Max={(int)oneRow.Max}][Min={(int)oneRow.Min}]"));
                        loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
                    }

                    AddMapSectionBeamDisableIntoList(oneRow);
                }

                loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Load BeamDisable File Ok."));

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        private void AddMapSectionBeamDisableIntoList(MapSectionBeamDisable oneRow)
        {
            try
            {
                if (!TheMapInfo.allMapSections.ContainsKey(oneRow.SectionId))
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"AddMapSectionBeamDisableIntoList +++FAIL+++. AllMapSections.ContainsKey({oneRow.SectionId})={false}"));

                    return;
                }
                MapSection mapSection = TheMapInfo.allMapSections[oneRow.SectionId];
                if (oneRow.Min < 0)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                     , $"Min < 0. [SectionId={oneRow.SectionId}][Min={oneRow.Min}]"));
                    return;
                }
                if (oneRow.Max > mapSection.Distance + 1)
                {
                    loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                    , $"Max > Distance. [SectionId={oneRow.SectionId}][Max={oneRow.Max}][Distance={mapSection.Distance}]"));

                    return;
                }
                if (oneRow.Min == 0 && oneRow.Max == 0)
                {
                    oneRow.Max = mapSection.Distance;
                }

                mapSection.BeamSensorDisables.Add(oneRow);
            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
            }
        }

        public bool IsPositionInThisSection(MapPosition aPosition, MapSection aSection, ref VehiclePosition vehicleLocation)
        {
            try
            {
                //當前Pos還在當前Address範圍內則不更新位置
                if (IsPositionInThisAddress(vehicleLocation.RealPosition, vehicleLocation.LastAddress.Position))
                {
                    return true;
                }

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

                #region Not in Section
                if (!(aPosition.X <= secMaxX && aPosition.X >= secMinX && aPosition.Y <= secMaxY && aPosition.Y >= secMinY))
                {
                    var msg = string.Format("Position({0:F2},{1:F2})不在[{2}]Section[{3}]內，MinX={4:F2},MaxX={5:F2},MinY={6:F2},MaxY={7:F2}。", aPosition.X, aPosition.Y, aSection.Type, aSection.Id, secMinX, secMaxX, secMinY, secMaxY);
                    loggerAgent.LogMsg("Debug", new LogFormat("Debug", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                            , msg));

                    return false;
                }
                #endregion


                #endregion

                #region NotInSection 2019.09.19

                //MapAddress myHeadAddr = aSection.HeadAddress;
                //MapAddress myTailAddr = aSection.TailAddress;

                //switch (aSection.Type)
                //{
                //    case EnumSectionType.Vertical:
                //        {
                //            if ((int)aSection.HeadAddress.Position.Y > (int)aSection.TailAddress.Position.Y)
                //            {
                //                myHeadAddr = aSection.TailAddress;
                //                myTailAddr = aSection.HeadAddress;
                //            }
                //        }
                //        break;
                //    case EnumSectionType.Horizontal:
                //    case EnumSectionType.R2000:
                //        {
                //            if ((int)aSection.HeadAddress.Position.X > (int)aSection.TailAddress.Position.X)
                //            {
                //                myHeadAddr = aSection.TailAddress;
                //                myTailAddr = aSection.HeadAddress;
                //            }
                //        }
                //        break;
                //    case EnumSectionType.None:
                //    default:
                //        break;
                //}



                //#region Not in Section
                ////Position 在 Head 西方過遠
                //if (aPosition.X + AddressAreaMm < myHeadAddr.Position.X)
                //{
                //    return false;
                //}
                ////Position 在 Tail 東方過遠
                //if (aPosition.X > myTailAddr.Position.X + AddressAreaMm)
                //{
                //    return false;
                //}
                ////Position 在 Head 北方過遠
                //if (aPosition.Y < myHeadAddr.Position.Y - AddressAreaMm)
                //{
                //    return false;
                //}
                ////Position 在 Tail 南方過遠
                //if (aPosition.Y - AddressAreaMm > myTailAddr.Position.Y)
                //{
                //    return false;
                //}
                //#endregion

                #endregion

                #region In Section    
                if (!IsPositionInThisAddress(aPosition, vehicleLocation.LastAddress.Position))
                {
                    foreach (var insideAddress in aSection.InsideAddresses)
                    {
                        if (IsPositionInThisAddress(aPosition, insideAddress.Position))
                        {
                            vehicleLocation.LastAddress = insideAddress;
                            break;
                        }
                    }
                }

                vehicleLocation.LastSection = aSection;

                vehicleLocation.LastSection.Distance = GetDistance(aPosition, aSection.HeadAddress.Position);

                return true;
                #endregion

            }
            catch (Exception ex)
            {
                loggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", ex.StackTrace));
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
