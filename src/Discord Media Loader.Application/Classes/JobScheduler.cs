﻿#region LICENSE
/**********************************************************************************************
 * Copyright (C) 2017-2019 - All Rights Reserved
 * 
 * This file is part of "DML.Application".
 * The official repository is hosted at https://github.com/Serraniel/DiscordMediaLoader
 * 
 * "DML.Application" is licensed under Apache 2.0.
 * Full license is included in the project repository.
 * 
 * Users who edited JobScheduler.cs under the condition of the used license:
 * - Serraniel (https://github.com/Serraniel)
 **********************************************************************************************/
#endregion

using Discord;
using DML.AppCore.Classes;
using SweetLib.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DML.Application.Classes
{
    public class JobScheduler
    {
        private ulong messageScanned = 0;
        private ulong totalAttachments = 0;
        private ulong attachmentsDownloaded = 0;

        private bool Run { get; set; } = false;
        public List<Job> JobList { get; set; } = new List<Job>();
        public Dictionary<int, Queue<IMessage>> RunningJobs = new Dictionary<int, Queue<IMessage>>();
        internal int RunningThreads { get; set; } = 0;

        internal ulong MessagesScanned
        {
            get
            {
                lock (this)
                {
                    return messageScanned;
                }
            }
            set
            {
                lock (this)
                {
                    messageScanned = value;
                }
            }
        }

        internal ulong TotalAttachments
        {
            get
            {
                lock (this)
                {
                    return totalAttachments;
                }
            }
            set
            {
                lock (this)
                {
                    totalAttachments = value;
                }
            }
        }

        internal ulong AttachmentsDownloaded
        {
            get
            {
                lock (this)
                {
                    return attachmentsDownloaded;
                }
            }
            set
            {
                lock (this)
                {
                    attachmentsDownloaded = value;
                }
            }
        }

        public void Stop()
        {
            Run = false;
        }

        public void ScanAll()
        {
            Logger.Info("Started JobScheduler...");

            Logger.Debug("Entering job list handler loop...");
            //foreach (var job in JobList)
            for (var i = JobList.Count - 1; i >= 0; i--)
            {
                if (JobList[i].State == JobState.Idle)
                {
                    try
                    {
                        var job = JobList[i];
                        Logger.Debug($"Checking job {job.Id}");

                        Task.Run(async () =>
                        {
                            var scanFinished = await job.Scan();
                            Logger.Trace($"Scan result of {job.Id}: {scanFinished}");

                            while (!scanFinished)
                            {
                                scanFinished = await job.Scan();
                                Logger.Trace($"Scan result of {job.Id}: {scanFinished}");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.Message);
                    }
                }
            }
        }
    }
}