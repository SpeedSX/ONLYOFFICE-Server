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
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region milestone

        ///<summary>
        ///Returns the list of all upcoming milestones within all portal projects
        ///</summary>
        ///<short>
        ///Upcoming milestones
        ///</short>
        /// <category>Milestones</category>
        ///<returns>List of milestones</returns>
        [Read(@"milestone")]
        public IEnumerable<MilestoneWrapper> GetMilestones()
        {
            var milestones = EngineFactory.GetMilestoneEngine().GetUpcomingMilestones((int)_context.Count);
            _context.SetDataPaginated();
            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all milestones matching the filter with the parameters specified in the request
        ///</summary>
        ///<short>
        ///Milestones by filter
        ///</short>
        /// <category>Milestones</category>
        ///<param name="projectid" optional="true">Project ID</param>
        ///<param name="tag" optional="true">Project tag</param>
        ///<param name="status" optional="true">Milstone status/ Can be open or closed</param>
        ///<param name="deadlineStart" optional="true">Minimum value of task deadline</param>
        ///<param name="deadlineStop" optional="true">Maximum value of task deadline</param>
        ///<param name="taskResponsible" optional="true">Responsible for the task in milestone GUID</param>
        ///<param name="lastId">Last milestone ID</param>
        ///<param name="myProjects">Miletone in my Projects</param>
        ///<param name="milestoneResponsible">Responsible for the milestone GUID</param>
        ///<returns>List of milestones</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"milestone/filter")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByFilter(
            int projectid,
            int tag,
            MilestoneStatus? status,
            ApiDateTime deadlineStart,
            ApiDateTime deadlineStop, Guid? taskResponsible,
            int lastId,
            bool myProjects,
            Guid milestoneResponsible)
        {
            var milestoneEngine = EngineFactory.GetMilestoneEngine();

            var filter = new TaskFilter
                {
                    UserId = milestoneResponsible,
                    ParticipantId = taskResponsible,
                    TagId = tag,
                    FromDate = deadlineStart,
                    ToDate = deadlineStop,
                    SortBy = _context.SortBy,
                    SortOrder = !_context.SortDescending,
                    SearchText = _context.FilterValue,
                    Offset = _context.StartIndex,
                    Max = _context.Count,
                    LastId = lastId,
                    MyProjects = myProjects
                };

            if (projectid != 0)
            {
                filter.ProjectIds.Add(projectid);
            }

            if (status != null)
            {
                filter.MilestoneStatuses.Add((MilestoneStatus)status);
            }

            _context.SetDataPaginated();
            _context.SetDataFiltered();
            _context.SetDataSorted();
            _context.TotalCount = milestoneEngine.GetByFilterCount(filter);

            return milestoneEngine.GetByFilter(filter).NotFoundIfNull().Select(r => new MilestoneWrapper(r)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all overdue milestones in the portal projects
        ///</summary>
        ///<short>
        ///Overdue milestones
        ///</short>
        /// <category>Milestones</category>
        ///<returns>List of milestones</returns>
        [Read(@"milestone/late")]
        public IEnumerable<MilestoneWrapper> GetLateMilestones()
        {
            var milestones = EngineFactory.GetMilestoneEngine().GetLateMilestones((int)_context.Count);
            _context.SetDataPaginated();
            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all milestones due on the date specified in the request
        ///</summary>
        ///<short>
        ///Milestones by full date
        ///</short>
        /// <category>Milestones</category>
        ///<param name="year">Deadline year</param>
        ///<param name="month">Deadline month</param>
        ///<param name="day">Deadline day</param>
        ///<returns>List of milestones</returns>
        [Read(@"milestone/{year}/{month}/{day}")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByDeadLineFull(int year, int month, int day)
        {
            var milestones = EngineFactory.GetMilestoneEngine().GetByDeadLine(new DateTime(year, month, day));
            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list of all milestones due in the month specified in the request
        ///</summary>
        ///<short>
        ///Milestones by month
        ///</short>
        /// <category>Milestones</category>
        ///<param name="year">Deadline year</param>
        ///<param name="month">Deadline month</param>
        ///<returns>List of milestones</returns>
        [Read(@"milestone/{year}/{month}")]
        public IEnumerable<MilestoneWrapper> GetMilestonesByDeadLineMonth(int year, int month)
        {
            var milestones = EngineFactory.GetMilestoneEngine().GetByDeadLine(new DateTime(year, month, DateTime.DaysInMonth(year, month)));
            return milestones.Select(x => new MilestoneWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the list with the detailed information about the milestone with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get milestone
        ///</short>
        /// <category>Milestones</category>
        ///<param name="id">Milestone ID</param>
        ///<returns>Milestone</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper GetMilestoneById(int id)
        {
            var milestoneEngine = EngineFactory.GetMilestoneEngine();
            if (!milestoneEngine.IsExists(id)) throw new ItemNotFoundException();
            return new MilestoneWrapper(milestoneEngine.GetByID(id));
        }

        ///<summary>
        ///Returns the list of all tasks within the milestone with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get milestone tasks 
        ///</short>
        /// <category>Milestones</category>
        ///<param name="id">Milestone ID </param>
        ///<returns>Tasks list</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Read(@"milestone/{id:[0-9]+}/task")]
        public IEnumerable<TaskWrapper> GetMilestoneTasks(int id)
        {
            if (!EngineFactory.GetMilestoneEngine().IsExists(id)) throw new ItemNotFoundException();
            return EngineFactory.GetTaskEngine().GetMilestoneTasks(id).Select(x => new TaskWrapper(x)).ToSmartList();
        }

        ///<summary>
        ///Updates the selected milestone changing the milestone parameters (title, deadline, status, etc.) specified in the request
        ///</summary>
        ///<short>
        ///Update milestone
        ///</short>
        /// <category>Milestones</category>
        ///<param name="id">Milestone ID</param>
        ///<param name="title">Title</param>
        ///<param name="deadline">Deadline</param>
        ///<param name="isKey">Is key or not</param>
        ///<param name="status">Status</param>
        ///<param name="isNotify">Remind me 48 hours before the due date</param>
        ///<param name="description">Milestone description</param>
        ///<param name="projectID">Project ID</param>
        ///<param name="responsible">Milestone responsible</param>
        ///<param name="notifyResponsible">Notify responsible</param>
        ///<returns>Updated milestone</returns>
        ///<exception cref="ArgumentNullException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     title:"New title",
        ///     deadline:"2011-03-23T14:27:14",
        ///     isKey:false,
        ///     status:"Open"
        /// }
        /// ]]>
        /// </example>
        [Update(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper UpdateMilestone(int id, string title, ApiDateTime deadline, bool? isKey, MilestoneStatus status, bool? isNotify, string description, int projectID, Guid responsible, bool notifyResponsible)
        {
            var milestoneEngine = EngineFactory.GetMilestoneEngine();

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();
            ProjectSecurity.DemandEdit(milestone);

            milestone.Description = Update.IfNotEmptyAndNotEquals(milestone.Description, description);
            milestone.Title = Update.IfNotEmptyAndNotEquals(milestone.Title, title);
            milestone.DeadLine = Update.IfNotEmptyAndNotEquals(milestone.DeadLine, deadline);
            milestone.Responsible = Update.IfNotEmptyAndNotEquals(milestone.Responsible, responsible);

            if (isKey.HasValue)
                milestone.IsKey = isKey.Value;

            if (isNotify.HasValue)
                milestone.IsNotify = isNotify.Value;
            
            if (projectID != 0)
            {
                var project = EngineFactory.GetProjectEngine().GetByID(projectID).NotFoundIfNull();
                milestone.Project = project;
            }

            milestone = milestoneEngine.SaveOrUpdate(milestone, notifyResponsible);
            MessageService.Send(Request, MessageAction.MilestoneUpdated, milestone.Project.Title, milestone.Title);

            return new MilestoneWrapper(milestone);
        }

        ///<summary>
        ///Updates the status of the milestone with the ID specified in the request
        ///</summary>
        ///<short>
        ///Update milestone status
        ///</short>
        /// <category>Milestones</category>
        ///<param name="id">Milestone ID</param>
        ///<param name="status">Status</param>
        ///<returns>Updated milestone</returns>
        ///<exception cref="ArgumentNullException"></exception>
        ///<exception cref="ItemNotFoundException"></exception>
        ///<example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     status:"Open"
        /// }
        /// ]]>
        /// </example>
        [Update(@"milestone/{id:[0-9]+}/status")]
        public MilestoneWrapper UpdateMilestone(int id, MilestoneStatus status)
        {
            var milestoneEngine = EngineFactory.GetMilestoneEngine();

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();

            milestone = milestoneEngine.ChangeStatus(milestone, status);
            MessageService.Send(Request, MessageAction.MilestoneUpdatedStatus, milestone.Project.Title, milestone.Title, LocalizedEnumConverter.ConvertToString(milestone.Status));

            return new MilestoneWrapper(milestone);
        }

        ///<summary>
        ///Deletes the milestone with the ID specified in the request
        ///</summary>
        ///<short>
        ///Delete milestone
        ///</short>
        /// <category>Milestones</category>
        ///<param name="id">Milestone ID</param>
        ///<returns>Deleted milestone</returns>
        ///<exception cref="ItemNotFoundException"></exception>
        [Delete(@"milestone/{id:[0-9]+}")]
        public MilestoneWrapper DeleteMilestone(int id)
        {
            var milestoneEngine = EngineFactory.GetMilestoneEngine();

            var milestone = milestoneEngine.GetByID(id).NotFoundIfNull();

            milestoneEngine.Delete(milestone);
            MessageService.Send(Request, MessageAction.MilestoneDeleted, milestone.Project.Title, milestone.Title);

            return new MilestoneWrapper(milestone);
        }

        #endregion
    }
}