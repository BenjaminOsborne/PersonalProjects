using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
    public class IndividualCell
    {
        public bool[] possibleValues = new bool[9];
        public bool valueKnown = false;
        public int cellValue;
        public int knownSolutionValue;

        public int row;
        public int column;
        public int cellSector;

        public IndividualCell()
        {
            setAllSame(true);
        }

        public IndividualCell(int minusOne) //create default object
        {
            if (minusOne != -1)
                throw new Exception("This Constructor should take arg as -1, creates default object");
        }

        public IndividualCell(int initialValue, int solutionValue, int rowVal, int columnVal, int cellSectorVal, List<IndividualCell> cellList)
        {
            row = rowVal;
            column = columnVal;
            cellSector = cellSectorVal;

            knownSolutionValue = solutionValue;

            if (initialValue==0)
            {
                setAllSame(true);
                cellValue = initialValue; //=0
            }
            else
            {
                UtilityLogic.SetValue(this, initialValue, cellList);
            }

        }

        public void SetValue(int localValue)
        {
            checkValid(localValue);
            setAllSame(false);
            
            cellValue = localValue;
            valueKnown = true;
        }

        private void checkValid(int value)
        {
            if (value < 1 || value > 9)
                throw new Exception("Value must be between 1 and 9");
        }

        private void setAllSame(bool value)
        {
            for (int i = 0; i < 9; i++)
                possibleValues[i] = value;
        }

        public IndividualCell GetClone()
        {
            IndividualCell CellClone = new IndividualCell();
            for(int i = 0; i<possibleValues.Length; i++)
                CellClone.possibleValues[i] = possibleValues[i];
            
            CellClone.valueKnown = valueKnown;
            CellClone.cellValue = cellValue;
            CellClone.knownSolutionValue = knownSolutionValue;
            CellClone.row = row;
            CellClone.column = column;
            CellClone.cellSector = cellSector;

            return CellClone;
        }
    }
}


//For each list, create array/list of val 1 to 9 (bool[] again?)
//Cross off vals already known from list (AND ON THIS CELL?)
//List which cells possible for each val 1 to 9?
//create a list of lists -> for each val 1 to 9, store index of cell in list which could be that value
//Any number (1 to 9) which is only met by one cell - set that cell to that value
//-??? Extend -> How to redo possible list for each cell if exact still not known but possibilities reduced?
//E.g. if only two possible '3s' in sec 1 on same line - remove all possible 3s from other cells in that line...
//Or if only possible '4s' in one column both in one sector, remove all other possible 4s from that sector...

////print
//for (int i = 0; i < 9; i++)
//{
//    Console.Write("Element {0}: ", i);
//    if (rowPossibles.ContainsKey(i))
//    {
//        foreach (IndividualCell singleCell in rowPossibles[i])
//        {
//            Console.Write("{0} |", singleCell.column);
//        }
//    }
//    Console.WriteLine();

//}
////print end


//public IndividualCell(int initialValue, int rowVal, int columnVal, int cellSectorVal, List<IndividualCell> cellList) :
//    this(initialValue, -1, rowVal, columnVal, cellSectorVal, cellList)
//{
//}

//public bool RemovePossibleValuesThroughKnownValues(List<IndividualCell> allCells)
//{
//    //no need to find if current cell as if current cell has a cellValue!=0, this won't be called!
//    foreach (IndividualCell singleCell in allCells)
//    {
//        if (singleCell.row == row || singleCell.column == column || singleCell.cellSector == cellSector)
//        {
//            if (singleCell.valueKnown)
//            {
//                possibleValues[singleCell.cellValue - 1] = false;
//            }
//        }
//    }

//    return CheckIfSinglePossibleValueRemains(allCells);
//}

//public bool RemovePossibleValuesThroughOtherPossibleValueLists(List<IndividualCell> allCells)
//{
//    bool valueFound = false;
//    //create list for row, col and sector (not inc own cell)
//    List<IndividualCell> rowList = new List<IndividualCell>();
//    List<IndividualCell> colList = new List<IndividualCell>();
//    List<IndividualCell> secList = new List<IndividualCell>();

//    foreach (IndividualCell singleCell in allCells)
//    {
//            if (singleCell.row == row)
//                rowList.Add(singleCell);
//            if (singleCell.column == column)
//                colList.Add(singleCell);
//            if (singleCell.cellSector == cellSector)
//                secList.Add(singleCell);
//    }

//    //Check that all lists are 9 elements long...
//    if (rowList.Count != 9 || colList.Count != 9 || secList.Count != 9)
//        throw new Exception("Row, col or sec list should be 9 elements long, one (or more) is not");

//    //xxxPossibles will have a list for each number (index 0 to 8) of possible cells to take each value...
//    Dictionary<int, List<IndividualCell>> rowPossibles = CreatePossibleDictionaries(rowList);
//    Dictionary<int, List<IndividualCell>> colPossibles = CreatePossibleDictionaries(colList);
//    Dictionary<int, List<IndividualCell>> secPossibles = CreatePossibleDictionaries(secList);

//    //if any number (1 to 9, indexed by 0 to 9 in dict) only found once, then set that cell to have that value
//    //if any number found only in 1 sector, remove possibles from other cells in that sector
//    valueFound = ProcessPossibleDictionaries(allCells, rowPossibles, "row");
//    if(!valueFound)
//        valueFound = ProcessPossibleDictionaries(allCells, colPossibles, "col");
//    if (!valueFound)
//        valueFound = ProcessPossibleDictionaries(allCells, secPossibles, "sec");

//    //if any number only possible within one sector, then remove that number as possible from every other cell in that sectors possible bool[]

//    return valueFound;
//}

//private bool ProcessPossibleDictionaries(List<IndividualCell> allCells, Dictionary<int, List<IndividualCell>> rowColOrSecPossibles, string mode)
//{
//    bool valueFound = false;

//    for (int i = 0; i < 9; i++) //loop over numbers 1 to 9 in dict...
//    {
//        if (valueFound)
//            break; //must break as changes in values make existing dicts/lists invalid.

//        if (rowColOrSecPossibles.ContainsKey(i)) //only for numbers which aren't already known
//        {
//            if (rowColOrSecPossibles[i].Count == 1) //if only one cell, this cell must contain the number (i+1)
//            {
//                rowColOrSecPossibles[i].ElementAt(0).SetValue(i + 1, allCells);
//                valueFound = true;
//                //break;
//            }

//            else
//            {
//                if (mode == "row" || mode == "col")
//                {
//                    int sectorNumber = rowColOrSecPossibles[i].ElementAt(0).cellSector;
//                    bool sectorsAllSame = true;

//                    foreach (IndividualCell singleCell in rowColOrSecPossibles[i])
//                    {
//                        if (singleCell.cellSector != sectorNumber)
//                            sectorsAllSame = false;
//                    }
//                    if (sectorsAllSame) //if any number only possible in one sector, remove this from possibles[] in other cells in this sector
//                    {
//                        if (mode == "row")
//                        {
//                            //Console.WriteLine("Sectors all same. Row: {2}, Number:{0}, Sector:{1}.", i + 1, sectorNumber, row);
//                            foreach (IndividualCell singleCell in allCells)
//                            {
//                                if (singleCell.row != row & singleCell.cellSector == sectorNumber & !singleCell.valueKnown)
//                                {
//                                    singleCell.possibleValues[i] = false;
//                                    valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
//                                    //if (valueFound) { break; }
//                                }
//                            }
//                        }
//                        if (mode == "col")
//                        {
//                            //Console.WriteLine("Sectors all same. Col: {2}, Number:{0}, Sector:{1}.", i + 1, sectorNumber, column);
//                            foreach (IndividualCell singleCell in allCells)
//                            {
//                                if (singleCell.column != column & singleCell.cellSector == sectorNumber & !singleCell.valueKnown)
//                                {
//                                    singleCell.possibleValues[i] = false;
//                                    valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
//                                    //if (valueFound) { break; }
//                                }
//                            }
//                        }
//                    }
//                }
//                else if (mode == "sec")
//                {
//                    int rowNumber = rowColOrSecPossibles[i].ElementAt(0).row;
//                    int colNumber = rowColOrSecPossibles[i].ElementAt(0).column;
//                    bool rowsAllSame = true;
//                    bool colsAllSame = true;

//                    foreach (IndividualCell singleCell in rowColOrSecPossibles[i])
//                    {
//                        if (singleCell.row != rowNumber)
//                            rowsAllSame = false;
//                        if (singleCell.column != colNumber)
//                            colsAllSame = false;
//                    }
//                    if (rowsAllSame) //if any number only possible in one row, remove this from possibles[] in other cells in this row
//                    {
//                        //Console.WriteLine("Rows all same. Row: {2}, Number:{0}, Sector:{1}.", i + 1, cellSector, row);
//                        foreach (IndividualCell singleCell in allCells)
//                        {
//                            if (singleCell.cellSector != cellSector & singleCell.row == rowNumber & !singleCell.valueKnown)
//                            {
//                                singleCell.possibleValues[i] = false;
//                                valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
//                                //if (valueFound) { break; }
//                            }
//                        }
//                    }
//                    else if (colsAllSame)
//                    {
//                        //Console.WriteLine("Cols all same. Col: {2}, Number:{0}, Sector:{1}.", i + 1, cellSector, column);
//                        foreach (IndividualCell singleCell in allCells)
//                        {
//                            if (singleCell.cellSector != cellSector & singleCell.column == colNumber & !singleCell.valueKnown)
//                            {
//                                singleCell.possibleValues[i] = false;
//                                valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
//                                //if (valueFound) { break; }
//                            }
//                        }
//                    }
//                }
//                else
//                    throw new Exception("Mode should be 'row', 'col' or 'sec', none of these used");
//            }
//        }
//    }

//    return valueFound;
//}

//private static Dictionary<int, List<IndividualCell>> CreatePossibleDictionaries(List<IndividualCell> rowColOrSecList)
//{
//    Dictionary<int, List<IndividualCell>> possibleNumbersToCells = new Dictionary<int, List<IndividualCell>>();

//    //specificList for either row, col or sec
//    foreach (IndividualCell singleCell in rowColOrSecList)
//    {
//        if (!singleCell.valueKnown)
//        {
//            //bool[] possibles = singleCell.possibleValues;
//            for (int i = 0; i < 9; i++)
//            {
//                if (singleCell.possibleValues[i] == true)
//                {
//                    if (!possibleNumbersToCells.ContainsKey(i))
//                        possibleNumbersToCells.Add(i, new List<IndividualCell>());

//                    possibleNumbersToCells[i].Add(singleCell);
//                }
//            }
//        }

//    }
//    return possibleNumbersToCells;
//}

//private bool CheckIfSinglePossibleValueRemains(List<IndividualCell> allCells)
//{
//    if (valueKnown)
//        throw new Exception("Should not be called if value known...");

//    int count = 0;
//    int foundIndex = -1;
//    for (int i = 0; i < 9; i++)
//    {
//        if (possibleValues[i] == true)
//        {
//            count++;
//            foundIndex = i;
//        }
//    }
//    if (count == 1)
//    {
//        SetValue(foundIndex + 1, allCells);
//        return true;
//    }
//    else
//    {
//        return false;
//    }
//}

//public void SetValue(int localValue, List<IndividualCell> allCells)
//{
//    //check valid again!
//    foreach (IndividualCell singleCell in allCells)
//    {
//        if (singleCell.row == row || singleCell.column == column || singleCell.cellSector == cellSector)
//        {
//            if (singleCell.cellValue == localValue)
//                throw new Exception("Value " +  localValue +
//                                    " already exists (row " + row +
//                                    ", column " + column + ")");
//        }
//    }

//    SetValue(localValue);

//    foreach (IndividualCell singleCell in allCells)
//    {
//        if (!singleCell.valueKnown)
//        {
//            if (singleCell.row == row || singleCell.column == column || singleCell.cellSector == cellSector)
//            {
//                singleCell.possibleValues[cellValue - 1] = false;
//                singleCell.CheckIfSinglePossibleValueRemains(allCells);
//            }
//        }
//    }
//}