using PlatFormSDK.OutputData;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace PlatFormSDK.Apn
{
    public class HttpExtends
    {
        public string SimTask { get; set; }

        private string serverUrl = "127.0.0.1";
        private string serverPort = "8080";
        private int taskId;
        public HttpExtends(string url,string port, int taskid)
        {
            serverUrl = url;
            taskId = taskid;
        }

        public string CurrentSimTask()
        {
            try
            {
                byte[] data = null;
                DoHttpGet(string.Format("http://{0}:{1}/GetTaskbyId/{2}", serverUrl, serverPort, taskId),
                             (wr) =>
                             {
                                 using (var memoryStream = new MemoryStream())
                                 {
                                     wr.GetResponseStream().CopyTo(memoryStream);
                                     data = memoryStream.ToArray();
                                 }
                                 ;
                             });
                return System.Text.Encoding.Default.GetString(data);
            }
            catch { return null; }
        }

        public ShareMemoryDataFrame CurrentDataFrameViaHTTP(int dataId)
        {
            try
            {
                byte[] data = null;
                DoHttpGet(string.Format("http://{0}:{1}/RawData/{2}", serverUrl, serverPort, dataId),
                             (wr) =>
                             {
                                 using (var memoryStream = new MemoryStream())
                                 {
                                     wr.GetResponseStream().CopyTo(memoryStream);
                                     data = memoryStream.ToArray();
                                 }
                                 ;
                             });
                return new ShareMemoryDataFrame(new DateTime(BitConverter.ToInt64(data.Take(sizeof(long)).ToArray(), 0)), data.Skip(sizeof(long)).ToArray());
            }
            catch { return null; }
        }
        #region 传感器 
        /// <summary>
        /// GPS数据
        /// </summary>
        /// <returns></returns>
        public ShareMemoryDataFrame<FDataFrameGPS> CurrentGPSHTTP(int gpsSensorId)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaHTTP(gpsSensorId);
                FDataFrameGPS sensorData = new FDataFrameGPS();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameGPS>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        public ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>> CurrentLaserRadarHTTP(int laserRadarId)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaHTTP(laserRadarId);
                FDataFrameArray<FDataFrameLaserRadar> sensorData = new FDataFrameArray<FDataFrameLaserRadar>();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        public ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>> CurrentWaveRadarHTTP(int waveRadarId)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaHTTP(waveRadarId);
                FDataFrameArray<FDataFrameWaveRadar> sensorData = new FDataFrameArray<FDataFrameWaveRadar>();
                sensorData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>>(data.TimeStamp, sensorData);
            }
            catch { return null; }
        }

        #endregion

        #region Vehicle
        /// <summary>
        /// 获得车辆状态信息
        /// </summary>
        /// <returns></returns>
        public ShareMemoryDataFrame<FDataFrameVehicleControl> CurrentVehicleControlHTTP(int vehicleId)
        {

            try
            {
                ShareMemoryDataFrame data =CurrentDataFrameViaHTTP(vehicleId);
                FDataFrameVehicleControl vehData = new FDataFrameVehicleControl();
                vehData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FDataFrameVehicleControl>(data.TimeStamp, vehData);
            }
            catch { return null; }
        }

        public bool ExecControlViaHTTP(int vehicleId, float throttle, float steer, float breaks)
        {
            try { DoHttpGet(string.Format("http://{0}:{5}/Command/CONTROL/{1}/{2}/{3}/{4}", serverUrl, vehicleId, throttle, steer, breaks, serverPort)); return true; } catch { return false; }
        }

        public bool ExecThrottleViaHTTP(int vehicleId, float throttle)
        {
            try { DoHttpGet(string.Format("http://{0}:{3}/Command/THROTTLE/{1}/{2}", serverUrl, vehicleId, throttle, serverPort)); return true; } catch { return false; }

        }
        public bool ExecSteerViaHTTP(int vehicleId, float steer)
        {
            try { DoHttpGet(string.Format("http://{0}:{3}/Command/STEERING/{1}/{2}", serverUrl, vehicleId, steer, serverPort)); return true; } catch { return false; }

        }
        public bool ExecBreaksViaHTTP(int vehicleId, float breaks)
        {
            try { DoHttpGet(string.Format("http://{0}:{3}/Command/BREAK/{1}/{2}", serverUrl, vehicleId, breaks, serverPort)); return true; } catch { return false; }
        }

        #endregion


        #region Sence
        public ShareMemoryDataFrame<FJudgementRoleItemArray> CurrentJudgementRoleItemsViaHTTP(int senceId)
        {
            try
            {
                ShareMemoryDataFrame data = CurrentDataFrameViaHTTP(senceId);
                FJudgementRoleItemArray vehData = new FJudgementRoleItemArray();
                vehData.FromByteArray(data.Data);
                return new ShareMemoryDataFrame<FJudgementRoleItemArray>(data.TimeStamp, vehData);
            }
            catch { return null; }
        }

        public bool ExecStartViaHTTP()
        {
            try { DoHttpGet(string.Format("http://{0}:{1}/Command/Start/", serverUrl, serverPort)); return true; } catch { return false; }
        }

        #endregion


        private bool DoHttpGet(string url, Action<WebResponse> action = null)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                req.Headers.Add("TaskId", taskId.ToString());
                using (WebResponse wr = req.GetResponse())
                {
                    if (action != null) { action(wr); }
                }
                return true;
            }
            catch { return false; }

        }

    }
}
