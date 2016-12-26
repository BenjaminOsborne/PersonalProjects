using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CSharpHelperFunctions
{
    public class Node
    {
        private int m_nInitialValue;

        public Node(int nValue, int nRow, int nAlong)
        {
            Value = nValue;
            m_nInitialValue = nValue;
            Row = nRow;
            Along = nAlong;
            IsOn = true;
        }

        public int Value { get; private set; }

        public bool IsOn { get; private set; }

        public void SetOff()
        {
            IsOn = false;
            Value = -1;
        }

        public int Row { get; private set; }

        public int Along { get; private set; }

        public Node Left { get; set; }

        public Node Right { get; set; }

        public Node BelowLeft { get; set; }

        public Node BelowRight { get; set; }

        public Node TopLeft { get; set; }

        public Node TopRight { get; set; }

        public bool IsBottom
        {
            get { return BelowLeft == null && BelowRight == null; }
        }

        public bool HasNoRoute
        {
            get { return IsBottom == false && ((BelowLeft == null || BelowLeft.IsOn == false) && (BelowRight == null || BelowRight.IsOn == false)); }
        }

        public IEnumerable<Node> GetSteps()
        {
            if (BelowLeft != null && BelowLeft.IsOn)
            {
                yield return BelowLeft;
            }
            if (BelowRight != null && BelowRight.IsOn)
            {
                yield return BelowRight;
            }
        }
        
        public override string ToString()
        {
            if (IsOn == false)
            {
                return "__";
            }

            if (Value < 10)
            {
                return "0" + Value.ToString();
            }
            return Value.ToString();
        }
    }

    public class NodeRoute
    {
        private List<Node> m_listNodes;

        public NodeRoute(Node oHeadNode)
        {
            m_listNodes = new List<Node>() { oHeadNode };
            Value = oHeadNode.Value;
        }

        private NodeRoute(IEnumerable<Node> enNodes, int nValue)
        {
            m_listNodes = enNodes.ToList();
            Value = nValue;
        }

        public NodeRoute Clone()
        {
            return new NodeRoute(m_listNodes, Value);
        }

        public IEnumerable<Node> Route { get { return m_listNodes; } }

        public void AddToRoute(Node oNode)
        {
            m_listNodes.Add(oNode);
            Value += oNode.Value;
        }

        public Node Head { get { return m_listNodes.Last(); } }

        public bool IsComplete { get { return Head.IsBottom && m_listNodes.All(x => x.IsOn); } }

        public bool HasFailed { get { return Head.HasNoRoute; } }

        public int Value { get; private set; }

        public IEnumerable<NodeRoute> SpawnRoutes()
        {
            if(HasFailed)
            {
                yield break;
            }
            if(IsComplete)
            {
                yield return this;
                yield break;
            }
            var listNodes = Head.GetSteps().ToList();
            if(listNodes.Any() == false || listNodes.Count > 2)
            {
                throw new Exception("Logic fail! Missing or too many Nodes");
            }

            if(listNodes.Count == 2)
            {
                var oClone = this.Clone();
                oClone.AddToRoute(listNodes[1]);
                yield return oClone;
            }
            AddToRoute(listNodes[0]);
            yield return this;
        }
    }

    public class Triange
    {
        //Top is row 0, width 1. Bottom is row nRows.
        private List<List<Node>> m_listNodes = new List<List<Node>>(); //List of Columns to list across
        private readonly int m_nRows;

        public Triange(int nRows)
        {
            m_nRows = nRows;
            for (int nRow = 0; nRow < nRows; nRow++)
            {
                m_listNodes.Add(new List<Node>(nRow + 1));
            }
        }

        public Node this[int nRow, int nAcross]
        {
            get
            {
                if (nRow >= 0 && nRow < m_listNodes.Count)
                {
                    var lstRow = m_listNodes[nRow];
                    if (nAcross >= 0 && nAcross < lstRow.Count)
                    {
                        return lstRow[nAcross];
                    }
                }
                return null;
            }
        }

        public bool SetRow(int nRow, IEnumerable<int> enRowItems)
        {
            var listItems = enRowItems.ToList();
            if (nRow != listItems.Count - 1)
            {
                return false;
            }
            var listRow = new List<Node>();
            for (int nAlong = 0; nAlong < listItems.Count; nAlong++)
            {
                listRow.Add(new Node(listItems[nAlong], nRow, nAlong));
            }
            m_listNodes[nRow] = listRow;
            return true;
        }

        public bool LinkRows()
        {
            for (int nRow = 0; nRow < m_nRows; nRow++)
            {
                for (int nPos = 0; nPos <= nRow; nPos++)
                {
                    var oNode = this[nRow, nPos];
                    oNode.Left = this[nRow, nPos - 1];
                    oNode.Right = this[nRow, nPos + 1];
                    oNode.TopLeft = this[nRow - 1, nPos - 1];
                    oNode.TopRight = this[nRow - 1, nPos];
                    oNode.BelowLeft = this[nRow + 1, nPos];
                    oNode.BelowRight = this[nRow + 1, nPos + 1];
                }
            }
            return true;
        }

        public IEnumerable<Node> Nodes { get { return m_listNodes.SelectMany(x => x); } }

        public override string ToString()
        {
            var oBuilder = new StringBuilder();
            var nRow = 0;
            foreach (var listRow in m_listNodes)
            {
                var nSpaces = m_nRows - nRow;
                for (int i = 0; i < nSpaces; i++)
                {
                    oBuilder.Append(" ");
                }
                foreach (var oNode in listRow)
                {
                    oBuilder.Append(oNode.ToString());
                    oBuilder.Append(" ");
                }
                oBuilder.AppendLine("\n    ");
                nRow++;
            }

            return oBuilder.ToString();
        }

        public int TraverseOnlyBest()
        {
            var oHeadNode = this[0, 0];
            var oHeadRoute = new NodeRoute(oHeadNode);

            var enRoutes = _GetNextRoutes(new[] { oHeadRoute });
            while(enRoutes.All(x => x.IsComplete == false))
            {
                enRoutes = _GetNextRoutes(enRoutes);
            }
            Debug.Assert(enRoutes.All(x => x.IsComplete), "All should be complete");
            var nMax = enRoutes.Max(x => x.Value);
            //var oRoute = enRoutes.First(x => x.Value == nMax);
            return nMax;
        }

        public IEnumerable<NodeRoute> _GetNextRoutes(IEnumerable<NodeRoute> enRoutes)
        {
            var nRow = enRoutes.First().Head.Row;
            Debug.Assert(enRoutes.All(x => x.Head.Row == nRow), "All Routes should have same row as Head");
            Debug.Assert(enRoutes.GroupBy(x => x.Head.Along).All(x => x.Count() == 1), "Only 1 route per offset along");

            var listNextRoutes = enRoutes.SelectMany(x => x.SpawnRoutes()).ToList();
            var listBestRoutes = new List<NodeRoute>();
            foreach(var oGroup in listNextRoutes.GroupBy(x => x.Head.Along))
            {
                var nMax = oGroup.Max(x => x.Value);
                listBestRoutes.Add(oGroup.First(x => x.Value == nMax));
            }
            return listBestRoutes;
        }
    }

    public class TriangleRunner
    {
        public void Run(List<List<int>> listInts)
        {
            var oTriange = new Triange(listInts.Count);
            for (var nRow = 0; nRow < listInts.Count; nRow++)
            {
                oTriange.SetRow(nRow, listInts[nRow]);
            }
            oTriange.LinkRows();
            
            Debug.WriteLine(oTriange.ToString());
            Console.WriteLine(oTriange.ToString());

            var oStopWatch = new Stopwatch();
            oStopWatch.Start();
            var nMaxValue = oTriange.TraverseOnlyBest();
            oStopWatch.Stop();

            Debug.WriteLine("Max Value: " + nMaxValue);
            Console.WriteLine("Max Value: " + nMaxValue);
            Debug.WriteLine("Time (ms): " + oStopWatch.ElapsedMilliseconds);
            Console.WriteLine("Time (ms): " + oStopWatch.ElapsedMilliseconds);
        }
    }
}
