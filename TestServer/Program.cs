using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace TestServer
{
    class Program
    {
        static MetaTrader createMetaTrader(string accountName, int accountNumber)
        {
            MetaTrader mt = new MetaTrader(accountName, accountNumber);

            mt.AddInstrument(new MtInstrument("EURUSD", 1.3, 1.4));
            mt.AddInstrument(new MtInstrument("EURJPY", 1.3, 1.4));
            mt.AddInstrument(new MtInstrument("EURAUD", 1.3, 1.4));
            mt.AddInstrument(new MtInstrument("USDAUD", 1.3, 1.4));
            mt.AddInstrument(new MtInstrument("USDJPY", 1.3, 1.4));

            return mt;
        }

        static MtInstrument selectInstrument(List<MtInstrument> instruments)
        {
            MtInstrument result = null;

            if (instruments != null)
            {
                Console.Clear();

                Console.WriteLine("Select Instrument:");

                foreach (var ins in instruments)
                    Console.WriteLine("{0} - {1}", instruments.IndexOf(ins), ins.Symbol);

                string selectedIndexStr = Console.ReadLine();
                int selectedIndex = -1;
                int.TryParse(selectedIndexStr, out selectedIndex);

                if (selectedIndex >= 0 && selectedIndex < instruments.Count)
                {
                    result = instruments[selectedIndex];
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid number of Instrument!\nPress any key...");
                }
            }

            return result;
        }

        static MtInstrumentChart selectChart(List<MtInstrumentChart> instrumentCharts)
        {
            MtInstrumentChart result = null;

            if (instrumentCharts != null)
            {
                Console.WriteLine("Select Chart:");

                foreach (var chart in instrumentCharts)
                {
                    if (chart.Instrument != null)
                    {
                        Console.WriteLine("{0} - {1}", instrumentCharts.IndexOf(chart), chart.Instrument.Symbol);
                    }
                }

                string selectedIndexStr = Console.ReadLine();
                int selectedIndex = -1;
                int.TryParse(selectedIndexStr, out selectedIndex);

                if (selectedIndex >= 0 && selectedIndex < instrumentCharts.Count)
                {
                    result = instrumentCharts[selectedIndex];
                }
                else
                {
                    Console.WriteLine("ERROR: Invalid number of Chart!\nPress any key...");
                }
            }

            return result;
        }

        static void addChart(MetaTrader mt)
        {
            Console.Clear();
            Console.WriteLine("Adding Chart.");

            var selectedIinstrument = selectInstrument(mt.Instruments);

            if (selectedIinstrument != null)
            {
                var instrumentChart = new MtInstrumentChart(selectedIinstrument);

                mt.AddInstrumentChart(instrumentChart);
            }
        }

        static void removeChart(MetaTrader mt)
        {
            var selectedChart = selectChart(mt.InstrumentCharts);

            if (selectedChart != null)
            {
                mt.RemoveInstrumentChart(selectedChart);
            }
        }

        static void addExpert(MetaTrader mt)
        {
            var selectedChart = selectChart(mt.InstrumentCharts);

            if (selectedChart != null)
            {
                bool isController = false;

                Console.WriteLine("Is Expert controller (y/n) ?");

                ConsoleKeyInfo keyInfo = Console.ReadKey();
                Console.WriteLine();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.N:
                        isController = false;
                        break;
                    case ConsoleKey.Y:
                        isController = true;
                        break;
                }

                Console.WriteLine("Input Port property (8222 - default): ");

                string portStr = Console.ReadLine();
                int port = 8222;
                if (int.TryParse(portStr, out port) == false)
                    port = 8222;
                var expert = new MtQuoteExpert(mt, port, isController);

                if (expert != null)
                {
                    selectedChart.AddExpert(expert);
                }
            }
        }

        static void printCharts(List<MtInstrumentChart> instrumentCharts)
        {
            if (instrumentCharts != null)
            {
                if (instrumentCharts.Count > 0)
                {
                    Console.WriteLine("Working Charts:");

                    foreach (var chart in instrumentCharts)
                    {
                        if (chart.Instrument != null)
                        {
                            string chartInfo = chart.Instrument.Symbol;
                            if (chart.Expert != null)
                            {
                                chartInfo += " - ";
                                chartInfo += chart.Expert.ToString();
                            }

                            Console.WriteLine(chartInfo);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No working Charts.");
                }

                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            var mt = createMetaTrader("Vyacheslav Demidyuk", 9107044);
            mt.SetDemo(false);

            mt.InfoUpdated += new MetaTraderInfoHandler(mt_InfoUpdated);

            mt.Start();

            Console.WriteLine("Test Server of MetaTraderApi.");
            Console.WriteLine("MetaTrader started.");
            Console.WriteLine("Press any key to continue...\n");

            Console.ReadKey();
            
            bool isWorked = true;

            do
            {
                Console.Clear();

                printCharts(mt.InstrumentCharts);

                Console.WriteLine("Choose command:");
                Console.WriteLine("A- Add Chart");
                Console.WriteLine("R- Remove Chart");
                Console.WriteLine("E- Add Expert");
                Console.WriteLine("Esc- Exit");

                ConsoleKeyInfo keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.A:
                        addChart(mt);
                        break;
                    case ConsoleKey.R:
                        removeChart(mt);
                        break;
                    case ConsoleKey.E:
                        addExpert(mt);
                        break;
                    case ConsoleKey.Escape:
                        isWorked = false;
                        break;
                }

            } while (isWorked);

            Console.WriteLine("MetaTrader stopped.");

            mt.Stop();
        }

        static void mt_InfoUpdated(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
