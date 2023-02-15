using BioMetrixCore.Info;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BioMetrixCore
{
    public partial class Machines : Form
    {

        private List<Machine> machines;
        public Machines()
        {
            InitializeComponent();
            machines = JsonConvert.DeserializeObject<List<Machine>>(File.ReadAllText(@"machines.json")).ToList();

            foreach (var machine in machines)
            {
                machine.Password = SimpleScripter.decode(machine.Password);
                if (machine.LastTime != "") machine.LastTime = SimpleScripter.decode(machine.LastTime);
            }

            BindToGridView(machines);
        }

        private void ClearGrid()
        {
            if (dgvMachines.Controls.Count > 2)
            { dgvMachines.Controls.RemoveAt(2); }


            dgvMachines.DataSource = null;
            dgvMachines.Controls.Clear();
            dgvMachines.Rows.Clear();
            dgvMachines.Columns.Clear();
        }
        private void BindToGridView(object list)
        {
            ClearGrid();
            dgvMachines.DataSource = list;
            dgvMachines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            UniversalStatic.ChangeGridProperties(dgvMachines, false);
        }

        private void dgvMachines_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 6 && e.Value != null)
            {
                e.Value = new String('*', e.Value.ToString().Length);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            List<Machine> newMachines = dgvMachines.DataSource as List<Machine>;
            foreach (var machine in newMachines)
            {
                machine.Password = SimpleScripter.encode(machine.Password);
                if (machine.LastTime == null) machine.LastTime = "";
                if (machine.LastTime != "") machine.LastTime = SimpleScripter.encode(machine.LastTime);
            }
            machines = newMachines;
            string json = JsonConvert.SerializeObject(newMachines, Formatting.Indented);
            File.WriteAllText("machines.json", json);
            this.Close();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            var newList = dgvMachines.DataSource as List<Machine>;
            newList.Add(new Machine()
            {
                MachineNumber = 0,
                IpAddress = "0.0.0.0",
                Port = 4370,
                TenantCode = "",
                UserName = "",
                Password = ""
            });
            BindToGridView(newList);
        }

        private void btnDeleteSelection_Click(object sender, EventArgs e)
        {
            int selectedRowCount = dgvMachines.Rows.GetRowCount(DataGridViewElementStates.Selected);
            if (selectedRowCount > 0)
            {
                int[] selectedIndexes = new int[selectedRowCount];
                for (int i = 0; i < selectedRowCount; i++)
                {
                    selectedIndexes[i] = dgvMachines.SelectedRows[i].Index;
                }

                var newList1 = dgvMachines.DataSource as List<Machine>;
                var newList2 = new List<Machine>();


                for (int i = 0; i < newList1.Count; i++)
                {
                    if (!selectedIndexes.Contains(i)) newList2.Add(newList1[i]);
                }

                BindToGridView(newList2);

            }
        }

        private void dgvMachines_MultiSelectChanged(object sender, EventArgs e)
        {
            int selectedRowCount = dgvMachines.Rows.GetRowCount(DataGridViewElementStates.Selected);
            btnDeleteSelection.Enabled = selectedRowCount > 0;
        }
    }
}
