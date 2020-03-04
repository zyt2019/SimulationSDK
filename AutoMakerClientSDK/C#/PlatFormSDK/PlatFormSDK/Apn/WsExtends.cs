using SimClusterLib.Task;
using SimClusterLib.UE.OutputData;
using SimClusterLib.Utils;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace SimClusterLib.Apn
{
    public static class WsExtends
    {
        #region 传感器 
        public static void StartReadCurrentDataFrameViaWS(this SimTaskAPNObject apnObj, Action<ShareMemoryDataFrame> onReviced, int port = 8081)
        {
            ReadLoop(apnObj, port, onReviced);
        }

        public static void StopReadCurrentDataFrameViaWS(this SimTaskAPNObject apnObj, int port = 8081)
        {
            RemoveReadLoop(apnObj, port);
        }

        public static void StartReadCurrentDataFrameViaWS(this SensorGps sensor, Action<ShareMemoryDataFrame<FDataFrameGPS>> onReviced, int port = 8081)
        {
            ReadLoop(sensor, port, (data) =>
            {
                FDataFrameGPS sensorData = new FDataFrameGPS();
                sensorData.FromByteArray(data.Data);
                if (onReviced != null) { onReviced(new ShareMemoryDataFrame<FDataFrameGPS>(data.TimeStamp, sensorData)); }
            });
        }

        public static void StartReadCurrentDataFrameViaWS(this SensorLaserRadar sensor, Action<ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>>> onReviced, int port = 8081)
        {
            ReadLoop(sensor, port, (data) =>
            {
                FDataFrameArray<FDataFrameLaserRadar> sensorData = new FDataFrameArray<FDataFrameLaserRadar>();
                sensorData.FromByteArray(data.Data);
                if (onReviced != null) { onReviced(new ShareMemoryDataFrame<FDataFrameArray<FDataFrameLaserRadar>>(data.TimeStamp, sensorData)); }
            });
        }

        public static void StartReadCurrentDataFrameViaWS(this SensorWaveRadar sensor, Action<ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>>> onReviced, int port = 8081)
        {
            ReadLoop(sensor, port, (data) =>
            {
                FDataFrameArray<FDataFrameWaveRadar> sensorData = new FDataFrameArray<FDataFrameWaveRadar>();
                sensorData.FromByteArray(data.Data);
                if (onReviced != null) { onReviced(new ShareMemoryDataFrame<FDataFrameArray<FDataFrameWaveRadar>>(data.TimeStamp, sensorData)); }
            });
        }

        #endregion


        #region Vehicle




        public static void StartReadCurrentVehicleControlViaWS(this Vehicle vehicle, Action<ShareMemoryDataFrame<FDataFrameVehicleControl>> onReviced, int port = 8081)
        {
            ReadLoop(vehicle, port, (data) =>
            {
                FDataFrameVehicleControl vehData = new FDataFrameVehicleControl();
                vehData.FromByteArray(data.Data);
                if (onReviced != null) { onReviced(new ShareMemoryDataFrame<FDataFrameVehicleControl>(data.TimeStamp, vehData)); }
            });
        }

        public static async Task<bool> ExecControlViaWS(this Vehicle vehicle, float throttle, float steer, float breaks, int port = 8081)
        {
            if (vehicle.Sence == null) { return false; }
            return await vehicle.Sence.ExecuteCommandViaWS(string.Format("CONTROL|{0}|{1}|{2}|{3}", vehicle.ID, throttle, steer, breaks), port);

        }

        public static async Task<bool> ExecThrottleViaWS(this Vehicle vehicle, float throttle, int port = 8081)
        {
            if (vehicle.Sence == null) { return false; }
            return await vehicle.Sence.ExecuteCommandViaWS(string.Format("THROTTLE|{0}|{1}", vehicle.ID, throttle), port);
        }
        public static async Task<bool> ExecSteerViaWS(this Vehicle vehicle, float steer, int port = 8081)
        {
            if (vehicle.Sence == null) { return false; }
            return await vehicle.Sence.ExecuteCommandViaWS(string.Format("STEERING|{0}|{1}", vehicle.ID, steer), port);
        }
        public static async Task<bool> ExecBreaksViaWS(this Vehicle vehicle, float breaks, int port = 8081)
        {
            if (vehicle.Sence == null) { return false; }
            return await vehicle.Sence.ExecuteCommandViaWS(string.Format("BREAK|{0}|{1}", vehicle.ID, breaks), port);
        }

        #endregion


        #region Sence
        public static void StartReadCurrentJudgementRoleItemsViaWS(this Sence sence, Action<ShareMemoryDataFrame<FJudgementRoleItemArray>> onReviced, int port = 8081)
        {

            ReadLoop(sence, port, (data) =>
             {
                 FJudgementRoleItemArray senceData = new FJudgementRoleItemArray();
                 senceData.FromByteArray(data.Data);
                 if (onReviced != null) { onReviced(new ShareMemoryDataFrame<FJudgementRoleItemArray>(data.TimeStamp, senceData)); }
             });
        }

        public static async Task<bool> ExecStartViaWS(this Sence sence, int port = 8081)
        {
            return await ExecuteCommandViaWS(sence, "Start", port);
        }

        public static async Task<bool> ExecuteCommandViaWS(this Sence sence, string command, int port = 8081)
        {
            try
            {
                await (await sence.WSClientAsync(port)).SendAsync(new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes(command)),
                    WebSocketMessageType.Text, true, System.Threading.CancellationToken.None).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static async Task<ClientWebSocket> WSClientAsync(this SimTaskAPNObject apnObj, int port, string handler = "Command", string key = "WSCommandClient")
        {
            ClientWebSocket client = apnObj.Cache[key + port.ToString()] as ClientWebSocket;

            if (client == null || client.State != WebSocketState.Open)
            {
                ClientWebSocket webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri(string.Format("ws://{0}:{2}/{1}", apnObj.APN, handler, port)), System.Threading.CancellationToken.None).ConfigureAwait(false);
                await webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes(apnObj.Token)),
                    WebSocketMessageType.Text, true, System.Threading.CancellationToken.None).ConfigureAwait(false);
                apnObj.Cache.Add(key + port.ToString(), webSocket);
            }
            return apnObj.Cache[key + port.ToString()] as ClientWebSocket;
        }
        #endregion


        private static async void ReadLoop(this SimTaskAPNObject apnObj, int port, Action<UE.OutputData.ShareMemoryDataFrame> action, string cacheKey = "LoopThread")
        {
            RemoveReadLoop(apnObj, port, cacheKey);

            ClientWebSocket client = await WSClientAsync(apnObj, port, string.Format("RawData/{0}", apnObj.ID), "WSReadClient");


            System.Threading.Thread workerThread = null;
            workerThread = new System.Threading.Thread(() =>
            {
                while (workerThread.IsAlive)
                {

                    byte[] data = client.ReceiveAllAsync().Result;

                    if (action != null)
                    {
                        action(new UE.OutputData.ShareMemoryDataFrame(
                            new DateTime(BitConverter.ToInt64(data.Take(sizeof(long)).ToArray(), 0)),
                            data.Skip(sizeof(long)).ToArray()));
                    }

                }
            });
            workerThread.Start();
            apnObj.Cache.Add(cacheKey + port.ToString(), workerThread);
        }


        private static void RemoveReadLoop(this SimTaskAPNObject apnObj, int port, string cacheKey = "LoopThread")
        {
            if (apnObj.Cache[cacheKey + port.ToString()] != null)
            {
                try
                {
                    System.Threading.Thread t = apnObj.Cache[cacheKey + port.ToString()] as System.Threading.Thread;
                    t.Abort();
                }
                catch { }
            }
        }




    }
}
