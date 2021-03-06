﻿using System;

namespace Financial_statement_v1._2
{
    public abstract class Element
    {
        protected int ID;
        protected string name;
        protected double cashflow;

        protected Config.Flow flow;

        private Random rnd;

        public Element(string name, double cashflow)
        {
            this.rnd = new Random();
            this.name = name;
            this.cashflow = cashflow;

            this.SetID();
        }

        #region Getters

        public int GetID()
        {
            return this.ID;
        }

        public string GetName()
        {
            return this.name;
        }

        public double GetCashflow()
        {
            return this.cashflow;
        }

        public Config.Flow GetFlow()
        {
            return this.flow;
        }

        public abstract Config.Balance GetBalance();

        public abstract double GetValue();

        #endregion

        #region Setters

        private void SetID()
        {
            this.ID = rnd.Next(1000) + 1;

            while (Main.GetInstance().ExistsID(ID))
                this.ID = rnd.Next(1000) + 1;
        }

        public void SetID(int ID)
        {
            this.ID = ID;
        }

        #endregion

    }
}
