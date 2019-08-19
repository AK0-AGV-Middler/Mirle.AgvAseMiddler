﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mirle.Agv.Controller;

namespace Mirle.Agv.Model.TransferCmds
{
    [Serializable]
    public class MoveCmdInfo : TransferStep
    {
        public List<MapPosition> AddressPositions { get; set; } = new List<MapPosition>();
        public List<EnumAddressAction> AddressActions { get; set; } = new List<EnumAddressAction>();
        public List<double> SectionSpeedLimits { get; set; } = new List<double>();
        public int PredictVehicleAngle { get; set; } = 0;
        public List<string> SectionIds { get; set; } = new List<string>();
        public List<string> AddressIds { get; set; } = new List<string>();

        public List<MapSection> MovingSections { get; set; } = new List<MapSection>();
        public int MovingSectionsIndex { get; set; } = 0;

        public MoveCmdInfo() : this(new MapInfo()) { }
        public MoveCmdInfo(MapInfo theMapInfo) : base(theMapInfo)
        {
            type = EnumTransferCommandType.Move;
        }

        public void SetAddressPositions()
        {
            AddressPositions = new List<MapPosition>();
            var firstPosition = Vehicle.Instance.AVehiclePosition.RealPosition;
            AddressPositions.Add(firstPosition);

            try
            {
                for (int i = 1; i < AddressIds.Count; i++)
                {
                    MapAddress mapAddress = theMapInfo.allMapAddresses[AddressIds[i]].DeepClone();
                    AddressPositions.Add(mapAddress.Position);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void SetNextUnloadAddressPositions()
        {
            AddressPositions = new List<MapPosition>();

            try
            {
                for (int i = 0; i < AddressIds.Count; i++)
                {
                    MapAddress mapAddress = theMapInfo.allMapAddresses[AddressIds[i]].DeepClone();
                    AddressPositions.Add(mapAddress.Position);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void SetSectionSpeedLimits()
        {
            SectionSpeedLimits = new List<double>();
            try
            {
                for (int i = 0; i < SectionIds.Count; i++)
                {
                    MapSection mapSection = theMapInfo.allMapSections[SectionIds[i]];
                    double SpeedLimit = mapSection.Speed;
                    SectionSpeedLimits.Add(SpeedLimit);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void SetAddressActions()
        {
            PredictVehicleAngle = theVehicle.AVehiclePosition.PredictVehicleAngle;

            AddressActions = new List<EnumAddressAction>();
            try
            {
                MapSection firstSection = theMapInfo.allMapSections[SectionIds[0]];
                if (firstSection.Type == EnumSectionType.R2000)
                {
                    AddressActions.Add(EnumAddressAction.R2000);
                }
                else
                {
                    AddressActions.Add(EnumAddressAction.ST);
                }

                for (int i = 0; i < SectionIds.Count - 1; i++)
                {
                    MapSection currentSection = theMapInfo.allMapSections[SectionIds[i]];
                    MapSection nextSection = theMapInfo.allMapSections[SectionIds[i + 1]];
                    EnumAddressAction addressMotion = SetAddressMotion(currentSection, nextSection);
                    AddressActions.Add(addressMotion);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
            AddressActions.Add(EnumAddressAction.End);

            theVehicle.AVehiclePosition.PredictVehicleAngle = PredictVehicleAngle;
        }

        public void SetMovingSections()
        {
            MovingSections = new List<MapSection>();
            for (int i = 0; i < SectionIds.Count; i++)
            {
                MapSection mapSection = new MapSection();
                try
                {
                    mapSection = theMapInfo.allMapSections[SectionIds[i]].DeepClone();
                    mapSection.CmdDirection = (mapSection.HeadAddress.Id == AddressIds[i]) ? EnumPermitDirection.Forward : EnumPermitDirection.Backward;
                }
                catch (Exception ex)
                {
                    var msg = ex.StackTrace;
                }
                MovingSections.Add(mapSection);
            }
        }

        private EnumAddressAction SetAddressMotion(MapSection currentSection, MapSection nextSection)
        {
            if (nextSection.Type == EnumSectionType.R2000)
            {
                //水平接R2000 或是 垂直接R2000 是否不同
                return EnumAddressAction.R2000;
            }
            else if (currentSection.Type == EnumSectionType.R2000)
            {
                //R2000接水平 或是 R2000接垂直 是否不同
                return EnumAddressAction.ST;
            }
            else if (currentSection.Type == nextSection.Type)
            {
                //水平接水平 或 垂直接垂直
                return EnumAddressAction.ST;
            }
            else
            {
                //水平接垂直 或 垂直接水平
                if (IsTurnRight(currentSection, nextSection))
                {
                    //右轉
                    PredictVehicleAngle -= 90;
                    if (PredictVehicleAngle < -100)
                    {
                        PredictVehicleAngle = 0;
                        return EnumAddressAction.BTR350;
                    }
                    return EnumAddressAction.TR350;
                }
                else
                {
                    //左轉
                    PredictVehicleAngle += 90;
                    if (PredictVehicleAngle > 100)
                    {
                        PredictVehicleAngle = 0;
                        return EnumAddressAction.BTR350;
                    }
                    return EnumAddressAction.TR350;
                }
            }
        }

        private bool IsTurnRight(MapSection currentSection, MapSection nextSection)
        {
            MapPosition curSectionMid = new MapPosition((currentSection.HeadAddress.Position.X + currentSection.TailAddress.Position.X) / 2,
                (currentSection.HeadAddress.Position.Y + currentSection.TailAddress.Position.Y) / 2);
            MapPosition nextSectionMid = new MapPosition((nextSection.HeadAddress.Position.X + nextSection.TailAddress.Position.X) / 2,
                (nextSection.HeadAddress.Position.Y + nextSection.TailAddress.Position.Y) / 2);

            if (currentSection.Type == EnumSectionType.Horizontal)
            {
                //水平接垂直
                if (curSectionMid.X < nextSectionMid.X)
                {
                    //W > XXX
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //W > S
                        return true;
                    }
                    else
                    {
                        //W > N
                        return false;
                    }
                }
                else
                {
                    //E > XXX
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //E > S
                        return false;
                    }
                    else
                    {
                        //E > N
                        return true;
                    }
                }
            }
            else
            {
                //垂直接水平
                if (curSectionMid.X < nextSectionMid.X)
                {
                    //XX > E
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //N > E
                        return false;
                    }
                    else
                    {
                        //S > E
                        return true;
                    }
                }
                else
                {
                    //XX > W
                    if (curSectionMid.Y < nextSectionMid.Y)
                    {
                        //N > W
                        return true;
                    }
                    else
                    {
                        //S > W
                        return false;
                    }
                }
            }

        }

    }
}
