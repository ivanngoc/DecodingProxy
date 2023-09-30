using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Buffers.Attributes
{

    /// <summary>
    /// Method guaranted read at least one byte. Infinity Loop Until Then
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ZeroReadBlockAttribute : Attribute
    {

    }

    /// <summary>
    /// Read until Zero then break.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReadUntilZeroAttribute : Attribute
    {

    }
}
