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
using System.Net.Http;
using Timer = System.Windows.Forms.Timer;
using ProgressBar = System.Windows.Forms.ProgressBar;
using Label = System.Windows.Forms.Label;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace BioMetrixCore
{

    [RunInstaller(true)]
    public partial class Master : Form
    {

        static readonly HttpClient client = new HttpClient();

        private List<ThreadInfo> threadInfos = new List<ThreadInfo>();

        private List<Machine> machines;
        private readonly List<ProgressBarItem> progressBars = new List<ProgressBarItem>();
        private readonly List<LabelItem> labels = new List<LabelItem>();
        //private readonly List<LabelItem> timerLabels = new List<LabelItem>();
        private readonly List<int> threadColors = new List<int>();
        private static Master frm;
        private static object _lockObject = new object();
        private static int autoRepeatTimer;
        private readonly bool autoRepeat;
        const int counterWidth = 0 /*80*/;

        // For catching error
        private string encodedLastTime;
        private string decodedLastTime;
        //======================================

        private readonly string apiLoginUrl; // == null? "http://core.vn:82/api/Authen/SignInPortalHR" : apiLoginUrl;
        private readonly string apiPostUrl; // == null ? "http://core.vn:82/api/hr/TimeSheetMonthly/ImportSwipeMachine" : apiPostUrl;


        public Master()
        {
            InitializeComponent();
            ShowStatusBar(string.Empty, true);
            try
            {
                frm = this;
                machines = JsonConvert.DeserializeObject<List<Machine>>(File.ReadAllText(@"machines.json")).ToList();

                foreach (Machine machine in machines)
                {
                    threadInfos.Add(new ThreadInfo(machine, false));
                }

                string autoRepeatTimerStr = ConfigurationManager.AppSettings["autoRepeatTimer"];
                autoRepeatTimer = autoRepeatTimerStr == null ? 150000 : (int)Convert.ToDecimal(autoRepeatTimerStr);
                autoRepeat = ConfigurationManager.AppSettings["autoRepeatWhenFails"] == "True";

                string apiLoginUrlCfg = ConfigurationManager.AppSettings["apiLoginUrl"];
                apiLoginUrl = apiLoginUrlCfg == null ? "http://core.vn:82/api/Authen/SignInPortalHR" : SimpleScripter.decode(apiLoginUrlCfg);
                string apiPostUrlCfg = ConfigurationManager.AppSettings["apiPostUrl"];
                apiPostUrl = apiPostUrlCfg == null ? "http://core.vn:82/api/hr/TimeSheetMonthly/ImportSwipeMachine" : SimpleScripter.decode(apiPostUrlCfg);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                WriteLog(ex.Message);
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

        private void WriteLog(string message)
        {
            lock (_lockObject)
            {
                File.AppendAllText(@"Log.txt", string.Format("{0}: {1}\n", DateTime.UtcNow, message));
            }

        }

        private async Task<ThreadResult> PullDataToDbThread(ThreadParameters threadParameters)
        {
            Machine machine = threadParameters.Machine;
            string ipAddress = machine.IpAddress;
            int port = machine.Port;
            int machineNumber = machine.MachineNumber;
            int index = threadParameters.Index;
            DateTime maxTime = new DateTime(1900, 1, 1, 0, 0, 0);
            Label label = labels[index].Label;
            //Label timerLabel = timerLabels[index].Label;
            ProgressBar progressBar = progressBars[index].ProgressBar;

            // if you want to imitate a long thread
            // Thread.Sleep(600000);
            string sDate = string.Empty;
            try
            {

                label.Invoke(new Action(() => label.Text = string.Format("Bắt đầu làm việc với máy {0} ({1})", machineNumber, ipAddress)));
                progressBar.Invoke(new Action(() => progressBar.SetState(threadColors[index])));

                // STEP 1
                // check ip
                // check ip
                bool isValidIpA = UniversalStatic.ValidateIP(ipAddress);
                if (!isValidIpA)
                {
                    string message = string.Format("Địa chỉ IP của máy {0} ({1})không hợp lệ", machineNumber, ipAddress);
                    WriteLog(message);
                    label.Invoke(new Action(() => label.Text = message));
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    // timerLabel.Invoke(new Action(() => timerLabel.Text = ""));
                    Thread.Sleep(2000);
                    return new ThreadResult(machines[index], false);
                }
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 1));

                // STEP 2
                // ping ip
                isValidIpA = UniversalStatic.PingTheDevice(ipAddress);
                if (!isValidIpA)
                {
                    string message = string.Format("Không thể ping được tới máy {0} ({1}), vui lòng kiểm tra hạ tầng", machineNumber, ipAddress);
                    label.Invoke(new Action(() => label.Text = message));
                    WriteLog(message);
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    // timerLabel.Invoke(new Action(() => timerLabel.Text = ""));
                    Thread.Sleep(2000);
                    return new ThreadResult(machines[index], false);
                }
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 2));

                // STEP 3
                // Connecting to the machine
                ZkemClient newZkeeper = new ZkemClient(RaiseDeviceEvent);
                label.Invoke(new Action(() => label.Text = string.Format("Kết nối tới máy {0} ({1})...", machineNumber, ipAddress)));
                bool IsTheDeviceConnected = newZkeeper.Connect_Net(ipAddress, port);
                progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 3));

                if (IsTheDeviceConnected)
                {
                    DeviceManipulator newManipulator = new DeviceManipulator();
                    string deviceInfo = newManipulator.FetchDeviceInfo(newZkeeper, machineNumber);
                    label.Invoke(new Action(() => label.Text = string.Format("Máy {0} ({1}) đã được kết nối. Tên máy: {2} !!", machineNumber, ipAddress, deviceInfo)));
                    Thread.Sleep(2000);

                    // STEP 4
                    // Pulling data
                    label.Invoke(new Action(() => label.Text = string.Format("Đọc dữ liệu từ máy {0} ({1})", machineNumber, ipAddress)));
                    WriteLog(string.Format("Bắt đầu đọc dữ liệu từ máy {0} ({1})", machineNumber, ipAddress));
                    
                    ReadResult readResult = null;
                    ICollection<MachineInfo> lstMachineInfo = null;
                    
                    try
                    {
                        if (machine.LastTime != "")
                        {
                            WriteLog(string.Format("Machine {0} LastTime is {1} ({2})", machine.MachineNumber, machine.LastTime, SimpleScripter.decode(machine.LastTime)));
                            encodedLastTime = machine.LastTime;
                            decodedLastTime = SimpleScripter.decode(encodedLastTime);

                            readResult = newManipulator.GetLogData(newZkeeper, machineNumber, DateTime.Parse(SimpleScripter.decode(machine.LastTime)));
                            lstMachineInfo = readResult.MachineInfos;

                        }
                        else
                        {
                            WriteLog(string.Format("Machine {0} 1-st time reading...", machine.MachineNumber));
                            readResult = newManipulator.GetLogData(newZkeeper, machineNumber);
                            lstMachineInfo = readResult.MachineInfos;
                        }
                        progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 4));
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message + " (reading failed)");
                        WriteLog("encodedLastTime: " + encodedLastTime);
                        WriteLog("decodedLastTime: " + decodedLastTime + " => Error");
                    }

                    if (readResult.Success)
                    {
                        if (lstMachineInfo != null && lstMachineInfo.Count > 0)
                        {
                            label.Invoke(new Action(() => label.Text = string.Format("Đọc được {0} dòng từ máy {1} ({2})", lstMachineInfo.Count, machineNumber, ipAddress)));

                            Thread.Sleep(1000);

                            // STEP 5
                            // Preparing data
                            label.Invoke(new Action(() => label.Text = string.Format("Chuyển đổi dữ liệu cho máy {0} ({1})", machineNumber, ipAddress)));

                            List<LogData> list = new List<LogData>();
                            sDate = lstMachineInfo.ElementAt(0).DateTimeRecord;
                            try
                            {
                                foreach (var item in lstMachineInfo)
                                {
                                    DateTime newTime = DateTime.Parse(item.DateTimeRecord);
                                    if (newTime > maxTime)
                                    {
                                        maxTime = newTime;
                                    }

                                    list.Add(new LogData()
                                    {
                                        MachineNumber = machineNumber,
                                        IndRegID = item.IndRegID,
                                        DateTimeRecord = item.DateTimeRecord,
                                        DateTimeOriginal = newTime,
                                        DateOnlyRecord = item.DateOnlyRecord,
                                        TimeOnlyRecord = item.TimeOnlyRecord
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message);
                            }
                            progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 5));

                            // ZKTECOEntities _db = new ZKTECOEntities();

                            // STEP 6
                            // Delete old data
                            // label.Invoke(new Action(() => label.Text = string.Format("Xóa dữ liệu cũ từ CSDL của máy {0} ({1})", machineNumber, ipAddress)));
                            label.Invoke(new Action(() => label.Text = string.Format("Lấy mã đăng nhập cho máy {0} ({1})", machineNumber, ipAddress)));
                            //_db.Database.ExecuteSqlCommand("DELETE FROM [LogData] WHERE [MachineNumber] = " + machineNumber);
                            progressBar.Invoke(new Action(() => progressBar.Value = 100 / 8 * 6));
                            var authResponse = JsonConvert.DeserializeObject<LoginResonseCDS>(await GetToken(machine));

                            WriteLog("authResponse.statusCode: " + authResponse.statusCode);

                            if (authResponse.statusCode != "200")
                            {
                                string message = string.Format("Đăng nhập từ máy {0} ({1}) đến server thất bại", machineNumber, ipAddress);
                                label.Invoke(new Action(() => label.Text = message));
                                progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                                WriteLog(message);
                                return new ThreadResult(machines[index], false);
                            }

                            var token = authResponse.data.token;

                            // STEP 7
                            // Push data to db
                            label.Invoke(new Action(() => label.Text = string.Format("Đẩy dữ liệu của máy {0} ({1}) tới máy chủ", machineNumber, ipAddress)));
                            WriteLog(string.Format("Đẩy dữ liệu của máy {0} ({1}) tới máy chủ", machineNumber, ipAddress));

                            //_db.BulkInsert(list);
                            var originalResponse = await PostApi(machine, list, token);
                            WriteLog(originalResponse);
                            var response = JsonConvert.DeserializeObject<GeneralResponse>(originalResponse);
                            if (response.statusCode == "200")
                            {
                                //_db.LogDatas.AddRange(list);
                                //_db.SaveChanges();
                                progressBar.Invoke(new Action(() => progressBar.Value = 100));
                                label.Invoke(new Action(() => label.Text = string.Format("Cập nhật dữ liệu thành công cho máy {0} ({1})", machineNumber, ipAddress)));
                                // timerLabel.Invoke(new Action(() => timerLabel.Text = ""));

                                /* Fix LastTime for current machine 
                                ==================================*/
                                lock (_lockObject)
                                {

                                    List<Machine> newMachines = machines;
                                    foreach (var x in newMachines)
                                    {
                                        if (x.MachineNumber == machine.MachineNumber && x.IpAddress == machine.IpAddress && x.Port == machine.Port)
                                        {
                                            x.LastTime = SimpleScripter.encode(maxTime.ToString());
                                        }
                                    }
                                    string json = JsonConvert.SerializeObject(newMachines, Formatting.Indented);
                                    machines = newMachines;
                                    File.WriteAllText("machines.json", json);
                                }
                                /*<============================== */

                                return new ThreadResult(machines[index], true);
                            }
                            else
                            {
                                string message = originalResponse;
                                label.Invoke(new Action(() => label.Text = string.Format("Không đẩy được dữ liệu từ máy {0} ({1}) đến server", machineNumber, ipAddress)));
                                progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                                WriteLog(message);
                                return new ThreadResult(machines[index], false);
                            }
                        }
                        else
                        {
                            label.Invoke(new Action(() => label.Text = string.Format("Máy {0} ({1}) không có dữ liệu hoặc dữ liệu mới", machineNumber, ipAddress)));
                            return new ThreadResult(machines[index], false);
                        }
                    } else
                    {
                        label.Invoke(new Action(() => label.Text = string.Format("Máy {0} ({1}) có lỗi dữ liệu bên trong", machineNumber, ipAddress)));
                        return new ThreadResult(machines[index], false);
                    }

                }
                else
                {
                    string message = string.Format("Không thể kết nối tới máy {0} ({1}). Có thể đây không phải là máy chấm công ZKTeco", machineNumber, ipAddress);
                    label.Invoke(new Action(() => label.Text = message));
                    WriteLog(message);
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    // timerLabel.Invoke(new Action(() => timerLabel.Text = ""));
                    Thread.Sleep(2000);
                    return new ThreadResult(machines[index], false);
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    Console.WriteLine($"ThreadAbortException raised for machine #{machineNumber} ({ipAddress})");
                    label.Invoke(new Action(() => label.Text = string.Format("Thời gian chờ máy {0} ({1}) đã hết.", machineNumber, ipAddress)));
                    //timerLabel.Invoke(new Action(() => timerLabel.Text = ""));
                    progressBar.Invoke(new Action(() => progressBar.SetState(2)));
                    Thread.Sleep(1000);
                    return new ThreadResult(machines[index], false, true);
                }

                var st = new StackTrace();
                var fr = st.GetFrame(0);
                var lines = fr.GetFileLineNumber().ToString();
                WriteLog(ex.Message + " (" + sDate + ')' + " inline: " + lines);


                label.Invoke(new Action(() => label.Text = string.Format("Lỗi hệ thống {0} ({1})", machineNumber, ipAddress)));
                Thread.Sleep(5000);
                label.Invoke(new Action(() => label.Text = ex.Message));
                progressBar.Invoke(new Action(() => progressBar.SetState(2)));


                return new ThreadResult(machines[index], false);
            }
        }

        private string DateToApiString(string datetimeString)
        {
            DateTime dt = DateTime.Parse(datetimeString);
            int Y = dt.Year; int M = dt.Month; int D = dt.Day; int h = dt.Hour; int m = dt.Minute; int s = dt.Second;
            string r = String.Format("{0}/{1}/{2} {3}:{4}:{5}",
                D < 10 ? "0" + D : D.ToString(),
                M < 10 ? "0" + M : M.ToString(),
                Y,
                h < 10 ? "0" + h : h.ToString(),
                m < 10 ? "0" + m : m.ToString(),
                s < 10 ? "0" + s : s.ToString()
                );
            return r;
        }

        private async Task<string> GetToken(Machine machine)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                LoginRequestCDS loginRequestCDS = new LoginRequestCDS()
                {
                    tenantCode = machine.TenantCode,
                    userName = machine.UserName,
                    passWord = SimpleScripter.decode(machine.Password)
                };

                WriteLog("tenantCode " + machine.TenantCode);
                WriteLog("tenantCode " + machine.UserName);
                WriteLog("tenantCode " + SimpleScripter.decode(machine.Password));

                StringContent content = new StringContent(JsonConvert.SerializeObject(loginRequestCDS),
                Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiLoginUrl, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw new Exception(e.Message);
            }

        }

        private async Task<string> PostApi(Machine machine, List<LogData> list, string token)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {

                List<ApiImportItem> listForApi = new List<ApiImportItem>();
                foreach (LogData logData in list)
                {
                    listForApi.Add(new ApiImportItem()
                    {
                        code = logData.IndRegID.ToString(),
                        time = DateToApiString(logData.DateTimeRecord)
                    });
                }
                string jsonBody = JsonConvert.SerializeObject(listForApi);
                StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = await client.PostAsync(apiPostUrl, content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                WriteLog(e.Message);
                WriteLog(apiLoginUrl);
                WriteLog(apiPostUrl);
                return "{ statusCode: \"500\" }";
            }
        }

        private void PullDataToDb_Click(object sender, EventArgs e)
        {

            var free = (from x in threadInfos where x.IsRunning select x).Count() == 0;

            if (!free)
            {
                MessageBox.Show("Some threads are still running. Please wait");
                return;
            }

            PrepareThreadUI();

            int i = 0;
            foreach (var item in machines)
            {
                //if (i > 0) break;
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

        private void PrepareThreadUI()
        {
            progressBars.Clear();
            labels.Clear();
            //timerLabels.Clear();
            threadColors.Clear();
            btnSettings.Focus();
            //btnPullDataToDb.Enabled = false;

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
                    Location = new Point(4 + counterWidth, 53 * (i + 1)),
                    Width = panel2.Width - 26 - counterWidth
                };
                Label label = new Label()
                {
                    Location = new Point(4 + counterWidth, 53 * (i + 1) + 25),
                    Text = "Label " + i.ToString(),
                    Width = panel2.Width - 26 - counterWidth,
                    Tag = "inner"
                };
                Label labelCurrentTimer = new Label()
                {
                    Location = new Point(4, 53 * (i + 1) + 6),
                    Text = "0",
                    Width = counterWidth - 5,
                    Tag = "timer"
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
                /*
                frm.timerLabels.Add(new LabelItem()
                {
                    Index = i,
                    Label = labelCurrentTimer
                });
                */
                panel2.Controls.Add(labelCurrentTimer);
                panel2.Controls.Add(progressBar);
                panel2.Controls.Add(label);
                threadColors.Add(1); // default color is Green
                i++;
            }
        }

        private async void DoThread(object obj)
        {
            ThreadParameters threadParameters = (ThreadParameters)obj;
            CancellationTokenSource cts = threadParameters.CancellationTokenSource;
            CancellationToken ct = threadParameters.CancellationToken;
            Machine machine = threadParameters.Machine;
            int i = threadParameters.Index;

            threadInfos[i].IsRunning = true;
            var threadResult = await PullDataToDbThread(threadParameters);
            threadInfos[i].IsRunning = false;

            threadParameters.Timer.Stop();
            //threadParameters.CountingTimer.Stop();
            threadParameters.Timer.Dispose();
            //threadParameters.CountingTimer.Dispose();
            cts.Dispose();


            if (!threadResult.IsSuccess)
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
            else
            {
                WriteLog(string.Format($"SUCCESS! Timer for {threadResult.Machine.MachineNumber} ({threadResult.Machine.IpAddress}) was disposed!"));
            }

        }

        private void StartThreat(Machine machine, int i)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            Thread t = new Thread(new ParameterizedThreadStart(DoThread));
            t.Name = machine.MachineNumber.ToString();
            t.IsBackground = false;
            // Thread counting timer
            //Timer timer_ = SetCountingTimer(i, t, cts);
            // Cancellation timer
            Timer timer = SetTimer(i, t, cts /*, timer_ */);

            //Console.WriteLine("Counting Timer is running = " + timer_.Enabled.ToString());
            ThreadParameters threadParameters = new ThreadParameters(t, cts, machine, i, timer /*, timer_*/);
            token.Register(() =>
            {
                CancelThread(threadParameters);
                if (autoRepeat)
                {
                    Thread.Sleep(5000);
                    StartThreat(machine, i);
                }
            });
            timer.Start();
            // timer_.Start();
            t.Start(threadParameters);
        }

        private void CancelThread(object sender)
        {
            ThreadParameters threadParameters = (ThreadParameters)sender;
            Console.WriteLine($"Thread {threadParameters.Machine.MachineNumber} ({threadParameters.Machine.IpAddress}) is being aborted...");
            threadParameters.Thread.Abort();
            threadParameters.Timer.Stop();
            //threadParameters.CountingTimer.Stop();
            threadParameters.Timer.Dispose();
            //threadParameters.CountingTimer.Dispose();
            threadParameters.CancellationTokenSource.Dispose();
        }

        private static Timer SetTimer(int index, Thread t, CancellationTokenSource cts /*, Timer childTimer */)
        {
            // Create a timer to cancel the thread.
            Timer timer = new Timer
            {
                Interval = autoRepeatTimer,
                // We can tag an object to the timer
                Tag = new ThreadStartInfo(index, t, cts /*, childTimer */)
            };
            timer.Tick += new EventHandler(TimerEventProcessor);
            return timer;
        }

        /*
        private static Timer SetCountingTimer(int index, Thread t, CancellationTokenSource cts)
        {
            // Create a timer to cancel the thread.
            Timer timer = new Timer
            {
                Interval = 1000,
                // We can tag an object to the timer
                Tag = new ThreadStartInfo(index, t, cts, null)
            };
            timer.Tick += new EventHandler(CountingTimerEventProcessor);
            return timer;
        }
        */

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
            // token will raised in the delagate (".Register(() => ...)")
            tsi.CancellationTokenSource.Cancel();
        }

        /*
        private static void CountingTimerEventProcessor(Object sender, EventArgs myEventArgs)
        {
            // Get the tag of the timer using Reflection
            System.Reflection.PropertyInfo pi = sender.GetType().GetProperty("Tag");
            ThreadStartInfo tsi = (ThreadStartInfo)(pi.GetValue(sender, null));
            tsi.TimeCount++;
            TimeSpan time = TimeSpan.FromSeconds((double)tsi.TimeCount);
            string str = time.ToString(@"hh\:mm\:ss");
            frm.timerLabels[tsi.Index].Label.Text = str;
        }
        */

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
            int newWidth = panel2.Width - 26 - counterWidth;
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
                    if (label.Tag.ToString() != "timer") label.Width = newWidth;
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

        #region DEMO of an another aproach
        /*
        private void button1_Click(object sender, EventArgs e)
        {
            // The Simple class controls access to the token source.
            CancellationTokenSource cts = new CancellationTokenSource();
            Thread t2 = new Thread(new ParameterizedThreadStart(StaticMethod));
            // Start the worker thread and pass it the token.
            Timer timer = new Timer();
            timer.Interval = 5000;
            timer.Tag = new ThreadStartInfo(0, t2, cts, null);
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
        */
        #endregion

        private void btnMachines_Click(object sender, EventArgs e)
        {
            Machines machines = new Machines();
            machines.ShowDialog();
        }
    }
}
