using System;
//using System.Collections.Generic;
using System.Text;
using SCG = System.Collections.Generic;
//using C5;

using state = System.Int32;
using input = System.Char;
using C5;

namespace Voorbeeld
{
    class SubsetMachine
    {
        private static int num = 0;

        /// <summary>
        /// Subset machine that employs the powerset construction or subset construction algorithm.
        /// It creates a DFA that recognizes the same language as the given NFA.
        /// </summary>
        public static DFA SubsetConstruct(NFA nfa)
        {
            DFA dfa = new DFA();

            // Sets of NFA states which is represented by some DFA state
            Set<Set<state>> markedStates = new Set<Set<state>>();
            Set<Set<state>> unmarkedStates = new Set<Set<state>>();

            // Gives a number to each state in the DFA
            HashDictionary<Set<state>, state> dfaStateNum = new HashDictionary<Set<state>, state>();

            Set<state> nfaInitial = new Set<state>();
            nfaInitial.Add(nfa.initial);

            // Initially, EpsilonClosure(nfa.initial) is the only state in the DFAs states and it's unmarked.
            Set<state> first = EpsilonClosure(nfa, nfaInitial);
            unmarkedStates.Add(first);

            // The initial dfa state
            state dfaInitial = GenNewState();
            dfaStateNum[first] = dfaInitial;
            dfa.start = dfaInitial;

            while (unmarkedStates.Count != 0)
            {
                // Takes out one unmarked state and posteriorly mark it.
                Set<state> aState = unmarkedStates.Choose();

                // Removes from the unmarked set.
                unmarkedStates.Remove(aState);

                // Inserts into the marked set.
                markedStates.Add(aState);
                

                // If this state contains the NFA's final state, add it to the DFA's set of
                // final states.
                if (aState.Contains(nfa.final))
                    dfa.final.Add(dfaStateNum[aState]);

                SCG.IEnumerator<input> iE = nfa.inputs.GetEnumerator();

                // For each input symbol the nfa knows...
                while (iE.MoveNext())
                {
                    // Next state
                    Set<state> next = EpsilonClosure(nfa, nfa.Move(aState, iE.Current));
                    Console.WriteLine(next);
                    // If we haven't examined this state before, add it to the unmarkedStates and make up a new number for it.
                    if (!unmarkedStates.Contains(next) && !markedStates.Contains(next))
                    {
                        unmarkedStates.Add(next);
                        dfaStateNum.Add(next, GenNewState());
                    }

                    KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
                    transition.Key = dfaStateNum[aState];
                    transition.Value = iE.Current;

                    dfa.transTable[transition] = dfaStateNum[next];
                }
            }
            HopcroftDFA(dfa);
            return dfa;
        }

        /// <summary>
        /// Builds the Epsilon closure of states for the given NFA 
        /// </summary>
        /// <param name="nfa"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        static Set<state> EpsilonClosure(NFA nfa, Set<state> states)
        {
            // Push all states onto a stack
            SCG.Stack<state> uncheckedStack = new SCG.Stack<state>(states);

            // Initialize EpsilonClosure(states) to states
            Set<state> epsilonClosure = states;

            while (uncheckedStack.Count != 0)
            {
                // Pop state t, the top element, off the stack
                state t = uncheckedStack.Pop();

                int i = 0;

                // For each state u with an edge from t to u labeled Epsilon
                foreach (input input in nfa.transTable[t])
                {
                    if (input == (char)NFA.Constants.Epsilon)
                    {
                        state u = Array.IndexOf(nfa.transTable[t], input, i);

                        // If u is not already in epsilonClosure, add it and push it onto stack
                        if (!epsilonClosure.Contains(u))
                        {
                            epsilonClosure.Add(u);
                            uncheckedStack.Push(u);
                        }
                    }

                    i = i + 1;
                }
            }

            return epsilonClosure;
        }




        /// <summary>
        /// Creates unique state numbers for DFA states
        /// </summary>
        /// <returns></returns>
        private static state GenNewState()
        {
            return num++;
        }

        public static DFA HopcroftDFA(DFA dfa)
        {
            Set<KeyValuePair<state, input>> transitions = new Set<KeyValuePair<state, input>>();
            Set<KeyValuePair<state, input>> finalTransitions = new Set<KeyValuePair<state, input>>();
            Set<state> finalStates = new Set<state>();
            Set<state> otherStates = new Set<state>();
            SCG.List<input> alphabet = new SCG.List<input>();

            finalStates = dfa.final;

            foreach (KeyValuePair<state, input> transStates in dfa.transTable.Keys)
            {
                if (!finalStates.Contains(transStates.Key))
                {
                    otherStates.Add(transStates.Key);
                    transitions.Add(transStates);

                }
                else
                {
                    finalTransitions.Add(transStates);
                }


  
            }

            int startState = 0;

            HopcroftState hopcroftState = new HopcroftState();
            SCG.List<HopcroftState> hlist = new SCG.List<HopcroftState>();

            //create first step, splitting final and other states
            foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in dfa.transTable)
            {
                KeyValuePair<state, input> transition = new KeyValuePair<state, input>();


                if (kvp.Key.Key != startState)
                {
                    startState++;
                    hlist.Add(hopcroftState);
                    hopcroftState = new HopcroftState();
                }

                hopcroftState.state = kvp.Key.Key;

                transition.Key = kvp.Value;//toestand leid naar deze toestand
                transition.Value = kvp.Key.Value;//op input dit


                hopcroftState.transition.Add(transition);


                //Set<HopcroftState> singleToestandGroup = new Set<HopcroftState>();

                if (finalStates.Contains(kvp.Key.Key))
                {
                    //hopcroftState.groupedState.ad
                    hopcroftState.group = 1;

                }
                else
                {
                    hopcroftState.group = 0;
                }


                if (!alphabet.Contains(kvp.Key.Value))
                {
                    alphabet.Add(kvp.Key.Value);
                }

            }

            //laatste nog toevoegen aan de lijst
            hlist.Add(hopcroftState);

            SCG.List<SCG.List<HopcroftState>> toestanden = new SCG.List<SCG.List<HopcroftState>>();


            SCG.List<HopcroftState> groep0 = new SCG.List<HopcroftState>();
            SCG.List<HopcroftState> groep1 = new SCG.List<HopcroftState>();
            //groeperen van de 2 toestanden
            foreach (HopcroftState hopcroft in hlist)
            {
                if (hopcroft.group == 0)
                {
                    groep0.Add(hopcroft);
                }
                else if (hopcroft.group == 1)
                {
                    groep1.Add(hopcroft);
                }
            }
            toestanden.Add(groep0);
            toestanden.Add(groep1);




            Set<KeyValuePair<state, input>> markedStates = new Set<KeyValuePair<state, input>>();
            Set<KeyValuePair<state, input>> unmarkedStates = new Set<KeyValuePair<state, input>>();
            




            SCG.List<HopcroftState> groepNew = new SCG.List<HopcroftState>();
            //kijken of er nieuwe toestanden gemaakt moeten worden
            foreach (SCG.List<HopcroftState> toestand in toestanden)
            {
                unmarkedStates = new Set<KeyValuePair<state, input>>();

                foreach (HopcroftState hopcroft in toestand)
                {

                    foreach (KeyValuePair<state, input> item in hopcroft.transition)
                    {
                        unmarkedStates.Add(item);
                    }
                }
                //nu heb je lijst van alle transitions van 1 groep
                //kijken of alle transitions zelfde zijn. zo niet, maak een nieuwe groep en voeg die toe.
                for (int i = 0; i < alphabet.Count; i++)
                {

                }








                
            }

        









                

            
            //transtable, alles met q1 naar a en b zelfde als q2 a en b zelfde is 1 state, als een otherstates naar een final state gaat wordt t nieuwe state
            //













            int intitalStatesCount = 1;//final states en other states
            int finalStatesCount = 1;

            DFA hopcroftDfa = new DFA();

            hopcroftDfa.start = dfa.start;

            //Set<state> finalStates = new Set<state>();
            //Set<state> otherStates = new Set<state>();


            //KeyValuePair<state, input> 

            HashDictionary<Set<state>, state> dfaStateNum = new HashDictionary<Set<state>, state>();

            //Set<KeyValuePair<state, input>> transitions = new Set<KeyValuePair<state, input>>();
            //Set<KeyValuePair<state, input>> finalTransitions = new Set<KeyValuePair<state, input>>();
            finalStates = dfa.final;


            SCG.SortedList<KeyValuePair<state, input>, state> finalTransitions2 = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());
            SCG.SortedList<KeyValuePair<state, input>, state> initialTransitions2 = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());

            // SCG.SortedList<KeyValuePair<state, input>, state> hopcroftTransTable = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());
            int endingstate = -1;
            foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in dfa.transTable)
            {
                KeyValuePair<state, input> transition = new KeyValuePair<state, input>();

                if (finalStates.Contains(kvp.Key.Key))
                {
                    //KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
                    transition.Key = kvp.Key.Key;
                    transition.Value = kvp.Key.Value;
                    //endingstate = 1;
                    int endingState = kvp.Value;

                    finalTransitions2.Add(transition, endingState);

                }
                else
                {
                    transition.Key = kvp.Key.Key;
                    transition.Value = kvp.Key.Value;
                    //endingstate = 0;
                    int endingState = kvp.Value;

                    initialTransitions2.Add(transition, endingState);
                }

            }

            Console.WriteLine("groep 1\n");
            foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in initialTransitions2)
                Console.WriteLine("Trans[{0}, {1}] = {2}\n", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            
            Console.WriteLine("groep 2\n");
            foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in finalTransitions2)
                Console.WriteLine("Trans[{0}, {1}] = {2}\n", kvp.Key.Key, kvp.Key.Value, kvp.Value);








                foreach (KeyValuePair<state, input> transStates in dfa.transTable.Keys)
            {
                if (!finalStates.Contains(transStates.Key))
                {
                    otherStates.Add(transStates.Key);
                    transitions.Add(transStates);

                }
                else
                {
                    finalTransitions.Add(transStates);
                }


            }

            foreach (KeyValuePair<state, input> transStates in finalTransitions)
            {

            }









            SCG.SortedList<KeyValuePair<state, input>, state> newTransTable = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());//dfa.transTable;
            newTransTable = dfa.transTable;


            foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in newTransTable)
            {
                KeyValuePair<state, input> transition = new KeyValuePair<state, input>();

                if (finalStates.Contains(kvp.Value))
                {
                    //KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
                    transition.Key = kvp.Key.Key;
                    transition.Value = kvp.Key.Value;

                    //finalStatesCount++;

                    hopcroftDfa.transTable.Add(transition, 1);

                    //Set<state> next = new Set<state>();
                    //next.Add(dfa.transTable.TryGetValue(kvp.Key.Key, out int toestand));
                    //kvp.key.key add new num

                    //dfaStateNum.Add(next, intitalStatesCount);
                }
                else if (finalStates.Contains(kvp.Key.Key))
                {


                    transition.Key = kvp.Key.Key;
                    transition.Value = kvp.Key.Value;

                    hopcroftDfa.transTable.Add(transition, 1);
                }
                else
                {
                    transition.Key = kvp.Key.Key;
                    transition.Value = kvp.Key.Value;

                    hopcroftDfa.transTable.Add(transition, 0);
                }

            }
            int x = 0;




            //public static DFA HopcroftDFA(DFA dfa)
            //{
            //    //transtable, alles met q1 naar a en b zelfde als q2 a en b zelfde is 1 state, als een otherstates naar een final state gaat wordt t nieuwe state
            //    //

            //    int intitalStatesCount = 1;//final states en other states
            //    int finalStatesCount = 1;

            //    DFA hopcroftDfa = new DFA();

            //    hopcroftDfa.start = dfa.start;

            //    Set<state> finalStates = new Set<state>();
            //    Set<state> otherStates = new Set<state>();


            //    //KeyValuePair<state, input> 

            //    HashDictionary<Set<state>, state> dfaStateNum = new HashDictionary<Set<state>, state>();

            //    Set<KeyValuePair<state, input>> transitions = new Set<KeyValuePair<state, input>>();
            //    Set<KeyValuePair<state, input>> finalTransitions = new Set<KeyValuePair<state, input>>();
            //    finalStates = dfa.final;


            //   // SCG.SortedList<KeyValuePair<state, input>, state> hopcroftTransTable = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());





            //    foreach (KeyValuePair<state, input> transStates in dfa.transTable.Keys)
            //    {
            //        if (!finalStates.Contains(transStates.Key))
            //        {
            //            otherStates.Add(transStates.Key);
            //            transitions.Add(transStates);

            //        }
            //        else
            //        {
            //            finalTransitions.Add(transStates);
            //        }


            //    }
            //    SCG.SortedList<KeyValuePair<state, input>, state> newTransTable = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());//dfa.transTable;
            //    newTransTable = dfa.transTable;


            //    foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in newTransTable)
            //    {
            //        KeyValuePair<state, input> transition = new KeyValuePair<state, input>();

            //        if (finalStates.Contains(kvp.Value))
            //        {
            //            //KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            finalStatesCount++;

            //            hopcroftDfa.transTable.Add(transition, finalStatesCount);

            //            //Set<state> next = new Set<state>();
            //            //next.Add(dfa.transTable.TryGetValue(kvp.Key.Key, out int toestand));
            //            //kvp.key.key add new num

            //            //dfaStateNum.Add(next, intitalStatesCount);
            //        }
            //        else if (finalStates.Contains(kvp.Key.Key))
            //        {


            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            hopcroftDfa.transTable.Add(transition, 1);
            //        }
            //        else
            //        {
            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            hopcroftDfa.transTable.Add(transition, 0);
            //        }

            //    }
            //    int x = 0;























            //    bool notDone = true;
            ////int finalAmountPartition = finalStatesCount;

            //while (notDone)
            //{


            //    intitalStatesCount = finalStatesCount;


            //    //copy list when second time entering
            //    //if (hopcroftDfa.transTable.Count !=0)
            //    //{
            //    //    newTransTable = new SCG.SortedList<KeyValuePair<state, input>, state>(new Comparer());
            //    //    //newTransTable.Clear();
            //    //    //for (int i = 0; i < hopcroftDfa.transTable.Count; i++)
            //    //    //{
            //    //    //    KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
            //    //    //    newTransTable.Add(hopcroftDfa.transTable.);
            //    //    //    hopcroftDfa.transTable.RemoveAt(i);
            //    //    //}

            //    //    newTransTable = hopcroftDfa.transTable;
            //    //    hopcroftDfa.transTable.Clear();
            //    //}






            //    //hopcroftDfa.transTable.Clear();
            //    foreach (SCG.KeyValuePair<KeyValuePair<state, input>, state> kvp in newTransTable)
            //    {
            //        KeyValuePair<state, input> transition = new KeyValuePair<state, input>();

            //        if (finalStates.Contains(kvp.Value))
            //        {
            //            //KeyValuePair<state, input> transition = new KeyValuePair<state, input>();
            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            finalStatesCount++;

            //            hopcroftDfa.transTable.Add(transition, finalStatesCount);

            //            //Set<state> next = new Set<state>();
            //            //next.Add(dfa.transTable.TryGetValue(kvp.Key.Key, out int toestand));
            //            //kvp.key.key add new num

            //            //dfaStateNum.Add(next, intitalStatesCount);
            //        }
            //        else if (finalStates.Contains(kvp.Key.Key))
            //        {


            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            hopcroftDfa.transTable.Add(transition, 1);
            //        }
            //        else
            //        {
            //            transition.Key = kvp.Key.Key;
            //            transition.Value = kvp.Key.Value;

            //            hopcroftDfa.transTable.Add(transition, 0);
            //        }


            //        //als  2 leid naar een 0 uit final. wordt een nieuwe state
            //        Console.Write("Trans[{0}, {1}] = {2}\n", kvp.Key.Key, kvp.Key.Value, kvp.Value);
            //    }

            //    //int newToestanden = finalStatesCount;





            //    foreach (KeyValuePair<state, input> transStates in dfa.transTable.Keys)
            //    {
            //        if (!finalStates.Contains(transStates.Key))
            //        {
            //            otherStates.Add(transStates.Key);
            //            transitions.Add(transStates);

            //        }
            //        else
            //        {
            //            finalTransitions.Add(transStates);
            //        }


            //    }
            //    //newTransTable = hopcroftDfa.transTable;
            //    if (intitalStatesCount == finalStatesCount)
            //    {
            //        notDone = false;
            //    }



            //final states, andere state.
            //als andere naar final verwijst, nieuwe state. fout
            //als alle andere states, is state 0, final state is state 1, als een andere state verwijst naar finale state, doe ++
            //





            //}


            //    foreach (KeyValuePair<state, input> transState in transitions)
            //    { 
            //    //als key = 0 en key = 2 op waarde a en waarde b naar zelfde dfa.transtable.value leid, is dit 1 state
            //    //zo niet, wordt dit een nieuwe state
            //    }





            //}

            //otherStates = dfa.transTable.Keys;

            //foreach (state transitionState1 in otherStates)
            //{
            //    dfa.transTable.TryGetValue()
            //}



            return hopcroftDfa;
        }

        //GevaarTypesItems_DBIndex.TryGetValue(kvp.Value, out string text);
        //public static DFA ReverseDFA(DFA dfa)
        //{
        //    //6 en 3 begintoestand
        //    DFA ndfaReverse = new DFA();


        //Set<Set<state>> markedStates = new Set<Set<state>>();
        //Set<Set<state>> unmarkedStates = new Set<Set<state>>();



        //    // Sets of NFA states which is represented by some DFA state
        //    Set<Set<state>> markedStates = new Set<Set<state>>();
        //    Set<Set<state>> unmarkedStates = new Set<Set<state>>();

        //    // Gives a number to each state in the DFA
        //    HashDictionary<Set<state>, state> dfaStateNum = new HashDictionary<Set<state>, state>();

        //    Set<state> nfaInitial = new Set<state>();
        //    nfaInitial.Add(dfa.start);

        //    // Initially, EpsilonClosure(nfa.initial) is the only state in the DFAs states and it's unmarked.
        //    Set<state> first = EpsilonClosure(ndfaReverse, nfaInitial);
        //    unmarkedStates.Add(first);

        //    // The initial dfa state
        //    state dfaInitial = GenNewState();
        //    dfaStateNum[first] = dfaInitial;
        //    dfa.start = dfaInitial;




        //    return ndfaReverse;

        //    state initial = dfa.;
        //    state final = dfa.final;
        //    int size;
        //    Inputs this NFA responds to
        //    SortedArray<input> inputs;
        //    input[][] transTable;
        //}



        //minimaliseer2(dfa)
        //= toDFA(reverse (toDFA (reverse (dfa)))
        //toDFA::NDFA -> DFA
        // deze methode moet je toch al maken!
        // let erop dat je de slimme aanpak implementeert
        // zodat je alleen bereikbare toestanden krijgt.
        //reverse :: DFA -> NDFA
        // deze methode kost 10 minuten:
        // - pijlen omdraaien
        // - eindtoestanden worden begintoestanden
        // - begintoestanden worden eindtoestanden


    }
}
