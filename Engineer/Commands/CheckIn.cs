﻿using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class CheckIn : EngineerCommand
    {
        public override string Name => "checkIn";

        public override async Task Execute(EngineerTask task)
        {
            Console.WriteLine(" doing check in");
            Tasking.FillTaskResults("checking in",task,EngTaskStatus.Complete);
        }
    }
}
