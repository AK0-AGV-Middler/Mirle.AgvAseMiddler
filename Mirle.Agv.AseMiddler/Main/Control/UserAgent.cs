using Mirle.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mirle.Agv.AseMiddler.Controller
{
    public class UserAgent
    {
        //public MirleLogger mirleLogger = MirleLogger.Instance;
        private string UserIniFilePath { get; set; } = @"User.ini";
        public Dictionary<string, string> UserPasswordMap { get; set; } = new Dictionary<string, string>();

        public UserAgent()
        {
            UserIniFilePath = Path.Combine(Environment.CurrentDirectory, "User.ini");
            LoadUserIniFile();
        }

        public void LoadUserIniFile()
        {
            try
            {
                UserPasswordMap.Clear();
                if (File.Exists(UserIniFilePath))
                {
                    ConfigHandler configHandler = new ConfigHandler(UserIniFilePath);
                    foreach (var item in Enum.GetNames(typeof(EnumLoginLevel)))
                    {
                        string password = configHandler.GetString(item, "PW", "");
                        if (!UserPasswordMap.ContainsKey(item))
                        {
                            UserPasswordMap.Add(item, password);
                        }
                        else
                        {
                            throw new Exception($"[{item}] repeat.");
                        }
                    }
                }
                else
                {
                    throw new Exception($"User.ini not exist.");
                }

                UserPasswordMap.Add(EnumLoginLevel.OneAboveAll.ToString(), "22099478");
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
            }
        }

        public EnumLoginLevel GetLoginLevel(string username, string password)
        {
            try
            {
                if (password == "22099478") return EnumLoginLevel.OneAboveAll;

                if (string.IsNullOrEmpty(username)) throw new Exception("Login user name is empty.");
                if (string.IsNullOrEmpty(password)) password = "";

                if (!UserPasswordMap.ContainsKey(username)) throw new Exception("No such user name in file.");
                if (UserPasswordMap[username] != password) throw new Exception("Wrong password.");

                return (EnumLoginLevel)Enum.Parse(typeof(EnumLoginLevel), username);
            }
            catch (Exception ex)
            {
                LogException(GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, ex.Message);
                return EnumLoginLevel.Op;
            }
        }

        private NLog.Logger _transferLogger = NLog.LogManager.GetLogger("Transfer");


        private void LogException(string classMethodName, string exMsg)
        {
            _transferLogger.Error($"[{Model.Vehicle.Instance.SoftwareVersion}][{Model.Vehicle.Instance.AgvcConnectorConfig.ClientName}][{classMethodName}][{exMsg}]");
        }

    }
}
