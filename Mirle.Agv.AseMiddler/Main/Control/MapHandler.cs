using Mirle.Agv.AseMiddler.Model;
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
        //private MirleLogger mirleLogger;
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;
        public string SectionPath { get; set; }
        public string AddressPath { get; set; }
        public string PortIdMapPath { get; set; }

        private string lastReadAdrId = "";
        private string lastReadSecId = "";
        private string lastReadPortId = "";     

        public MapHandler()
        {
            //mirleLogger = MirleLogger.Instance;
            SectionPath = Path.Combine(Environment.CurrentDirectory, Vehicle.MapConfig.SectionFileName);
            AddressPath = Path.Combine(Environment.CurrentDirectory, Vehicle.MapConfig.AddressFileName);
            PortIdMapPath = Path.Combine(Environment.CurrentDirectory, Vehicle.MapConfig.PortIdMapFileName);           

            LoadMapInfo();
        }

        public void LoadMapInfo()
        {
            ReadAddressCsv();
            ReadPortIdMapCsv();
            //ReadAgvStationCsv();
            ReadSectionCsv();
        }

        public void ReadAddressCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AddressPath))
                {
                    throw new Exception($"IsAddressPathNull={string.IsNullOrWhiteSpace(AddressPath)}");
                }
                Vehicle.Mapinfo.addressMap.Clear();
                Vehicle.Mapinfo.chargerAddressMap.Clear();

                string[] allRows = File.ReadAllLines(AddressPath);
                if (allRows == null || allRows.Length < 2)
                {
                    throw new Exception("There are no address in file");
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
                        if (dicHeaderIndexes.ContainsKey("GateType"))
                        {
                            oneRow.GateType = getThisRow[dicHeaderIndexes["GateType"]];
                        }
                        if (dicHeaderIndexes.ContainsKey("ChargeDirection"))
                        {
                            oneRow.ChargeDirection = oneRow.AddressDirectionParse(getThisRow[dicHeaderIndexes["ChargeDirection"]]);
                        }
                        if (dicHeaderIndexes.ContainsKey("PioDirection"))
                        {
                            oneRow.PioDirection = oneRow.AddressDirectionParse(getThisRow[dicHeaderIndexes["PioDirection"]]);
                        }
                        //oneRow.CanSpin = bool.Parse(getThisRow[dicHeaderIndexes["CanSpin"]]);
                        //oneRow.IsTR50 = bool.Parse(getThisRow[dicHeaderIndexes["IsTR50"]]);
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
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadAddressCsv read oneRow : [lastReadAdrId={lastReadAdrId}][{ex.Message}]");
                    }

                    lastReadAdrId = oneRow.Id;
                    Vehicle.Mapinfo.addressMap.TryAdd(oneRow.Id, oneRow);
                    if (oneRow.IsCharger())
                    {
                        Vehicle.Mapinfo.chargerAddressMap.Add(oneRow);
                    }
                    Vehicle.Mapinfo.gateTypeMap.Add(oneRow.Id, oneRow.GateType);

                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Load Address File Ok. [lastReadAdrId={lastReadAdrId}]");
            }
            catch (Exception ex)
            {                
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadAddressCsv : [lastReadAdrId={lastReadAdrId}][{ex.Message}]");
            }
        }

        public void ReadPortIdMapCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PortIdMapPath))
                {
                    return;
                }

                Vehicle.Mapinfo.portMap.Clear();

                string[] allRows = File.ReadAllLines(PortIdMapPath);
                if (allRows == null || allRows.Length < 2)
                {
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
                    try
                    {
                        string portId = getThisRow[dicHeaderIndexes["Id"]];
                        lastReadPortId = portId;
                        string addressId = getThisRow[dicHeaderIndexes["AddressId"]];
                        string portNumber = getThisRow[dicHeaderIndexes["PortNumber"]];
                        bool isVitualPort = false;
                        if (dicHeaderIndexes.ContainsKey("IsVitualPort"))
                        {
                            isVitualPort = bool.Parse(getThisRow[dicHeaderIndexes["IsVitualPort"]]);
                        }
                        string agvStationId = "";
                        if (dicHeaderIndexes.ContainsKey("AgvStationId"))
                        {
                            agvStationId = string.IsNullOrEmpty(getThisRow[dicHeaderIndexes["AgvStationId"]]) ? "" : getThisRow[dicHeaderIndexes["AgvStationId"]].Trim();
                        }

                        MapPort port = new MapPort()
                        {
                            ID = portId,
                            ReferenceAddressId = addressId,
                            Number = portNumber,
                            IsVitualPort = isVitualPort,
                            AgvStationId = agvStationId
                        };

                        if (!Vehicle.Mapinfo.portMap.ContainsKey(port.ID))
                        {
                            Vehicle.Mapinfo.portMap.Add(port.ID, port);
                        }

                        if (!string.IsNullOrEmpty(agvStationId))
                        {
                            if (Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
                            {
                                Vehicle.Mapinfo.addressMap[addressId].AgvStationId = agvStationId;
                            }

                            if (Vehicle.Mapinfo.agvStationMap.ContainsKey(agvStationId))
                            {
                                Vehicle.Mapinfo.agvStationMap[agvStationId].Ports.Add(port);
                            }
                            else
                            {
                                Vehicle.Mapinfo.agvStationMap.Add(agvStationId, new MapAgvStation
                                {
                                    ID = agvStationId,
                                    Ports = new List<MapPort>() { port }
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"lastReadPortId=[{lastReadPortId}]" + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"lastReadPortId=[{lastReadPortId}]" + ex.Message);
            }
        }

        private string FitZero(string v)
        {
            int sectionIdToInt = int.Parse(v);
            return sectionIdToInt.ToString("0000");
        }

        //private void ReadAgvStationCsv()
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(AgvStationPath))
        //        {
        //            return;
        //        }

        //        string[] allRows = File.ReadAllLines(AgvStationPath);
        //        if (allRows == null || allRows.Length < 2)
        //        {
        //            return;
        //        }

        //        string[] titleRow = allRows[0].Split(',');
        //        allRows = allRows.Skip(1).ToArray();

        //        int nRows = allRows.Length;
        //        int nColumns = titleRow.Length;

        //        Dictionary<string, int> dicHeaderIndexes = new Dictionary<string, int>();
        //        for (int i = 0; i < nColumns; i++)
        //        {
        //            var keyword = titleRow[i].Trim();
        //            if (!string.IsNullOrWhiteSpace(keyword))
        //            {
        //                dicHeaderIndexes.Add(keyword, i);
        //            }
        //        }
        //        for (int i = 0; i < nRows; i++)
        //        {
        //            string[] getThisRow = allRows[i].Split(',');
        //            try
        //            {
        //                string stationId = getThisRow[dicHeaderIndexes["Id"]];
        //                failAgvStationId = stationId;
        //                string addressId = getThisRow[dicHeaderIndexes["AddressId"]];
        //                failAddressIdInReadAgvStationFile = addressId;

        //                if (Vehicle.Mapinfo.addressMap.ContainsKey(addressId))
        //                {
        //                    Vehicle.Mapinfo.addressMap[addressId].AgvStationId = stationId;
        //                    if (Vehicle.Mapinfo.agvStationMap.ContainsKey(stationId))
        //                    {
        //                        Vehicle.Mapinfo.agvStationMap[stationId].AddressIds.Add(addressId);
        //                    }
        //                    else
        //                    {
        //                        MapAgvStation agvStation = new MapAgvStation();
        //                        agvStation.ID = stationId;
        //                        agvStation.AddressIds.Add(addressId);
        //                        Vehicle.Mapinfo.agvStationMap.Add(stationId, agvStation);
        //                    }
        //                }
        //                else
        //                {
        //                    throw new Exception("Address ID not in the addressMap.");
        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"FailStationId=[{failAgvStationId}], FailAddressId=[{failAddressIdInReadAgvStationFile}]. " + ex.Message);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"lastReadPortId=[{lastReadPortId}]" + ex.Message);
        //    }
        //}

        public void ReadSectionCsv()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SectionPath))
                {
                    throw new Exception($"IsSectionPathNull={string.IsNullOrWhiteSpace(SectionPath)}");
                }
                Vehicle.Mapinfo.sectionMap.Clear();

                string[] allRows = File.ReadAllLines(SectionPath);
                if (allRows == null || allRows.Length < 2)
                {
                    throw new Exception("There are no section in file");
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
                        if (!Vehicle.Mapinfo.addressMap.ContainsKey(getThisRow[dicHeaderIndexes["FromAddress"]]))
                        {
                            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadSectionCsv read oneRow fail, headAddress is not in the map : [secId={oneRow.Id}][headAddress={getThisRow[dicHeaderIndexes["FromAddress"]]}]");
                        }
                        oneRow.HeadAddress = Vehicle.Mapinfo.addressMap[getThisRow[dicHeaderIndexes["FromAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.HeadAddress);
                        if (!Vehicle.Mapinfo.addressMap.ContainsKey(getThisRow[dicHeaderIndexes["ToAddress"]]))
                        {
                            LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadSectionCsv read oneRow fail, tailAddress is not in the map : [secId={oneRow.Id}][tailAddress={getThisRow[dicHeaderIndexes["ToAddress"]]}]");
                        }
                        oneRow.TailAddress = Vehicle.Mapinfo.addressMap[getThisRow[dicHeaderIndexes["ToAddress"]]];
                        oneRow.InsideAddresses.Add(oneRow.TailAddress);
                        oneRow.HeadToTailDistance = GetDistance(oneRow.HeadAddress.Position, oneRow.TailAddress.Position);
                        oneRow.Speed = double.Parse(getThisRow[dicHeaderIndexes["Speed"]]);
                        oneRow.Type = oneRow.SectionTypeParse(getThisRow[dicHeaderIndexes["Type"]]);
                    }
                    catch (Exception ex)
                    {
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadSectionCsv read oneRow fail : [lastReadSecId={lastReadSecId}]");
                        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                    }

                    lastReadSecId = oneRow.Id;
                    Vehicle.Mapinfo.sectionMap.Add(oneRow.Id, oneRow);
                }

                //LoadBeamSensorDisable();

                AddInsideAddresses();

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"Load Section File Ok. [lastReadSecId={lastReadSecId}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"LoadSectionCsv : [lastReadSecId={lastReadSecId}][{ex.Message}]");
            }
        }

        private void WriteAddressBackup()
        {
            var directionName = Path.GetDirectoryName(AddressPath);
            if (!Directory.Exists(directionName))
            {
                Directory.CreateDirectory(directionName);
            }

            var backupPath = Path.ChangeExtension(AddressPath, ".backup.csv");

            string titleRow = "Id,PositionX,PositionY,TransferPortDirection,GateType,PioDirection,ChargeDirection,CanSpin,IsTR50,InsideSectionId,OffsetX,OffsetY,OffsetTheta,VehicleHeadAngle" + Environment.NewLine;
            File.WriteAllText(backupPath, titleRow);
            List<string> lineInfos = new List<string>();
            foreach (var item in Vehicle.Mapinfo.addressMap.Values)
            {
                var lineInfo = string.Format("{0},{1:F0},{2:F0},{3},{4},{5},{6},{7},{8},{9:0000},{10:F2},{11:F2},{12:F2},{13:N0}",
                    item.Id, item.Position.X, item.Position.Y, item.TransferPortDirection, item.GateType, item.PioDirection, item.ChargeDirection,
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

            string titleRow = "Id,FromAddress,ToAddress,Speed,Type" + Environment.NewLine;
            File.WriteAllText(backupPath, titleRow);
            List<string> lineInfos = new List<string>();
            foreach (var item in Vehicle.Mapinfo.sectionMap.Values)
            {
                var lineInfo = string.Format("{0},{1},{2},{3},{4}",
                    item.Id, item.HeadAddress.Id, item.TailAddress.Id, item.Speed, item.Type
                    );
                lineInfos.Add(lineInfo);
            }
            File.AppendAllLines(backupPath, lineInfos);
        }

        private void AddInsideAddresses()
        {
            try
            {
                foreach (var adr in Vehicle.Mapinfo.addressMap.Values)
                {
                    if (Vehicle.Mapinfo.sectionMap.ContainsKey(adr.InsideSectionId))
                    {
                        Vehicle.Mapinfo.sectionMap[adr.InsideSectionId].InsideAddresses.Add(adr);
                    }
                }

                LogDebug(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, $"AddInsideAddresses Ok.");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name,
                    $"AddInsideAddresses FAIL at Sec[{lastReadSecId}] and Adr[{lastReadAdrId}]" + ex.Message);
            }
        }

        //private void AddMapSectionBeamDisableIntoList(MapSectionBeamDisable oneRow)
        //{
        //    try
        //    {
        //        if (!Vehicle.Mapinfo.sectionMap.ContainsKey(oneRow.SectionId))
        //        {
        //            throw new Exception($"Section[{oneRow.SectionId}]加入Beam Sensor Disable清單失敗, 圖資不包含Section[{oneRow.SectionId}]");                  
        //        }
        //        MapSection mapSection = Vehicle.Mapinfo.sectionMap[oneRow.SectionId];
        //        if (oneRow.Min < -30)
        //        {
        //            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
        //             , $"Min < 0. [SectionId={oneRow.SectionId}][Min={oneRow.Min}]"));
        //            return;
        //        }
        //        if (oneRow.Max > mapSection.HeadToTailDistance + 31)
        //        {
        //            mirleLogger.Log(new LogFormat("Error", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
        //            , $"Max > Distance. [SectionId={oneRow.SectionId}][Max={oneRow.Max}][Distance={mapSection.HeadToTailDistance}]"));

        //            return;
        //        }
        //        if (oneRow.Min == 0 && oneRow.Max == 0)
        //        {
        //            oneRow.Min = -30;
        //            oneRow.Max = mapSection.HeadToTailDistance + 30;
        //        }

        //        mapSection.BeamSensorDisables.Add(oneRow);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
        //    }
        //}

        public bool IsPositionInThisSection(MapSection aSection, MapPosition position)
        {
            try
            {
                MapPosition aPosition = position;

                #region NotInSection 2019.09.23
                double secMinX, secMaxX, secMinY, secMaxY;

                if (aSection.HeadAddress.Position.X >= aSection.TailAddress.Position.X)
                {
                    secMaxX = aSection.HeadAddress.Position.X + Vehicle.MapConfig.AddressAreaMm;
                    secMinX = aSection.TailAddress.Position.X - Vehicle.MapConfig.AddressAreaMm;
                }
                else
                {
                    secMaxX = aSection.TailAddress.Position.X + Vehicle.MapConfig.AddressAreaMm;
                    secMinX = aSection.HeadAddress.Position.X - Vehicle.MapConfig.AddressAreaMm;
                }

                if (aSection.HeadAddress.Position.Y >= aSection.TailAddress.Position.Y)
                {
                    secMaxY = aSection.HeadAddress.Position.Y + Vehicle.MapConfig.AddressAreaMm;
                    secMinY = aSection.TailAddress.Position.Y - Vehicle.MapConfig.AddressAreaMm;
                }
                else
                {
                    secMaxY = aSection.TailAddress.Position.Y + Vehicle.MapConfig.AddressAreaMm;
                    secMinY = aSection.HeadAddress.Position.Y - Vehicle.MapConfig.AddressAreaMm;
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
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return false;
            }
        }

        public double GetDistance(MapPosition aPosition, MapPosition bPosition)
        {
            var diffX = Math.Abs(aPosition.X - bPosition.X);
            var diffY = Math.Abs(aPosition.Y - bPosition.Y);
            return Math.Sqrt((diffX * diffX) + (diffY * diffY));
        }

        #region Log

        private NLog.Logger _transferLogger = NLog.LogManager.GetLogger("Transfer");

        private void LogException(string classMethodName, string exMsg)
        {
            try
            {
                _transferLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");
            }
            catch (Exception)
            {
            }
        }

        private void LogDebug(string classMethodName, string msg)
        {
            try
            {
                _transferLogger.Debug($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{msg}]");
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }

}
