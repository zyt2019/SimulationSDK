using PlatFormSDK.OutputData;


namespace PlatFormSDK.Apn
{
    public static class WcfExtends
    {

        #region 传感器 
        public static UE.OutputData.ShareMemoryDataFrame CurrentDataFrameViaWCF(this SimTaskAPNObject apnObj, int port=8000)
        {
            try
            {
                SimClusterLib.WCFAPNService.ShareMemoryDataFrame data = apnObj.WCFClient(port).GetCurrentDataFrame(apnObj.ID, apnObj.Token);
                return new ShareMemoryDataFrame(data.TimeStamp, data.Data);
            }
            catch { return null; }
        }

        public static ShareMemoryDataFrame<FDataFrameGPS> CurrentDataFrameViaWCF(this SensorGps sensor, int port = 8000)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaWCF((SimTaskAPNObject)sensor, port);
                FDataFrameGPS sensorData = new FDataFrameGPS();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameGPS>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        public static ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>> CurrentDataFrameViaWCF(this SensorLaserRadar sensor, int port = 8000)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaWCF((SimTaskAPNObject)sensor, port);
                FDataFrameArray<FDataFrameLaserRadar> sensorData = new FDataFrameArray<FDataFrameLaserRadar>();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        public static ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>> CurrentDataFrameViaWCF(this SensorWaveRadar sensor, int port = 8000)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaWCF((SimTaskAPNObject)sensor, port);
                FDataFrameArray<FDataFrameWaveRadar> sensorData = new FDataFrameArray<FDataFrameWaveRadar>();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        #endregion


        #region Vehicle




        public static ShareMemoryDataFrame<FDataFrameVehicleControl> CurrentVehicleControlViaWCF(this Vehicle vehicle, int port = 8000)
        {

            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaWCF(vehicle, port);
                FDataFrameVehicleControl vehData = new FDataFrameVehicleControl();
                vehData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameVehicleControl>(data.TimeStamp, vehData);
            }
            catch { return null; }

        }

        public static bool ExecControlViaWCF(this Vehicle vehicle, float throttle, float steer, float breaks, int port = 8000)
        {
            try { vehicle.WCFClient(port).SendCommandAsync("CONTROL", vehicle.Token, new string[] { vehicle.ID.ToString(), throttle.ToString(), steer.ToString(), breaks.ToString() }); return true; } catch { return false; }
        }

        public static bool ExecThrottleViaWCF(this Vehicle vehicle, float throttle, int port = 8000)
        {
            try { vehicle.WCFClient(port).SendCommandAsync("THROTTLE", vehicle.Token, new string[] { vehicle.ID.ToString(), throttle.ToString() }); return true; } catch { return false; }

        }
        public static bool ExecSteerViaWCF(this Vehicle vehicle, float steer, int port = 8000)
        {
            try { vehicle.WCFClient(port).SendCommandAsync("STEERING", vehicle.Token, new string[] { vehicle.ID.ToString(), steer.ToString() }); return true; } catch { return false; }

        }
        public static bool ExecBreaksViaWCF(this Vehicle vehicle, float breaks, int port = 8000)
        {
            try { vehicle.WCFClient(port).SendCommandAsync("BREAK", vehicle.Token, new string[] { vehicle.ID.ToString(), breaks.ToString() }); return true; } catch { return false; }

        }

        #endregion


        #region Sence
        public static ShareMemoryDataFrame<FJudgementRoleItemArray> CurrentJudgementRoleItemsViaWCF(this Sence sence, int port = 8000)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaWCF(sence, port);
                FJudgementRoleItemArray vehData = new FJudgementRoleItemArray();
                vehData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FJudgementRoleItemArray>(data.TimeStamp, vehData);
            }
            catch { return null; }
        }

        public static bool ExecStartViaWCF(this Sence sence, int port = 8000)
        {
            try { sence.WCFClient(port).SendCommandAsync("START", sence.Token, null); return true; } catch(System.Exception ex) { return false; }

        }

        #endregion




        private static WCFAPNService.MemberNodeServiceClient WCFClient(this SimTaskAPNObject simObj, int port, string key = "WCFClient")
        {
            if (simObj.Cache[key + port.ToString()] == null)
            {
                WCFAPNService.MemberNodeServiceClient wcfClient = new WCFAPNService.MemberNodeServiceClient();
                wcfClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(string.Format("http://{0}:{1}/MemberNodeService/", simObj.APN, port));
                wcfClient.Open();
                simObj.Cache.Add(key + port.ToString(), wcfClient);
            }
            return simObj.Cache[key + port.ToString()] as WCFAPNService.MemberNodeServiceClient;
        }

    }
}
