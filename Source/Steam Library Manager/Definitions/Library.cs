﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Steam_Library_Manager.Definitions
{
    public class Library : INotifyPropertyChanged
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Enums.LibraryType Type { get; set; }
        public System.IO.DirectoryInfo DirectoryInfo { get; set; }
        public SteamLibrary Steam { get; set; }
        public OriginLibrary Origin { get; set; }

        public List<FrameworkElement> ContextMenu
        {
            get
            {
                var cMenu = new List<FrameworkElement>();
                try
                {
                    foreach (var cMenuItem in List.LibraryCMenuItems.Where(x => x.IsActive && x.ShowToSLMBackup))
                    {
                        if (!cMenuItem.ShowToNormal && !cMenuItem.ShowToSLMBackup)
                        {
                            continue;
                        }

                        if (cMenuItem.IsSeparator)
                        {
                            cMenu.Add(new Separator());
                        }
                        else
                        {
                            MenuItem SLMItem = new MenuItem()
                            {
                                Tag = cMenuItem.Action,
                                Header = Framework.StringFormat.Format(cMenuItem.Header, new { LibraryFullPath = DirectoryInfo.FullName, FreeDiskSpace = PrettyFreeSpace }),
                                Icon = Functions.FAwesome.GetAwesomeIcon(cMenuItem.Icon, cMenuItem.IconColor),
                                HorizontalContentAlignment = HorizontalAlignment.Left,
                                VerticalContentAlignment = VerticalAlignment.Center
                            };

                            SLMItem.Click += Main.FormAccessor.LibraryCMenuItem_Click;

                            cMenu.Add(SLMItem);
                        }
                    }

                    return cMenu;
                }
                catch (FormatException)
                {
                    return cMenu;
                }
            }
        }

        public void ParseMenuItemAction(string Action)
        {
            if (Type == Enums.LibraryType.SLM)
            {
                switch (Action.ToLowerInvariant())
                {
                    // Opens game installation path in explorer
                    case "disk":
                        if (DirectoryInfo.Exists)
                        {
                            Process.Start(DirectoryInfo.FullName);
                        }

                        break;
                    // Removes a backup library from list
                    case "removefromlist":
                        try
                        {
                            if (Type == Enums.LibraryType.SLM)
                            {
                                List.Libraries.Remove(this);

                                if (SLM.CurrentSelectedLibrary == this)
                                    Main.FormAccessor.AppView.AppPanel.ItemsSource = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Fatal(ex);
                        }
                        break;
                }
            }
            else if (Type == Enums.LibraryType.Steam)
            {
                Steam.ParseMenuItemActionAsync(Action);
            }
            else if (Type == Enums.LibraryType.Origin)
            {
                Origin.ParseMenuItemActionAsync(Action);
            }
        }

        public long FreeSpace => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? Functions.FileSystem.GetAvailableFreeSpace(DirectoryInfo.FullName) : 0;

        public long TotalSize => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? Functions.FileSystem.GetAvailableTotalSpace(DirectoryInfo.FullName) : 0;

        public string PrettyFreeSpace => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? $"{Functions.FileSystem.FormatBytes(FreeSpace)} / {Functions.FileSystem.FormatBytes(TotalSize)}" : "";

        public int FreeSpacePerc => DirectoryInfo.Exists && !DirectoryInfo.FullName.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? 100 - ((int)Math.Round((double)(100 * FreeSpace) / Functions.FileSystem.GetAvailableTotalSpace(DirectoryInfo.FullName))) : 0;

        public void UpdateDiskDetails()
        {
            OnPropertyChanged("DirectoryInfo");
            OnPropertyChanged("FreeSpace");
            OnPropertyChanged("PrettyFreeSpace");
            OnPropertyChanged("FreeSpacePerc");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }
}