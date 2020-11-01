## Monitors

Monitors can be used to monitor different changes on Mt4 structs.

## MtMonitorBase
This is the base class for monitoring extensions. If inherited `MtMonitorBase` needs an instance of `MtApiClient` and `IMonitorTrigger`.

`SyncTrigger` (which can be set in the constructor or as Property) can be used to define whether the trigger should be started and stopped as well if `Start()` or `Stop()` will be called on an instance of a child class of `MtMonitorBase`.
Keep in mind: If an `IMonitorTrigger` will be used for several monitors, `SyncTrigger` set to `true` would cause that all monitors related to this trigger would stop:

```
var fooTrigger = new FooTrigger();
var fooMonitor = new FooMonitor(apiClient, fooTrigger, true);
var barMonitor = new FooMonitor(apiClient, fooTrigger, false);

fooTrigger.Start();
barMonitor.Start();
fooMonitor.Start();

fooMonitor.Stop(); //Because of SyncTrigger = true in the constructor of FooMonitor, fooTrigger.Stop() were triggered as well. Therefore barMonitor will not get any further triggers.
```

## IMonitorTrigger
An `IMonitorTrigger` is for defining when the monitor should check whether his conditions are met for invoking his event.

There are already two `IMonitorTrigger`s which can be used:
### NewBarTrigger
Triggers when a new bar starts
### TimeElapsedTrigger
Triggers when a defined time elapsed

## Default monitors:
There are already two different monitors defined. You can extend them or define new ones by inheriting from `MtMonitorBase`.

### TradeMonitor
Can be used to get updates on new opened trades or closed trades.

## ModifiedOrdersMonitor
Can be used to get updates on modified trades (takeprofit, stoploss, operation).

`OrderModifiedTypes` defines which modifications should be monitored:

1. `None` would cause no monitoring. But please use `Start()` and `Stop()` instead.
2. `TakeProfit` would cause observing whether the TakeProfit were changed.
3. `StopLoss` would cause observing whether the StopLoss were changed.
4. `Operation` would cause observing trades which changed from a stop / limit order to an open order.
5. `All` would cause observing all above defined.

Because `OrderModifiedTypes` is defined with the Flag-Attribute, you can combine the above monitoring types with a pipe: `OrderModifiedTypes.TakeProfit | OrderModifiedTypes.StopLoss`.

### Example
```
var orderModifyMonitor = new ModifiedOrdersMonitor(
    _apiClient,
    new MtApi.Monitors.Triggers.TimeElapsedTrigger(TimeSpan.FromSeconds(1)),
    OrderModifiedTypes.All,
    true
);
orderModifyMonitor.OrdersModified += OrderModifyMonitor_OrdersModified;
orderModifyMonitor.Start();

private void OrderModifyMonitor_OrdersModified(object sender, ModifiedOrdersEventArgs e)
{
    //Receives the event
}
```
