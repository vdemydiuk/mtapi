using MtApi5;

static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
{
    // Unix timestamp is seconds past epoch
    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
    return dateTime;
}

const string HOST = "localhost";
const int PORT = 8228;
const int HISTORY_DAYS = 20;

var client = new MtApi5Client();

Console.WriteLine("Start history test");

try
{
    client.Connect(HOST, PORT).Wait();
}
catch (Exception e)
{
    Console.WriteLine($"Connection failed: {e.Message}");
    return;
}

Console.WriteLine($"Connected to {HOST}:{PORT}");

var to = DateTime.Now;
var from = to.AddDays(-HISTORY_DAYS);
if (client.HistorySelect(from, to))
{
    var orders = client.HistoryOrdersTotal();
    Console.WriteLine($"Order count: {orders}");

    for (int i = 0; i < orders; i++)
    {
        var ticket = client.HistoryOrderGetTicket(i);

        var order_type = (ENUM_ORDER_TYPE) client.HistoryOrderGetInteger(ticket, ENUM_ORDER_PROPERTY_INTEGER.ORDER_TYPE);
        long time_setup_msc = client.HistoryOrderGetInteger(ticket, ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_SETUP_MSC);
        long time_done_msc = client.HistoryOrderGetInteger(ticket, ENUM_ORDER_PROPERTY_INTEGER.ORDER_TIME_DONE_MSC);
        double order_open_price = client.HistoryOrderGetDouble(ticket, ENUM_ORDER_PROPERTY_DOUBLE.ORDER_PRICE_OPEN);
        double order_volume = client.HistoryOrderGetDouble(ticket, ENUM_ORDER_PROPERTY_DOUBLE.ORDER_VOLUME_INITIAL);

        Console.WriteLine($"{i + 1} - Order #{ticket} type = {order_type}" +
            $" ORDER_TIME_SETUP_MSC={time_setup_msc} => {UnixTimeStampToDateTime(time_setup_msc / 1000)}," +
            $" ORDER_TIME_DONE_MSC={time_done_msc} => {UnixTimeStampToDateTime(time_done_msc / 1000)}," +
            $" Open price = {order_open_price}, Volume = {order_volume}");
    }
}
else
{
    var last_error = client.GetLastError();
    Console.WriteLine($"Failed to select order history. ErrorCode = {last_error}");
}

Console.WriteLine("History test completed.");

client.Disconnect();