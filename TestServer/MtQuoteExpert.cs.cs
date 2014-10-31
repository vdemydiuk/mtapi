using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace TestServer
{
    class MtQuoteExpert : MtExpert
    {
        #region Dll Import functions
        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int initExpert(int isLocal, int port, [MarshalAs(UnmanagedType.LPStr)] string symbol, int expertHandle, IntPtr err);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int deinitExpert(int port, int expertHandle, IntPtr err);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int updateQuote(int port, [MarshalAs(UnmanagedType.LPStr)] string symbol, double bid, double ask, IntPtr err);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int getCommand(int port, ref int res, IntPtr err);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int getCommandType(int port, int commandId, ref int res);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendIntResponse(int port, int commandId, int response);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendBooleanResponse(int port, int commandId, int response);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendDoubleResponse(int port, int commandId, double response);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendVoidResponse(int port, int commandId);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendDoubleArrayResponse(int port, int commandId, int size);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int sendStringResponse(int port, int commandId, [MarshalAs(UnmanagedType.LPStr)]StringBuilder response);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int getIntValue(int port, int commandId, int paramIndex, ref int res);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int getDoubleValue(int port, int commandId, int paramIndex, ref double res);

        //[DllImport(@"d:\dw\project\forex\MetaTraderApi\Debug\MTConnector.dll")]
        [DllImport("MT5Connector.dll")]
        public static extern int getStringValue(int port, int commandId, int paramIndex, IntPtr res);

        #endregion

        public MtQuoteExpert(IMetaTrader metatrader, int port, bool isController) :
            base(MtExpertType.MtQuoteExpert, metatrader)
        {
            Port = port;
            IsController = isController;
        }

        #region Public Methods
        public override string ToString()
        {
            return string.Format("ExpertType = {0}; Port = {1}; IsController = {2}", ExpertType, Port, IsController);
        }
        #endregion


        #region Protected Methods
        protected override void init()
        {
            //int isDemo = IsDemo() == true ? 1 : 0;
            int isDemo = 1;
            mId = IDcount++;

            int isLocal = 1;

            if (initExpert(isLocal, Port, Symbol, mId, message) == 0)
            {
                MessageBoxA("Init error: " + "MetaTraderApi");
                isCrashed = true;
                return;
            }

            runController();

            //if (IsController == true)
            //{
            //    runController();

            //    if (deinitExpert(Port, mId, message) == 0)
            //    {
            //        MessageBoxA("Deinitilization error:\n" + "MetaTraderApi");
            //        return;
            //    }
            //}
        }

        protected override void deinit()
        {   
            if (isCrashed == false) 
            {
                if (deinitExpert(Port, mId, message) == 0)
                {
                    MessageBoxA("Deinit error: " + "MetaTraderApi");
                    isCrashed = true;
                }
            }
        }

        protected override void start()
        {
            if (isCrashed == false)
            {
                if (updateQuote(Port, Symbol, Bid, Ask, message) == 0)
                {
                    MessageBoxA("Start error: " + "MetaTraderApi");
                    isCrashed = true;
                }
            }
        }

        #endregion

        #region Private Methods
        private void runController()
        {
            string symbolValue = string.Empty;
            string commentValue = string.Empty;
            string msgValue = string.Empty;
            string captionValue = string.Empty;
            string filenameValue = string.Empty;
            string ftp_pathValue = string.Empty;
            string subjectValue = string.Empty;
            string some_textValue = string.Empty;
            int cmdValue = -1;
            int slippageValue = -1;
            int ticketValue = -1;
            int oppositeValue = -1;
            int magicValue = -1;
            int expirationValue = -1;
            int arrow_colorValue = -1;
            int colorValue = -1;
            int indexValue = -1;
            int selectValue = -1;
            int poolValue = -1;
            int errorCodeValue = -1;
            int typeValue = -1;
            int flagValue = -1;
            int millisecondsValue = -1;
            int dateValue = -1;
            int timeValue = -1;
            double lotsValue = double.NaN;
            double volumeValue = double.NaN;
            double priceValue = double.NaN;
            double stoplossValue = double.NaN;
            double takeprofitValue = double.NaN;

            int a1 = IsDemo() == true ? 1 : 0;

            Print("MetaTraderApi Expert is runned as controller. Symbol: " + Symbol);

            while (!IsStopped())
            {
                Thread.Sleep(1000);

                int commandId = -1;

                if (getCommand(Port, ref commandId, message) == 0)
                {
                    Print("[ERROR] getCommand");
                    isCrashed = true;
                    return;
                }     


                //try
                //{
                //    waitRequest(Port, ref commandId, message);
                //}
                //catch
                //{
                //    Print("[ERROR] waitRequest");
                //    isCrashed = true;
                //    return;
                //}
                
                //if (waitRequest(Port, ref commandId, message) == 0)
                //{
                //    Print("[ERROR] waitRequest");
                //    isCrashed = true;
                //    return;
                //}

                if (commandId < 0)
                    continue;

                Print("MetaTraderApi Expert recieved command. Symbol: " + Symbol);      

                 int commandType = 0;

                 if (getCommandType(Port, commandId, ref commandType) == 0)
                 {
                     Print("[ERROR] getCommandType");
                     continue;
                 }

                 switch (commandType)
                {
                    case 0: //NoCommmand
                        break;
                    case 1: //OrderSend
                        {
                            //if (getStringValue(Port, mId, 0, ref symbolValue) == 0)
                            //{
                            //    PrintParamError("symbolValue");
                            //    return;
                            //}

                            if (getIntValue(Port, commandId, 1, ref cmdValue) == 0)
                            {
                                PrintParamError("cmd");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 2, ref volumeValue) == 0)
                            {
                                PrintParamError("volume");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 3, ref priceValue) == 0)
                            {
                                PrintParamError("price");
                                continue;
                            }

                            if (getIntValue(Port, commandId, 4, ref slippageValue) == 0)
                            {
                                PrintParamError("slippage");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 5, ref stoplossValue) == 0)
                            {
                                PrintParamError("stoploss");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 6, ref takeprofitValue) == 0)
                            {
                                PrintParamError("takeprofit");
                                continue;
                            }

                            //if (getStringValue(Port, mId, 7, ref commentValue) == 0)
                            //{
                            //    PrintParamError("commentValue");
                            //    return;
                            //}

                            if (getIntValue(Port, commandId, 8, ref magicValue) == 0)
                            {
                                PrintParamError("magic");
                                continue;
                            }

                            if (getIntValue(Port, commandId, 9, ref expirationValue) == 0)
                            {
                                PrintParamError("expiration");
                                continue;
                            }

                            if (getIntValue(Port, commandId, 10, ref arrow_colorValue) == 0)
                            {
                                PrintParamError("arrow_color");
                                continue;
                            }


                            if (sendIntResponse(Port, commandId, OrderSend(symbolValue, cmdValue, volumeValue, priceValue, slippageValue, stoplossValue, takeprofitValue, commentValue, magicValue, expirationValue, arrow_colorValue)) == 0)
                            {
                                PrintResponseError("OrderSend");
                                continue;
                            }
                        }
                        break;

                    //OrderClose
                    case 2:
                        {
                            if (getIntValue(Port, commandId, 0, ref ticketValue) == 0)
                            {
                                PrintParamError("ticket");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 1, ref lotsValue) == 0)
                            {
                                PrintParamError("lots");
                                continue;
                            }

                            if (getDoubleValue(Port, commandId, 2, ref priceValue) == 0)
                            {
                                PrintParamError("price");
                                continue;
                            }

                            if (getIntValue(Port, commandId, 3, ref slippageValue) == 0)
                            {
                                PrintParamError("slippage");
                                continue;
                            }

                            if (getIntValue(Port, commandId, 4, ref colorValue) == 0)
                            {
                                PrintParamError("color");
                                continue;
                            }

                            //RefreshRates();

                            var result = OrderClose(ticketValue, lotsValue, priceValue, slippageValue, colorValue);
                            int responseValue = result == true ? 1 : 0;

                            if (sendBooleanResponse(Port, commandId, responseValue) == 0)
                            {
                                PrintResponseError("OrderClose");
                                continue;
                            }

                        }
                        break;

                    //IsConnected
                    case 27:
                        {
                            var result = IsConnected();
                            int responseValue = result == true ? 1 : 0;
                            if (sendBooleanResponse(Port, commandId, responseValue) == 0)
                            {
                                PrintResponseError("IsConnected");
                                continue;
                            }
                        }
                        break;

                    //IsDemo
                    case 28:
                        {
                            var result = true;// IsDemo();
                            int responseValue = result == true ? 1 : 0;
                            sendBooleanResponse(Port, commandId, responseValue);
                        }
                        break;

                    //ErrorDescription
                    case 39:
                        {
                            //int errorCode = getIntValueOfField(0);
                            //var result = ErrorDescription(errorCode);
                            //sendStringResponse(result);
                        }
                        break;

                    //iCloseArray
                    case 144:
                        {
                            int sizeValue = 0;
                            if (getIntValue(Port, commandId, 3, ref sizeValue) == 0)
                            {
                                PrintParamError("slippage");
                                continue;
                            }

                            if (sendDoubleArrayResponse(Port, commandId/*, prices*/, 100) == 0)
                            {
                                PrintResponseError("iCloseArray");
                                continue;
                            }
                        }
                        break;

                    default: //Unknown Commmand                        
                        Print(string.Format("MetaTrader: Unknown Commmand: code = {0}", commandType));
                        sendVoidResponse(Port, commandId);
                        break;
                }
            }
        }

        private void PrintParamError(string paramName)
        {
            Print("[ERROR] parameter: " + paramName);
        }

        private void PrintResponseError(string commandName)
        {
            Print("[ERROR] response: " + commandName);
        }
        #endregion

        #region Properties
        public int Port { get; private set; }
        public bool IsController { get; private set; }
        #endregion

        #region Fields
        private IntPtr message = IntPtr.Zero;
        private bool isCrashed = false;
        private int mId = -1;
        private static int IDcount = 0;
        #endregion
    }
}
