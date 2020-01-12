using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Enumeration
{
    public enum ChrominoStatus
    {
        InStack = 1,
        InPlayer,
        InGame,
    }
}
