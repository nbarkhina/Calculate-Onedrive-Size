using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculate_Onedrive_Size
{
    public partial class Form1 : Form
    {
        string allSizes;
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            /*
            allSizes = "";
            FileInfo file1 = new FileInfo(@"C:\Users\nbark\OneDrive\Videos\Random\Neil Drumming.MOV");
            FileInfo file2 = new FileInfo(@"C:\Users\nbark\OneDrive\Videos\Random\Mad ball racer 1.mp4");

            long file1size = GetFileSizeOnDisk(@"C:\Users\nbark\OneDrive\Videos\Random\Neil Drumming.MOV");
            long file2size = GetFileSizeOnDisk(@"C:\Users\nbark\OneDrive\Videos\Random\Mad ball racer 1.mp4");



            //string folder = @"C:\Users\nbark\OneDrive\Videos";
            */
            string folder = @"C:\Users\nbark\OneDrive";
            long foldersize = 0;
            textBox1.Text = "Please wait...";

            await Task.Run(() => foldersize = CalculateFolderSize(folder, 0));
            
            string sizeformatted = FormatSize(foldersize);
            textBox1.Text = "ALL - " + sizeformatted + "\r\n\r\n" + allSizes;
        }

        public string FormatSize(long foldersize)
        {
            try
            {
                string returner = ((double)foldersize / 1073741824).ToString("#.##") + " GB";
                if (returner.Equals(" GB"))
                    return "";
                else return returner;
            }
            catch(Exception ex)
            {
                return "";
            }
            
        }

        public static long GetFileSizeOnDisk(string file)
        {
            FileInfo info = new FileInfo(file);
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = GetCompressedFileSizeW(file, out hosize);
            long size;
            size = (long)hosize << 32 | losize;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
           out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);
        protected long CalculateFolderSize(string folder, int depth)
        {
            long folderSize = 0;
            try
            {
                //Checks if the path is valid or not
                if (!Directory.Exists(folder))
                    return folderSize;
                else
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            if (File.Exists(file))
                            {
                                //FileInfo finfo = new FileInfo(file);
                                //folderSize += finfo.Length;
                                folderSize += GetFileSizeOnDisk(file);

                            }
                        }
                        foreach (string dir in Directory.GetDirectories(folder))
                        {
                            folderSize += CalculateFolderSize(dir,depth+1);
                        }
                        if (depth < 3)
                        {
                            string tab = "";
                            for (int i = 0; i < depth; i++)
                                tab += "   ";
                            string formattedSize = FormatSize(folderSize);
                            allSizes = tab + depth + ") " + folder + " - " + formattedSize + "\r\n"
                                + allSizes;
                        }
                    }
                    catch (NotSupportedException e)
                    {
                        Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Unable to calculate folder size: {0}", e.Message);
            }
            return folderSize;
        }
    }
}
