using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
    public class UtilityLogic
    {
        //First Stage Logic
        public static bool RemovePossibleValuesThroughKnownValues(IndividualCell cell, List<IndividualCell> allCells)
        {
            //no need to find if current cell as if current cell has a cellValue!=0, this won't be called!
            foreach (IndividualCell singleCell in allCells)
            {
                if (singleCell.row == cell.row || singleCell.column == cell.column || singleCell.cellSector == cell.cellSector)
                {
                    if (singleCell.valueKnown)
                    {
                        cell.possibleValues[singleCell.cellValue - 1] = false;
                    }
                }
            }

            return UtilityLogic.CheckIfSinglePossibleValueRemains(cell, allCells);
        }


        //Second Stage Logic
        public static bool RemovePossibleValuesThroughOtherPossibleValueLists(IndividualCell cell, List<IndividualCell> allCells)
        {
            bool valueFound = false;
            //create list for row, col and sector (not inc own cell)
            List<IndividualCell> rowList = new List<IndividualCell>();
            List<IndividualCell> colList = new List<IndividualCell>();
            List<IndividualCell> secList = new List<IndividualCell>();

            foreach (IndividualCell singleCell in allCells)
            {
                if (singleCell.row == cell.row)
                    rowList.Add(singleCell);
                if (singleCell.column == cell.column)
                    colList.Add(singleCell);
                if (singleCell.cellSector == cell.cellSector)
                    secList.Add(singleCell);
            }

            //Check that all lists are 9 elements long...
            if (rowList.Count != 9 || colList.Count != 9 || secList.Count != 9)
                throw new Exception("Row, col or sec list should be 9 elements long, one (or more) is not");

            //xxxPossibles will have a list for each number (index 0 to 8) of possible cells to take each value...
            Dictionary<int, List<IndividualCell>> rowPossibles = CreatePossibleDictionaries(rowList);
            Dictionary<int, List<IndividualCell>> colPossibles = CreatePossibleDictionaries(colList);
            Dictionary<int, List<IndividualCell>> secPossibles = CreatePossibleDictionaries(secList);

            //if any number (1 to 9, indexed by 0 to 9 in dict) only found once, then set that cell to have that value
            //if any number found only in 1 sector, remove possibles from other cells in that sector
            valueFound = ProcessPossibleDictionaries(cell, allCells, rowPossibles, "row");
            if (!valueFound)
                valueFound = UtilityLogic.ProcessPossibleDictionaries(cell, allCells, colPossibles, "col");
            if (!valueFound)
                valueFound = UtilityLogic.ProcessPossibleDictionaries(cell, allCells, secPossibles, "sec");

            //if any number only possible within one sector, then remove that number as possible from every other cell in that sectors possible bool[]

            return valueFound;
        }

        private static Dictionary<int, List<IndividualCell>> CreatePossibleDictionaries(List<IndividualCell> rowColOrSecList)
        {
            Dictionary<int, List<IndividualCell>> possibleNumbersToCells = new Dictionary<int, List<IndividualCell>>();

            //specificList for either row, col or sec
            foreach (IndividualCell singleCell in rowColOrSecList)
            {
                if (!singleCell.valueKnown)
                {
                    //bool[] possibles = singleCell.possibleValues;
                    for (int i = 0; i < 9; i++)
                    {
                        if (singleCell.possibleValues[i] == true)
                        {
                            if (!possibleNumbersToCells.ContainsKey(i))
                                possibleNumbersToCells.Add(i, new List<IndividualCell>());

                            possibleNumbersToCells[i].Add(singleCell);
                        }
                    }
                }

            }
            return possibleNumbersToCells;
        }

        private static bool ProcessPossibleDictionaries(IndividualCell cell, List<IndividualCell> allCells, Dictionary<int, List<IndividualCell>> rowColOrSecPossibles, string mode)
        {
            bool valueFound = false;

            for (int i = 0; i < 9; i++) //loop over numbers 1 to 9 in dict...
            {
                if (valueFound)
                    break; //must break as changes in values make existing dicts/lists invalid.

                if (rowColOrSecPossibles.ContainsKey(i)) //only for numbers which aren't already known
                {
                    if (rowColOrSecPossibles[i].Count == 1) //if only one cell, this cell must contain the number (i+1)
                    {
                        UtilityLogic.SetValue(rowColOrSecPossibles[i].ElementAt(0), i + 1, allCells);
                        //rowColOrSecPossibles[i].ElementAt(0).SetValue(i + 1, allCells);
                        valueFound = true;
                        //break;
                    }

                    else
                    {
                        if (mode == "row" || mode == "col")
                        {
                            int sectorNumber = rowColOrSecPossibles[i].ElementAt(0).cellSector;
                            bool sectorsAllSame = true;

                            foreach (IndividualCell singleCell in rowColOrSecPossibles[i])
                            {
                                if (singleCell.cellSector != sectorNumber)
                                    sectorsAllSame = false;
                            }
                            if (sectorsAllSame) //if any number only possible in one sector, remove this from possibles[] in other cells in this sector
                            {
                                if (mode == "row")
                                {
                                    //Console.WriteLine("Sectors all same. Row: {2}, Number:{0}, Sector:{1}.", i + 1, sectorNumber, row);
                                    foreach (IndividualCell singleCell in allCells)
                                    {
                                        if (singleCell.row != cell.row & singleCell.cellSector == sectorNumber & !singleCell.valueKnown)
                                        {
                                            singleCell.possibleValues[i] = false;
                                            valueFound = UtilityLogic.CheckIfSinglePossibleValueRemains(singleCell, allCells);
                                            //valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
                                            //if (valueFound) { break; }
                                        }
                                    }
                                }
                                if (mode == "col")
                                {
                                    //Console.WriteLine("Sectors all same. Col: {2}, Number:{0}, Sector:{1}.", i + 1, sectorNumber, column);
                                    foreach (IndividualCell singleCell in allCells)
                                    {
                                        if (singleCell.column != cell.column & singleCell.cellSector == sectorNumber & !singleCell.valueKnown)
                                        {
                                            singleCell.possibleValues[i] = false;
                                            valueFound = UtilityLogic.CheckIfSinglePossibleValueRemains(singleCell, allCells);
                                            //valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
                                            //if (valueFound) { break; }
                                        }
                                    }
                                }
                            }
                        }
                        else if (mode == "sec")
                        {
                            int rowNumber = rowColOrSecPossibles[i].ElementAt(0).row;
                            int colNumber = rowColOrSecPossibles[i].ElementAt(0).column;
                            bool rowsAllSame = true;
                            bool colsAllSame = true;

                            foreach (IndividualCell singleCell in rowColOrSecPossibles[i])
                            {
                                if (singleCell.row != rowNumber)
                                    rowsAllSame = false;
                                if (singleCell.column != colNumber)
                                    colsAllSame = false;
                            }
                            if (rowsAllSame) //if any number only possible in one row, remove this from possibles[] in other cells in this row
                            {
                                //Console.WriteLine("Rows all same. Row: {2}, Number:{0}, Sector:{1}.", i + 1, cellSector, row);
                                foreach (IndividualCell singleCell in allCells)
                                {
                                    if (singleCell.cellSector != cell.cellSector & singleCell.row == rowNumber & !singleCell.valueKnown)
                                    {
                                        singleCell.possibleValues[i] = false;
                                        valueFound = UtilityLogic.CheckIfSinglePossibleValueRemains(singleCell, allCells);
                                        //valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
                                        //if (valueFound) { break; }
                                    }
                                }
                            }
                            else if (colsAllSame)
                            {
                                //Console.WriteLine("Cols all same. Col: {2}, Number:{0}, Sector:{1}.", i + 1, cellSector, column);
                                foreach (IndividualCell singleCell in allCells)
                                {
                                    if (singleCell.cellSector != cell.cellSector & singleCell.column == colNumber & !singleCell.valueKnown)
                                    {
                                        singleCell.possibleValues[i] = false;
                                        valueFound = UtilityLogic.CheckIfSinglePossibleValueRemains(singleCell, allCells);
                                        //valueFound = singleCell.CheckIfSinglePossibleValueRemains(allCells);
                                        //if (valueFound) { break; }
                                    }
                                }
                            }
                        }
                        else
                            throw new Exception("Mode should be 'row', 'col' or 'sec', none of these used");
                    }
                }
            }

            return valueFound;
        }


        //Thrid Stage Logic
        internal static List<List<IndividualCell>> GetCompleteListOfGridClones(ref List<IndividualCell> cellList, ref bool finalSolutionFound)
        {
            List<List<IndividualCell>> ListOfGridClones = new List<List<IndividualCell>>();
            
            foreach (IndividualCell singleCell in cellList)
            {
                if (!singleCell.valueKnown)
                {
                    int count = 1;
                    foreach (bool possibleValue in singleCell.possibleValues)
                    {
                        if (possibleValue)
                        {
                            //create new grid;
                            List<IndividualCell> cellListClone = UtilityLogic.GetClone(cellList);

                            //find the same cellClone as singleCell
                            foreach (IndividualCell singleCellClone in cellListClone)
                            {
                                if (singleCell.row == singleCellClone.row && singleCell.column == singleCellClone.column)
                                {
                                    try
                                    {
                                        UtilityLogic.SetValue(singleCellClone, count, cellListClone);
                                        finalSolutionFound = UtilityLogic.FinalSolutionFound(cellListClone);
                                        if (finalSolutionFound)
                                        {
                                            cellList = cellListClone;
                                            ListOfGridClones.Add(cellListClone);
                                            UtilityLogic.PrintOutGrid(ListOfGridClones.Last(), "Final Solution Found:");
                                            return ListOfGridClones;
                                        }
                                    }
                                    catch (Exception Ex)
                                    {
                                        Console.WriteLine(  "Clone trial run exception:\n" +
                                                            Ex.Message + 
                                                            "For row: " + singleCellClone.row + 
                                                            ", column: " + singleCellClone.column + ".\n");
                                        break;
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
            }
            return ListOfGridClones;
        }

        public static List<IndividualCell> GetClone(List<IndividualCell> cellList)
        {
            List<IndividualCell> cellListClone = new List<IndividualCell>();

            foreach (IndividualCell singleCell in cellList)
            {
                cellListClone.Add(singleCell.GetClone());
            }

            return cellListClone;

        }

    #region SingleCell setting and checking

        public static void SetValue(IndividualCell cell, int localValue, List<IndividualCell> allCells)
        {
            //check valid again!
            foreach (IndividualCell singleCell in allCells)
            {
                if (singleCell.row == cell.row || singleCell.column == cell.column || singleCell.cellSector == cell.cellSector)
                {
                    if (singleCell.cellValue == localValue)
                        throw new Exception("Value " + localValue +
                                            " already exists (row " + cell.row +
                                            ", column " + cell.column + ")");
                }
            }

            cell.SetValue(localValue);

            foreach (IndividualCell singleCell in allCells)
            {
                if (!singleCell.valueKnown)
                {
                    if (singleCell.row == cell.row || singleCell.column == cell.column || singleCell.cellSector == cell.cellSector)
                    {
                        singleCell.possibleValues[cell.cellValue - 1] = false;
                        UtilityLogic.CheckIfSinglePossibleValueRemains(singleCell, allCells);
                    }
                }
            }
        }

        private static bool CheckIfSinglePossibleValueRemains(IndividualCell cell, List<IndividualCell> allCells)
        {
            if (cell.valueKnown)
                throw new Exception("Should not be called if value known...");

            int count = 0;
            int foundIndex = -1;
            for (int i = 0; i < 9; i++)
            {
                if (cell.possibleValues[i] == true)
                {
                    count++;
                    foundIndex = i;
                }
            }
            if (count == 1)
            {
                UtilityLogic.SetValue(cell, foundIndex + 1, allCells);
                //cell.SetValue(foundIndex + 1, allCells);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool FinalSolutionFound(List<IndividualCell> allCells)
        {
            foreach (IndividualCell singleCell in allCells)
            {
                if (!singleCell.valueKnown)
                    return false;
            }
            return true;
        }

    #endregion

    #region Console Printing

        public static void PrintCurrentPossibilities(List<IndividualCell> cellList)
        {
            int currentRow = 1;
            foreach (IndividualCell singleCell in cellList)
            {
                if (singleCell.row != currentRow)
                    Console.WriteLine();

                Console.Write("{0},{1}: ", singleCell.row, singleCell.column);
                for (int i = 0; i < 9; i++)
                //foreach (bool val in )
                {
                    if (singleCell.possibleValues[i])
                        Console.Write("{0} | ", i + 1);
                }
                Console.WriteLine();
                currentRow = singleCell.row;
            }
            Console.WriteLine();
        }

        public static void CompareToSolution(List<IndividualCell> cellList, int[,] initialValuesLocal)
        {
            bool solutionFound = true;

            List<IndividualCell> checkerList = new List<IndividualCell>();
            for (int i = 0; i < cellList.Count; i++) //(IndividualCell cell in cellList)
            {

                checkerList.Add(new IndividualCell(-1));
                checkerList.ElementAt(i).row = cellList.ElementAt(i).row;
                checkerList.ElementAt(i).column = cellList.ElementAt(i).column;

                if (initialValuesLocal[cellList.ElementAt(i).row - 1, cellList.ElementAt(i).column - 1] != 0)
                {
                    checkerList.ElementAt(i).cellValue = 0;
                }
                else if (cellList.ElementAt(i).cellValue == 0)
                {
                    checkerList.ElementAt(i).cellValue = 2;
                    solutionFound = false;
                }
                else if (cellList.ElementAt(i).knownSolutionValue == cellList.ElementAt(i).cellValue)
                {
                    checkerList.ElementAt(i).cellValue = 1;
                }
                else
                {
                    checkerList.ElementAt(i).cellValue = 8;
                    solutionFound = false;
                }
            }
            if (solutionFound)
                Console.WriteLine("Solution Found OK\n");
            else
                PrintOutGrid(checkerList, "Checker List (0: Initial Seed, 1: Found, 2: Still to find, 8: Invalid):");

        }

        public static void PrintOutGrid(List<IndividualCell> cellList, string message)
        {
            int _previousVal = 1;
            int count = 0;
            Console.WriteLine(message);
            foreach (IndividualCell cell in cellList)
            {
                if (cell.row != _previousVal)
                {
                    Console.WriteLine();
                    if (count == 2 || count == 5)
                        Console.WriteLine(" -------+-------+-------");
                    count++;
                }

                if (cell.column == 1 || cell.column == 4 || cell.column == 7)
                    Console.Write("| ");

                Console.Write(cell.cellValue);
                Console.Write(" ");

                if (cell.column == 9)
                    Console.Write("|");

                _previousVal = cell.row;

            }
            Console.WriteLine("\n\n");
        }

    #endregion



       
    }
}
