﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Financial_statement_v1._2
{
    public partial class Main : Form
    {
        /*
         * Income / Expense:    Type;ID;name;cashflow
         * Asset / Liability:   Type;ID;name;cashflow;value
        */

        // TODO: unique ID's

        #region Variables

        // povezava
        private static Main instance = null;

        // ime datoteke
        private string file = "file.bin";

        // vsi elementi
        public List<Element> elements;

        // total of everything
        private double totalIncome = 0, totalExpenses = 0, passive = 0, badDebth = 0;

        // the selected ID to update or delete
        private string selectedID = null;

        #endregion

        #region Setup

        // konstruktor
        public Main()
        {
            InitializeComponent();
            instance = this;
            elements = new List<Element>();
        }

        // form load
        private void Main_Load(object sender, EventArgs e)
        {
            Update();
        }

        // povezava
        public static Main GetInstance()
        {
            return instance;
        }

        public List<Element> GetElements()
        {
            return this.elements;
        }

        #endregion

        #region Validators

        private bool ValidLine(string line)
        {
            string[] words = line.Split(';');

            if (words.Length != 4 && words.Length != 5)
                return false;

            if (!Config.ValidType(words[0]))
                return false;

            if (!double.TryParse(words[3], out _))
                return false;

            if (words.Length == 5)
                if (!double.TryParse(words[4], out _))
                    return false;

            return true;
        }

        // used in 'Add Flow'
        public bool ValidInfo(string ID, string flow, string name, decimal cashflow)
        {
            if (ID == "" || name == "")
                return false;

            if (!Config.IsFlow(flow))
                return false;

            return double.TryParse(cashflow.ToString(), out _);
        }

        // used in 'Add Balance'
        public bool ValidInfo(string ID, string flow, string name, decimal cashflow, decimal value)
        {
            if (ID == "" || name == "")
                return false;

            if (!Config.IsBalance(flow))
                return false;

            if (!double.TryParse(value.ToString(), out _))
                return false;

            return double.TryParse(cashflow.ToString(), out _);
        }

        #endregion

        #region File Handling 

        private void ReadFile()
        {
            try {
                StreamReader sr = new StreamReader(file);
                string line = "";
                while ((line = sr.ReadLine()) != null)
                    if (ValidLine(line))
                        ProcessLine(line);
                sr.Close();
            } catch (Exception) {

            }
        }

        public void WriteToFile(Element e)
        {
            if (Balance.ToString() == e.GetType().Name)
                using (StreamWriter sw = File.AppendText(file))
                    sw.WriteLine(e.GetBalance() + ";" + e.GetID() + ";" + e.GetName() + ";" + e.GetCashflow() + ";" + e.GetValue());
            else
                using (StreamWriter sw = File.AppendText(file))
                    sw.WriteLine(e.GetFlow() + ";" + e.GetID() + ";" + e.GetName() + ";" + e.GetCashflow());
        }

        public void DeleteElement(Element e)
        {
            File.Delete(file);

            foreach(Element element in elements)
                if(element.GetID() != e.GetID())
                    WriteToFile(element);

            Update();
        }

        #endregion

        #region Updates

        // clear all listboxes, reset values
        private void ClearAll()
        {
            lbIncome.Items.Clear();
            lbExpense.Items.Clear();
            lbAssets.Items.Clear();
            lbLiabilities.Items.Clear();

            elements = new List<Element>();
            totalIncome = totalExpenses = passive = badDebth = 0;
        }

        private int GetPassive()
        {
            if (totalExpenses == 0)
                return 0;

            double tmp = passive / totalExpenses * 100;
            if (tmp >= 100)
                return 100;
            return (int)tmp;
        }

        // updates total income text, passive ...
        private void UpdateInformation()
        {
            lblTotalIncome.Text = "Total Income: " + totalIncome + "€";
            lblTotalExpenses.Text = "Total Expenses: " +  totalExpenses + "€";
            lblPassive.Text = "Passiv: " + passive + "€ (" + GetPassive() + " %)";
            lblPayday.Text = "Payday: " + (totalIncome - totalExpenses) + "€";
        }

        // update all listboxes, total income, total expenses, passive income (+ %)
        public new void Update()
        {
            ClearAll();
            ReadFile();

            foreach (Element e in elements) { // loop through every valid element in read file
                string ID = e.GetID();
                string name = e.GetName();
                double cashflow = e.GetCashflow();

                if(e.GetType() == typeof(Balance)) {                    // if element is asset or liability
                    try {
                        double value = e.GetValue();
                        string output = ID + " " + name + " - " + value;
                        if(e.GetBalance() == Config.Balance.Asset) {    // element is asset
                            lbAssets.Items.Add(output);
                            lbIncome.Items.Add(ID + " " + name + " - " + cashflow);

                            totalIncome += cashflow;
                            passive += cashflow;
                        } else {                                        // is liability
                            lbLiabilities.Items.Add(output);
                            lbExpense.Items.Add(ID + " " + name + " - " + cashflow);

                            totalExpenses += cashflow;
                            badDebth += value;
                        }
                    } catch(NotImplementedException) {

                    }              
                } else {
                    string output = ID + " " + name + " - " + cashflow;
                    if (e.GetFlow() == Config.Flow.Income) {
                        lbIncome.Items.Add(output);

                        totalIncome += cashflow;
                    } else {
                        lbExpense.Items.Add(output);

                        totalExpenses += cashflow;
                    }
                }
            }

            UpdateInformation();
        }

        #endregion

        // Build the list with all elements
        private void ProcessLine(string line)
        {
            try {
                string[] words = line.Split(';');

                string ID = words[1];
                string name = words[2];
                string type = words[0];
                double cashflow = double.Parse(words[3]);

                if (words.Length == 4)
                    elements.Add(new Flow(ID, name, cashflow, Config.GetFlow(type)));
                else
                    elements.Add(new Balance(ID, name, cashflow, Config.GetBalance(type), double.Parse(words[4])));

            } catch(Exception) {
                Console.WriteLine("Error Proccessing Line");
            }
        }

        #region Button Clicks

        private void BtnAddFlow_Click(object sender, EventArgs e)
        {
            AddFlow window = new AddFlow();
            window.ShowDialog();
        }

        private void BtnAddBalance_Click(object sender, EventArgs e)
        {
            AddBalance window = new AddBalance();
            window.ShowDialog();
        }

        private Element FindID(string ID)
        {
            foreach (Element e in elements)
                if (ID == e.GetID())
                    return e;
            return null;
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if(selectedID != null) { 
                Element element = FindID(selectedID.ToString());

                if (element == null) {
                    MessageBox.Show("Error with element ID");
                    selectedID = null;
                } else {
                    if(Balance.ToString() == element.GetType().Name) {
                        EditBalance editBalance = new EditBalance(element);
                        editBalance.ShowDialog();
                    } else {
                        EditFlow editFlow = new EditFlow(element);
                        editFlow.ShowDialog();
                    }
                }

            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedID != null) {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this element? ", "Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(result == DialogResult.Yes) {
                    DeleteElement(FindID(selectedID));
                    selectedID = null;
                    DisableButtons();
                    Update();
                    MessageBox.Show("Element deleted successfully");
                }
            }
        }

        #endregion

        #region Selected Index Changed

        private void EnableButtons()
        {
            btnEdit.Enabled = true;
            btnDelete.Enabled = true;
        }

        private void DisableButtons()
        {
            btnEdit.Enabled = false;
            btnDelete.Enabled = false;
        }

        private string GetID(string line)
        {
            return line.Split(' ')[0];
        }

        private void LbIncome_SelectedIndexChanged(object sender, EventArgs e)
        {
            try {
                selectedID = GetID(lbIncome.SelectedItem.ToString());
                EnableButtons();
            } catch(Exception) {
                MessageBox.Show("Error");
                selectedID = null;
                DisableButtons();
            }
        }

        private void LbExpense_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedID = GetID(lbExpense.SelectedItem.ToString());
            EnableButtons();
        }

        private void LbAssets_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedID = GetID(lbAssets.SelectedItem.ToString());
            EnableButtons();
        }

        private void LbLiabilities_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedID = GetID(lbLiabilities.SelectedItem.ToString());
            EnableButtons();
        }

        #endregion

    }
}