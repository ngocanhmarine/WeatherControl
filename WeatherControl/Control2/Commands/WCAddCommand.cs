using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherControl.Control2.Commands
{
    public class WCAddCommand : WCActionCommand
    {
        public WCAddCommand() : base(action => Add(), canExecute => CanAdd()) { }
        public static void Add() 
        {

        }
        public static bool CanAdd() 
        {
            return true;
        }
    }
}
