# Trinket
Event driven C# Wrapper for League of Legends LiveEvents API

Available on NuGet: https://www.nuget.org/packages/floh22.Trinket/

# Installation

**These are steps that any user of your project has to also complete. It is advised that you include these instructions as well or perform these steps automatically for the user.**

Add the following to `Riot Games\League of Legends\Config\game.cfg`

```
[LiveEvents]
Enable=1
Port=34243
```

Download [LiveEvents.ini](https://github.com/floh22/Trinket/blob/master/LiveEvents.ini) and move it into `Riot Games\League of Legends\Config\`.

Most events of the list do not actually get triggered, a small subset actually do. All are included for completeness. Remove events as needed, though keep in mind that all programs on a machine which use the LiveEvents endpoint rely on this list, so removing an event for one project may break another!

# Usage
Trinket can either detect League startup on its own or rely on the caller to start it when the Game has started.

Example usage: 
```
Trinket trinket = new Trinket(true, true)
trinket.OnConnect += (s,e) => Console.WriteLine("Trinket Connected");
trinket.OnDisconnect += (s,e) => Console.WriteLine("Trinket Disconnected");
trinket.OnLiveEvent += (s,e) => Console.WriteLine(e);
```
