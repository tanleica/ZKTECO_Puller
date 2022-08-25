using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using BioMetrixCore.Info;
using System.Configuration;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace BioMetrixCore
{

    [RunInstaller(true)]
    public partial class Master : Form
    {
        public ZkemClient objZkeeper;
        private readonly List<Machine> machines;

        private readonly List<ProgressBarItem> progressBars = new List<ProgressBarItem>();
        private readonly List<LabelItem> labels = new List<LabelItem>();

        public Master()
        {
            InitializeComponent();
            ShowStatusBar(string.Empty, true);
            try
            {
                //Crypto.EncryptConnectionString();
                machines = JsonConvert.DeserializeObject<List<Machine>>(File.ReadAllText(@"machines.json")).ToList();
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Your Events will reach here if implemented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="actionType"></param>
        private void RaiseDeviceEvent(object sender, string actionType)
        {
            switch (actionType)
            {
                case UniversalStatic.acx_Disconnect:
                    {
                        //ShowStatusBar("The device is switched off", true);
                        break;
                    }
                default:
                    break;
            }

        }

        public void ShowStatusBar(string message, bool type)
        {
            if (message.Trim() == string.Empty)
            {
                lblStatus.Visible = false;
                return;
            }

            lblStatus.Visible = true;
            lblStatus.Text = message;
            lblStatus.ForeColor = Color.White;

            if (type)
                lblStatus.BackColor = Color.FromArgb(79, 208, 154);
            else
                lblStatus.BackColor = Color.FromArgb(230, 112, 134);
        }

        private void pnlHeader_Paint(object sender, PaintEventArgs e)
        { UniversalStatic.DrawLineInFooter(pnlHeader, Color.FromArgb(204, 204, 204), 2); }

        private void PullDataToDbThread(string ipAddress, int port, int machineNumber, int Index)
        {
            try
            {

                Label label = labels[Index].Label;
                ProgressBar progressBar = progressBars[Index].ProgressBar;

                label.Invoke(new Action(() => label.Text = string.Format("Bắt đầu làm việc với máy {0} ({1})", machineNumber, ipAddress)));
                progressBar.Invoke(new Action(() => progressBar.SetState(1)));

                // STEP 1
                // check ip
                bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                if (!isValidIpA)
                {
                    label.Invoke(new Action(() => label.Text = string.Format("Địa chỉ IP của máy {0} ({1})không hợp lệ", machineNumber, ipAddress)));
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    return;
                }
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 1));

                // STEP 2
                // ping ip
                isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                if (!isValidIpA)
                {
                    label.Invoke(new Action(() => label.Text = string.Format("Không thể ping được tới máy {0} ({1}), vui lòng kiểm tra hạ tầng", machineNumber, ipAddress)));
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    return;
                }
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 2));

                // STEP 3
                // Connecting to the machine
                ZkemClient newZkeeper = new ZkemClient(RaiseDeviceEvent);
                bool IsTheDeviceConnected = newZkeeper.Connect_Net(ipAddress, port);
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 3));

                if (IsTheDeviceConnected)
                {
                    DeviceManipulator newManipulator = new DeviceManipulator();
                    string deviceInfo = newManipulator.FetchDeviceInfo(newZkeeper, machineNumber);
                    label.Invoke(new Action(() => label.Text = string.Format("Máy {0} ({1}) đã được kết nối. Tên máy: {2} !!", machineNumber, ipAddress, deviceInfo)));

                    // STEP 4
                    // Pulling data
                    label.Invoke(new Action(() => label.Text = string.Format("Đọc dữ liệu từ máy {0} ({1})", machineNumber, ipAddress)));
                    ICollection<MachineInfo> lstMachineInfo = newManipulator.GetLogData(newZkeeper, machineNumber);
                    progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 4));

                    if (lstMachineInfo != null && lstMachineInfo.Count > 0)
                    {
                        label.Invoke(new Action(() => label.Text = string.Format("Đọc được {0} dòng từ máy {1} ({2})", lstMachineInfo.Count, machineNumber, ipAddress)));

                        // STEP 5
                        // Preparing data
                        label.Invoke(new Action(() => label.Text = string.Format("Chuyển đổi dữ liệu cho máy {0} ({1})", machineNumber, ipAddress)));
                        List<LogData> list = new List<LogData>();
                        foreach (var item in lstMachineInfo)
                        {
                            list.Add(new LogData()
                            {
                                MachineNumber = machineNumber,
                                IndRegID = item.IndRegID,
                                DateTimeRecord = item.DateTimeRecord,
                                DateOnlyRecord = item.DateOnlyRecord,
                                TimeOnlyRecord = item.TimeOnlyRecord
                            });
                        }
                        progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 5));

                        ZKTECOEntities _db = new ZKTECOEntities();

                        // STEP 6
                        // Delete old data
                        label.Invoke(new Action(() => label.Text = string.Format("Xóa dữ liệu cũ từ CSDL của máy {0} ({1})", machineNumber, ipAddress)));
                        _db.Database.ExecuteSqlCommand("DELETE FROM [LogData] WHERE [MachineNumber] = " + machineNumber);
                        progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 6));

                        // STEP 7
                        // Push data to db
                        label.Invoke(new Action(() => label.Text = string.Format("Đẩy dữ liệu của máy {0} ({1}) tới máy chủ", machineNumber, ipAddress)));

                        _db.BulkInsert(list);

                        //_db.LogDatas.AddRange(list);
                        //_db.SaveChanges();
                        progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 8));
                    }
                    progressBar.Invoke(new Action(() => progressBar.Value = 100));
                    label.Invoke(new Action(() => label.Text = string.Format("Cập nhật dữ liệu thành công cho máy {0} ({1})", machineNumber, ipAddress)));

                }
                else
                {
                    label.Invoke(new Action(() => label.Text = string.Format("Không thể kết nối tới máy {0} ({1}). Có thể đây không phải là máy chấm công ZKTeco", machineNumber, ipAddress)));
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    return;
                }
            } catch (Exception ex)
            {
                File.AppendAllText(@"Log.txt", string.Format("{0}: {1}\n", DateTime.UtcNow, ex.Message));
            }
        }

        private void PullDataToDb_Click(object sender, EventArgs e)
        {

            progressBars.Clear();
            labels.Clear();

            // Delete all existing ProgressBars and related Labels
            List<Control> controlList = new List<Control>();
            foreach (var item in panel2.Controls)
            {
                controlList.Add((Control)item);
            }

            foreach (var item in controlList)
            {
                Console.WriteLine(item.GetType().ToString() + "\n");
                if (item.GetType().ToString() == "System.Windows.Forms.ProgressBar" || item.GetType().ToString() == "System.Windows.Forms.Label")
                {
                    panel2.Controls.Remove(item);
                }
            }

            int i = 0;
            foreach (var item in machines)
            {
                ProgressBar progressBar = new ProgressBar()
                {
                    Location = new Point(4, 53 * (i + 1)),
                    Width = panel2.Width - 8
                };
                Label label = new Label()
                {
                    Location = new Point(4, 53 * (i + 1) + 25),
                    Text = "Label " + i.ToString(),
                    Width = panel2.Width - 8
                };
                progressBars.Add(new ProgressBarItem()
                {
                    Index = i,
                    ProgressBar = progressBar
                });
                labels.Add(new LabelItem()
                {
                    Index = i,
                    Label = label
                });
                panel2.Controls.Add(progressBar);
                panel2.Controls.Add(label);
                i++;
            }

            i = 0;
            foreach (var item in machines)
            {
                if (i >= machines.Count) break;
                // When each thread starts, it is imposible to wait (like async javascript code)
                string ipAddress = machines[i].IpAddress;
                int port = machines[i].Port;
                int machineNumber = machines[i].MachineNumber;
                Console.WriteLine("i = " + i);

                Thread t = new Thread(() =>
                {
                    PullDataToDbThread(ipAddress, port, machineNumber, i);
                });
                t.IsBackground = true;
                t.Start();

                // Wait for thread starts before i changes
                Thread.Sleep(200);

                i++;
            }

        }

        private void Master_Load(object sender, EventArgs e)
        {
            lblHeader.Text += Constants.AppVersion;
        }

        private void btnCheckConfigSecurity_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection connstrings = config.ConnectionStrings;
            if (connstrings.SectionInformation.IsProtected)
            {
                MessageBox.Show("The App.config file is protected");
            } else
            {
                MessageBox.Show("The App.config file is not secured!!!");
            }
        }
    }
}
