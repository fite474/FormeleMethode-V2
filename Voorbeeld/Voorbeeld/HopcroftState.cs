using System;
using SCG = System.Collections.Generic;
using System.Text;
using C5;
using state = System.Int32;
using input = System.Char;

namespace Voorbeeld
{
    class HopcroftState
    {
        public SCG.List<KeyValuePair<state, input>> transition { set; get; }


        public int state;




        //public state transitionTo { set; get; }
        public int group;
        //public SCG.List<SCG.List<KeyValuePair<state, input>>> groupedState;



        public HopcroftState()
        {
            //transTable = inputTranstable;
            transition = new SCG.List<KeyValuePair<state, input>>();
            //groupedState = new SCG.List<SCG.List<KeyValuePair<state, input>>>();
        }


    }
}
