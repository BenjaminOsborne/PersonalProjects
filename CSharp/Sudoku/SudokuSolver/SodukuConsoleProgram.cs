using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver
{
    public class SodukuConsoleProgram
    {
        static void Main()
        {
            try
            {
                InitialSetup setup;
                List<IndividualCell> cellList = InitialSetupCall(out setup);

                bool solutionFound = MainLogic(ref cellList);

                if(!solutionFound)
                    throw new Exception("Solution Not Found!");
                
                bool knownSolution = false;
                if (knownSolution)
                {
                    int[,] initialValues = setup.GetInitialValues();
                    UtilityLogic.CompareToSolution(cellList, initialValues);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static List<IndividualCell> InitialSetupCall(out InitialSetup setup)
        {
            int mode = 4;
            setup = new InitialSetup(mode);

            List<IndividualCell> cellList = setup.SetCellList();
            UtilityLogic.PrintOutGrid(cellList, "Initial Seeded Grid:");

            return cellList;
        }

        public static bool MainLogic(ref List<IndividualCell> cellList)
        {
            bool logicLevelOneSuccess = RemovePossibleOptionsUsingKnownCells(cellList);
            if (logicLevelOneSuccess)
                return true;
            
            bool logicLevelTwoSuccess = RemovePossibleOptionsUsingRowColumnAndSectorPatterns(cellList);
            if (logicLevelTwoSuccess)
                return true;
            
            bool logicLevelThreeSuccess = CreateHypotheticalGridsAndReRun(ref cellList);
            if (logicLevelThreeSuccess)
                return true;
            
            return false;
        }

        private static bool RemovePossibleOptionsUsingKnownCells(List<IndividualCell> cellList)
        {
            int count = 0;
            bool anyChangedOccured = true;
            bool lastCallChanged = false;

            //Determine 'possible values' arrays by removing values in line/sector. Determine any immediatly obvious cell values.
            while (anyChangedOccured)
            {
                anyChangedOccured = false;
                foreach (IndividualCell singleCell in cellList)
                {
                    if (singleCell.valueKnown == false)
                    {
                        lastCallChanged = UtilityLogic.RemovePossibleValuesThroughKnownValues(singleCell, cellList);
                        if (lastCallChanged) { anyChangedOccured = true; }
                    }
                }
                count++;
            }
            Console.WriteLine("Number of iterations required on first stage logic (based just on values): {0}\n", count);

            if (!UtilityLogic.FinalSolutionFound(cellList))
            {
                UtilityLogic.PrintCurrentPossibilities(cellList);
                return false;
            }
            else
            {
                UtilityLogic.PrintOutGrid(cellList, "Final Solution Found:");
                return true;
            }
        }

        private static bool RemovePossibleOptionsUsingRowColumnAndSectorPatterns(List<IndividualCell> cellList)
        {
            int count = 0;
            bool anyChangedOccured = true;
            bool lastCallChanged = false;

            //while (count < 10)
            while (anyChangedOccured)
            {
                anyChangedOccured = false;
                foreach (IndividualCell singleCell in cellList)
                {
                    if (!singleCell.valueKnown)
                    {
                        lastCallChanged = UtilityLogic.RemovePossibleValuesThroughOtherPossibleValueLists(singleCell, cellList);
                        if (lastCallChanged) { anyChangedOccured = true; }
                    }
                }
                count++;
            }
            Console.WriteLine("Number of iterations required on second stage logic: {0}\n", count);

            if (!UtilityLogic.FinalSolutionFound(cellList))
            {
                UtilityLogic.PrintCurrentPossibilities(cellList);
                return false;
            }
            else
            {
                UtilityLogic.PrintOutGrid(cellList, "Final Solution Found:");
                return true;
            }
        }

        private static bool CreateHypotheticalGridsAndReRun(ref List<IndividualCell> cellList)
        {
            bool finalSolutionFound = false;
            List<List<IndividualCell>> ListOfGridClones = UtilityLogic.GetCompleteListOfGridClones(ref cellList, ref finalSolutionFound);
            
            if (ListOfGridClones.Count < 1)
                throw new Exception("List of GridClones should contain at least one object.");

            if (finalSolutionFound)
                return true;

            foreach (List<IndividualCell> cellListCloneLoop in ListOfGridClones)
            {
                List<IndividualCell> cellListClone = cellListCloneLoop;
                finalSolutionFound = MainLogic(ref cellListClone);
                if (finalSolutionFound)
                {
                    cellList = cellListClone;
                    return true;
                }
            }
            
            return false;
        }

    }
}

#region Test references in lists

    ////test - make list A of objects, make list B containing some of list A. If alter object in A, does it alter same object in B?
    //List<IndividualCell> testList = new List<IndividualCell>();
    //testList.Add(new IndividualCell(2, 2, 1,1,1));
    //testList.Add(new IndividualCell(3, 3, 1, 2, 1));
    //testList.Add(new IndividualCell(4, 4, 1, 3, 1));

    //List<IndividualCell> testList2 = new List<IndividualCell>();
    //testList2.Add(testList.ElementAt(2));
    //testList2.Add(testList.ElementAt(1));

    //Console.WriteLine("List 1, element 3 val: {0}", testList.ElementAt(2).cellValue);
    //Console.WriteLine("List 2, element 1 val: {0}", testList.ElementAt(2).cellValue);

    //testList.ElementAt(2).cellValue++;
    //Console.WriteLine("CHANGE");

    //Console.WriteLine("List 1, element 3 val: {0}", testList.ElementAt(2).cellValue);
    //Console.WriteLine("List 2, element 1 val: {0}", testList.ElementAt(2).cellValue);


#endregion
