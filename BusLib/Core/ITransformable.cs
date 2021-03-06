﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusLib.Core
{
    public interface ITransformable<out T>
    {
        T Transform();

        bool CanTransform();
    }
}
