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
        public SquareViewModel SquareViewModel { get => new SquareViewModel { Edge = Edge, State = (SquareViewModelState)Color }; }

        [NotMapped]
        public Coordinate Coordinate { get => new Coordinate(X, Y); }

        [NotMapped]
        // 0 : right ; 1:bottom ; 2:left ; 3:left ; 4:top
        // TODO utiliser et suipprimer Edge
        public bool[] OpenSides { get; set; } = new bool[4];


        public void OpenLeftSide()
        {
            OpenSides[3] = true;
            Edge = Edge switch
            {
                OpenEdge.BottomTop => OpenEdge.BottomLeftTop,
                OpenEdge.Right => OpenEdge.RightLeft,
                OpenEdge.Top => OpenEdge.LeftTop,
                _ => OpenEdge.BottomLeft,
            };
        }

        public void OpenTopSide()
        {
            OpenSides[4] = true;
            Edge = Edge switch
            {
                OpenEdge.RightLeft => OpenEdge.RightLeftTop,
                OpenEdge.Right => OpenEdge.RightTop,
                OpenEdge.Left => OpenEdge.LeftTop,
                _ => OpenEdge.BottomTop,
            };
        }


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
