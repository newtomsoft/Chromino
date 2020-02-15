using Data.Core;
using Data.DAL;
using Data.Enumeration;
using Data.Models;
using Data.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    public partial class Square
    {
        [NotMapped]
        public SquareVM SquareViewModel { get => new SquareVM { OpenRight = OpenRight, OpenBottom = OpenBottom, OpenLeft = OpenLeft, OpenTop = OpenTop, State = (SquareVMState)Color }; }

        [NotMapped]
        public Coordinate Coordinate { get => new Coordinate(X, Y); }

        public Coordinate[] GetAround()
        {
            return new Coordinate[] { GetRight(), GetBottom(), GetLeft(), GetTop() };
        }

        public Coordinate GetRight()
        {
            return new Coordinate(X + 1, Y);
        }

        public Coordinate GetBottom()
        {
            return new Coordinate(X, Y + 1);
        }

        public Coordinate GetLeft()
        {
            return new Coordinate(X - 1, Y);
        }

        public Coordinate GetTop()
        {
            return new Coordinate(X, Y - 1);
        }
    }
}
