using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using Sys = Cosmos.System;

namespace Cosmares
{
    public class Kernel : Sys.Kernel
    {
        public static CosmosVFS vfs = new CosmosVFS();
        public static int finalSelection = 0;

        public const string kernelName = "YourKernelNameHere";
        public const string kernelFileExtension = ".bin"; // if using gzip compression add `.gz` as the end.
        public const string kernelVersion = "YourKernelVersionHere"; // set your kernel version such as v1.0
        public const string cosmaresVersion = "v1.0"; // DO NOT EDIT THIS
        public const string gzFileSymbol = ""; // please set this to `$` if using .bin.gz as extension.
        public static readonly Color kernelAccentColor = Color.DarkCyan;
        public static readonly ConsoleColor consoleKernelAccentColor = ConsoleColor.Blue;
        public static readonly ConsoleColor consoleKernelUpdatesColor = ConsoleColor.Red;
        public static readonly ConsoleColor consoleKernelProgressBarColor = ConsoleColor.Green;
        //public static List<DirectoryEntry> AvailableDisks = new List<DirectoryEntry>();
        // public static List<Sys.FileSystem.Listing.DirectoryEntry> AvailableVolumes = new List<Sys.FileSystem.Listing.DirectoryEntry>();
        public static int AvailableDisksel = 0;
        public static int SelectedDisk = 0;
        public static int selected = 0;
        public static DirectoryEntry selectedDiskName;

        // PLEASE DO NOT MODIFY THESE RESOURCES IN THE SETTINGS!
        [ManifestResourceStream(ResourceName = "Cosmares.Resources.empty_sector")] public static byte[] emptysector; // 10 empty sectors for disk fill
        [ManifestResourceStream(ResourceName = "Cosmares.Resources.limine")] public static byte[] limine_bootsector; // boot sectot
        [ManifestResourceStream(ResourceName = "Cosmares.Resources.limine-bios.sys")] public static byte[] LimineBIOSSys; // boot\limine-bios.sys
        public const string LimineConfig = "TIMEOUT=0\r\nVERBOSE=yes\r\n\r\nTERM_WALLPAPER=boot:///boot/liminewp.bmp\r\nINTERFACE_RESOLUTION=800x600x32\r\n\r\n:" + kernelName + ".bin\r\n    COMMENT=Boot " + kernelName + kernelFileExtension + " using multiboot2.\r\n\r\n    PROTOCOL=multiboot2\r\n    KERNEL_PATH=" + gzFileSymbol + "boot:///boot/" + kernelName + ".bin\r\n"; // boot\limine.cfg

        // CAN BE EDITED (SAFE ZONE)
        [ManifestResourceStream(ResourceName = "Cosmares.Resources.liminewp.bmp")] public static byte[] LimineWallpaper; // boot\liminewp.bmp
        // public static readonly Dictionary<string, string> defaultOSFiles; // optional but after main install your Kernel's default configs can be copied over!

        // PLEASE ADD YOUR KERNEL FILE WITH THE ABOVE `kernelName + kernelFileExtension` being your file name, Modify the below path for the same!
        [ManifestResourceStream(ResourceName = "Cosmares.Resources.YourKernelNameHere.bmp")] public static byte[] KernelFile; // boot\<kernel>.bin

        public const bool GUIMode = false;
        public const int CWidth = 85;
        public const int CHeight = 25;

        protected override void BeforeRun()
        {
            Console.WriteLine("Booting into the Cosmares Installer.");
            WriteSystemInfo(Result.OK, "Setting filesystem up.");
            try
            {
                VFSManager.RegisterVFS(vfs, true, false);
            } catch (Exception)
            {
                WriteSystemInfo(Result.WARN, "Filesystem is not working, have you properly put the ISO?");
            }
            WriteSystemInfo(Result.OK, "Checking all installer files.");
            if (kernelName != "" || kernelName != null || (kernelFileExtension == ".bin" || kernelFileExtension == ".bin.gz"))
            {
                WriteSystemInfo(Result.PASS, "Kernel file names are properly inserted.");
            } else
            {
                WriteSystemInfo(Result.FAIL, "Kernel file names are not proper and are null or extensions are incorrect. Please recheck.");
                WriteSystemInfo(Result.WARN, "Shutting down in 3 seconds due to lack of properly inserted file names.");
                Thread.Sleep(3000);
            }
            if (LimineConfig != "TIMEOUT=0\r\nVERBOSE=yes\r\n\r\nTERM_WALLPAPER=boot:///boot/liminewp.bmp\r\nINTERFACE_RESOLUTION=800x600x32\r\n\r\n:" + kernelName + ".bin\r\n    COMMENT=Boot " + kernelName + kernelFileExtension + " using multiboot2.\r\n\r\n    PROTOCOL=multiboot2\r\n    KERNEL_PATH=" + gzFileSymbol + "boot:///boot/" + kernelName + ".bin\r\n")
            {
                WriteSystemInfo(Result.PANIC, "Please do not modify system config files. If you have modified any other files too, please revert it, else the setup will have issues");
            } else
            {
                WriteSystemInfo(Result.PASS, "Limine Config file is setup properly!");
            }
            if (LimineWallpaper == null)
            {
                WriteSystemInfo(Result.PANIC, "Please recheck the Limine Wallpaper file as it has an incorrect path or a null file inserted.");
            } else
            {
                WriteSystemInfo(Result.PASS, "Limine Wallpaper is setup properly!");
            }
            WriteSystemInfo(Result.OK, "All files checks have been passed!");
            WriteSystemInfo(Result.OK, "Storing lists of all disks available.");
            // AvailableVolumes = VFSManager.GetVolumes();
            if (!GUIMode)
            {
                Console.WriteLine("Entered Cosmares Installer");
            }
        }
        
        protected override void Run()
        {
            if (!GUIMode)
            {
                Console.Write("Hello There! You are about to install ");
                Console.ForegroundColor = consoleKernelAccentColor;
                Console.Write(kernelName);
                Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("!");
                RunCUIInstaller();
            } else
            {

            }
            Heap.Collect();
        }
        
        public static void WriteSystemInfo(Result result, string proc)
        {
            switch (result)
            {
                case Result.OK:
                case Result.PASS:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[" + ResultToString(result) + "] ");
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(proc);
                    return;
                case Result.ERROR:
                case Result.PANIC:
                case Result.FAIL:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[" + ResultToString(result) + "] ");
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(proc);
                    return;
                case Result.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[" + ResultToString(result) + "] ");
                    Console.BackgroundColor = ConsoleColor.Black; Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(proc);
                    return;
                default:
                    return;
            }
        }

        public static string ResultToString(Result result)
        {
            switch (result)
            {
                case Result.OK:
                    return "  OK  ";
                case Result.PASS:
                    return " PASS ";
                case Result.WARN:
                    return " WARN ";
                case Result.ERROR:
                    return " ERR  ";
                case Result.PANIC:
                    return "KPANIC";
                case Result.FAIL:
                    return " FAIL ";
                default:
                    return "";
            }
        }

        public enum Result
        {
            OK,
            PASS,
            WARN,
            ERROR,
            PANIC,
            FAIL
        }

        public static void DrawUpdate(string Text)
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;
            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = consoleKernelUpdatesColor;
            Console.SetCursorPosition(1, CHeight - 1);
            Console.Write(new string(' ', 78));
            Console.SetCursorPosition(1, CHeight - 1);
            Console.Write(Text);

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.SetCursorPosition(x, y);
        }

        public static void DrawInstallProgressBar(int progress)
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;
            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            //"    START     FORMAT     PARTITIONING     BOOT     SYSTEM FILES     REBOOT  "

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(1, CHeight - 1);
            Console.Write(new string(' ', 78));
            Console.SetCursorPosition(1, CHeight - 2);
            Console.Write(new string(' ', 78));
            Console.BackgroundColor = consoleKernelProgressBarColor;
            Console.SetCursorPosition(1, CHeight - 1);
            Console.Write(new string(' ', progress));
            Console.SetCursorPosition(1, CHeight - 2);
            Console.Write(new string(' ', progress));
            Console.ResetColor(); Console.BackgroundColor = (ConsoleColor)0;
            Console.SetCursorPosition(6, CHeight - 1);
            Console.Write("START");

            Console.SetCursorPosition(16, CHeight - 1);
            Console.Write("FORMAT");

            Console.SetCursorPosition(28, CHeight - 1);
            Console.Write("PARTITIONING");

            Console.SetCursorPosition(45, CHeight - 1);
            Console.Write("BOOT");

            Console.SetCursorPosition(54, CHeight - 1);
            Console.Write("SYSTEM FILES");

            Console.SetCursorPosition(71, CHeight - 1);
            Console.Write("REBOOT");

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.SetCursorPosition(x, y);
        }

        public static void RunCUIInstaller()
        {
            List<DriveInfo> disks = new List<DriveInfo>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                disks.Add(drive);
            }

            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");

            Console.SetCursorPosition(2, Console.CursorTop - 1);
            // Console.CursorLeft = 0;
            // Console.CursorTop = CHeight;
            DrawUpdate("[ARROWS] Move                                   [SPACE] Select;   [ENTER] Next");
            var winX = CWidth / 16;
            var winY = 3;
            var winWidth = CWidth - (CWidth / 16) - 10;
            var winHeight = CHeight - 5;
            Console.BackgroundColor = ConsoleColor.White;
            for (var i = 0; i < winHeight; i++)
            {
                Console.SetCursorPosition(winX, winY + i);
                // Console.CursorLeft = winX;
                // Console.CursorTop = winY + i;
                Console.Write(new string(' ', winWidth));
            }

            Console.SetCursorPosition(winX + 2, winY + 1);
            // Console.CursorLeft = winX + 2;
            // Console.CursorTop = winY + 1;
            Console.Write("Select a disk:");
            Console.SetCursorPosition(winX + 2, winY + 2);
            Console.Write("(Arrow keys to move, space to select and enter to next)");

            var boxX = winX + 4;
            var boxY = winY + 4;
            var boxWidth = winWidth - 8; // (winWidth - (4*2))
            var boxHeight = winHeight - 8; // (winHeight - (4*2))
            Console.BackgroundColor = ConsoleColor.Gray;
            for (var i = 0; i < boxHeight; i++)
            {
                Console.SetCursorPosition(boxX, boxY + i);
                // Console.CursorLeft = boxX;
                // Console.CursorTop = boxY + i;
                Console.Write(new string(' ', boxWidth));
            }

            var selected = 0;
            var selectedVal = disks[selected];
            var hovered = 0;
            var hoveredVal = disks[hovered];
        update:
            for (var i = 0; i < disks.Count; i++)
            {
                if (i == selected)
                {
                    Console.BackgroundColor = consoleKernelAccentColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (i == hovered)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.SetCursorPosition(boxX, boxY + i);
                // Console.CursorLeft = boxX;
                // Console.CursorTop = boxY + i;
                Console.Write(new string(' ', boxWidth));
                Console.SetCursorPosition(boxX, boxY + i);
                // Console.CursorLeft = boxX;
                // Console.CursorTop = boxY + i;
                Console.Write(disks[i].VolumeLabel.Replace("  ", "").ToString() + ", " + disks[i].RootDirectory.ToString() + ", " + disks[i].DriveFormat.ToString() + ", " + (disks[i].TotalFreeSpace / 1024).ToString() + " KB free of " + (disks[i].TotalSize / 1024).ToString() + " KB");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
            }

            var key = Console.ReadKey(true);
            while (key.Modifiers != ConsoleModifiers.Control && key.Modifiers != ConsoleModifiers.Shift && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.UpArrow && hovered - 1 > -1)
                {
                    hovered--;
                    goto update;
                }
                else if (key.Key == ConsoleKey.DownArrow && hovered + 1 < disks.Count)
                {
                    hovered++;
                    goto update;
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    finalSelection = selected;
                    selectedVal = disks[selected];
                    SelectedDisk = selected;
                    goto nextSetupProc;
                }
                else if (key.Key == ConsoleKey.Spacebar)
                {
                    selected = hovered;
                    goto update;
                }
                key = Console.ReadKey(true);
            }
            return;


        nextSetupProc:
            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");
            DrawUpdate("Target install disk: " + selectedVal.Name + ", " + selectedVal.VolumeLabel);
            var winX1 = CWidth / 16;
            var winY1 = 3;
            var winWidth1 = CWidth - (CWidth / 16) - 10;
            var winHeight1 = CHeight - 5;
            Console.BackgroundColor = ConsoleColor.White;
            for (var i = 0; i < winHeight1; i++)
            {
                Console.SetCursorPosition(winX1, winY1 + i);
                // Console.CursorLeft = winX1;
                // Console.CursorTop = winY1 + i;
                Console.Write(new string(' ', winWidth1));
            }

            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(winX1 + 2, winY1 + 1);
            Console.Write("You are about to install " + kernelName + "!");
            Console.SetCursorPosition(winX1 + 2, winY1 + 2);
            Console.Write("on Drive " + selectedVal + " via Cosmares.");
            Console.SetCursorPosition(winX1 + 2, winY1 + 4);
            Console.Write("Your Drive's partition layout will be:");
            Console.SetCursorPosition(winX1 + 5, winY1 + 6);
            Console.Write("Drive (" + selectedVal + ")");
            Console.SetCursorPosition(winX1 + 5, winY1 + 7);
            Console.Write("|--Boot/EFI partition (with Limine) [FAT32, MBR]");
            Console.SetCursorPosition(winX1 + 5, winY1 + 8);
            Console.Write("|--Filesystem partition (for " + kernelName + ") [FAT32, MBR]");

            Console.SetCursorPosition(winX1 + 2, winY1 + 10);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("WARNING: This action cannot be reversed after this dialog,");
            Console.SetCursorPosition(winX1 + 2, winY1 + 11);
            Console.Write("and pausing after here can break your system very badly.");

            Console.ForegroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(winX1 + 2, winY1 + 13);
            Console.Write("Do you want to proceed?");
            Console.SetCursorPosition(winX1 + 2, winY1 + 14);
            Console.Write("Press Enter to proceed or ESC to shutdown!");

            var key1 = Console.ReadKey(true);
            while (key1.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Enter)
                {
                    goto startSetup;
                }
                key1 = Console.ReadKey(true);
            }
            //shutdown
            Console.SetCursorPosition(0, 0);
            WriteSystemInfo(Result.FAIL, "Installer was interrupted by user");
            System.Threading.Thread.Sleep(2000);
            Sys.Power.Shutdown();

        startSetup:
            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");

            DrawUpdate("Working on disk, Please do not interrupt installation, It may harm you disk");
            Console.ForegroundColor = ConsoleColor.Black;
            var winX2 = CWidth / 16;
            var winY2 = 3;
            var winWidth2 = CWidth - (CWidth / 16) - 10;
            var winHeight2 = CHeight - 5;
            Console.BackgroundColor = ConsoleColor.White;
            for (var i = 0; i < winHeight2; i++)
            {
                Console.SetCursorPosition(winX2, winY2 + i);
                // Console.CursorLeft = winX2;
                // Console.CursorTop = winY2 + i;
                Console.Write(new string(' ', winWidth2));
            }

            //int installationStartTime = RTC.Hour * 3600 + RTC.Minute * 60 + RTC.Second;

            Console.SetCursorPosition(winX2 + 2, winY2 + 1);
            Console.Write("Cosmares Installer has started");

            DrawInstallProgressBar(8);
            System.Threading.Thread.Sleep(1000); // Just wait

            goto updateFormat;

            DrawInstallProgressBar(18); // IK that this code is unreachable, i need some values from there, so dont remove it :) 
            System.Threading.Thread.Sleep(2000);

            Console.SetCursorPosition(winX2 + 5, winY2 + 4);
            Console.Write("Making partitions on the disk..");

            DrawInstallProgressBar(32);
            System.Threading.Thread.Sleep(2000);

            Console.SetCursorPosition(winX2 + 5, winY2 + 5);
            Console.Write("Copying bootloader files..");

            DrawInstallProgressBar(47);
            System.Threading.Thread.Sleep(2000);

            Console.SetCursorPosition(winX2 + 5, winY2 + 6);
            Console.Write("Installing " + kernelName + "..");

            DrawInstallProgressBar(55);
            System.Threading.Thread.Sleep(2000);

            Console.SetCursorPosition(winX2 + 5, winY2 + 7);
            Console.Write("Installing " + kernelName + "'s files.");

            DrawInstallProgressBar(61);
            System.Threading.Thread.Sleep(2000);

            DrawInstallProgressBar(73);
            System.Threading.Thread.Sleep(1000);
            DrawInstallProgressBar(78);
            Sys.Power.Shutdown();

        updateFormat:

            List<string> sel = new List<string>();
            sel.Add("Format    QUICK - Data recover possible");
            sel.Add("Format    SLOW  - No data recover possible");
            sel.Add("Fill      0     - Takes long time");
            sel.Add("Nope, no format - Really risky");

            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");

            DrawInstallProgressBar(16);

            for (var i = 0; i < sel.Count; i++)
            {
                if (i == selected)
                {
                    Console.BackgroundColor = consoleKernelAccentColor;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else if (i == hovered)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.SetCursorPosition(boxX, boxY + i);
                // Console.CursorLeft = boxX;
                // Console.CursorTop = boxY + i;
                Console.Write(new string(' ', boxWidth));
                Console.SetCursorPosition(boxX, boxY + i);
                // Console.CursorLeft = boxX;
                // Console.CursorTop = boxY + i;
                Console.Write(sel[i]);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
            }

            var key2 = Console.ReadKey(true);
            while (key2.Modifiers != ConsoleModifiers.Control && key2.Modifiers != ConsoleModifiers.Shift && key2.Key != ConsoleKey.Escape)
            {
                if (key2.Key == ConsoleKey.UpArrow && hovered - 1 > -1)
                {
                    hovered--;
                    goto updateFormat;
                }
                else if (key2.Key == ConsoleKey.DownArrow && hovered + 1 < sel.Count)
                {
                    hovered++;
                    goto updateFormat;
                }
                else if (key2.Key == ConsoleKey.Enter)
                {
                    if (selected == 0)
                    {
                        vfs.Disks[SelectedDisk].FormatPartition(0, "FAT32", true);
                    }
                    else if (selected == 1)
                    {
                        vfs.Disks[SelectedDisk].FormatPartition(0, "FAT32", false);
                    }
                    else if (selected == 2)
                    {
                        int start = RTC.Hour * 3600 + RTC.Minute * 60 + RTC.Second;
                        int end = 0;
                        int one = 0;
                        int eta = 0;
                        for (int i = 0; i < (((vfs.Disks[SelectedDisk].Size) / 512) / 10); i++)
                        {
                            vfs.Disks[SelectedDisk].Host.WriteBlock((ulong)(i - 1), 10, ref emptysector);
                            Console.SetCursorPosition(0, 1);
                            Console.Write("Formated " + (i * 10) + " / " + (vfs.Disks[SelectedDisk].Size / 512) + " Run time: " + (end - start) + "s" + " ETA: " + eta.ToString() + "s" + new string(' ', 5));
                            Heap.Collect();
                            if (i.ToString().EndsWith("00"))
                            {
                                end = RTC.Hour * 3600 + RTC.Minute * 60 + RTC.Second;
                                eta = (((((vfs.Disks[SelectedDisk].Size) / 512) / 10) - i) / one);
                            }
                            if ((end - start) == 1)
                            {
                                one = i;
                            }
                        }
                    }
                    else if (selected == 3)
                    {

                    }

                    DrawInstallProgressBar(20);
                    System.Threading.Thread.Sleep(1000);
                    goto updatePartitioning;
                }
                else if (key2.Key == ConsoleKey.Spacebar)
                {
                    selected = hovered;
                    goto updateFormat;
                }
                key2 = Console.ReadKey(true);
            }


        updatePartitioning:
            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");

            DrawInstallProgressBar(30);

            Thread.Sleep(1000);

            Console.SetCursorPosition(0, 2);

            WriteSystemInfo(Result.WARN, "Setupping partitions for you");

            Thread.Sleep(1000);
            WriteSystemInfo(Result.WARN, "Partition: 0, 0:/, 96 Mb, BOOT");

            vfs.Disks[SelectedDisk].CreatePartition(96);
            WriteSystemInfo(Result.OK, "Done!, Now Enter size(in MB) for Partition 1(max " + ((vfs.Disks[SelectedDisk].Size / 1048576) - 96) + "): ");
            string size = Console.ReadLine();

            while (Int32.Parse(size) - 1 >= ((vfs.Disks[SelectedDisk].Size / 1048576) - 96))
            {
                WriteSystemInfo(Result.ERROR, "Size is cannot be bigger than disk's size!! Enter new size: ");
                size = Console.ReadLine();
            }

            WriteSystemInfo(Result.WARN, "Partition: 1, 1:/, " + size + " Mb, HOME");

            vfs.Disks[SelectedDisk].CreatePartition(Int32.Parse(size));

            WriteSystemInfo(Result.OK, "All partitions are set up!!! Now lets move to writing boot!");

            DrawInstallProgressBar(39);

            Thread.Sleep(5000);
            goto updateBootsector;

            return;


        updateBootsector:

            Console.BackgroundColor = consoleKernelAccentColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            Console.SetCursorPosition(0, 0);
            Console.WriteLine(kernelName + " " + kernelVersion + " Installer (via Cosmares)");

            DrawInstallProgressBar(45);
            Console.SetCursorPosition(0, 2);
            Console.WriteLine("Now lets install Limine!");
            Thread.Sleep(1000);
            try
            {
                byte[] byt = new byte[512];


                vfs.Disks[SelectedDisk].Host.ReadBlock(0, 1, ref byt);

                string boot = Encoding.ASCII.GetString(limine_bootsector);

                

                Console.WriteLine(boot);
                byte[] data = Encoding.ASCII.GetBytes(boot);

                WriteSystemInfo(Result.PASS, "Bytes prepared, lets write boot sector");
                Console.WriteLine(data.Length);

                vfs.Disks[SelectedDisk].Host.WriteBlock(0, 1, ref data);

            }
            catch (Exception ex)
            {
                WriteSystemInfo(Result.PANIC, "INSTALLER cannot continue, you arent able to boot you os. "+ ex.Message);
                Thread.Sleep(10000);
                Sys.Power.Shutdown();
            }

            Thread.Sleep(1000);


            return;
        }

    }
}
