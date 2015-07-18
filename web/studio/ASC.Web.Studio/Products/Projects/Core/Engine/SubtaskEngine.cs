/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Linq;

using ASC.Core;
using ASC.Core.Tenants;

using ASC.Projects.Core.DataInterfaces;
using ASC.Projects.Core.Domain;
using ASC.Projects.Core.Services.NotifyService;
using IDaoFactory = ASC.Projects.Core.DataInterfaces.IDaoFactory;

namespace ASC.Projects.Engine
{
    public class SubtaskEngine : ProjectEntityEngine
    {
        private readonly EngineFactory factory;
        private readonly ISubtaskDao subtaskDao;
        private readonly ITaskDao taskDao;
        private readonly TaskEngine taskEngine;

        public SubtaskEngine(IDaoFactory daoFactory, EngineFactory factory)
            : base(NotifyConstants.Event_NewCommentForTask, factory)
        {
            this.factory = factory;
            subtaskDao = daoFactory.GetSubtaskDao();
            taskDao = daoFactory.GetTaskDao();
            taskEngine = factory.GetTaskEngine();
        }

        #region get 

        public List<Task> GetByDate(DateTime from, DateTime to)
        {
            var subtasks = subtaskDao.GetUpdates(from, to).ToDictionary(x => x.Task, x => x);
            var ids = subtasks.Select(x => x.Value.Task).Distinct().ToList();
            var tasks = taskDao.GetById(ids);
            foreach (var task in tasks)
            {
                Subtask subtask;
                subtasks.TryGetValue(task.ID, out subtask);
                task.SubTasks.Add(subtask);
            }
            return tasks;
        }

        public int GetSubtaskCount(int taskid, params TaskStatus[] statuses)
        {
            return subtaskDao.GetSubtaskCount(taskid, statuses);
        }

        public int GetSubtaskCount(int taskid)
        {
            return subtaskDao.GetSubtaskCount(taskid, null);
        }

        public Subtask GetById(int id)
        {
            return subtaskDao.GetById(id);
        }

        #endregion

        #region Actions 

        public Subtask ChangeStatus(Task task, Subtask subtask, TaskStatus newStatus)
        {
            if (subtask == null) throw new Exception("subtask.Task");
            if (task == null) throw new ArgumentNullException("task");
            if (task.Status == TaskStatus.Closed) throw new Exception("task can't be closed");

            if (subtask.Status == newStatus) return subtask;

            ProjectSecurity.DemandEdit(task, subtask);
           
            subtask.Status = newStatus;
            subtask.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            subtask.LastModifiedOn = TenantUtil.DateTimeNow();
            subtask.StatusChangedOn = TenantUtil.DateTimeNow();

            if (subtask.Responsible.Equals(Guid.Empty))
                subtask.Responsible = SecurityContext.CurrentAccount.ID;

            var senders = GetSubscribers(task);

            if (task.Status != TaskStatus.Closed && newStatus == TaskStatus.Closed && !factory.DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutSubTaskClosing(senders, task, subtask);

            if (task.Status != TaskStatus.Closed && newStatus == TaskStatus.Open && !factory.DisableNotifications && senders.Count != 0)
                NotifyClient.Instance.SendAboutSubTaskResumed(senders, task, subtask);

            return subtaskDao.Save(subtask);
        }

        public Subtask SaveOrUpdate(Subtask subtask, Task task)
        {
            if (subtask == null) throw new Exception("subtask.Task");
            if (task == null) throw new ArgumentNullException("task");
            if (task.Status == TaskStatus.Closed) throw new Exception("task can't be closed");

            // check guest responsible
            if (ProjectSecurity.IsVisitor(subtask.Responsible))
            {
                ProjectSecurity.CreateGuestSecurityException();
            }

            var isNew = subtask.ID == default(int); //Task is new
            var oldResponsible = Guid.Empty;

            subtask.LastModifiedBy = SecurityContext.CurrentAccount.ID;
            subtask.LastModifiedOn = TenantUtil.DateTimeNow();

            if (isNew)
            {
                if (subtask.CreateBy == default(Guid)) subtask.CreateBy = SecurityContext.CurrentAccount.ID;
                if (subtask.CreateOn == default(DateTime)) subtask.CreateOn = TenantUtil.DateTimeNow();

                ProjectSecurity.DemandEdit(task);
                subtask = subtaskDao.Save(subtask);
            }
            else
            {
                var oldSubtask = subtaskDao.GetById(new[] { subtask.ID }).First();

                if (oldSubtask == null) throw new ArgumentNullException("subtask");

                oldResponsible = oldSubtask.Responsible;

                //changed task
                ProjectSecurity.DemandEdit(task, oldSubtask);
                subtask = subtaskDao.Save(subtask);
            }

            NotifySubtask(task, subtask, isNew, oldResponsible);

            var senders = new HashSet<Guid> { subtask.Responsible, subtask.CreateBy };
            senders.Remove(Guid.Empty);

            foreach (var sender in senders)
            {
                taskEngine.Subscribe(task, sender);
            }

            return subtask;
        }

        private void NotifySubtask(Task task, Subtask subtask, bool isNew, Guid oldResponsible)
        {
            //Don't send anything if notifications are disabled
            if (factory.DisableNotifications) return;

            var recipients = GetSubscribers(task);

            if (!subtask.Responsible.Equals(Guid.Empty) && (isNew || !oldResponsible.Equals(subtask.Responsible)))
            {
                NotifyClient.Instance.SendAboutResponsibleBySubTask(subtask, task);
                recipients.RemoveAll(r => r.ID.Equals(subtask.Responsible.ToString()));
            }

            if (isNew)
            {
                NotifyClient.Instance.SendAboutSubTaskCreating(recipients, task, subtask);
            }
            else
            {
                NotifyClient.Instance.SendAboutSubTaskEditing(recipients, task, subtask);
            }
        }

        public void Delete(Subtask subtask, Task task)
        {
            if (subtask == null) throw new ArgumentNullException("subtask");
            if (task == null) throw new ArgumentNullException("task");

            ProjectSecurity.DemandEdit(task, subtask);
            subtaskDao.Delete(subtask.ID);

            var recipients = GetSubscribers(task);

            if (recipients.Any())
            {
                NotifyClient.Instance.SendAboutSubTaskDeleting(recipients, task, subtask);
            }
        }

        #endregion
    }
}
