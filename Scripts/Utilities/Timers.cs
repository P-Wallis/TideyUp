using Godot;
using System;

namespace TideyUp.Utils
{
    public static class Timers
    {
        public static Timer CreateTimer(Node parent, string functionName, float waitTime, bool start = true, bool oneShot = true)
        {
            Timer timer = new Timer();
            timer.OneShot = oneShot;
            timer.WaitTime = waitTime;
            timer.Connect("timeout", parent, functionName);
            parent.AddChild(timer);
            if(start)
            {
                timer.Start();
            }

            return timer;
        }
    }
}
