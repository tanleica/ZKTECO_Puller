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
using Timer = System.Windows.Forms.Timer;
using ProgressBar = System.Windows.Forms.ProgressBar;
using Label = System.Windows.Forms.Label;

namespace BioMetrixCore
{

    [RunInstaller(true)]
    public partial class Master : Form
    {
        private readonly List<Machine> machines;
        private readonly List<ProgressBarItem> progressBars = new List<ProgressBarItem>();
        private readonly List<LabelItem> labels = new List<LabelItem>();
        private readonly List<int> threadColors = new List<int>();
        private static object _lockObject = new object();
        private static int autoRepeatTimer;
        private static int count = 0;

        public Master()
        {
            InitializeComponent();
            ShowStatusBar(string.Empty, true);
            try
            {
                //Crypto.EncryptConnectionString();
                machines = JsonConvert.DeserializeObject<List<Machine>>(File.ReadAllText(@"machines.json")).ToList();
                string autoRepeatTimerStr = ConfigurationManager.AppSettings["autoRepeatTimer"];
                autoRepeatTimer = autoRepeatTimerStr == null ? 15000 : (int)Convert.ToDecimal(autoRepeatTimerStr);
            }
            catch (Exception ex)
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

        private ThreadResult PullDataToDbThread(CancellationToken ct, string ipAddress, int port, int machineNumber, int index)
        {
            Label label = labels[index].Label;
            ProgressBar progressBar = progressBars[index].ProgressBar;

            while (!ct.IsCancellationRequested)
            {
                try
                {

                    label.Invoke(new Action(() => label.Text = string.Format("Bắt đầu làm việc với máy {0} ({1})", machineNumber, ipAddress)));
                    progressBar.Invoke(new Action(() => progressBar.SetState(threadColors[index])));

                    // STEP 1
                    // check ip
                    bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                    if (!isValidIpA)
                    {
                        label.Invoke(new Action(() => label.Text = string.Format("Địa chỉ IP của máy {0} ({1})không hợp lệ", machineNumber, ipAddress)));
                        progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                        Thread.Sleep(2000);
                        continue;
                    }
                    progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 1));

                    // STEP 2
                    // ping ip
                    isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                    if (!isValidIpA)
                    {
                        label.Invoke(new Action(() => label.Text = string.Format("Không thể ping được tới máy {0} ({1}), vui lòng kiểm tra hạ tầng", machineNumber, ipAddress)));
                        progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                        Thread.Sleep(2000);
                        continue;
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
                        return new ThreadResult(machines[index], true);
                    }
                    else
                    {
                        label.Invoke(new Action(() => label.Text = string.Format("Không thể kết nối tới máy {0} ({1}). Có thể đây không phải là máy chấm công ZKTeco", machineNumber, ipAddress)));
                        progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                        Thread.Sleep(2000);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    lock (_lockObject)
                    {
                        File.AppendAllText(@"Log.txt", string.Format("{0}: {1}\n", DateTime.UtcNow, ex.Message));
                    }
                    continue;
                }
            }
            label.Invoke(new Action(() => label.Text = string.Format("Thời gian chờ máy {0} ({1}) đã hết.", machineNumber, ipAddress)));
            progressBar.Invoke(new Action(() => progressBar.SetState(2)));
            Thread.Sleep(2000);
            return new ThreadResult(machines[index], false, true);
        }


        private void PullDataToDb_Click(object sender, EventArgs e)
        {

            progressBars.Clear();
            labels.Clear();
            threadColors.Clear();
            btnSettings.Focus();
            btnPullDataToDb.Enabled = false;

            Label labelTimer = new Label();
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();


            // Delete all existing ProgressBars and related Labels
            List<Control> controlList = new List<Control>();
            foreach (var item in panel2.Controls)
            {
                controlList.Add((Control)item);
            }

            foreach (var item in controlList)
            {
                // Console.WriteLine(item.GetType().ToString() + "\n");
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
                    Width = panel2.Width - 26
                };
                Label label = new Label()
                {
                    Location = new Point(4, 53 * (i + 1) + 25),
                    Text = "Label " + i.ToString(),
                    Width = panel2.Width - 26
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
                threadColors.Add(1); // default color is Green
                i++;
            }
            //this.Height = panel2.Top + (53 + 25) * i;

            i = 0;
            foreach (var item in machines)
            {
                // When each thread starts, it is imposible to wait (like async javascript code)
                string ipAddress = machines[i].IpAddress;
                int port = machines[i].Port;
                int machineNumber = machines[i].MachineNumber;

                StartThreat(machines[i], i);

                // tanleica: Wait for current thread starts before i changes
                Thread.Sleep(100);
                i++;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ++count;
            btnPullDataToDb.Text = count.ToString();
        }

        private void DoThread(object obj)
        {
            bool autoRepeat = ConfigurationManager.AppSettings["autoRepeatWhenFails"] == "True";
            ThreadParameters threadParameters = (ThreadParameters)obj;
            CancellationTokenSource cts = threadParameters.CancellationTokenSource;
            CancellationToken ct = threadParameters.CancellationToken;
            Machine machine = threadParameters.Machine;
            int i = threadParameters.Index;
            Timer timer = threadParameters.Timer;

            ThreadResult threadResult = PullDataToDbThread(ct, machine.IpAddress, machine.Port, machine.MachineNumber, i);
            if (threadResult.IsSuccess)
            {
                // Free the timer if current thread has done
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                cts.Dispose();
                lock (_lockObject)
                {
                    File.AppendAllText(@"Log.txt", string.Format($"SUCCESS! Timer for {threadResult.Machine.MachineNumber} ({threadResult.Machine.IpAddress}) was disposed!"));
                }
            }
            else
            {
                // tanleica: Using percusive method to run a new thread
                if (autoRepeat)
                {
                    threadColors[i] = 3;
                    Random rnd = new Random();
                    Thread.Sleep(rnd.Next(2, 5) * 1000);
                    StartThreat(machine, i);
                }
            }
        }

        private void StartThreat(Machine machine, int i)
        {
            bool autoRepeat = ConfigurationManager.AppSettings["autoRepeatWhenFails"] == "True";
            CancellationTokenSource cts = new CancellationTokenSource();
            Thread t = new Thread(new ParameterizedThreadStart(DoThread));
            t.Name = "MachineNumber: " + machine.MachineNumber;
            t.IsBackground = false;
            Timer timer = SetTimer(t, cts);
            ThreadParameters threadParameters = new ThreadParameters(cts, machine, i, timer);
            timer.Start();
            t.Start(threadParameters);
        }

        private static Timer SetTimer(Thread t, CancellationTokenSource cts)
        {
            // Create a timer with a two second interval.
            Timer timer = new Timer();
            // Hook up the Elapsed event for the timer. 
            timer.Interval = autoRepeatTimer;
            // We can tag an object to the timer
            timer.Tag = new ThreadStartInfo(t, cts);
            timer.Tick += new EventHandler(TimerEventProcessor);
            return timer;
        }

        // This is the method to run when the timer is raised   .
        private static void TimerEventProcessor(Object sender, EventArgs myEventArgs)
        {
            // Get the tag of the timer using Reflection
            System.Reflection.PropertyInfo pi = sender.GetType().GetProperty("Tag");
            ThreadStartInfo tsi = (ThreadStartInfo)(pi.GetValue(sender, null));

            lock (_lockObject)
            {
                File.AppendAllText(@"Log.txt", $"Thread {tsi.Thread.Name} is being aborted!\n");
            }
            // token will raised in the delagate ("PullDataToDbThread")
            tsi.CancellationTokenSource.Cancel();
            Timer timer = (Timer)sender;
            timer.Stop();
            timer.Dispose();
            tsi.CancellationTokenSource.Dispose();
        }

        private void Master_Load(object sender, EventArgs e)
        {
            lblHeader.Text += Constants.AppVersion;
            string autoStart = ConfigurationManager.AppSettings["autoStart"];
            if (autoStart == "True")
            {
                PullDataToDb_Click(sender, e);
            }
        }

        private void btnCheckConfigSecurity_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection connstrings = config.ConnectionStrings;
            if (connstrings.SectionInformation.IsProtected)
            {
                MessageBox.Show("The App.config file is protected");
            }
            else
            {
                MessageBox.Show("The App.config file is not secured!!!");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void Master_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void Master_Resize(object sender, EventArgs e)
        {
            int newWidth = panel2.Width - 26;
            foreach (var item in panel2.Controls)
            {
                // Console.WriteLine(item.GetType().ToString() + "\n");
                if (item.GetType().ToString() == "System.Windows.Forms.ProgressBar")
                {
                    ProgressBar progressBar = (ProgressBar)item;
                    progressBar.Width = newWidth;
                }
                else if (item.GetType().ToString() == "System.Windows.Forms.Label")
                {
                    Label label = (Label)item;
                    label.Width = newWidth;
                }
            }
        }

        private static void StaticMethod(object obj)
        {
            CancellationToken ct = (CancellationToken)obj;
            Console.WriteLine("AHA !!! StaticMethod is running on another thread.");

            // Simulate work that can be canceled.
            while (!ct.IsCancellationRequested)
            {
                Thread.Sleep(500);
                Console.WriteLine("AHA !!! StaticMethod is running on another thread.");
            }
            Console.WriteLine("The worker thread has been canceled!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // The Simple class controls access to the token source.
            CancellationTokenSource cts = new CancellationTokenSource();
            Thread t2 = new Thread(new ParameterizedThreadStart(StaticMethod));
            // Start the worker thread and pass it the token.
            Timer timer = new Timer();
            timer.Interval = 5000;
            timer.Tag = new ThreadStartInfo(t2, cts);
            timer.Tick += Timer_Tick1;
            timer.Start();
            t2.Start(cts.Token);
        }

        private void Timer_Tick1(object sender, EventArgs e)
        {
            // Get the tag of the timer using Reflection
            System.Reflection.PropertyInfo pi = sender.GetType().GetProperty("Tag");
            ThreadStartInfo tsi = (ThreadStartInfo)(pi.GetValue(sender, null));
            tsi.CancellationTokenSource.Cancel();
            Timer timer = (Timer)sender;
            timer.Stop();
            tsi.CancellationTokenSource.Dispose();
        }
    }
}
