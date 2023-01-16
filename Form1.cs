using System;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.InteropServices;


//This namespace is used to work with WMI classes. For using this namespace add reference of System.Management.dll .using System.Collections;





namespace DarkDemo
{
    public partial class Form1 : Form
    {
        DirectoryInfo dInfo = new DirectoryInfo(@"C:/Windows/Temp");
        DirectoryInfo pInfo = new DirectoryInfo(@"C:/Windows/Prefetch");
        DirectoryInfo temp2 = new DirectoryInfo(@"C:/Users/" + Environment.UserName+ "/AppData/Local/Temp");
        DirectoryInfo downInfo = new DirectoryInfo(@"C:/Users/" + Environment.UserName + "/Downloads");

        Form2 f2 = new Form2();
        bool iscontinue = true;
        private static Org.Mentalis.Utilities.CpuUsage cpu;
        ManagementObjectSearcher tyui =
            new ManagementObjectSearcher(
                "SELECT * FROM Win32_Processor");
        public Form1()
        {     
        InitializeComponent();
        }

        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        static long CalculateDirectorySize(DirectoryInfo directory, bool includeSubdirectories)
        {
            long totalSize = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (FileInfo file in files)
            {
                totalSize += file.Length;
            }

            if (includeSubdirectories)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    totalSize += CalculateDirectorySize(dir, true);
                }
            }

            return totalSize;
        }

        private void GetInfo(ManagementObject mobj, string property_name)
        {
            object property_obj = mobj[property_name];

            ulong property_value = (ulong)property_obj / 1024 / 1024;

            label9.Text = property_value.ToString();


        }


        private void GpuInfo(ManagementObject mobj, string property_name)
        {
            object property_obj = mobj[property_name];

            string gpu = property_obj.ToString();
            label21.Text = gpu;


        }

        private void ShowPowerStatus()
        {
            PowerStatus status = SystemInformation.PowerStatus;            
            label3.Text = status.BatteryLifePercent.ToString("P0");
            label23.Text = status.BatteryChargeStatus.ToString();
            if (status.BatteryLifeRemaining == -1)
                label25.Text = "Unknown";
            else
                label25.Text = status.BatteryLifeRemaining.ToString();
        }

        private string machineName = System.Environment.MachineName;


        private void populateCPUInfo()
        {
            try
            {
                // Creates and returns a CpuUsage instance that can be used to query the CPU time on this operating system.
                cpu = Org.Mentalis.Utilities.CpuUsage.Create();

                /// Creating a New Thread 
                Thread thread = new Thread(new ThreadStart(delegate ()
                {
                    try
                    {
                        while (iscontinue)
                        {
                            //To Update The UI Thread we have to Invoke  it. 
                            this.Invoke(new System.Windows.Forms.MethodInvoker(delegate ()
                            {
                                int process = cpu.Query(); //Determines the current average CPU load.
                                label26.Text = process.ToString() + "%";

                                cpuUsageChart.Series[0].Points.AddY(process);//Add process to chart 

                                if (cpuUsageChart.Series[0].Points.Count > 40)//clear old data point after Thrad Sleep time * 40
                                    cpuUsageChart.Series[0].Points.RemoveAt(0);

                            }));

                            Thread.Sleep(450);//Thread sleep for 450 milliseconds 
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }));

                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;
                thread.Start();//Start the Thread
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (ManagementObject mobj in tyui.Get())
            {
                Double tempo = Convert.ToDouble(mobj["NumberOfEnabledCore"].ToString());
                comboBox1.Text = tempo.ToString();
            }

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            label50.Text = allDrives[0].DriveFormat.ToString();
            label51.Text = allDrives[1].DriveFormat.ToString();
            label52.Text = allDrives[2].DriveFormat.ToString();

            double kli = Convert.ToDouble(allDrives[0].TotalFreeSpace.ToString());
            double ty = kli / 1024 / 1024 / 1024;                                           //C spazio libero
            double jkl = Math.Round(ty, 0);

            double iuop = Convert.ToDouble(allDrives[1].TotalFreeSpace.ToString());
            double uryhg = iuop / 1024 / 1024 / 1024;                                           
            double yttgf = Math.Round(uryhg, 0);

            double fhhfg = Convert.ToDouble(allDrives[2].TotalFreeSpace.ToString());
            double fhjf = fhhfg / 1024 / 1024 / 1024;                                           
            double fhhryf = Math.Round(fhjf, 0);



            double hjk = Convert.ToDouble(allDrives[0].TotalSize.ToString());
            double a = hjk / 1024 / 1024 / 1024;
            double h = Math.Round(a, 0);                                                  //C spazio totale
            label53.Text = jkl.ToString() + "/"+ h.ToString() +" GB";


            double sdfsdf = Convert.ToDouble(allDrives[1].TotalSize.ToString());
            double dfsdf = sdfsdf / 1024 / 1024 / 1024;
            double sdfs = Math.Round(dfsdf, 0);                                                          
            label54.Text = yttgf.ToString() + "/" + sdfs.ToString() + " GB";


            double gdfgdf = Convert.ToDouble(allDrives[2].TotalSize.ToString());
            double dfgdf = gdfgdf / 1024 / 1024 / 1024;
            double dfgdfa = Math.Round(dfgdf, 0);                                                  
            label55.Text = fhhryf.ToString() + "/" + dfgdfa.ToString() + " GB";

            double disk_1 = dfgdfa - fhhryf;
            double disk_2 = sdfs - yttgf;
            double disk_3 = h - jkl;

            progressBar3.Maximum = (int)h;
            progressBar3.Value = (int)disk_3;

            progressBar4.Maximum = (int)sdfs;
            progressBar4.Value = (int)disk_2;

            progressBar5.Maximum = (int)dfgdfa;
            progressBar5.Value = (int)disk_1;


            string[] driv = Directory.GetLogicalDrives();           
            foreach (string item in driv)
            {
                listBox2.Items.Add(item);
            }

            string[] myListItemArray = new string[listBox2.Items.Count];
            
            listBox2.Items.CopyTo(myListItemArray, 0);
            String listResults = "";
            foreach (string myItem in myListItemArray)
            {
                listResults = listResults + myItem + "<br />";
            }
           label47.Text = myListItemArray[0] + "Windows";
            label48.Text = myListItemArray[1];
            label49.Text = myListItemArray[2];



            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                listBox1.Items.Add (theprocess.ProcessName);
            }

            //Chart Settings 

            // Populating the data arrays.
            this.cpuUsageChart.Series.Clear();
            this.cpuUsageChart.Palette = ChartColorPalette.SeaGreen;

            // Set chart title.
            this.cpuUsageChart.Titles.Add("CPU Usage");

            // Add chart series
            Series series = this.cpuUsageChart.Series.Add("Cpu Usage");
            
            

            // Add Initial Point as Zero.
            series.Points.Add(0);

            //Populating X Y Axis  Information 
            cpuUsageChart.Series[0].YAxisType = AxisType.Primary;
            cpuUsageChart.Series[0].YValueType = ChartValueType.Int32;
            cpuUsageChart.Series[0].IsXValueIndexed = false;

            cpuUsageChart.ResetAutoValues();
            cpuUsageChart.ChartAreas[0].AxisY.Maximum = 100;//Max Y 
            cpuUsageChart.ChartAreas[0].AxisY.Minimum = 0;
            cpuUsageChart.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
            cpuUsageChart.ChartAreas[0].AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            populateCPUInfo();
            

            ManagementObjectSearcher gpu_search =new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

            foreach (ManagementObject mobj in gpu_search.Get())
            {

                GpuInfo(mobj, "VideoProcessor");

            }
            string yu = ramCounter.NextValue().ToString();
            ManagementObjectSearcher os_searcher =
             new ManagementObjectSearcher(
                 "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject mobj in os_searcher.Get())
            {
                Double tempo = Convert.ToDouble(mobj["TotalVisibleMemorySize"].ToString());
                tempo = tempo / 1024 / 1024;
                
                double tempotest = Math.Round(tempo, 2);
                label9.Text = tempotest.ToString() + " GB";
               
            }


            RegistryKey Rkeyu = Registry.LocalMachine;
            Rkeyu = Rkeyu.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\BIOS");            
            label12.Text = (string)Rkeyu.GetValue("SystemProductName");
            

            String q2 = Environment.UserName;
            label14.Text = q2;

            int q4 = Environment.ProcessorCount;
            label6.Text = Convert.ToString(q4);


            label7.Text = System.Environment.OSVersion.ToString();

            RegistryKey Rkey = Registry.LocalMachine;
            Rkey = Rkey.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
            label8.Text = (string)Rkey.GetValue("ProcessorNameString");

            groupBox1.Visible = false;
            panel4.Visible = true;
            label57.Visible = false;
            label56.Visible = false;
            groupBox12.Visible = false;
            groupBox4.Visible = false;
            label27.Visible = false;
            pictureBox4.Visible = false;
            label16.Visible = false;
            label17.Visible = false;
            label32.Visible = false;
            progressBar1.Visible = false;
            groupBox5.Visible = false;
            timer1.Start();
            

        }

        static long DirectorySize(DirectoryInfo dInfo, bool includeSubDir)
        {
            long totalSize = dInfo.EnumerateFiles()
                         .Sum(file => file.Length);
            if (includeSubDir)
            {
                totalSize += dInfo.EnumerateDirectories()
                         .Sum(dir => DirectorySize(dir, true));
            }
            return totalSize;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            string yui = ramCounter.NextValue().ToString();                                    //
            ManagementObjectSearcher ram_get =
             new ManagementObjectSearcher(
                 "SELECT * FROM Win32_OperatingSystem");

            ManagementObjectCollection queryCollection = ram_get.Get();
            ManagementObject mol = queryCollection.OfType<ManagementObject>().FirstOrDefault();


            Double tempo_s = Convert.ToDouble(mol["TotalVisibleMemorySize"].ToString());
            int j = (int)tempo_s / 1024;
            int usedram = j - int.Parse(yui);
            progressBar2.Maximum = (int)tempo_s /1024;
            progressBar2.Value = usedram;
            label45.Text = usedram.ToString();
          

            ManagementObjectSearcher mos = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject mo in mos.Get())
            {
                Double temp = Convert.ToDouble(mo["CurrentTemperature"].ToString());
                temp = (temp - 2732) / 10.0;
                label29.Text = temp.ToString()+ "°C";

            }
            ShowPowerStatus();
            
            label19.Text = String.Format("{0} MB", ramCounter.NextValue());                   
            
        }

        private void button6_Click(object sender, EventArgs e)
        {          
            this.Close();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            panel2.Visible = false;
            groupBox12.Visible = true;
            groupBox1.Visible = true;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = true;
            groupBox5.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel2.Visible = true;
            groupBox12.Visible = false;
            panel4.Visible = true;
            groupBox1.Visible = false;
            groupBox2.Visible = true;
            groupBox3.Visible = true;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
        }
        
        private void button7_Click(object sender, EventArgs e)
        {
            long sizeOfDir = DirectorySize(dInfo, true) / 1024;
            long tempdata = DirectorySize(temp2, true) / 1024;
            long prefetch = DirectorySize(pInfo, true) / 1024;
            long all = sizeOfDir + tempdata;
            long download = DirectorySize(downInfo, true) / 1024;

            long removed_item = all + prefetch + download;

            f2.Show();
            f2.getTextHandler = f2.getValue;
            f2.getTextHandler(removed_item.ToString() + "Kb  has been removed!");

            button7.Enabled = false;
            button8.Enabled = false;
            pictureBox4.Visible = true;
            label27.Visible = true;
            emptyRecycleBin();
            Thread.Sleep(2000);
        }

        private void button8_Click(object sender, EventArgs e)
        {           
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            SHQUERYRBINFO bb_Query = new SHQUERYRBINFO();
            bb_Query.cbSize = Marshal.SizeOf(bb_Query.GetType());
            SHQueryRecycleBin(null, ref bb_Query);

            ulong ghyuj = bb_Query.i64Size;
            ulong jhkl = ghyuj / 1024;            

            progressBar1.Visible = true;
            label32.Visible = true;
            string percent;
            if (progressBar1.Value == 100)
            {
                
                long sizeOfDir = DirectorySize(dInfo, true) / 1024;
                long tempdata = DirectorySize(temp2, true) / 1024;
                long prefetch = DirectorySize(pInfo, true) / 1024;
                long all = sizeOfDir + tempdata;
                long download = DirectorySize(downInfo, true) / 1024;

                label57.Text = "(" + prefetch.ToString() + " Kb)";
                    label16.Text = "(" + all.ToString() + " Kb)";

                
                
                label17.Text = "(" + download.ToString() + " Kb)";

                label56.Text = "(" +jhkl.ToString()+ ")" + " Kb";
                groupBox1.Visible = false;
                label57.Visible = true;
                label56.Visible = true;
                label16.Visible = true;
                label17.Visible = true;
                label32.Visible = false;
                progressBar1.Visible = false;
                button7.Enabled = true;
                button8.Enabled = true;
                
                timer2.Stop();
                progressBar1.Value = 0;

            }
            else
            {
                percent = progressBar1.Value.ToString();
                label32.Text = percent + " %";
                progressBar1.Value += 1;
                button8.Enabled = false;
                button7.Enabled = false;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel4.Visible = false;
            groupBox1.Visible = false;
            panel2.Visible = false;
            groupBox12.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = true;
            
                      

        }

        private void button13_Click(object sender, EventArgs e)
        {
            RegistryKey gkey = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics");
            gkey.SetValue("MinAnimate", "0");

            RegistryKey vkey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
            vkey.SetValue("TaskbarAnimations", "0");

            RegistryKey zkey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\DWM");
            zkey.SetValue("CompositionPolicy", 0);
            zkey.SetValue("ColorizationOpaqueBlend", 0);
            zkey.SetValue("AlwaysHibernateThumbnails", "dword:00000000");

            RegistryKey ikey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
            ikey.SetValue("DisableThumbnails", "dword:00000001");

            RegistryKey ukey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
            ukey.SetValue("ListviewAlphaSelect", 0);

            RegistryKey ekey = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop");
            ekey.SetValue("DragFullWindows", 0);
            ekey.SetValue("FontSmoothing", 0);

            RegistryKey bkey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
            bkey.SetValue("ListviewShadow", 0);

            RegistryKey wkey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\ThemeManager");
            wkey.SetValue("ThemeActive", "0");

            RegistryKey ykey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ThemeManage");
            ykey.SetValue("ThemeActive", "-");

        }


        private void button15_Click_1(object sender, EventArgs e)
        {
            Thread.Sleep(2000);

            System.Diagnostics.Process pl = new System.Diagnostics.Process();
            pl.StartInfo.UseShellExecute = false;
            pl.StartInfo.FileName = "cmd.exe";

            pl.StartInfo.Arguments = "/c ipconfig/flushdns";

            pl.StartInfo.RedirectStandardError = true;
            pl.StartInfo.RedirectStandardInput = true;
            pl.StartInfo.RedirectStandardOutput = true;

            pl.Start();

            StreamReader outputWriter = pl.StandardOutput;
            String errorReader = pl.StandardError.ReadToEnd();
            String line = outputWriter.ReadLine();

            while (line != null)
            {

                richTextBox1.Text = outputWriter.ReadToEnd();
                line = outputWriter.ReadLine();

            }
            
            if (checkBox7.Checked == true)
            {
                System.Diagnostics.Process.Start("CMD.exe", "/K sfc /scannow");
            }
            
           if (checkBox9.Checked == true)
            {
                System.Diagnostics.Process.Start("CMD.exe", "/K DISM /Online /Cleanup-image /Restorehealth");
            }
            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == "") {


            }
            if (radioButton3.Checked == true)
            {
                Process[] processes = Process.GetProcessesByName(listBox1.SelectedItem.ToString());
                foreach (Process proc in processes)
                {
                    proc.PriorityClass = ProcessPriorityClass.RealTime;
                }
            }
            else if(radioButton3.Checked == true)
            {
                Process[] processes = Process.GetProcessesByName(listBox1.SelectedItem.ToString());
                foreach (Process proc in processes)
                {
                    proc.PriorityClass = ProcessPriorityClass.Normal;
                }
            }
            else if (radioButton5.Checked == true)
            {
                Process[] processes = Process.GetProcessesByName(listBox1.SelectedItem.ToString());
                foreach (Process proc in processes)
                {
                    proc.PriorityClass = ProcessPriorityClass.Idle;
                }
            }
            
        }

        private void button16_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.Show();

        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct SHQUERYRBINFO
        {
            public Int32 cbSize;
            public UInt64 i64Size;
            public UInt64 i64NumItems;
        }
        /// <summary>
        /// Call SHQueryRecycleBin() method of shell32 dll to query file size and file number
        /// in recycle bin.
        /// </summary>
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHQueryRecycleBin(
                [MarshalAs(UnmanagedType.LPTStr)]
                String pszRootPath,
                ref SHQUERYRBINFO pSHQueryRBInfo
            );
        enum RecycleFlags : int
        {
            SHERB_NOCONFIRMATION = 0x00000001,          //No confirmation dialog will open while emptying recycle bin.
            SHERB_NOPROGRESSUI = 0x00000001,            //No progress tracking window appears while emptying recycle bin.
            SHERB_NOSOUND = 0x00000004                  //No sound whent while emptying recycle bin.
        }
        /// <summary>
        /// Call SHEmptyRecycleBin() method of shell32 dll to empty recycle bin.
        /// </summary>
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        static extern int SHEmptyRecycleBin(IntPtr hwnd, string psrRootPath, RecycleFlags dwFlags);
        /// <summary>
        /// This method will empty recycle bin without prompting confirmation message.
        /// </summary>
        public static void emptyRecycleBin()
        {
            SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION);
        }
    }
}
   