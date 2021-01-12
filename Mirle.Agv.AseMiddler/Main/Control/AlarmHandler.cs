using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.AseMiddler.Model;
using Mirle.Agv.AseMiddler.Model.Configs;

using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using Mirle.Tools;

namespace Mirle.Agv.AseMiddler.Controller
{

    public class AlarmHandler
    {
        #region Containers
        public Dictionary<int, Alarm> allAlarms = new Dictionary<int, Alarm>();
        public ConcurrentDictionary<int, Alarm> dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
        public string LastAlarmMsg { get; set; } = "";

        public System.Text.StringBuilder SbAlarmMsg { get; set; } = new System.Text.StringBuilder(short.MaxValue);
        public System.Text.StringBuilder SbAlarmHistoryMsg { get; set; } = new System.Text.StringBuilder(short.MaxValue);

        #endregion     

        //private MirleLogger mirleLogger = MirleLogger.Instance;
        public Vehicle Vehicle { get; set; } = Vehicle.Instance;

        private NLog.Logger _alarmHistoryLogger = NLog.LogManager.GetLogger("AlarmHistory");


        //public string AlarmLogMsg { get; set; } = "";
        //public string AlarmHistoryLogMsg { get; set; } = "";

        public AlarmHandler()
        {
            LoadAlarmFile();
        }

        private void LoadAlarmFile()
        {
            try
            {
                if (string.IsNullOrEmpty(Vehicle.AlarmConfig.AlarmFileName))
                {
                    throw new Exception($"string.IsNullOrEmpty(alarmConfig.AlarmFileName)={string.IsNullOrEmpty(Vehicle.AlarmConfig.AlarmFileName)}");
                }

                string alarmFullPath = Path.Combine(Environment.CurrentDirectory, Vehicle.AlarmConfig.AlarmFileName);
                Dictionary<string, int> dicAlarmIndexes = new Dictionary<string, int>();
                allAlarms.Clear();

                string[] allRows = File.ReadAllLines(alarmFullPath);
                if (allRows == null || allRows.Length < 2)
                {
                    throw new Exception("There are no alarms in file");
                }

                string[] titleRow = allRows[0].Split(',');
                allRows = allRows.Skip(1).ToArray();

                int nRows = allRows.Length;
                int nColumns = titleRow.Length;

                //Id, AlarmText, PlcAddress, PlcBitNumber, Level, Description
                for (int i = 0; i < nColumns; i++)
                {
                    var keyword = titleRow[i].Trim();
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        keyword = keyword.ToUpper();
                        dicAlarmIndexes.Add(keyword, i);
                    }
                }

                for (int i = 0; i < nRows; i++)
                {
                    string[] getThisRow = allRows[i].Split(',');
                    Alarm oneRow = new Alarm();
                    oneRow.Id = int.Parse(getThisRow[dicAlarmIndexes["ID"]]);
                    oneRow.AlarmText = getThisRow[dicAlarmIndexes["ALARMTEXT"]];
                    if (Enum.TryParse(getThisRow[dicAlarmIndexes["LEVEL"]], out EnumAlarmLevel level))
                    {
                        oneRow.Level = level;
                    }
                    oneRow.Description = getThisRow[dicAlarmIndexes["DESCRIPTION"]];

                    allAlarms.Add(oneRow.Id, oneRow);
                }

                NLog.Logger _transferlogger = NLog.LogManager.GetLogger("Transfer");
                _transferlogger.Debug($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}]Load Alarm File Ok");
                //mirleLogger.Log(new LogFormat("Debug", "5", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID"
                //     , "Load Alarm File Ok"));
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void SetAlarm(int id)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                Alarm alarm = allAlarms.ContainsKey(id) ? allAlarms[id] : new Alarm { Id = id };
                alarm.SetTime = timeStamp;

                dicHappeningAlarms.TryAdd(id, alarm);
                LastAlarmMsg = $@"[{alarm.Id}][{alarm.AlarmText}]";

                LogAlarmHistory(alarm);
                var alarmMessage = $@"[ID={alarm.Id}][Text={alarm.AlarmText}][{alarm.Level}][SetTime={alarm.SetTime.ToString("HH-mm-ss.fff")}][Description={alarm.Description}]";
                AppendAlarmLogMsg(alarmMessage);
                AppendAlarmHistoryLogMsg(alarmMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void ResetAllAlarms()
        {
            try
            {
                lock (dicHappeningAlarms)
                {
                    dicHappeningAlarms = new ConcurrentDictionary<int, Alarm>();
                    LastAlarmMsg = "";
                }

                string resetMessage = "Reset All Alarms.";
                LogAlarmHistory(resetMessage);
                AppendAlarmHistoryLogMsg(resetMessage);
                SbAlarmMsg.Clear();
                //AlarmLogMsg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"), "\t", resetMessage);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private EnumAlarmLevel EnumAlarmLevelParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EnumAlarmLevel)Enum.Parse(typeof(EnumAlarmLevel), v);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return EnumAlarmLevel.Warn;
            }
        }

        private void LogAlarmHistory(Alarm alarm)
        {
            try
            {
                //mirleLogger.LogString("AlarmHistory", msg);

                _alarmHistoryLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{alarm.Id},{alarm.AlarmText},{alarm.Level},{alarm.SetTime},{alarm.ResetTime},{alarm.Description}]");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogAlarmHistory(string msg)
        {
            try
            {
                //mirleLogger.LogString("AlarmHistory", msg);

                _alarmHistoryLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{msg}]");

            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public string GetAlarmText(int errorCode)
        {
            if (allAlarms.ContainsKey(errorCode))
            {
                var alarm = allAlarms[errorCode];
                return $"[Id={errorCode}][Text={alarm.AlarmText}]";
            }

            return $"[Id={errorCode}][Text=Unknow]";
        }

        public bool IsAlarm(int errorCode)
        {
            if (allAlarms.ContainsKey(errorCode))
            {
                return allAlarms[errorCode].Level == EnumAlarmLevel.Alarm;
            }

            return false;
        }

        public void AppendAlarmLogMsg(string msg)
        {
            try
            {
                int th = Vehicle.MainFlowConfig.StringBuilderMax;
                int thHalf = th / 2;

                lock (SbAlarmMsg)
                {
                    if (SbAlarmMsg.Length + msg.Length > th)
                    {
                        SbAlarmMsg.Remove(0, thHalf);
                    }
                    SbAlarmMsg.AppendLine($"{DateTime.Now:HH:mm:ss} {msg}");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public void AppendAlarmHistoryLogMsg(string msg)
        {
            try
            {
                int th = Vehicle.MainFlowConfig.StringBuilderMax;
                int thHalf = th / 2;

                lock (SbAlarmHistoryMsg)
                {
                    if (SbAlarmHistoryMsg.Length + msg.Length > th)
                    {
                        SbAlarmHistoryMsg.Remove(0, thHalf);
                    }
                    SbAlarmHistoryMsg.AppendLine($"{DateTime.Now:HH:mm:ss} {msg}");
                }
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        private void LogException(string classMethodName, string exMsg)
        {
            _alarmHistoryLogger.Error($"[{Vehicle.SoftwareVersion}][{Vehicle.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");
        }

    }
}
